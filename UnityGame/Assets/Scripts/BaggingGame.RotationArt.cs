using UnityEngine;

namespace BaggingOrder
{
    public partial class BaggingGame
    {
        Texture2D cleanTurnIcon,cleanTipIcon;
        void PrepareRotationIcons()
        {
            if(cleanTurnIcon!=null)return;
            cleanTurnIcon=Stamp(192,(x,y)=>{
                float radius=Mathf.Sqrt(x*x+y*y),angle=Mathf.Atan2(y,x);
                bool arc=radius>.46f&&radius<.72f&&(angle>-.7f||angle<-2.5f);
                bool arrow=y<.02f&&y>-.56f&&Mathf.Abs(x+.58f)<(y+.56f)*.65f;
                return arc||arrow;
            });
            cleanTipIcon=Stamp(192,(x,y)=>{
                float u=x*.866f+y*.5f,v=-x*.5f+y*.866f;
                bool box=Mathf.Abs(u)<.39f&&Mathf.Abs(v-.05f)<.54f;
                bool baseLine=Mathf.Abs(x)<.79f&&Mathf.Abs(y+.66f)<.075f;
                return box||baseLine;
            });
        }
        void DrawCleanRotation(Rect face,bool tip,bool allowed,bool hover)
        {
            PrepareRotationIcons();
            var body=new Rect(face.x+7,face.y+7,face.width-14,face.height-14);
            Color color=allowed?(tip?Tomato:Sun):new Color(.75f,.76f,.73f);
            Color ink=allowed?(tip?Paper:Ink):new Color(.43f,.46f,.43f);
            if(hover)color=Color.Lerp(color,Paper,.12f);
            SoftShadow(body,.12f);PaintedTile(body,Paper);
            PaintedTile(new Rect(body.x+3,body.y+3,body.width-6,body.height-6),color);
            float size=body.height*.50f;
            PrintTexture(new Rect(body.center.x-size*.5f,body.y+5,size,size),tip?cleanTipIcon:cleanTurnIcon,ink);
            Text(new Rect(body.x+4,body.y+body.height*.57f,body.width-8,body.height*.34f),tip?"Tip":"Turn",22,ink,true,TextAnchor.MiddleCenter);
        }
    }
}
