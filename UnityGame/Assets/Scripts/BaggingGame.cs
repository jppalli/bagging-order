using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BaggingOrder
{
    public class QueuePick : MonoBehaviour { public int Id; }
    public class PackedPick : MonoBehaviour { public int Id; }
    public partial class BaggingGame : MonoBehaviour
    {
        readonly Dictionary<int,Transform> queueObjects=new Dictionary<int,Transform>();
        readonly List<Transform> beltRibs=new List<Transform>();
        Coroutine feedRoutine;
        bool feeding, switchingBag;
        Coroutine bagSwitchRoutine;
        float beltSpeed,beltTravel,completedAt;
        Vector3 ghostAim,ghostVelocity,dropOrigin;
        Quaternion ghostRotation,dropRotation;
        bool hasDropOrigin;
        Texture2D rounded;
        Font displayFont,bodyFont;
        readonly List<PackedHoverTransparency> packedVisibility=new List<PackedHoverTransparency>();
        Camera view;
        Camera dragView;
        PackedHoverTransparency bagVisibility;
        Transform world, bagArt, packedArt, queueArt, hints;
        List<PackedItem> remaining=new List<PackedItem>();
        List<List<PackedItem>> bags=new List<List<PackedItem>>();
        class Snapshot { public List<PackedItem> Queue; public List<List<PackedItem>> Bags; public int Bag; }
        Stack<Snapshot> history=new Stack<Snapshot>();
        int level,bagIndex;
        PackedItem selected,movingOriginal;
        Snapshot moveSnapshot;
        Vector3 selectedOrigin;
        Quaternion selectedOriginRotation;
        string inspectedKey;
        float inspectedUntil;
        readonly Dictionary<int,Transform> packedObjects=new Dictionary<int,Transform>();
        int orientation;
        bool turnUsed,held,tapPlacement,showList,showLevels,showOptions;
        bool dragging,complete,muted;
        GameObject ghost;
        Placement hovered;
        readonly List<Placement> candidates=new List<Placement>();
        readonly List<Renderer> hintRenderers=new List<Renderer>();
        Vector2 pressPosition;
        Vector3 grabLocal;
        float grabDepth,heldScale=1f;
        bool dragHasMoved;
        Placement outlinedPlacement;
        Material footprintMaterial;
        string status="Drag a grocery from the conveyor into the bag";
        AudioSource audioOut;
        AudioClip placeSound;


        Material cream,green,paper,edge,metal;
        bool initialized;
        Checkout Level=>PackingRules.Levels[level];
        public void Awake(){Initialize();}
        public void Initialize()
        {
            if(initialized)return;initialized=true;
            QualitySettings.antiAliasing=4;QualitySettings.shadowDistance=30;
            Application.targetFrameRate=60;
            cream=Mat(new Color(.96f,.91f,.75f));green=Mat(new Color(.16f,.32f,.23f));
            paper=Mat(new Color(.72f,.54f,.31f));edge=Mat(new Color(.55f,.39f,.21f));metal=Mat(new Color(.30f,.38f,.32f));
            world=new GameObject("Checkout environment").transform;world.SetParent(transform);
            view=new GameObject("Game Camera").AddComponent<Camera>();view.tag="MainCamera";view.transform.SetParent(transform);
            view.orthographic=true;view.nearClipPlane=.1f;view.farClipPlane=100;view.backgroundColor=new Color(.70f,.79f,.64f);view.clearFlags=CameraClearFlags.SolidColor;
            view.transform.position=new Vector3(2.8f,7.2f,-13);view.transform.LookAt(new Vector3(0,1,-.7f));
            view.gameObject.AddComponent<AudioListener>();
            // A separate depth buffer keeps the held product readable through the bag.
            view.cullingMask&=~((1<<31)|(1<<30));
            dragView=new GameObject("Held product camera").AddComponent<Camera>();dragView.transform.SetParent(view.transform,false);
            dragView.clearFlags=CameraClearFlags.Depth;dragView.cullingMask=1<<31;dragView.depth=view.depth+1;
            dragView.orthographic=true;dragView.nearClipPlane=view.nearClipPlane;dragView.farClipPlane=view.farClipPlane;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.82f,.88f,.77f);RenderSettings.ambientEquatorColor=new Color(.56f,.62f,.48f);RenderSettings.ambientGroundColor=new Color(.32f,.37f,.26f);
            var light=new GameObject("Warm shop light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;light.color=new Color(1,.94f,.81f);light.transform.rotation=Quaternion.Euler(45,-35,0);light.shadows=LightShadows.Soft;light.transform.SetParent(world);
            Box(world,"Shop floor",new Vector3(0,-.7f,0),new Vector3(40,.1f,40),Mat(new Color(.64f,.73f,.55f)));
            Box(world,"Counter",new Vector3(0,-.34f,-1.2f),new Vector3(9,.48f,7.2f),Mat(new Color(.84f,.73f,.53f)));
            Box(world,"Counter trim",new Vector3(0,-.47f,-1.2f),new Vector3(9.1f,.17f,7.3f),green);
            Box(world,"Conveyor housing",new Vector3(0,.03f,-3.15f),new Vector3(6.8f,.26f,2.35f),metal);
            Box(world,"Rubber belt",new Vector3(0,.18f,-3.15f),new Vector3(6.5f,.07f,2.15f),Mat(new Color(.22f,.28f,.23f)));
            for(int i=-12;i<=12;i++)beltRibs.Add(Box(world,"Belt rib",new Vector3(i*.26f,.224f,-3.15f),new Vector3(.016f,.012f,2.09f),metal).transform);
            // A small shop backdrop gives the board a setting without obscuring it.
            Box(world,"Back wall",new Vector3(0,2,5),new Vector3(18,6,.2f),Mat(new Color(.61f,.73f,.53f)));
            for(int row=0;row<2;row++){
                Box(world,"Shelf",new Vector3(-3,1+row*1.8f,4.55f),new Vector3(4.2f,.12f,.65f),cream);
                for(int i=0;i<7;i++)Box(world,"Pantry package",new Vector3(-4.6f+i*.53f,1.4f+row*1.8f,4.6f),new Vector3(.35f,.7f,.3f),Mat(Color.Lerp(new Color(.73f,.37f,.21f),new Color(.81f,.72f,.34f),i/7f)));
            }
            bagArt=new GameObject("Paper bag").transform;bagArt.SetParent(world);
            packedArt=new GameObject("Packed groceries").transform;packedArt.SetParent(world);
            queueArt=new GameObject("Choose a grocery").transform;queueArt.SetParent(world);
            hints=new GameObject("Landing previews").transform;hints.SetParent(world);
            InitializeAudio();
            DressShop();level=Mathf.Clamp(PlayerPrefs.GetInt("checkout-curriculum-v2",0),0,PackingRules.Levels.Length-1);StartLevel();PrepareMarketUI();mapSelection=level;showLevels=!Environment.GetCommandLineArgs().Contains("-baggingSmokeTest");
            if(Application.isPlaying&&Environment.GetCommandLineArgs().Contains("-baggingSmokeTest"))StartCoroutine(SmokeTest());
        }
        static Material Mat(Color color){var m=new Material(Shader.Find("Standard"));m.color=color;m.SetFloat("_Glossiness",.12f);return m;}
        static GameObject Box(Transform parent,string name,Vector3 position,Vector3 size,Material material)
        {var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(parent);o.transform.localPosition=position;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=material;return o;}
        static void Remove(UnityEngine.Object o){if(Application.isPlaying)Destroy(o);else DestroyImmediate(o);}
        static void Clear(Transform parent){foreach(Transform child in parent.Cast<Transform>().ToArray()){child.gameObject.SetActive(false);Remove(child.gameObject);}}
        void StartLevel()
        {
            if(successOut!=null)successOut.Stop();
            ResetBagTransition();CancelDrag();if(feedRoutine!=null)StopCoroutine(feedRoutine);feedRoutine=null;feeding=false;queueObjects.Clear();Clear(queueArt);complete=false;history.Clear();bagIndex=0;bags=new List<List<PackedItem>>{new List<PackedItem>()};
            remaining=Level.Items.Select((key,id)=>new PackedItem{Key=key,Id=id}).ToList();
            Clear(bagArt);float h=Level.Height,w=Level.Width,d=Level.Depth;
            Box(bagArt,"Bag floor",new Vector3(0,-.02f,0),new Vector3(w+.25f,.1f,d+.24f),paper);
            Box(bagArt,"Back of bag",new Vector3(0,h*.5f,d*.5f+.1f),new Vector3(w+.25f,h,.06f),paper);
            Box(bagArt,"Folded side",new Vector3(-w*.5f-.11f,h*.5f,0),new Vector3(.06f,h,d+.2f),paper);
            Box(bagArt,"Open front lip",new Vector3(0,.14f,-d*.5f-.1f),new Vector3(w+.25f,.3f,.06f),paper);
            Box(bagArt,"Open side lip",new Vector3(w*.5f+.11f,.14f,0),new Vector3(.06f,.3f,d+.2f),paper);
            for(int y=1;y<h;y++)Box(bagArt,"Paper crease",new Vector3(0,y,d*.5f+.062f),new Vector3(w,.009f,.012f),edge);
            for(int x=1;x<w;x++)Box(bagArt,"Floor fold",new Vector3(x-w*.5f,.036f,0),new Vector3(.009f,.006f,d),edge);
            for(int z=1;z<d;z++)Box(bagArt,"Depth fold",new Vector3(0,.036f,z-d*.5f),new Vector3(w,.006f,.009f),edge);
            if(Level.SealHeight>0){
                var mark=Mat(new Color(.97f,.75f,.25f));
                Box(bagArt,"Seal height guide",new Vector3(0,Level.SealHeight,d*.5f+.047f),new Vector3(w,.035f,.025f),mark);
                Box(bagArt,"Seal height side guide",new Vector3(-w*.5f-.075f,Level.SealHeight,0),new Vector3(.025f,.035f,d),mark);
            }
            for(int i=0;i<2;i++){
                var line=new GameObject("Paper handle").AddComponent<LineRenderer>();line.transform.SetParent(bagArt);line.useWorldSpace=false;line.positionCount=17;line.startWidth=line.endWidth=.045f;line.sharedMaterial=edge;
                for(int j=0;j<17;j++){float a=Mathf.PI*j/16;line.SetPosition(j,new Vector3((i==0?-1:1)*w*.21f+Mathf.Cos(a)*.23f,h+Mathf.Sin(a)*.4f,d*.5f+.1f));}
            }
            DressBag();if(bagVisibility!=null)Remove(bagVisibility);bagVisibility=bagArt.gameObject.AddComponent<PackedHoverTransparency>();bagVisibility.Initialize(true,.32f);
            status=Level.Hint;showOptions=false;showList=false;showLevels=false;Rebuild();
        }
        GameObject Model(string key,Transform parent)
        {
            var asset=Resources.Load<GameObject>("Groceries/"+key);
            if(asset==null)throw new InvalidOperationException("Missing Blender model: "+key);
            var o=Instantiate(asset);o.name=key;
            o.transform.position=Vector3.zero;o.transform.rotation=Quaternion.Euler(0,180,0);o.transform.localScale=Vector3.one;
            var renderers=o.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            var wanted=PackingRules.Catalog[key].Size;
            var scale=new Vector3(wanted.x/bounds.size.x,wanted.y/bounds.size.y,wanted.z/bounds.size.z);
            o.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            var root=new GameObject(key);root.transform.SetParent(parent,false);
            var wrapper=new GameObject(key+" normalized");wrapper.transform.SetParent(root.transform,false);
            o.transform.SetParent(wrapper.transform,false);wrapper.transform.localScale=scale;
            if(Level.ColdGoal&&PackingRules.Catalog[key].Cold)ColdGroceryBadge.Attach(root.transform,wanted,view);
            return root;
        }
        void Rebuild(int animateId=-1,HashSet<int> newlyCrushed=null)
        {
            packedVisibility.Clear();packedObjects.Clear();Clear(packedArt);
            foreach(var item in bags[bagIndex]){
                var o=Model(item.Key,packedArt);o.transform.position=item.VisualPosition;o.transform.rotation=item.Rotation;
                packedObjects[item.Id]=o.transform;var pick=o.AddComponent<PackedPick>();pick.Id=item.Id;
                var collider=o.AddComponent<BoxCollider>();var dimensions=PackingRules.Catalog[item.Key].Size;
                collider.size=(Vector3)dimensions;collider.center=Vector3.up*dimensions.y*.5f;
                o.AddComponent<GroceryDamageVisual>().Initialize(item.Crushed,newlyCrushed!=null&&newlyCrushed.Contains(item.Id));
                var visibility=o.AddComponent<PackedHoverTransparency>();visibility.Initialize(item.Z==0&&item.Size.z==1);packedVisibility.Add(visibility);
                if(item.Id==animateId&&Application.isPlaying)StartCoroutine(Settle(o.transform,item.VisualPosition,o.transform.localScale));
            }
            SyncConveyor();
        }
        static Vector3 Slot(int n)=>new Vector3((n-1)*2.05f,.25f,-3.15f);
        void SyncConveyor()
        {
            if(feedRoutine!=null){StopCoroutine(feedRoutine);feedRoutine=null;}feeding=false;
            var visible=remaining.Take(3).ToList();var ids=new HashSet<int>(visible.Select(i=>i.Id));
            foreach(int id in queueObjects.Keys.Where(id=>!ids.Contains(id)).ToArray()){
                queueObjects[id].gameObject.SetActive(false);Remove(queueObjects[id].gameObject);queueObjects.Remove(id);
            }
            var starts=new List<Vector3>();var objects=new List<Transform>();
            for(int n=0;n<visible.Count;n++){
                var item=visible[n];
                if(!queueObjects.TryGetValue(item.Id,out var root)){
                    root=new GameObject("Pick "+item.Key).transform;root.SetParent(queueArt);root.position=new Vector3(4.2f+n*.7f,.25f,-3.15f);
                    var art=Model(item.Key,root);art.transform.localPosition=Vector3.zero;art.transform.localScale=Vector3.one;
                    var collider=root.gameObject.AddComponent<BoxCollider>();var size=PackingRules.Catalog[item.Key].Size;collider.size=(Vector3)size;collider.center=Vector3.up*size.y*.5f;root.gameObject.AddComponent<QueuePick>().Id=item.Id;queueObjects.Add(item.Id,root);
                }
                root.gameObject.SetActive(true);starts.Add(root.position);objects.Add(root);
            }
            if(!Application.isPlaying){for(int n=0;n<objects.Count;n++)objects[n].position=Slot(n);return;}
            if(objects.Count>0)feedRoutine=StartCoroutine(FeedConveyor(objects,starts));
        }
        IEnumerator FeedConveyor(List<Transform> objects,List<Vector3> starts)
        {
            float distance=0;for(int n=0;n<objects.Count;n++)distance=Mathf.Max(distance,Vector3.Distance(starts[n],Slot(n)));
            if(distance<.001f)yield break;
            feeding=true;float duration=Mathf.Clamp(distance/4f,.48f,1.6f),elapsed=0;
            while(elapsed<duration){
                elapsed+=Time.deltaTime;float t=Mathf.Clamp01(elapsed/duration),ease=t*t*(3-2*t);
                for(int n=0;n<objects.Count;n++)if(objects[n]!=null){
                    objects[n].position=Vector3.Lerp(starts[n],Slot(n),ease);
                    objects[n].rotation=Quaternion.Euler(0,0,Mathf.Sin(t*Mathf.PI*4)*1.4f*(1-t));
                    // Products emerge at the belt intake, instead of floating beside the counter.
                    foreach(var r in objects[n].GetComponentsInChildren<Renderer>())r.enabled=objects[n].position.x<3.35f;
                }
                yield return null;
            }
            for(int n=0;n<objects.Count;n++)if(objects[n]!=null){objects[n].position=Slot(n);objects[n].rotation=Quaternion.identity;foreach(var r in objects[n].GetComponentsInChildren<Renderer>())r.enabled=true;}
            feeding=false;feedRoutine=null;
        }
        IEnumerator Settle(Transform item,Vector3 target,Vector3 scale)
        {
            Vector3 origin=hasDropOrigin?dropOrigin:target+Vector3.up*.5f;
            Quaternion originRotation=hasDropOrigin?dropRotation:item.rotation,targetRotation=item.rotation;
            hasDropOrigin=false;float t=0;
            while(t<.24f&&item!=null){t+=Time.deltaTime;float u=Mathf.Clamp01(t/.24f),ease=u*u*u*(u*(u*6-15)+10);item.position=Vector3.Lerp(origin,target,ease);item.rotation=Quaternion.Slerp(originRotation,targetRotation,ease);yield return null;}
            if(item!=null){item.position=target;item.rotation=targetRotation;item.localScale=scale;}
        }
        void ResetBagTransition()
        {
            if(bagSwitchRoutine!=null)StopCoroutine(bagSwitchRoutine);
            bagSwitchRoutine=null;switchingBag=false;
            if(bagArt!=null)bagArt.localPosition=Vector3.zero;
            if(packedArt!=null)packedArt.localPosition=Vector3.zero;
        }
        void OpenNewBag()
        {
            if(switchingBag||dragging||feeding||complete)return;
            int empty=bags.FindIndex(b=>b.Count==0);
            SwitchBag(empty>=0?empty:bags.Count);
        }
        void SwitchBag(int destination)
        {
            if(switchingBag||dragging||feeding||complete||destination<0||destination>bags.Count||destination==bagIndex)return;
            if(destination==bags.Count&&(bags[bagIndex].Count==0||bags.Count>=Level.TargetBags)){status="This checkout allows "+Level.TargetBags+" bag(s). Rearrange groceries to make room.";return;}
            Remember();CancelDrag();
            if(destination==bags.Count)bags.Add(new List<PackedItem>());
            PlaySoft(paperSound);switchingBag=true;bagSwitchRoutine=StartCoroutine(AnimateBagSwitch(destination));
        }
        IEnumerator AnimateBagSwitch(int destination)
        {
            int direction=destination>bagIndex?1:-1;status="Opening bag "+(destination+1)+"...";
            float elapsed=0;
            while(elapsed<.22f){elapsed+=Time.deltaTime;float u=Mathf.Clamp01(elapsed/.22f);float x=-direction*8*u*u;
                bagArt.localPosition=packedArt.localPosition=Vector3.right*x;yield return null;}
            bagArt.localPosition=packedArt.localPosition=Vector3.zero;
            bagIndex=destination;Rebuild();
            elapsed=0;
            while(elapsed<.3f){elapsed+=Time.deltaTime;float u=Mathf.Clamp01(elapsed/.3f);float x=direction*8*Mathf.Pow(1-u,3);
                bagArt.localPosition=packedArt.localPosition=Vector3.right*x;yield return null;}
            bagArt.localPosition=packedArt.localPosition=Vector3.zero;
            switchingBag=false;bagSwitchRoutine=null;
            status="Bag "+(bagIndex+1)+" open · "+bags[bagIndex].Count+" groceries packed here";
        }
        Snapshot CaptureSnapshot()=>new Snapshot{Queue=remaining.Select(i=>i.Copy()).ToList(),Bags=bags.Select(b=>b.Select(i=>i.Copy()).ToList()).ToList(),Bag=bagIndex};
        void Remember(){history.Push(CaptureSnapshot());}
        void Undo(){if(switchingBag||history.Count==0)return;CancelDrag();var s=history.Pop();remaining=s.Queue;bags=s.Bags;bagIndex=s.Bag;complete=false;status="Undone. Try another arrangement.";Rebuild();}
        void BuildTargets()
        {
            Clear(hints);candidates.Clear();hintRenderers.Clear();hovered=null;outlinedPlacement=null;
            for(int z=0;z<Level.Depth;z++)for(int x=0;x<Level.Width;x++){
                var p=PackingRules.Preview(bags[bagIndex],selected.Key,selected.Id,x,z,orientation,Level.Height,Level.Width,Level.Depth);if(!p.Valid)continue;candidates.Add(p);
            }
            BakePlacementShadow();DrawLandingHints(null);
        }
        void BeginDrag(int id,Vector2 point)
        {
            if(feeding||switchingBag)return;CancelDrag();
            inspectedDamage=null;selected=remaining.First(i=>i.Id==id);selectedOrigin=queueObjects[id].position;selectedOriginRotation=Quaternion.identity;
            StartSelection(point,0);queueObjects[id].gameObject.SetActive(false);
        }
        void BeginPackedDrag(int id,Vector2 point)
        {
            if(feeding||switchingBag)return;CancelDrag();
            var item=bags[bagIndex].First(i=>i.Id==id);inspectedDamage=item.CrushedBy;inspectedKey=item.Key;inspectedUntil=Time.unscaledTime+4;
            if(!PackingRules.CanLift(bags[bagIndex],item)){status="Move the groceries resting on this one first.";return;}
            moveSnapshot=CaptureSnapshot();movingOriginal=item.Copy();selected=item.Copy();
            selectedOrigin=item.VisualPosition;selectedOriginRotation=item.Rotation;
            bags[bagIndex].Remove(item);packedObjects[id].gameObject.SetActive(false);
            StartSelection(point,item.Orientation);
        }
        void StartSelection(Vector2 point,int initialOrientation)
        {
            PlaySoft(pickupSound);
            orientation=initialOrientation;turnUsed=selected.UsedTurn;held=true;tapPlacement=false;dragging=true;pressPosition=point;
            inspectedKey=selected.Key;ghost=Model(selected.Key,world);ghost.name="Dragged grocery";if(selected.Crushed)ghost.AddComponent<GroceryDamageVisual>().Initialize(true);
            ghost.transform.position=selectedOrigin;ghost.transform.rotation=selectedOriginRotation;ghost.transform.localScale=Vector3.one;
            ghostAim=selectedOrigin;ghostVelocity=Vector3.zero;ghostRotation=selectedOriginRotation;
            heldScale=1f;dragHasMoved=false;lastDragPoint=point;
            grabDepth=view.WorldToScreenPoint(ghostAim+ghostRotation*(Vector3.up*PackingRules.Catalog[selected.Key].Size.y*.5f)).z;
            grabLocal=Quaternion.Inverse(ghostRotation)*(view.ScreenToWorldPoint(new Vector3(point.x,point.y,grabDepth))-ghostAim);
            ghostAim+=Vector3.up*.3f;
            foreach(var child in ghost.GetComponentsInChildren<Transform>())child.gameObject.layer=31;
            foreach(var renderer in ghost.GetComponentsInChildren<Renderer>())renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            BuildTargets();status="Turn / Tip, then choose a landing shadow";LayoutRotationControls();
        }
        Vector2 lastDragPoint;
        Rect rotationControls;
        float nextWheelTip;
        float UIScale=>Mathf.Max(.65f,Mathf.Min(Screen.width/1100f,Screen.height/720f));
        float RotationButtonHeight=>Mathf.Max(88,62/UIScale);
        void LayoutRotationControls()
        {
            if(selected==null)return;
            float scale=UIScale,w=Screen.width/scale,h=Screen.height/scale;
            float side=RotationButtonHeight;
            // A stable paired dock beneath the selected grocery, like the concept art.
            float x=w*.5f;
            rotationControls=new Rect(Mathf.Clamp(x-side,22,Mathf.Max(22,w-side*2-22)),h-112-side,side*2,side);
        }
        bool PointerOverBoard(Vector2 point)
        {
            float scale=UIScale,w=Screen.width/scale,h=Screen.height/scale;
            var gui=new Vector2(point.x/scale,(Screen.height-point.y)/scale);
            if(OverMarketUI(gui,w,h))return false;
            return selected==null||held||!rotationControls.Contains(gui);
        }
        void ParkSelection()
        {
            held=false;hovered=null;SetBehindHover(false);if(bagVisibility!=null)bagVisibility.SetBehindHover(false);
            ghostAim=selectedOrigin+Vector3.up*.3f;heldScale=1f;outlinedPlacement=null;DrawLandingHints(null);
            LayoutRotationControls();status="Turn / Tip, then drag or tap a landing spot. Tap empty space to cancel.";
        }
        void MoveDrag(Vector2 point)
        {
            lastDragPoint=point;dragHasMoved=true;
            var previousHover=hovered;hovered=null;float nearest=float.MaxValue;
            // Both the landing shadow and the product center are generous pickup targets.
            // Keep a bounded world-space margin so moving beyond the bag still cancels.
            var pointerRay=view.ScreenPointToRay(point);
            foreach(var p in candidates){
                float distance=float.MaxValue;
                foreach(float height in new[]{p.Item.Y+.055f,p.Item.Y+p.Item.Size.y*.5f}){
                    var anchor=p.Item.Position;anchor.y=height;
                    if(!new Plane(Vector3.up,anchor).Raycast(pointerRay,out float along))continue;
                    var hit=pointerRay.GetPoint(along);const float margin=.16f;
                    if(hit.x<p.Item.X-Level.Width*.5f-margin||hit.x>p.Item.X-Level.Width*.5f+p.Item.Size.x+margin||hit.z<p.Item.Z-Level.Depth*.5f-margin||hit.z>p.Item.Z-Level.Depth*.5f+p.Item.Size.z+margin)continue;
                    distance=Mathf.Min(distance,Vector2.Distance(point,view.WorldToScreenPoint(anchor)));
                }
                float radius=Mathf.Min(76*UIScale,Mathf.Min(Screen.width,Screen.height)*.12f);
                if(distance>radius+(p==previousHover?12*UIScale:0))continue;
                float score=distance-(p==previousHover?12*UIScale:0);
                if(score<nearest){nearest=score;hovered=p;}
            }
            if(!PointerOverBoard(point))hovered=null;
            ghostRotation=PackingRules.Orientations[orientation];
            heldScale=1f;
            // Targeting affects the landing shadow only; the grocery stays attached to the pointer.
            ghostAim=view.ScreenToWorldPoint(new Vector3(point.x,point.y,grabDepth))-ghostRotation*(grabLocal*heldScale)+Vector3.up*.3f;
            status=hovered==null?"Move into the bag to preview a fit":hovered.Crushed.Count>0?"Too much weight here — this will crush a grocery":"Release to pack";
            bool overBag=OverBagSilhouette(point);
            if(overBag&&hovered==null)status="Outside a supported spot â€” cannot place here";
            ghostRotation=PackingRules.Orientations[orientation];
            ShowFootprint(hovered);
        }
        Rect GroceryScreenRect(Transform root,Vector3 size)
        {
            Vector2 min=new Vector2(float.MaxValue,float.MaxValue),max=new Vector2(float.MinValue,float.MinValue);
            for(int x=0;x<2;x++)for(int y=0;y<2;y++)for(int z=0;z<2;z++){
                Vector2 point=view.WorldToScreenPoint(root.TransformPoint(new Vector3((x-.5f)*size.x,y*size.y,(z-.5f)*size.z)));
                min=Vector2.Min(min,point);max=Vector2.Max(max,point);
            }
            return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
        }
        void UpdateHeldOverlap()
        {
            if(ghost==null||selected==null)return;
            var heldRect=GroceryScreenRect(ghost.transform,PackingRules.Catalog[selected.Key].Size);
            foreach(var entry in packedObjects){
                var item=bags[bagIndex].FirstOrDefault(i=>i.Id==entry.Key);if(item==null||!entry.Value.gameObject.activeSelf)continue;
                var visibility=entry.Value.GetComponent<PackedHoverTransparency>();
                var rect=GroceryScreenRect(entry.Value,PackingRules.Catalog[item.Key].Size);
                if(visibility.IsFaded){rect.xMin-=5;rect.xMax+=5;rect.yMin-=5;rect.yMax+=5;}
                visibility.SetFaded(heldRect.Overlaps(rect));
            }
            foreach(var entry in queueObjects){
                if(!entry.Value.gameObject.activeSelf)continue;
                var item=remaining.FirstOrDefault(i=>i.Id==entry.Key);if(item==null)continue;
                var visibility=entry.Value.GetComponent<PackedHoverTransparency>();
                if(visibility==null){visibility=entry.Value.gameObject.AddComponent<PackedHoverTransparency>();visibility.Initialize(false,.25f);}
                var rect=GroceryScreenRect(entry.Value,PackingRules.Catalog[item.Key].Size);
                if(visibility.IsFaded){rect.xMin-=5;rect.xMax+=5;rect.yMin-=5;rect.yMax+=5;}
                visibility.SetFaded(heldRect.Overlaps(rect));
            }
        }
        void ShowFootprint(Placement placement)
        {
            if(placement==outlinedPlacement)return;
            outlinedPlacement=placement;DrawLandingHints(placement);
        }
        RenderTexture placementMask;
        Vector2 shadowSize;
        void BakePlacementShadow()
        {
            if(placementMask==null){placementMask=new RenderTexture(128,128,16,RenderTextureFormat.ARGB32);placementMask.wrapMode=TextureWrapMode.Clamp;placementMask.Create();}
            if(footprintMaterial==null)footprintMaterial=new Material(Resources.Load<Shader>("PlacementShadow"));
            var copy=Instantiate(ghost);copy.name="Placement silhouette baker";
            foreach(var t in copy.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
            foreach(var r in copy.GetComponentsInChildren<LineRenderer>())r.enabled=false;
            foreach(var badge in copy.GetComponentsInChildren<ColdGroceryBadge>()){badge.enabled=false;badge.GetComponent<MeshRenderer>().enabled=false;}
            copy.transform.rotation=PackingRules.Orientations[orientation];copy.transform.localScale=Vector3.one;
            var center=new Vector3(100,100,100);
            copy.transform.position=center-copy.transform.rotation*(Vector3.up*PackingRules.Catalog[selected.Key].Size.y*.5f);
            var renderers=copy.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled).ToArray();var bounds=renderers[0].bounds;
            foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            shadowSize=new Vector2(bounds.size.x+.22f,bounds.size.z+.22f);
            var cameraObject=new GameObject("Placement silhouette camera");var camera=cameraObject.AddComponent<Camera>();
            camera.enabled=false;camera.orthographic=true;camera.orthographicSize=shadowSize.y*.5f;camera.aspect=shadowSize.x/shadowSize.y;
            camera.transform.position=bounds.center+Vector3.up*10;camera.transform.rotation=Quaternion.LookRotation(Vector3.down,Vector3.forward);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=1<<30;camera.nearClipPlane=.1f;camera.farClipPlane=20;
            camera.targetTexture=placementMask;camera.RenderWithShader(Resources.Load<Shader>("PlacementSilhouette"),"");camera.targetTexture=null;
            footprintMaterial.mainTexture=placementMask;copy.SetActive(false);Remove(copy);Remove(cameraObject);
        }
        void DrawLandingHints(Placement active)
        {
            Clear(hints);
            foreach(var p in candidates){
                bool focused=p==active;
                Color color=p.Crushed.Count>0?new Color(.72f,.12f,.08f,1):new Color(.16f,.36f,.20f,1);
                color.a=focused?.88f:(active==null?.42f:.22f);
                var shadow=GameObject.CreatePrimitive(PrimitiveType.Quad);shadow.name=focused?"Active placement shadow":"Available placement shadow";
                Remove(shadow.GetComponent<Collider>());shadow.transform.SetParent(hints);shadow.layer=31;
                shadow.transform.position=p.Item.Position+Vector3.up*.045f;shadow.transform.rotation=Quaternion.Euler(90,0,0);
                shadow.transform.localScale=new Vector3(shadowSize.x,shadowSize.y,1);
                var renderer=shadow.GetComponent<MeshRenderer>();renderer.sharedMaterial=footprintMaterial;
                var properties=new MaterialPropertyBlock();properties.SetColor("_Color",color);renderer.SetPropertyBlock(properties);
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            }
        }
        void SetBehindHover(bool behind){
            foreach(var item in packedVisibility)if(item!=null){if(behind)item.SetBehindHover(true);else item.SetFaded(false);}
            if(!behind)foreach(var source in queueObjects.Values){var fade=source.GetComponent<PackedHoverTransparency>();if(fade!=null)fade.SetFaded(false);}
        }
        bool OverBagSilhouette(Vector2 point)
        {
            Vector2 min=new Vector2(float.MaxValue,float.MaxValue),max=new Vector2(float.MinValue,float.MinValue);
            foreach(float x in new[]{-Level.Width*.5f-.2f,Level.Width*.5f+.2f})foreach(float y in new[]{0f,Level.Height+.5f})foreach(float z in new[]{-Level.Depth*.5f-.15f,Level.Depth*.5f+.15f}){
                Vector2 p=view.WorldToScreenPoint(new Vector3(x,y,z));min=Vector2.Min(min,p);max=Vector2.Max(max,p);
            }
            return point.x>=min.x&&point.x<=max.x&&point.y>=min.y&&point.y<=max.y;
        }
        void CancelDrag()
        {
            bool restore=movingOriginal!=null;
            if(restore){bags[bagIndex].Add(movingOriginal);movingOriginal=null;moveSnapshot=null;}
            SetBehindHover(false);if(bagVisibility!=null)bagVisibility.SetBehindHover(false);
            if(selected!=null&&queueObjects.TryGetValue(selected.Id,out var source))source.gameObject.SetActive(true);
            dragging=false;held=false;selected=null;hovered=null;inspectedKey=null;
            if(ghost!=null){ghost.SetActive(false);Remove(ghost);ghost=null;}
            if(hints!=null)Clear(hints);candidates.Clear();hintRenderers.Clear();
            if(restore)Rebuild();
        }
        void Commit(Placement placement)
        {
            if(placement==null||!placement.Valid)return;
            bool rearranged=movingOriginal!=null;
            if(rearranged)history.Push(moveSnapshot);else Remember();
            var item=placement.Item.Copy();if(rearranged){item.Crushed=movingOriginal.Crushed;item.CrushedBy=movingOriginal.CrushedBy;}
            var newDamage=new HashSet<int>(bags[bagIndex].Where(i=>!i.Crushed&&placement.Crushed.Contains(i.Id)).Select(i=>i.Id));
            movingOriginal=null;moveSnapshot=null;item.UsedTurn=turnUsed&&orientation!=0;item.UsedTip=!PackingRules.IsUpright(item.Orientation);bags[bagIndex].Add(item);
            foreach(var i in bags[bagIndex])if(placement.Crushed.Contains(i.Id)){if(!i.Crushed)i.CrushedBy=PackingRules.Catalog[item.Key].Name;i.Crushed=true;}
            hasDropOrigin=ghost!=null;if(hasDropOrigin){dropOrigin=ghost.transform.position;dropRotation=ghost.transform.rotation;}remaining.RemoveAll(i=>i.Id==item.Id);CancelDrag();Rebuild(item.Id,newDamage);
            if(newDamage.Count>0&&!muted&&paperSound!=null)audioOut.PlayOneShot(paperSound,.4f);
            status=placement.Crushed.Count>0?"Oops! You can undo that.":"Nice fit!";
            PlaySoft(placeSound);
            if(remaining.Count==0){
                if(!PackingRules.ShapeGoalMet(Level,bags)){status=Level.PerfectFit?"Fill every space - rearrange the groceries.":"Level the top at the gold mark, then seal.";return;}
                complete=true;completedAt=Time.unscaledTime;status="All packed!";BeginWinCelebration();
                if(!Environment.GetCommandLineArgs().Contains("-baggingSmokeTest")){
                    int stars=PackingRules.Stars(Level,bags);PlayerPrefs.SetInt(BestKey(level),Mathf.Max(stars,PlayerPrefs.GetInt(BestKey(level),0)));
                    PlayerPrefs.SetInt("checkout-curriculum-v2",Mathf.Min(level+1,PackingRules.Levels.Length-1));PlayerPrefs.Save();
                }
                if(Level.SealHeight>0)StartCoroutine(SealBag());
            }
        }
        IEnumerator SealBag()
        {
            var hinge=new GameObject("Closing paper fold").transform;hinge.SetParent(bagArt,false);hinge.localPosition=new Vector3(0,Level.SealHeight+.2f,Level.Depth*.5f+.08f);
            Box(hinge,"Paper lid",new Vector3(0,0,-Level.Depth*.5f),new Vector3(Level.Width+.2f,.045f,Level.Depth+.18f),paper);
            float elapsed=0;
            while(elapsed<.5f&&hinge!=null&&hinge.gameObject.activeInHierarchy){elapsed+=Time.deltaTime;float t=Mathf.SmoothStep(0,1,elapsed/.5f);hinge.localRotation=Quaternion.Euler(90*(1-t),0,0);yield return null;}
        }
        void ReturnSelection()
        {
            if(ghost==null||selected==null){CancelDrag();return;}
            if(movingOriginal!=null){
                int id=selected.Id;var returningPacked=ghost;ghost=null;CancelDrag();
                var packedSource=packedObjects[id];packedSource.gameObject.SetActive(false);
                StartCoroutine(ReturnToBelt(returningPacked,packedSource));status="Returned to its previous position.";return;
            }
            var returning=ghost;var source=queueObjects[selected.Id];ghost=null;CancelDrag();source.gameObject.SetActive(false);
            StartCoroutine(ReturnToBelt(returning,source));
        }
        IEnumerator ReturnToBelt(GameObject returning,Transform source)
        {
            Vector3 start=returning.transform.position,scale=returning.transform.localScale;Quaternion rotation=returning.transform.rotation,targetRotation=source.rotation;
            float elapsed=0;
            while(elapsed<.2f&&source!=null&&!source.gameObject.activeSelf){
                elapsed+=Time.deltaTime;float t=1-Mathf.Pow(1-Mathf.Clamp01(elapsed/.2f),3);
                returning.transform.position=Vector3.Lerp(start,source.position,t);returning.transform.rotation=Quaternion.Slerp(rotation,targetRotation,t);returning.transform.localScale=Vector3.Lerp(scale,Vector3.one,t);
                if(elapsed>=.2f)break;yield return null;
            }
            if(source!=null)source.gameObject.SetActive(true);Remove(returning);
        }
        void RotateSelected(bool tip)
        {
            if(selected==null)return;
            int next=PackingRules.Rotate(selected.Key,orientation,tip);
            if(next==orientation){status=tip&&PackingRules.Catalog[selected.Key].Upright?"This product must stay upright.":"That turn leaves the same shape.";return;}
            PlaySoft(turnSound);
            var centerOffset=Vector3.up*PackingRules.Catalog[selected.Key].Size.y*.5f;
            var previousCenter=ghostAim+ghostRotation*centerOffset;orientation=next;if(!tip)turnUsed=true;BuildTargets();
            if(held)MoveDrag(lastDragPoint);
            else{ghostRotation=PackingRules.Orientations[orientation];ghostAim=previousCenter-ghostRotation*centerOffset;SetBehindHover(false);if(bagVisibility!=null)bagVisibility.SetBehindHover(false);status="Choose a landing spot, or rotate again.";}
            if(candidates.Count==0)status="No supported spot in this orientation. Try another turn.";
        }
        void Update()
        {
            if(!initialized)return;
            UpdateCozyAudio();UpdateCrushWarnings();
            float aspect=Screen.width/(float)Mathf.Max(1,Screen.height);FrameCamera(aspect);
            beltSpeed=Mathf.MoveTowards(beltSpeed,feeding?2.8f:0,Time.deltaTime*9);beltTravel+=beltSpeed*Time.deltaTime;
            for(int i=0;i<beltRibs.Count;i++){var p=beltRibs[i].localPosition;p.x=Mathf.Repeat((i-12)*.26f-beltTravel+3.25f,6.5f)-3.25f;beltRibs[i].localPosition=p;}
            if(complete||showLevels||showList||showOptions||switchingBag)return;
            Vector2 pointer=Input.mousePosition;
            bool overBoard=PointerOverBoard(pointer);
            bool started=false;
            if(!feeding&&!dragging&&Input.GetMouseButtonDown(0)&&overBoard){
                var ray=view.ScreenPointToRay(pointer);
                foreach(var hit in Physics.RaycastAll(ray,100).OrderBy(h=>h.distance)){
                    var pick=hit.collider.GetComponent<QueuePick>();var packedPick=hit.collider.GetComponent<PackedPick>();
                    if(pick!=null){BeginDrag(pick.Id,pointer);started=true;break;}
                    if(packedPick!=null){BeginPackedDrag(packedPick.Id,pointer);started=dragging;break;}
                }
            }
            if(dragging){
                if(Input.GetKeyDown(KeyCode.R)||Input.GetMouseButtonDown(1))RotateSelected(false);
                if(Input.GetKeyDown(KeyCode.F))RotateSelected(true);
                if(Input.touchCount==0&&Mathf.Abs(Input.mouseScrollDelta.y)>.01f&&Time.unscaledTime>=nextWheelTip){RotateSelected(true);nextWheelTip=Time.unscaledTime+.18f;}
                if(Input.GetKeyDown(KeyCode.Escape)){CancelDrag();status="Not placed";return;}
                if(!started&&!held&&Input.GetMouseButtonDown(0)&&overBoard){
                    held=true;tapPlacement=true;pressPosition=pointer;
                    grabDepth=view.WorldToScreenPoint(ghost.transform.position).z;
                    grabLocal=Quaternion.Inverse(ghostRotation)*(view.ScreenToWorldPoint(new Vector3(pointer.x,pointer.y,grabDepth))+Vector3.up*.3f-ghost.transform.position)/heldScale;
                }
                if(held&&(dragHasMoved||tapPlacement||Vector2.Distance(pointer,pressPosition)>8*UIScale)){MoveDrag(pointer);LayoutRotationControls();}
                ghost.transform.position=held&&dragHasMoved?ghostAim:Vector3.SmoothDamp(ghost.transform.position,ghostAim,ref ghostVelocity,.075f);
                ghost.transform.rotation=Quaternion.Slerp(ghost.transform.rotation,ghostRotation,1-Mathf.Exp(-18*Time.deltaTime));
                ghost.transform.localScale=Vector3.Lerp(ghost.transform.localScale,Vector3.one*heldScale,1-Mathf.Exp(-14*Time.deltaTime));
                UpdateHeldOverlap();
                if(held&&Input.GetMouseButtonUp(0)){
                    bool moved=Vector2.Distance(pointer,pressPosition)>8*UIScale;
                    if(overBoard&&hovered!=null&&(moved||tapPlacement))Commit(hovered);
                    else if(!moved&&!tapPlacement)ParkSelection();
                    else{bool fromBag=movingOriginal!=null;ReturnSelection();status=fromBag?"Returned to its previous position.":"Returned to the belt. Try another spot.";}
                }
            }
            if(Input.GetKeyDown(KeyCode.Z)&&!dragging)Undo();
        }
        void OnApplicationFocus(bool focus){if(!focus&&dragging)CancelDrag();}
        static Texture2D RoundedTexture(Color color)
        {
            const int size=32;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.wrapMode=TextureWrapMode.Clamp;
            for(int y=0;y<size;y++)for(int x=0;x<size;x++){float dx=Mathf.Max(8-x,x-23),dy=Mathf.Max(8-y,y-23);float d=new Vector2(Mathf.Max(0,dx),Mathf.Max(0,dy)).magnitude;var c=color;c.a*=Mathf.Clamp01(8.5f-d);texture.SetPixel(x,y,c);}texture.Apply();return texture;
        }
        IEnumerator SmokeTest()
        {
            yield return null;level=0;StartLevel();yield return new WaitUntil(()=>!feeding);
            if(emblem==null||starStamp==null||leafStamp==null||paintedRotations==null||paintedLogo==null||shopBackdrop==null||displayFont==null)throw new Exception("Market UI artwork not bundled");
            var testPick=remaining[0];BeginDrag(testPick.Id,view.WorldToScreenPoint(Slot(0)));
            if(ghost.transform.position!=queueObjects[testPick.Id].position||ghost.transform.localScale!=Vector3.one||hints.childCount==0)throw new Exception("Pickup must retain full size and reveal landing hints");
            ParkSelection();if(ghostAim!=queueObjects[testPick.Id].position+Vector3.up*.3f)throw new Exception("Tap selection must lift the product");held=true;
            tapPlacement=true;
            foreach(var landing in candidates){MoveDrag(view.WorldToScreenPoint(landing.Item.Position+Vector3.up*.055f));if(hovered!=landing)throw new Exception("Tap did not match visible landing outline");}
            tapPlacement=false;
            MoveDrag(view.WorldToScreenPoint(new Vector3(-1,.5f,1.22f)));
            if(hovered!=null||bagVisibility.IsFaded)throw new Exception("Behind-bag hover must keep the bag opaque and reject placement");
            if(ghost.GetComponentsInChildren<Renderer>().Any(r=>r.gameObject.layer!=31)||dragView.cullingMask!=(1<<31))throw new Exception("Held product visibility layer failed");
            int before=remaining.Count;Commit(hovered);if(remaining.Count!=before)throw new Exception("Invalid drop changed the queue");
            ghost.transform.position=ghostAim;CaptureBoard(System.IO.Path.Combine(Application.dataPath,"../invalid-depth-preview.png"));
            ReturnSelection();float returnDeadline=Time.unscaledTime+3;
            while(!queueObjects[testPick.Id].gameObject.activeSelf&&Time.unscaledTime<returnDeadline)yield return null;
            if(bagVisibility.IsFaded||!queueObjects[testPick.Id].gameObject.activeSelf)throw new Exception("Return to belt failed");
            if(PackingRules.Levels.Any(l=>l.Height!=4))throw new Exception("Bag height must be four for every level");
            // Exercise a middle-slot removal and undo while the new product is still arriving.
            OpenNewBag();if(bags.Count!=1)throw new Exception("Single-bag limit bypassed");
            level=5;StartLevel();yield return new WaitUntil(()=>!feeding);
            var bagTest=PackingRules.Preview(bags[0],remaining[0].Key,remaining[0].Id,0,0,0,Level.Height);
            Commit(bagTest);yield return new WaitUntil(()=>!feeding);yield return new WaitForSeconds(.3f);
            int originalId=bags[0][0].Id;OpenNewBag();yield return new WaitUntil(()=>!switchingBag&&!feeding);
            if(bagIndex!=1||bags.Count!=2||bags[0][0].Id!=originalId||bags[1].Count!=0)throw new Exception("New bag transition lost contents");
            SwitchBag(0);yield return new WaitUntil(()=>!switchingBag&&!feeding);
            OpenNewBag();yield return new WaitUntil(()=>!switchingBag&&!feeding);
            if(bags.Count!=2||bagIndex!=1||bagArt.localPosition!=Vector3.zero||packedArt.localPosition!=Vector3.zero)throw new Exception("Empty bag reuse or transition position failed");
            Undo();yield return new WaitUntil(()=>!feeding);if(bagIndex!=0||bags[0][0].Id!=originalId)throw new Exception("Bag navigation undo failed");
            level=0;StartLevel();yield return new WaitUntil(()=>!feeding);
            Commit(PackingRules.Preview(bags[0],remaining[0].Key,remaining[0].Id,0,0,0,4));yield return new WaitUntil(()=>!feeding);
            Commit(PackingRules.Preview(bags[0],remaining[0].Key,remaining[0].Id,0,0,0,4));yield return new WaitUntil(()=>!feeding);yield return new WaitForSeconds(.3f);
            OpenNewBag();if(bags.Count!=1||switchingBag)throw new Exception("Occupied single-bag limit bypassed");
            int queueCount=remaining.Count;
            BeginPackedDrag(0,view.WorldToScreenPoint(bags[0][0].Position));
            if(dragging||bags[0].Count!=2)throw new Exception("Cannot lift a supporting product");
            BeginPackedDrag(1,view.WorldToScreenPoint(bags[0][1].Position));CancelDrag();yield return new WaitUntil(()=>!feeding);
            if(bags[0].Count!=2||bags[0].First(i=>i.Id==1).Y!=1)throw new Exception("Cancelled move did not restore original");
            BeginPackedDrag(1,view.WorldToScreenPoint(bags[0].First(i=>i.Id==1).Position));
            Commit(PackingRules.Preview(bags[0],selected.Key,selected.Id,1,0,orientation,4));yield return new WaitUntil(()=>!feeding);yield return new WaitForSeconds(.3f);
            if(remaining.Count!=queueCount||bags[0].Count!=2||bags[0].First(i=>i.Id==1).X!=1)throw new Exception("Rearranging changed the queue or duplicated an item");
            Undo();yield return new WaitUntil(()=>!feeding);
            if(bags[0].First(i=>i.Id==1).Y!=1||remaining.Count!=queueCount)throw new Exception("Undo failed to restore moved grocery");
            StartLevel();yield return new WaitUntil(()=>!feeding);
            var middle=remaining[1];var middleDrop=PackingRules.Preview(bags[0],middle.Key,middle.Id,1,0,false,Level.Height);
            Commit(middleDrop);if(!feeding)throw new Exception("Restocking did not start");
            Undo();yield return new WaitUntil(()=>!feeding);
            if(remaining.Count!=6||!queueObjects.Keys.OrderBy(i=>i).SequenceEqual(new[]{0,1,2}))throw new Exception("Conveyor undo lost or duplicated products");
            foreach(var entry in queueObjects)if(!entry.Value.gameObject.activeSelf)throw new Exception("Restored product is hidden");
            int[] xs={0,1,0,1,0,0},zs={0,0,1,1,0,1};
            for(int i=0;i<6;i++){var item=remaining.First();var p=PackingRules.Preview(bags[0],item.Key,item.Id,xs[i],zs[i],false,Level.Height);if(!p.Valid||p.Crushed.Count>0)throw new Exception("Smoke placement failed "+i);Physics.SyncTransforms();
                var source=view.WorldToScreenPoint(new Vector3(-2.05f,.25f+PackingRules.Catalog[item.Key].Size.y*.5f,-3.15f));
                if(!Physics.Raycast(view.ScreenPointToRay(source),out var hit,100)||hit.collider.GetComponent<QueuePick>()?.Id!=item.Id)throw new Exception("Conveyor picking failed "+i);
                BeginDrag(item.Id,source);MoveDrag(view.WorldToScreenPoint(p.Item.Position+Vector3.up*p.Item.Size.y*.5f));
                if(hovered==null||hovered.Item.X!=xs[i]||hovered.Item.Z!=zs[i])throw new Exception("Drag target failed "+i);
                if(bagVisibility.IsFaded||hints.GetComponentsInChildren<LineRenderer>().Length!=0||hints.GetComponentsInChildren<MeshRenderer>().Length==0)throw new Exception("Placement must show silhouette shadows and keep the bag opaque");
                if(i==2){
                    if(packedVisibility.Count(v=>v.IsFaded)!=2)throw new Exception("Front groceries did not fade for back hover");
                    ghost.transform.position=ghostAim;
                    CaptureBoard(System.IO.Path.Combine(Application.dataPath,"../hover-preview.png"));
                    var front=candidates.First(c=>c.Item.Z==0);
                    MoveDrag(view.WorldToScreenPoint(front.Item.Position+Vector3.up*front.Item.Size.y*.5f));
                    if(packedVisibility.Any(v=>v.IsFaded))throw new Exception("Front hover did not restore opacity");
                    MoveDrag(view.WorldToScreenPoint(p.Item.Position+Vector3.up*p.Item.Size.y*.5f));CancelDrag();
                    if(packedVisibility.Any(v=>v.IsFaded))throw new Exception("Cancel did not restore opacity");
                    BeginDrag(item.Id,source);MoveDrag(view.WorldToScreenPoint(p.Item.Position+Vector3.up*p.Item.Size.y*.5f));
                }
                Commit(hovered);if(packedVisibility.Any(v=>v.IsFaded))throw new Exception("Drop did not restore opacity");yield return new WaitUntil(()=>!feeding);yield return new WaitForSeconds(.45f);}
            if(!complete)throw new Exception("Checkout did not complete");
            Undo();yield return new WaitUntil(()=>!feeding);if(remaining.Count!=1)throw new Exception("Undo failed");
            if(displayFont==null||bodyFont!=displayFont)throw new Exception("All UI must use the Kalam font");
            string path=System.IO.Path.Combine(Application.dataPath,"../smoke-preview.png");
            CaptureBoard(path);
            level=3;StartLevel();yield return new WaitUntil(()=>!feeding);
            BeginDrag(remaining[0].Id,view.WorldToScreenPoint(Slot(0)));RotateSelected(true);
            if(orientation==0||candidates.Count==0)throw new Exception("Tip control failed");
            var tipped=candidates.First();MoveDrag(view.WorldToScreenPoint(tipped.Item.Position+Vector3.up*tipped.Item.Size.y*.5f));
            if(hovered==null||hovered.Item.Orientation!=orientation)throw new Exception("Tipped target failed");
            Commit(hovered);yield return new WaitUntil(()=>!feeding);yield return new WaitForSeconds(.5f);
            if(!bags[0][0].UsedTip||bags[0][0].Size.y!=1)throw new Exception("Tip lost on commit");
            CaptureBoard(System.IO.Path.Combine(Application.dataPath,"../rotation-preview.png"));
            Undo();yield return new WaitUntil(()=>!feeding);if(bags[0].Count!=0)throw new Exception("Tipped undo failed");
            BeginDrag(remaining[0].Id,view.WorldToScreenPoint(Slot(0)));ParkSelection();
            var parked=ghostAim+ghostRotation*(Vector3.up*PackingRules.Catalog[selected.Key].Size.y*.5f);var controls=rotationControls;var buttonPoint=new Vector2(controls.center.x*UIScale,Screen.height-controls.center.y*UIScale);
            if(held||PointerOverBoard(buttonPoint))throw new Exception("Floating controls overlap placement input");
            RotateSelected(true);RotateSelected(false);
            if(held||Vector3.Distance(ghostAim+ghostRotation*(Vector3.up*PackingRules.Catalog[selected.Key].Size.y*.5f),parked)>.001f||rotationControls!=controls||orientation==0||!turnUsed||candidates.Count==0)throw new Exception("Stationary rotation controls failed");
            var tapTarget=candidates.First();held=true;tapPlacement=true;MoveDrag(view.WorldToScreenPoint(tapTarget.Item.Position+Vector3.up*.055f));
            if(hovered==null)throw new Exception("Tap placement preview failed");Commit(hovered);yield return new WaitUntil(()=>!feeding);
            if(!bags[0][0].UsedTurn||!bags[0][0].UsedTip)throw new Exception("Floating rotation lost on placement");
            level=1;StartLevel();yield return new WaitUntil(()=>!feeding);
            var bananaModel=Model("bananas",world);
            var bananaParts=bananaModel.GetComponentsInChildren<Renderer>();
            if(bananaParts.Count(r=>r.name.StartsWith("Curved banana finger"))!=4||bananaParts.Any(r=>r.name.StartsWith("Bundle cell")))throw new Exception("Bananas must be one natural four-finger bunch without grid trays");
            Remove(bananaModel);
            var banana=remaining[0];Commit(PackingRules.Preview(bags[0],banana.Key,banana.Id,0,0,0,4));yield return new WaitUntil(()=>!feeding);
            var can=remaining.First(i=>i.Key=="can");Commit(PackingRules.Preview(bags[0],can.Key,can.Id,1,1,0,4));yield return new WaitUntil(()=>!feeding);yield return new WaitForSeconds(.5f);
            CaptureBoard(System.IO.Path.Combine(Application.dataPath,"../banana-preview.png"));
            level=4;StartLevel();yield return new WaitUntil(()=>!feeding);
            BeginDrag(remaining[0].Id,view.WorldToScreenPoint(Slot(0)));RotateSelected(true);if(orientation!=0)throw new Exception("Upright control failed");CancelDrag();
            level=6;StartLevel();yield return new WaitUntil(()=>!feeding);
            if(queueArt.GetComponentsInChildren<ColdGroceryBadge>().Length!=3||queueArt.GetComponentsInChildren<LineRenderer>().Length!=0)throw new Exception("Cold groceries must use snowflake badges without box outlines");
            CaptureBoard(System.IO.Path.Combine(Application.dataPath,"../cold-preview.png"));
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(Application.dataPath,"../ui-preview.png"));yield return null;yield return new WaitForSeconds(.3f);
            level=11;StartLevel();yield return new WaitUntil(()=>!feeding);
            if(queueObjects.Count!=3||Resources.Load<GameObject>("Groceries/cornflakes")==null)throw new Exception("Family cornflakes not available");
            BeginDrag(remaining[0].Id,view.WorldToScreenPoint(Slot(0)));ParkSelection();RotateSelected(false);RotateSelected(true);
            if(candidates.Count==0)throw new Exception("Large cereal rotation has no landing");
            Commit(candidates.First());yield return new WaitUntil(()=>!feeding);yield return new WaitForSeconds(.3f);
            CaptureBoard(System.IO.Path.Combine(Application.dataPath,"../cornflakes-preview.png"));
            Debug.Log("BAGGING_SMOKE_PASS: Conveyor, hover fade, drag targets, checkout, undo, Tip control and placement, upright restriction, natural banana model, cold markers, bundled fonts.");
            Application.Quit(0);
        }
        void FrameCamera(float aspect)
        {
            view.transform.LookAt(new Vector3(0,1+(Level.Height-3)*.28f,-.7f));
            view.orthographicSize=Mathf.Max(4.55f+(Level.Height-3)*.15f,4.1f/aspect);
            FrameBackdrop(aspect);
            if(dragView!=null){dragView.orthographicSize=view.orthographicSize;dragView.aspect=aspect;}
        }
        public void CaptureBoard(string path)
        {
            var rt=new RenderTexture(1280,800,24);view.targetTexture=rt;view.aspect=1.6f;FrameCamera(1.6f);view.Render();
            if(dragView!=null){dragView.targetTexture=rt;dragView.Render();dragView.targetTexture=null;}
            var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1280,800,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,800),0,0);image.Apply();System.IO.File.WriteAllBytes(path,image.EncodeToPNG());
            RenderTexture.active=previous;view.targetTexture=null;Remove(rt);Remove(image);
        }
    }
}

