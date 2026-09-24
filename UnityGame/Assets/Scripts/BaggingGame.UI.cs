using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BaggingOrder
{
    public partial class BaggingGame
    {
        static readonly Color Ink=new Color(.137f,.302f,.220f), Paper=new Color(1,.957f,.847f), Tomato=new Color(.835f,.314f,.231f), Sun=new Color(.965f,.776f,.361f), Sage=new Color(.74f,.80f,.61f);
        Texture2D emblem,starStamp,leafStamp,receiptTooth,paintedLogo,paintedRotations;
        Texture2D paintGrain;
        readonly Dictionary<string,float> buttonHover=new Dictionary<string,float>();
        GUIStyle marketPanel;
        readonly Dictionary<string,float> buttonPress=new Dictionary<string,float>();
        float displayedProgress;
        bool uiReady;
        void PrepareMarketUI()
        {
            if(uiReady)return;uiReady=true;
            displayFont=Resources.Load<Font>("Fonts/Kalam-Bold");bodyFont=displayFont;
            paintedLogo=Resources.Load<Texture2D>("UI/bagging-logo-painted");paintedRotations=Resources.Load<Texture2D>("UI/turn-tip-painted");
            emblem=Resources.Load<Texture2D>("UI/grocery-emblem");rounded=RoundedTexture(Color.white);
            marketPanel=new GUIStyle{border=new RectOffset(10,10,10,10)};marketPanel.normal.background=rounded;
            starStamp=Stamp(96,(x,y)=>{
                float a=Mathf.Atan2(y,x)+Mathf.PI*.5f,r=Mathf.Sqrt(x*x+y*y);
                var polygon=new Vector2[10];for(int i=0;i<10;i++){float angle=-Mathf.PI*.5f+i*Mathf.PI/5;polygon[i]=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(i%2==0?.95f:.43f);}
                bool inside=false;for(int i=0,j=9;i<10;j=i++)if((polygon[i].y>y)!=(polygon[j].y>y)&&x<(polygon[j].x-polygon[i].x)*(y-polygon[i].y)/(polygon[j].y-polygon[i].y)+polygon[i].x)inside=!inside;
                return inside;
            });
            leafStamp=Stamp(64,(x,y)=>x*x/.36f+y*y<.92f);
            receiptTooth=Stamp(32,(x,y)=>y>Mathf.Abs(x)*2-1);
        }
        static Texture2D Stamp(int size,Func<float,float,bool> inside)
        {
            var t=new Texture2D(size,size,TextureFormat.RGBA32,false);t.wrapMode=TextureWrapMode.Clamp;
            for(int y=0;y<size;y++)for(int x=0;x<size;x++){
                int covered=0;for(int sy=0;sy<4;sy++)for(int sx=0;sx<4;sx++)if(inside((x+(sx+.5f)/4)/size*2-1,(y+(sy+.5f)/4)/size*2-1))covered++;
                t.SetPixel(x,y,new Color(1,1,1,covered/16f));
            }
            t.filterMode=FilterMode.Bilinear;
            t.Apply();return t;
        }
        void Fill(Rect r,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        void Tile(Rect r,Color color){GUI.DrawTexture(r,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,color,0,Mathf.Min(12,r.height*.5f));}
        void SoftShadow(Rect r,float opacity=.18f)
        {
            for(int i=3;i>=1;i--)Tile(new Rect(r.x-i,r.y+3-i,r.width+i*2,r.height+i*2),new Color(Ink.r,Ink.g,Ink.b,opacity/(i+2)));
        }
        void Text(Rect r,string value,int size,Color color,bool bold=false,TextAnchor align=TextAnchor.MiddleLeft)
        {
            var s=new GUIStyle(GUI.skin.label){font=bold?displayFont:bodyFont,fontSize=size,alignment=align,wordWrap=true,padding=new RectOffset(0,0,0,0)};s.normal.textColor=color;GUI.Label(r,value,s);
        }
        void PrintTexture(Rect r,Texture2D texture,Color tint,float rotation=0)
        {
            var matrix=GUI.matrix;var color=GUI.color;GUI.color=tint;GUIUtility.RotateAroundPivot(rotation,r.center);GUI.DrawTexture(r,texture,ScaleMode.ScaleToFit);GUI.color=color;GUI.matrix=matrix;
        }
        void PaperCard(Rect r)
        {
            SoftShadow(r);PaintedTile(r,Paper);
        }
        void PaintedTile(Rect r,Color baseColor)
        {
            if(paintGrain==null){
                paintGrain=new Texture2D(256,128,TextureFormat.RGBA32,false);paintGrain.wrapMode=TextureWrapMode.Clamp;paintGrain.filterMode=FilterMode.Bilinear;
                var random=new System.Random(5);
                for(int y=0;y<128;y++)for(int x=0;x<256;x++){
                    float value=.95f+.04f*y/127f+((float)random.NextDouble()-.5f)*.018f;
                    paintGrain.SetPixel(x,y,new Color(value,value,value,1));
                }
                paintGrain.Apply();
            }
            Tile(r,Color.Lerp(baseColor,Ink,.18f));
            var inner=new Rect(r.x+1,r.y+1,r.width-2,r.height-2);
            GUI.DrawTexture(inner,paintGrain,ScaleMode.StretchToFill,true,0,baseColor,0,Mathf.Min(11,inner.height*.5f));
            Tile(new Rect(r.x+10,r.y+2,r.width-20,1),new Color(1,1,1,.15f));
        }
        void Rule(Rect r){for(float x=r.x;x<r.xMax;x+=12)Fill(new Rect(x,r.y,Mathf.Min(6,r.xMax-x),2),new Color(Ink.r,Ink.g,Ink.b,.22f));}
        void Sprig(Vector2 p,float scale=1)
        {
            PrintTexture(new Rect(p.x,p.y,16*scale,29*scale),leafStamp,Ink,-35);
            PrintTexture(new Rect(p.x+13*scale,p.y-13*scale,16*scale,29*scale),leafStamp,new Color(.37f,.49f,.24f),28);
            PrintTexture(new Rect(p.x+10*scale,p.y+16*scale,13*scale,23*scale),leafStamp,Ink,35);
        }
        void Line(Vector2 a,Vector2 b,float width,Color c)
        {
            var matrix=GUI.matrix;GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg,a);Fill(new Rect(a.x,a.y-width*.5f,Vector2.Distance(a,b),width),c);GUI.matrix=matrix;
        }
        void TurnIcon(Rect r,bool tip,Color c)
        {
            var center=r.center;float radius=r.width*.32f;
            if(tip){var m=GUI.matrix;GUIUtility.RotateAroundPivot(-30,center);Tile(new Rect(center.x-7,center.y-10,14,20),c);GUI.matrix=m;Line(center+new Vector2(-13,13),center+new Vector2(14,13),2,c);}
            else{Vector2 last=center+Vector2.right*radius;for(int i=1;i<=16;i++){float angle=i/16f*4.8f;var next=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;Line(last,next,2.5f,c);last=next;}Line(last,last+new Vector2(-6,1),2.5f,c);Line(last,last+new Vector2(0,-6),2.5f,c);}
        }
        bool Sticker(string id,Rect r,string label,Color color,bool enabled=true,int icon=0)
        {
            bool prior=GUI.enabled;GUI.enabled=prior&&enabled;
            bool hover=GUI.enabled&&r.Contains(Event.current.mousePosition);
            float target=hover&&Input.GetMouseButton(0)?1:0;
            if(!buttonPress.ContainsKey(id))buttonPress[id]=0;
            if(Event.current.type==EventType.Repaint)buttonPress[id]=Mathf.MoveTowards(buttonPress[id],target,Time.unscaledDeltaTime*18);
            if(!buttonHover.ContainsKey(id))buttonHover[id]=0;
            if(Event.current.type==EventType.Repaint)buttonHover[id]=Mathf.MoveTowards(buttonHover[id],hover?1:0,Time.unscaledDeltaTime*9);
            float glow=buttonHover[id];float press=buttonPress[id];bool clicked=GUI.Button(r,GUIContent.none,GUIStyle.none);
            var face=new Rect(r.x,r.y+press*2-glow,r.width,r.height);
            Color fill=GUI.enabled?Color.Lerp(color,Color.white,glow*.10f):Color.Lerp(Paper,Sage,.55f);
            SoftShadow(r,GUI.enabled?.16f:.07f);
            Tile(new Rect(r.x,r.y+3,r.width,r.height),GUI.enabled?Color.Lerp(color,Ink,.4f):Color.Lerp(Ink,Paper,.8f));
            PaintedTile(face,fill);
            Color lettering=color==Ink||color==Tomato?Paper:Ink;if(!GUI.enabled)lettering=Color.Lerp(Ink,Paper,.3f);
            if(icon!=0)TurnIcon(new Rect(face.x+7,face.center.y-17,32,34),icon==2,lettering);
            Text(new Rect(face.x+(icon==0?8:43),face.y+2,face.width-(icon==0?16:49),face.height-4),label,Mathf.RoundToInt(Mathf.Max(19,14/UIScale)),lettering,true,TextAnchor.MiddleCenter);
            GUI.enabled=prior;if(clicked)PlaySoft(uiSound);return clicked;
        }
        bool PaintedRotation(bool tip,Rect r,bool allowed)
        {
            bool prior=GUI.enabled;GUI.enabled=prior&&allowed&&!held;
            bool hover=GUI.enabled&&r.Contains(Event.current.mousePosition);string id=tip?"painted-tip":"painted-turn";
            if(!buttonPress.ContainsKey(id))buttonPress[id]=0;
            if(Event.current.type==EventType.Repaint)buttonPress[id]=Mathf.MoveTowards(buttonPress[id],hover&&Input.GetMouseButton(0)?1:0,Time.unscaledDeltaTime*18);
            float down=buttonPress[id];bool clicked=GUI.Button(r,GUIContent.none,GUIStyle.none);
            var face=new Rect(r.x+down*2,r.y+down*4,r.width-down*4,r.height-down*4);
            DrawCleanRotation(face,tip,allowed,hover);
            GUI.enabled=prior;return clicked;
        }
        bool OverMarketUI(Vector2 point,float w,float h)
        {
            if(complete||showLevels||showList||showOptions||switchingBag)return true;
            if(point.y>h-112||point.y<12)return true;
            if(new Rect(20,18,270,84).Contains(point)||new Rect(w-230,20,210,64).Contains(point))return true;
            return PropertyTagVisible&&PropertyTagRect(w,h).Contains(point);
        }
        void OnGUI()
        {
            if(!initialized)return;PrepareMarketUI();float scale=UIScale;GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
            float w=Screen.width/scale,h=Screen.height/scale;
            if(showLevels){GUI.enabled=true;DrawWorldMap(w,h);return;}
            bool modal=complete||showLevels||showList||showOptions;
            GUI.enabled=!modal&&!switchingBag;
            var card=new Rect(20,18,270,84);PaperCard(card);
            Text(new Rect(34,24,242,31),(level+1)+"  "+Level.Name,22,Ink,true);
            Text(new Rect(34,57,242,35),Level.Goal,13,Ink);
            DrawGroceryProgress(w);
            if(!modal)DrawCrushFeedback();
            if(!modal&&PropertyTagVisible)DrawPropertyTag(w,h);
            Fill(new Rect(0,h-76,w,76),Paper);
            if(dragging||feeding)Text(new Rect(22,h-110,w-44,30),feeding?"Fresh groceries coming through...":status,14,Ink,false,TextAnchor.MiddleCenter);
            if(Sticker("undo",new Rect(22,h-60,94,42),"Undo",Paper,!dragging&&history.Count>0))Undo();
            if(Sticker("options",new Rect(w-116,h-60,94,42),"Menu",Paper)){CancelDrag();showOptions=true;}
            DrawBagSelector();
            if(selected!=null&&!modal){float bh=RotationButtonHeight;
                if(PaintedRotation(false,new Rect(rotationControls.x,rotationControls.y,bh,bh),PackingRules.Rotate(selected.Key,orientation,false)!=orientation))RotateSelected(false);
                if(PaintedRotation(true,new Rect(rotationControls.x+bh,rotationControls.y,bh,bh),PackingRules.Rotate(selected.Key,orientation,true)!=orientation))RotateSelected(true);
            }
            GUI.enabled=true;
            if(complete){if(Time.unscaledTime-completedAt>=WinRevealDelay)DrawReceipt(w,h);else DrawWinCelebration(w,h);}
            else if(showOptions)DrawSimpleMenu(w,h);
            else if(showLevels||showList)DrawMarketModal(w,h);
        }
        bool PropertyTagVisible=>selected!=null||(inspectedKey!=null&&Time.unscaledTime<inspectedUntil);
        Rect PropertyTagRect(float w,float h)
        {
            // A viewport-anchored dock never jumps when selection changes from tap to drag.
            float side=RotationButtonHeight;
            float controlsRight=w*.5f+side;
            if(controlsRight+248<=w-16)return new Rect(controlsRight+12,h-112-side,236,96);
            return new Rect(Mathf.Clamp(w*.5f-118,16,w-252),Mathf.Max(110,h-112-side-104),236,96);
        }
        void PropertyIcon(Vector2 center,string kind,Color color)
        {
            if(kind=="cold"){
                for(int i=0;i<3;i++){float a=i*Mathf.PI/3;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*7;Line(center-d,center+d,1.5f,color);}
            }else if(kind=="fragile"){
                Line(center+new Vector2(-6,-7),center+new Vector2(-4,3),1.5f,color);Line(center+new Vector2(6,-7),center+new Vector2(4,3),1.5f,color);
                Line(center+new Vector2(-4,3),center+new Vector2(4,3),1.5f,color);Line(center+new Vector2(0,3),center+new Vector2(0,8),1.5f,color);
                Line(center+new Vector2(1,-6),center+new Vector2(-2,-2),1.5f,color);Line(center+new Vector2(-2,-2),center+new Vector2(2,0),1.5f,color);
            }else{
                Tile(new Rect(center.x-6,center.y-3,12,11),color);Tile(new Rect(center.x-3,center.y-7,6,5),color);
                Tile(new Rect(center.x-1,center.y-5,2,2),Paper);
            }
        }
        void DrawPropertyTag(float w,float h)
        {
            var def=PackingRules.Catalog[selected!=null?selected.Key:inspectedKey];var r=PropertyTagRect(w,h);PaperCard(r);
            Text(new Rect(r.x+12,r.y+7,190,24),def.Name,18,Ink,true);
            if(def.Cold)PropertyIcon(new Vector2(r.xMax-20,r.y+19),"cold",new Color(.2f,.49f,.59f));
            PropertyIcon(new Vector2(r.x+20,r.y+43),"weight",Ink);
            Text(new Rect(r.x+34,r.y+32,190,23),PackingRules.WeightLabel(def)+(def.Cold?"  /  Cold":""),13,Ink);
            PropertyIcon(new Vector2(r.x+20,r.y+69),"fragile",def.Capacity<=3?Tomato:Ink);
            bool danger=selected!=null&&hovered!=null&&hovered.Crushed.Count>0;
            string cause=selected!=null?selected.CrushedBy:inspectedDamage;
            string strength=!string.IsNullOrEmpty(cause)?"Crushed by "+cause:danger?"Will crush here - choose another spot":def.Upright?"Upright / "+PackingRules.StrengthLabel(def):PackingRules.StrengthLabel(def);
            Text(new Rect(r.x+34,r.y+56,190,33),strength,12,danger?Tomato:Ink);
        }
        void DrawBagSelector()
        {
            if(Level.TargetBags<=1)return;
            float w=Screen.width/UIScale,h=Screen.height/UIScale;
            int first=Mathf.Clamp(bagIndex-1,0,Mathf.Max(0,bags.Count-3));
            int count=Mathf.Min(3,bags.Count);bool canAdd=bags.Count<Level.TargetBags;
            const float tabWidth=76,gap=8;
            int slots=count+(canAdd?1:0);float start=(w-(slots*tabWidth+(slots-1)*gap))*.5f;
            for(int slot=0;slot<count;slot++){
                int index=first+slot;var r=new Rect(start+slot*(tabWidth+gap),h-57,tabWidth,38);
                if(BagTab(r,"Bag "+(index+1),index==bagIndex,!dragging&&!feeding&&index!=bagIndex))SwitchBag(index);
            }
            if(canAdd&&BagTab(new Rect(start+count*(tabWidth+gap),h-57,tabWidth,38),"+ Bag",false,!dragging&&!feeding&&bags[bagIndex].Count>0))OpenNewBag();
        }
        bool BagTab(Rect r,string label,bool active,bool allowed)
        {
            bool prior=GUI.enabled;GUI.enabled=prior&&allowed;
            bool hover=GUI.enabled&&r.Contains(Event.current.mousePosition);
            bool clicked=GUI.Button(r,GUIContent.none,GUIStyle.none);GUI.enabled=prior;
            Color fill=active?Sun:hover?Color.Lerp(Paper,Sun,.15f):Paper;
            SoftShadow(r,.10f);PaintedTile(r,fill);
            var style=new GUIStyle(GUI.skin.label){font=displayFont,fontSize=17,alignment=TextAnchor.MiddleCenter,wordWrap=false,padding=new RectOffset(0,0,0,0),margin=new RectOffset(0,0,0,0)};
            style.normal.textColor=active||allowed?Ink:Color.Lerp(Ink,Paper,.35f);
            GUI.Label(new Rect(r.x+4,r.y,r.width-8,r.height),label,style);
            if(clicked)PlaySoft(uiSound);return clicked;
        }
        void DrawGroceryProgress(float w)
        {
            var card=new Rect(w-230,20,210,64);SoftShadow(card);PaintedTile(card,Ink);
            int packed=Level.Items.Length-remaining.Count,total=Level.Items.Length;
            Text(new Rect(card.x+16,card.y+9,110,24),"Packed",14,Paper,true);
            Text(new Rect(card.x+128,card.y+5,66,32),packed+" / "+total,23,Paper,true,TextAnchor.MiddleRight);
            float target=packed/(float)Mathf.Max(1,total);
            if(Event.current.type==EventType.Repaint)displayedProgress=Mathf.Lerp(displayedProgress,target,1-Mathf.Exp(-8*Time.unscaledDeltaTime));
            var track=new Rect(card.x+16,card.y+43,178,13);
            Tile(new Rect(track.x,track.y-1,track.width,track.height+2),new Color(.08f,.22f,.15f));
            Tile(track,new Color(.29f,.43f,.30f));
            float fillWidth=track.width*Mathf.Clamp01(displayedProgress);
            if(fillWidth>.5f){
                var fill=new Rect(track.x,track.y,fillWidth,track.height);
                GUI.DrawTexture(fill,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,Sun,0,Mathf.Min(6.5f,fillWidth*.5f));
                if(fillWidth>10)Tile(new Rect(track.x+4,track.y+2,fillWidth-8,2),new Color(1,.94f,.65f,.65f));
            }

        }
        void DrawAlignedStars(float centerX,float centerY,float size,float gap,int earned,float age=-1)
        {
            for(int i=0;i<3;i++){
                float pop=1;
                if(age>=0){float u=Mathf.Clamp01((age-.12f-i*.14f)/.34f);pop=1-Mathf.Pow(1-u,3)+Mathf.Sin(u*Mathf.PI)*.12f;}
                float side=size*pop;
                var star=new Rect(centerX+(i-1)*(size+gap)-side*.5f,centerY-side*.5f,side,side);
                if(side>0)PrintTexture(star,starStamp,i<earned?Sun:new Color(.62f,.66f,.54f),0);
            }
        }
        void DrawReceipt(float w,float h)
        {
            Fill(new Rect(0,0,w,h),new Color(Ink.r,Ink.g,Ink.b,.68f));
            float age=Mathf.Max(0,Time.unscaledTime-completedAt-WinRevealDelay),t=Mathf.Clamp01(age/.4f);
            var r=new Rect(w*.5f-200,h*.5f-190+(1-t)*(1-t)*40,400,380);
            DrawReceiptPaper(r);
            Text(new Rect(r.x+24,r.y+20,r.width-48,26),"Checkout "+(level+1),15,Ink,false,TextAnchor.MiddleCenter);
            DrawAlignedStars(r.center.x,r.y+104,64,14,PackingRules.Stars(Level,bags),age);
            Text(new Rect(r.x+20,r.y+155,r.width-40,43),ResultTitle,29,Ink,true,TextAnchor.MiddleCenter);
            bool goal=PackingRules.GoalMet(Level,bags);
            string message=ResultStars==3?"Every grocery safe. Great fit!":bags.Any(b=>b.Any(i=>i.Crushed))?"Keep groceries safe for another star.":Level.ColdGoal&&!PackingRules.ColdConnected(bags)?"Keep chilled groceries touching.":!goal?"Meet the packing goal for another star.":"Try again for three stars.";
            Text(new Rect(r.x+24,r.y+205,r.width-48,31),message,16,Ink,false,TextAnchor.MiddleCenter);
            if(Sticker("continue",new Rect(r.x+40,r.y+256,r.width-80,48),"Next level",Ink)){level=(level+1)%PackingRules.Levels.Length;StartLevel();}
            if(Sticker("replay",new Rect(r.x+40,r.y+319,152,38),"Replay",Paper))StartLevel();
            if(Sticker("receipt-map",new Rect(r.x+208,r.y+319,152,38),"Map",Paper))OpenWorldMap();
        }
        void DrawMarketModal(float w,float h)
        {
            Fill(new Rect(0,0,w,h),new Color(Ink.r,Ink.g,Ink.b,.68f));var r=new Rect(w/2-280,Mathf.Max(20,h/2-235),560,470);PaperCard(r);
            Text(new Rect(r.x+25,r.y+20,390,38),showLevels?"Today's checkouts":"Your shopping list",26,Ink,true);
            if(Sticker("close",new Rect(r.xMax-98,r.y+20,74,38),"Close",Tomato)){showList=false;showLevels=false;return;}
            Rule(new Rect(r.x+24,r.y+80,r.width-48,2));
            if(showLevels){for(int i=0;i<PackingRules.Levels.Length;i++)if(Sticker("level-"+i,new Rect(r.x+24+(i%2)*262,r.y+100+(i/2)*55,250,45),(i+1).ToString("00")+"  "+PackingRules.Levels[i].Name,i==level?Sun:Paper)){level=i;StartLevel();}}
            else{int row=0;foreach(var group in remaining.GroupBy(i=>i.Key)){var def=PackingRules.Catalog[group.Key];float y=r.y+104+row*38;Text(new Rect(r.x+27,y,370,32),group.Count()+" × "+def.Name,17,Ink,true);Text(new Rect(r.x+394,y,138,32),def.Upright?"UPRIGHT":def.Cold?"COLD":"",12,Tomato,true,TextAnchor.MiddleRight);row++;}}
        }
    }
}
