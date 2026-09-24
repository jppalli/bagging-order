using UnityEngine;

namespace BaggingOrder
{
    public partial class BaggingGame
    {
        Transform shopBackdrop;
        Material warmPaper;
        Texture2D paperGrain;
        Texture2D Grain(bool wood)
        {
            const int size=256;var t=new Texture2D(size,size,TextureFormat.RGBA32,false);t.wrapMode=TextureWrapMode.Repeat;
            var random=new System.Random(21);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++){
                float fibre=Mathf.PerlinNoise(x*(wood?.012f:.08f),y*(wood?.25f:.12f));
                float fine=(float)random.NextDouble();float value=.84f+fibre*.12f+fine*.04f;
                t.SetPixel(x,y,new Color(value,value*.985f,value*.95f,1));
            }
            t.Apply();return t;
        }
        void DressShop()
        {
            paperGrain=Grain(false);warmPaper=Mat(new Color(.73f,.49f,.255f));warmPaper.mainTexture=paperGrain;warmPaper.mainTextureScale=new Vector2(3,4);
            paper=warmPaper;
            foreach(Transform child in world){
                if(child.name=="Shop floor"||child.name=="Back wall"||child.name=="Shelf"||child.name=="Pantry package")child.gameObject.SetActive(false);
                if(child.name=="Counter"){
                    child.localScale=new Vector3(40,.48f,7.2f);
                    var wood=Mat(new Color(.71f,.46f,.245f));wood.mainTexture=Grain(true);wood.mainTextureScale=new Vector2(3,3);child.GetComponent<Renderer>().sharedMaterial=wood;
                }
                if(child.name=="Counter trim")child.localScale=new Vector3(40.1f,.17f,7.3f);
            }
            var backdrop=Resources.Load<Texture2D>("UI/shop-backdrop");
            if(backdrop!=null){
                var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);quad.name="Painted grocery interior";Remove(quad.GetComponent<Collider>());
                shopBackdrop=quad.transform;shopBackdrop.SetParent(view.transform,false);shopBackdrop.localPosition=new Vector3(0,0,38);
                var material=new Material(Resources.Load<Shader>("ShopBackdrop"));material.mainTexture=backdrop;quad.GetComponent<Renderer>().sharedMaterial=material;
                quad.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
        void FrameBackdrop(float aspect)
        {
            if(shopBackdrop==null)return;float height=view.orthographicSize*2;float width=height*aspect;
            float artAspect=1.5f;if(width/height>artAspect)height=width/artAspect;else width=height*artAspect;
            shopBackdrop.localScale=new Vector3(width,height,1);
        }
        void DressBag()
        {
            // Folded rims and creases catch the warm light without blocking the packing area.
            float h=Level.Height;var rim=Mat(new Color(.85f,.65f,.39f));rim.mainTexture=paperGrain;
            Box(bagArt,"Folded top seam",new Vector3(0,h,Level.Depth*.5f+.1f),new Vector3(Level.Width+.28f,.045f,.09f),rim);
            Box(bagArt,"Left folded seam",new Vector3(-Level.Width*.5f-.09f,h*.5f,Level.Depth*.5f+.04f),new Vector3(.08f,h,.07f),rim);
            Box(bagArt,"Right folded seam",new Vector3(Level.Width*.5f+.07f,h*.5f,Level.Depth*.5f+.07f),new Vector3(.055f,h,.07f),rim);
            foreach(Transform child in bagArt)if(child.name=="Paper crease")child.GetComponent<Renderer>().sharedMaterial=Mat(new Color(.63f,.42f,.22f));
        }
    }
}
