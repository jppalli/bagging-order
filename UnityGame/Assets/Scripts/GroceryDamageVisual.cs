using System.Collections.Generic;
using UnityEngine;

namespace BaggingOrder
{
    // Deform only the rendered mesh; the packing grid and support surfaces remain stable.
    public sealed class GroceryDamageVisual : MonoBehaviour
    {
        public bool AtRisk;
        bool damaged;
        float impactAt=-100;
        readonly List<Mesh> meshes=new List<Mesh>();
        readonly List<Vector3[]> rest=new List<Vector3[]>();
        readonly List<Vector3[]> dented=new List<Vector3[]>();
        Renderer[] renderers;
        Color[][] colors;
        MaterialPropertyBlock block;
        public void Initialize(bool crushed,bool impact=false)
        {
            block=new MaterialPropertyBlock();
            damaged=crushed;renderers=GetComponentsInChildren<Renderer>();colors=new Color[renderers.Length][];
            for(int i=0;i<renderers.Length;i++){
                var materials=renderers[i].sharedMaterials;colors[i]=new Color[materials.Length];
                for(int j=0;j<materials.Length;j++)colors[i][j]=materials[j].HasProperty("_Color")?materials[j].color:Color.white;
            }
            if(!crushed)return;
            foreach(var filter in GetComponentsInChildren<MeshFilter>()){
                if(filter.sharedMesh==null||!filter.sharedMesh.isReadable)continue;
                var mesh=Instantiate(filter.sharedMesh);filter.sharedMesh=mesh;
                var source=mesh.vertices;var target=(Vector3[])source.Clone();var bounds=mesh.bounds;
                for(int v=0;v<source.Length;v++){
                    Vector3 q=source[v];float h=Mathf.InverseLerp(bounds.min.y,bounds.max.y,q.y);
                    // Crease the side wall, preserving top and bottom contact points.
                    float crease=Mathf.Sin(h*Mathf.PI);float side=Mathf.InverseLerp(bounds.min.x,bounds.max.x,q.x);
                    q.x-=bounds.size.x*.035f*crease*side*side;
                    target[v]=q;
                }
                meshes.Add(mesh);rest.Add(target);dented.Add((Vector3[])target.Clone());
                mesh.vertices=target;mesh.RecalculateBounds();
            }
            if(impact)impactAt=Time.unscaledTime+.12f;
        }
        void LateUpdate()
        {
            if(block==null||renderers==null)return;
            float pulse=AtRisk?.16f+.07f*Mathf.Sin(Time.unscaledTime*5):0;
            for(int i=0;i<renderers.Length;i++)for(int j=0;j<colors[i].Length;j++){
                Color c=colors[i][j];
                if(damaged)c=Color.Lerp(c,new Color(.52f,.33f,.20f),.08f);
                c=Color.Lerp(c,new Color(1,.62f,.12f),pulse);
                var material=renderers[i].sharedMaterials[j];if(material.HasProperty("_Color"))c.a=material.color.a;
                renderers[i].GetPropertyBlock(block,j);block.SetColor("_Color",c);renderers[i].SetPropertyBlock(block,j);
            }
            float age=Time.unscaledTime-impactAt;
            if(impactAt<0||age<0)return;
            if(age>=.5f){
                for(int i=0;i<meshes.Count;i++){meshes[i].vertices=rest[i];meshes[i].RecalculateBounds();}
                impactAt=-100;return;
            }
            float squash=.025f*Mathf.Sin(Mathf.Clamp01(age/.5f)*Mathf.PI);
            for(int i=0;i<meshes.Count;i++){
                var source=rest[i];var target=dented[i];float floor=meshes[i].bounds.min.y;
                for(int v=0;v<source.Length;v++){target[v]=source[v];target[v].y=floor+(source[v].y-floor)*(1-squash);}
                meshes[i].vertices=target;meshes[i].RecalculateBounds();
            }
        }
        void OnDestroy(){foreach(var mesh in meshes){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}}
    }
}
