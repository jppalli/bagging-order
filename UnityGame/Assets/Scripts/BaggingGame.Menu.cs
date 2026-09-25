using UnityEngine;

namespace BaggingOrder
{
    public partial class BaggingGame
    {
        void DrawSimpleMenu(float w,float h)
        {
            Fill(new Rect(0,0,w,h),new Color(Ink.r,Ink.g,Ink.b,.55f));
            if(confirmProgressReset){
                var confirmation=new Rect(w*.5f-175,h*.5f-137,350,274);PaperCard(confirmation);
                Text(new Rect(confirmation.x+24,confirmation.y+20,302,40),"Start fresh?",27,Ink,true,TextAnchor.MiddleCenter);
                Text(new Rect(confirmation.x+24,confirmation.y+73,302,76),"Clear all stars and level progress on this device? Your sound setting stays the same.",17,Ink,false,TextAnchor.MiddleCenter);
                if(Sticker("reset-cancel",new Rect(confirmation.x+24,confirmation.y+183,142,48),"Keep progress",Paper))confirmProgressReset=false;
                if(Sticker("reset-confirm",new Rect(confirmation.x+184,confirmation.y+183,142,48),"Reset",Tomato))ResetProgress();
                return;
            }
            var r=new Rect(w*.5f-165,h*.5f-225,330,450);PaperCard(r);
            Text(new Rect(r.x+20,r.y+18,290,36),"Take your time",26,Ink,true,TextAnchor.MiddleCenter);
            if(Sticker("menu-resume",new Rect(r.x+30,r.y+72,270,44),"Keep packing",Sun))showOptions=false;
            if(Sticker("menu-list",new Rect(r.x+30,r.y+127,270,44),"Shopping list",Paper)){showOptions=false;showList=true;}
            if(Sticker("menu-map",new Rect(r.x+30,r.y+182,270,44),"Level map",Paper)){showOptions=false;OpenWorldMap();}
            if(Sticker("menu-restart",new Rect(r.x+30,r.y+237,270,44),"Start again",Paper)){showOptions=false;StartLevel();}
            if(Sticker("menu-sound",new Rect(r.x+30,r.y+292,270,44),muted?"Sound off":"Sound on",Sage))ToggleSound();
            if(Sticker("menu-reset-progress",new Rect(r.x+30,r.y+347,270,44),"Reset progress",Paper))confirmProgressReset=true;
            Text(new Rect(r.x+20,r.y+404,290,28),Level.Dimensions+" / "+Level.TargetBags+" bag(s)",14,Ink,false,TextAnchor.MiddleCenter);
        }
    }
}
