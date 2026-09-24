using System.Linq;
using UnityEngine;

namespace BaggingOrder
{
    public partial class BaggingGame
    {
        string inspectedDamage;
        void UpdateCrushWarnings()
        {
            foreach(var pair in packedObjects){
                if(pair.Value==null)continue;
                var visual=pair.Value.GetComponent<GroceryDamageVisual>();
                if(visual!=null)visual.AtRisk=dragging&&hovered!=null&&hovered.Crushed.Contains(pair.Key);
            }
        }
        Vector2 DamageScreenPoint(Vector3 position)
        {
            Vector3 p=view.WorldToScreenPoint(position);
            return new Vector2(p.x/UIScale,(Screen.height-p.y)/UIScale);
        }
        void DrawCrushFeedback()
        {
            if(dragging&&hovered!=null&&hovered.Crushed.Count>0){
                var victim=bags[bagIndex].FirstOrDefault(i=>hovered.Crushed.Contains(i.Id));
                if(victim!=null){
                    var p=DamageScreenPoint(victim.Position+Vector3.up*victim.Size.y);
                    p.x=Mathf.Clamp(p.x,65,Screen.width/UIScale-65);
                    var tag=new Rect(p.x-59,Mathf.Max(108,p.y-37),118,29);
                    PaintedTile(tag,Sun);Text(tag,"Too heavy",16,Ink,true,TextAnchor.MiddleCenter);
                }
            }
        }
    }
}
