using System;
using UnityEngine;

namespace BaggingOrder
{
    public partial class BaggingGame
    {
        Texture2D journeyPainting;
        int mapSelection;
        Vector2 mapScroll,mapPress;
        bool mapPreviewOpen,mapDragging,mapPointerDown,mapNeedsFocus=true;
        int mapFocus=-1;
        const float StopSpacing=240;
        static string BestKey(int index)=>"bagging-best-"+PackingRules.Levels[index].Name;
        static string VisitKey(int index)=>"bagging-visited-"+PackingRules.Levels[index].Name;
        void OpenWorldMap(){CancelDrag();mapSelection=level;showList=false;showLevels=true;mapPreviewOpen=false;mapNeedsFocus=true;mapFocus=level;}
        Color DistrictColor(int index)=>index<4?new Color(.57f,.73f,.40f):index<8?new Color(.36f,.67f,.67f):index<12?new Color(.87f,.57f,.31f):index<15?new Color(.70f,.48f,.65f):new Color(.39f,.62f,.48f);
        string DistrictName(int index)=>index<4?"Corner shop":index<8?"Market square":index<12?"Bakery lane":index<15?"Delivery gardens":"Little discoveries";
        Vector2 MapStop(int index,float width,float height)
        {
            float wave=Mathf.Sin(index*.94f)*Mathf.Min(185,width*.16f);
            return new Vector2(width*.5f+wave,height-130-index*StopSpacing);
        }
        void MapDisc(Rect r,Color color){GUI.DrawTexture(r,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,color,0,Mathf.Min(r.width,r.height)*.5f);}
        void DrawWorldMap(float w,float h)
        {
            Fill(new Rect(0,0,w,h),new Color(.82f,.89f,.73f));
            var viewport=new Rect(0,88,w,Mathf.Max(100,h-148));
            float canvasWidth=Mathf.Max(280,w-18),canvasHeight=PackingRules.Levels.Length*StopSpacing+100;
            if(mapNeedsFocus){int focus=mapFocus<0?level:mapFocus;mapScroll=new Vector2(0,Mathf.Clamp(MapStop(focus,canvasWidth,canvasHeight).y-viewport.height*.56f,0,canvasHeight-viewport.height));mapNeedsFocus=false;}
            var e=Event.current;
            if(!mapPreviewOpen){
                if(e.type==EventType.MouseDown&&e.button==0&&viewport.Contains(e.mousePosition)){mapPress=e.mousePosition;mapPointerDown=true;mapDragging=false;}
                if(e.type==EventType.MouseDrag&&mapPointerDown&&(mapDragging||Vector2.Distance(mapPress,e.mousePosition)>7)){
                    mapDragging=true;GUIUtility.hotControl=0;mapScroll.y=Mathf.Clamp(mapScroll.y-e.delta.y,0,Mathf.Max(0,canvasHeight-viewport.height));e.Use();
                }
                if(e.type==EventType.MouseUp&&mapPointerDown){mapPointerDown=false;if(mapDragging){mapDragging=false;e.Use();}}
            }
            GUI.enabled=!mapPreviewOpen;
            mapScroll=GUI.BeginScrollView(viewport,mapScroll,new Rect(0,0,canvasWidth,canvasHeight));mapScroll.x=0;
            if(journeyPainting==null)journeyPainting=Resources.Load<Texture2D>("UI/journey-map-painted");
            if(journeyPainting!=null)GUI.DrawTexture(new Rect(0,0,canvasWidth,canvasHeight),journeyPainting,ScaleMode.StretchToFill);
            DrawSmoothTrail(canvasWidth,canvasHeight);
            for(int district=0;district<5;district++){
                int first=district<4?district*4:15;
                int last=district<3?first+3:district==3?14:PackingRules.Levels.Length-1;
                var banner=new Rect(canvasWidth*.5f-105,MapStop(last,canvasWidth,canvasHeight).y-121,210,32);
                SoftShadow(banner);PaintedTile(banner,Paper);Text(banner,DistrictName(first),18,Ink,true,TextAnchor.MiddleCenter);
            }
            for(int i=0;i<PackingRules.Levels.Length;i++){
                Vector2 at=MapStop(i,canvasWidth,canvasHeight);
                if(at.y<mapScroll.y-110||at.y>mapScroll.y+viewport.height+110)continue;
                DrawJourneyStop(i,at);
            }
            GUI.EndScrollView();
            Fill(new Rect(0,0,w,88),Paper);Line(new Vector2(0,87),new Vector2(w,87),2,new Color(.3f,.4f,.2f,.16f));
            Text(new Rect(24,12,w-168,38),"The grocery trail",29,Ink,true);
            Text(new Rect(26,51,w-166,25),"All checkouts unlocked - choose your next stop",13,Ink);
            if(Sticker("map-close",new Rect(w-127,24,102,43),complete?"Results":"Back",Paper)){showLevels=false;mapPreviewOpen=false;return;}
            Fill(new Rect(0,h-60,w,60),Paper);
            if(Sticker("map-current",new Rect(22,h-49,128,36),"Current level",Paper)){mapFocus=level;mapNeedsFocus=true;}
            if(Sticker("map-new",new Rect(w-157,h-49,135,36),"New puzzles",Sun)){mapFocus=15;mapNeedsFocus=true;}
            if(Sticker("map-sound",new Rect(w*.5f-64,h-49,128,36),muted?"Sound off":"Sound on",Sage))ToggleSound();
            GUI.enabled=true;
            if(mapPreviewOpen)DrawJourneyPreview(w,h);
        }
        void DrawJourneyStop(int index,Vector2 at)
        {
            var checkout=PackingRules.Levels[index];int stars=PlayerPrefs.GetInt(BestKey(index),0);bool current=index==level;
            float press=GUI.enabled&&Input.GetMouseButton(0)&&Vector2.Distance(Event.current.mousePosition,at)<43?2:0;
            if(current){float pulse=1+Mathf.Sin(Time.unscaledTime*2)*.025f;MapDisc(new Rect(at.x-52*pulse,at.y-52*pulse,104*pulse,104*pulse),new Color(1,.86f,.37f,.52f));}
            MapDisc(new Rect(at.x-43,at.y-36,86,86),new Color(.22f,.28f,.14f,.2f));
            MapDisc(new Rect(at.x-43,at.y-43+press,86,86),Color.Lerp(DistrictColor(index),Ink,.28f));
            MapDisc(new Rect(at.x-41,at.y-43+press,82,78),current?Sun:stars>0?new Color(.47f,.67f,.34f):Paper);
            MapDisc(new Rect(at.x-29,at.y-37+press,58,16),new Color(1,1,1,.19f));
            Text(new Rect(at.x-40,at.y-39+press,80,73),(index+1).ToString(),32,Ink,true,TextAnchor.MiddleCenter);
            bool circularHit=Vector2.Distance(Event.current.mousePosition,at)<45;
            if(GUI.Button(new Rect(at.x-43,at.y-43,86,86),GUIContent.none,GUIStyle.none)&&circularHit){PlaySoft(uiSound);mapSelection=index;mapPreviewOpen=true;mapPointerDown=false;}
            DrawAlignedStars(at.x,at.y+60,21,4,stars);
            PaintedTile(new Rect(at.x-96,at.y+74,192,32),Paper);
            Text(new Rect(at.x-96,at.y+74,192,32),checkout.Name,15,Ink,true,TextAnchor.UpperCenter);
            if(current){var marker=new Rect(at.x-61,at.y-76,122,24);PaintedTile(marker,Ink);Text(marker,"YOU ARE HERE",11,Paper,true,TextAnchor.MiddleCenter);}
            if(checkout.Experimental){var badge=new Rect(at.x+24,at.y-48,51,22);PaintedTile(badge,Tomato);Text(badge,"NEW",11,Paper,true,TextAnchor.MiddleCenter);}
        }
        void DrawJourneyPreview(float w,float h)
        {
            Fill(new Rect(0,0,w,h),new Color(Ink.r,Ink.g,Ink.b,.58f));
            var c=PackingRules.Levels[mapSelection];float width=Mathf.Min(420,w-36),height=Mathf.Min(500,h-32);
            var r=new Rect((w-width)*.5f,(h-height)*.5f,width,height);PaperCard(r);
            Text(new Rect(r.x+23,r.y+15,r.width-115,27),c.Experimental?"NEW EXPERIMENT":"CHECKOUT "+(mapSelection+1),14,Tomato,true);
            if(Sticker("preview-close",new Rect(r.xMax-90,r.y+12,70,34),"Close",Paper)){mapPreviewOpen=false;return;}
            Text(new Rect(r.x+22,r.y+55,r.width-44,59),c.Name,28,Ink,true,TextAnchor.MiddleCenter);
            int stars=PlayerPrefs.GetInt(BestKey(mapSelection),0);
            DrawAlignedStars(r.center.x,r.y+135,28,6,stars);
            float bw=c.Width*21,bh=c.Height*18;var bag=new Rect(r.x+38,r.y+173+(75-bh),bw,bh);PaintedTile(bag,new Color(.80f,.61f,.36f));
            Line(new Vector2(bag.x+6,bag.y+5),new Vector2(bag.xMax-6,bag.y+5),3,Paper);
            if(c.SealHeight>0){float y=bag.yMax-c.SealHeight*18;Line(new Vector2(bag.x+4,y),new Vector2(bag.xMax-4,y),3,Sun);}
            Text(new Rect(r.x+133,r.y+164,r.width-153,41),c.Dimensions,15,Ink,true);
            Text(new Rect(r.x+133,r.y+209,r.width-153,33),c.TargetBags+" bag"+(c.TargetBags==1?"":"s")+" allowed",14,Ink);
            Rule(new Rect(r.x+23,r.y+267,r.width-46,2));
            Text(new Rect(r.x+24,r.y+277,r.width-48,47),c.Goal,16,Ink,true,TextAnchor.MiddleCenter);
            Text(new Rect(r.x+24,r.y+329,r.width-48,58),c.Hint,13,Ink,false,TextAnchor.MiddleCenter);
            Text(new Rect(r.x+22,r.yMax-100,r.width-44,26),MapRecord(mapSelection),13,Ink,false,TextAnchor.MiddleCenter);
            if(Sticker("map-play",new Rect(r.x+36,r.yMax-65,r.width-72,46),"Play",Ink)){
                level=mapSelection;PlayerPrefs.SetInt(VisitKey(level),1);PlayerPrefs.Save();mapPreviewOpen=false;mapNeedsFocus=true;mapFocus=level;StartLevel();
            }
        }
        string MapRecord(int index)
        {
            int stars=PlayerPrefs.GetInt(BestKey(index),0);
            return stars>0?"Best: "+stars+" / 3 stars":PlayerPrefs.GetInt(VisitKey(index),0)>0?"Tried - not completed":"Not tried yet";
        }
    }
}
