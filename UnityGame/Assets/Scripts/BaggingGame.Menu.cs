using UnityEngine;

namespace BaggingOrder
{
    public partial class BaggingGame
    {
        void DrawSimpleMenu(float w,float h)
        {
            Fill(new Rect(0,0,w,h),new Color(Ink.r,Ink.g,Ink.b,.55f));
            var r=new Rect(w*.5f-165,h*.5f-203,330,406);PaperCard(r);
            Text(new Rect(r.x+20,r.y+18,290,36),"Take your time",26,Ink,true,TextAnchor.MiddleCenter);
            if(Sticker("menu-resume",new Rect(r.x+30,r.y+72,270,44),"Keep packing",Sun))showOptions=false;
            if(Sticker("menu-list",new Rect(r.x+30,r.y+127,270,44),"Shopping list",Paper)){showOptions=false;showList=true;}
            if(Sticker("menu-map",new Rect(r.x+30,r.y+182,270,44),"Level map",Paper)){showOptions=false;OpenWorldMap();}
            if(Sticker("menu-restart",new Rect(r.x+30,r.y+237,270,44),"Start again",Paper)){showOptions=false;StartLevel();}
            if(Sticker("menu-sound",new Rect(r.x+30,r.y+292,270,44),muted?"Sound off":"Sound on",Sage))ToggleSound();
            Text(new Rect(r.x+20,r.y+349,290,33),Level.Dimensions+" / "+Level.TargetBags+" bag(s)",14,Ink,false,TextAnchor.MiddleCenter);
        }
    }
}
