using UnityEngine;

namespace BaggingOrder
{
    // A camera-facing paper seal: readable even when a carton is turned or tipped.
    public class ColdGroceryBadge : MonoBehaviour
    {
        static Material material;
        Transform grocery;
        Camera cameraView;
        Vector3 size;
        public static void Attach(Transform parent,Vector3 dimensions,Camera view)
        {
            if(material==null){material=new Material(Resources.Load<Shader>("HoverFade"));material.mainTexture=MakeSeal();material.color=Color.white;}
            var badge=GameObject.CreatePrimitive(PrimitiveType.Quad);badge.name="Cold snowflake badge";
            Object.Destroy(badge.GetComponent<Collider>());badge.transform.SetParent(parent,false);
            badge.transform.localScale=Vector3.one*.38f;
            var renderer=badge.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            var behavior=badge.AddComponent<ColdGroceryBadge>();behavior.grocery=parent;behavior.size=dimensions;behavior.cameraView=view;behavior.LateUpdate();
        }
        void LateUpdate()
        {
            if(cameraView==null||grocery==null)return;
            var forward=cameraView.transform.forward;
            float front=(Mathf.Abs(Vector3.Dot(forward,grocery.right))*size.x+Mathf.Abs(Vector3.Dot(forward,grocery.up))*size.y+Mathf.Abs(Vector3.Dot(forward,grocery.forward))*size.z)*.5f;
            transform.position=grocery.TransformPoint(Vector3.up*size.y*.5f)-forward*(front+.045f)+cameraView.transform.right*.2f+cameraView.transform.up*.12f;
            transform.rotation=cameraView.transform.rotation;
        }
        static float Segment(Vector2 p,Vector2 a,Vector2 b)
        {
            var d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude));
        }
        static Texture2D MakeSeal()
        {
            const int n=128;var texture=new Texture2D(n,n,TextureFormat.RGBA32,false);texture.wrapMode=TextureWrapMode.Clamp;texture.filterMode=FilterMode.Bilinear;
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){
                var p=new Vector2((x+.5f)/n*2-1,(y+.5f)/n*2-1);float radius=p.magnitude;
                float alpha=Mathf.Clamp01((.96f-radius)*n*.5f);float line=10;
                for(int arm=0;arm<6;arm++){
                    float angle=arm*Mathf.PI/3;var d=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));var side=new Vector2(-d.y,d.x);
                    line=Mathf.Min(line,Segment(p,Vector2.zero,d*.62f));
                    line=Mathf.Min(line,Segment(p,d*.35f,d*.52f+side*.16f));
                    line=Mathf.Min(line,Segment(p,d*.35f,d*.52f-side*.16f));
                }
                Color paper=new Color(.91f,.97f,.92f);Color blue=new Color(.20f,.49f,.59f);
                var color=Color.Lerp(paper,blue,Mathf.Clamp01((.041f-line)*n*.5f));
                float rim=Mathf.Clamp01((radius-.82f)*n*.5f)*Mathf.Clamp01((.87f-radius)*n*.5f);
                color=Color.Lerp(color,new Color(.52f,.73f,.73f),rim*.75f);color.a=alpha;texture.SetPixel(x,y,color);
            }
            texture.Apply();return texture;
        }
    }
}
