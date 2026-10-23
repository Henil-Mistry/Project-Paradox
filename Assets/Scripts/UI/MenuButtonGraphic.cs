using UnityEngine;
using UnityEngine.UI;
namespace Paradox
{
    // Native UI geometry: bevelled fantasy frame, inset panel, corner diamonds.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuButtonGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();Rect r=rectTransform.rect;
            Hex(vh,r,color);Hex(vh,new Rect(r.x+2,r.y+2,r.width-4,r.height-4),new Color(.035f,.025f,.065f,.96f));
            HexOutline(vh,new Rect(r.x+7,r.y+6,r.width-14,r.height-12),new Color(color.r,color.g,color.b,.45f));
            foreach(float x in new[]{r.x+28,r.xMax-28}){
                Diamond(vh,new Vector2(x,r.center.y),6,color);
                Diamond(vh,new Vector2(x,r.center.y),3,new Color(.07f,.04f,.1f));
            }
        }
        private static Vector2[] Points(Rect r)=>new[]{new Vector2(r.x,r.center.y),new Vector2(r.x+22,r.yMax),new Vector2(r.xMax-22,r.yMax),new Vector2(r.xMax,r.center.y),new Vector2(r.xMax-22,r.y),new Vector2(r.x+22,r.y)};
        private void Hex(VertexHelper v,Rect r,Color c){var p=Points(r);for(int i=1;i<5;i++)Tri(v,p[0],p[i],p[i+1],c);}
        private void HexOutline(VertexHelper v,Rect r,Color c){var p=Points(r);var q=Points(new Rect(r.x+1,r.y+1,r.width-2,r.height-2));for(int i=0;i<6;i++){int j=(i+1)%6;Tri(v,p[i],p[j],q[j],c);Tri(v,p[i],q[j],q[i],c);}}
        private void Diamond(VertexHelper v,Vector2 p,float s,Color c){Tri(v,p+Vector2.left*s,p+Vector2.up*s,p+Vector2.right*s,c);Tri(v,p+Vector2.left*s,p+Vector2.right*s,p+Vector2.down*s,c);}
        private void Tri(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Color col){int i=v.currentVertCount;v.AddVert(a,col,Vector2.zero);v.AddVert(b,col,Vector2.zero);v.AddVert(c,col,Vector2.zero);v.AddTriangle(i,i+1,i+2);}
    }
}
