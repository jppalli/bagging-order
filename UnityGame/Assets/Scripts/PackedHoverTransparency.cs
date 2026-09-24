using UnityEngine;
using UnityEngine.Rendering;

namespace BaggingOrder
{
    // Each packed grocery owns its temporary materials; imported assets stay unchanged.
    public sealed class PackedHoverTransparency : MonoBehaviour
    {
        Renderer[] renderers;
        Material[][] original, faded;
        ShadowCastingMode[] shadows;
        bool frontOnly;
        float fadedOpacity=.18f;
        public bool IsFaded { get; private set; }

        public void Initialize(bool isFrontOnly,float opacity=.18f)
        {
            frontOnly=isFrontOnly;
            fadedOpacity=opacity;
            renderers=GetComponentsInChildren<Renderer>();
            original=new Material[renderers.Length][];faded=new Material[renderers.Length][];
            shadows=new ShadowCastingMode[renderers.Length];
            for(int i=0;i<renderers.Length;i++){
                original[i]=renderers[i].sharedMaterials;shadows[i]=renderers[i].shadowCastingMode;
            }
        }
        public void SetBehindHover(bool behind)
        {
            SetFaded(behind&&frontOnly);
        }
        public void SetFaded(bool fade)
        {
            if(fade==IsFaded)return;
            for(int i=0;i<renderers.Length;i++){
                if(fade&&faded[i]==null){
                    faded[i]=new Material[original[i].Length];
                    for(int j=0;j<original[i].Length;j++){
                        var source=original[i][j];var color=source.HasProperty("_Color")?source.color:Color.white;color.a=fadedOpacity;
                        var material=new Material(Resources.Load<Shader>("HoverFade")){color=color};
                        if(source.HasProperty("_MainTex")){material.mainTexture=source.mainTexture;material.mainTextureScale=source.mainTextureScale;material.mainTextureOffset=source.mainTextureOffset;}
                        faded[i][j]=material;
                    }
                }
                renderers[i].sharedMaterials=fade?faded[i]:original[i];
                renderers[i].shadowCastingMode=fade?ShadowCastingMode.Off:shadows[i];
            }
            IsFaded=fade;
        }
        void OnDestroy()
        {
            if(faded==null)return;
            foreach(var group in faded)if(group!=null)foreach(var material in group){
                if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);
            }
        }
    }
}
