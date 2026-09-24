using UnityEngine;

namespace BaggingOrder
{
    public partial class BaggingGame
    {
        Texture2D smoothTrail,receiptPaper;
        float trailCanvasWidth,trailCanvasHeight,trailLeft,trailWidth;
        void DrawSmoothTrail(float width,float height)
        {
            if(smoothTrail==null||Mathf.Abs(width-trailCanvasWidth)>.5f||height!=trailCanvasHeight){
                if(smoothTrail!=null)Remove(smoothTrail);
                trailCanvasWidth=width;trailCanvasHeight=height;
                float amplitude=Mathf.Min(185,width*.16f);
                trailWidth=amplitude*2+48;trailLeft=width*.5f-trailWidth*.5f;
                int limit=Mathf.Min(8192,SystemInfo.maxTextureSize);
                int tw=Mathf.Min(limit,Mathf.CeilToInt(trailWidth*2)),th=Mathf.Min(limit,Mathf.CeilToInt(height*2));
                smoothTrail=new Texture2D(tw,th,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
                var pixels=new Color32[tw*th];
                float bottom=MapStop(0,width,height).y,top=MapStop(PackingRules.Levels.Length-1,width,height).y;
                for(int y=0;y<th;y++){
                    float py=height-(y+.5f)/th*height,cy=Mathf.Clamp(py,top,bottom);
                    float link=(bottom-cy)/StopSpacing;
                    int i=Mathf.Min(Mathf.FloorToInt(link),PackingRules.Levels.Length-2);
                    float t=link-i;Vector2 a=MapStop(i,width,height),b=MapStop(i+1,width,height);
                    float cx=Mathf.Lerp(a.x,b.x,t*t*(3-2*t)),slope=(b.x-a.x)*6*t*(1-t)/StopSpacing;
                    float normal=Mathf.Sqrt(1+slope*slope);
                    for(int x=0;x<tw;x++){
                        float dx=(trailLeft+(x+.5f)/tw*trailWidth-cx)/normal;
                        float distance=Mathf.Sqrt(dx*dx+(py-cy)*(py-cy));
                        float alpha=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(13.2f,14.2f,distance));
                        if(alpha<=0)continue;
                        float rim=Mathf.SmoothStep(0,1,Mathf.InverseLerp(10.5f,13.5f,distance));
                        Color color=Color.Lerp(new Color(.99f,.91f,.71f),new Color(.72f,.59f,.36f),rim*.65f);
                        color.a=alpha;pixels[y*tw+x]=color;
                    }
                }
                smoothTrail.SetPixels32(pixels);smoothTrail.Apply(false,true);
            }
            GUI.DrawTexture(new Rect(trailLeft,0,trailWidth,height),smoothTrail);
        }
        void DrawReceiptPaper(Rect r)
        {
            if(receiptPaper==null){
                const int width=860,height=952;
                receiptPaper=new Texture2D(width,height,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
                var pixels=new Color32[width*height];
                for(int y=0;y<height;y++)for(int x=0;x<width;x++){
                    // One continuous silhouette: teeth and paper share the exact same edges.
                    float tooth=Mathf.Abs(Mathf.Repeat((x+.5f)/width*30,1)*2-1)*14;
                    float edge=Mathf.Min(y+.5f-tooth,height-y-.5f-tooth);
                    Color c=Paper;c.a=Mathf.Clamp01(edge+.5f);pixels[y*width+x]=c;
                }
                receiptPaper.SetPixels32(pixels);receiptPaper.Apply(false,true);
            }
            Rect paperRect=new Rect(r.x,r.y-7,r.width,r.height+14);
            var old=GUI.color;GUI.color=new Color(Ink.r,Ink.g,Ink.b,.2f);
            GUI.DrawTexture(new Rect(paperRect.x+3,paperRect.y+5,paperRect.width,paperRect.height),receiptPaper);
            GUI.color=old;GUI.DrawTexture(paperRect,receiptPaper);
        }
    }
}
