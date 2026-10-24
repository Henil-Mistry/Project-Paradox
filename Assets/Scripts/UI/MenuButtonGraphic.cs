using UnityEngine;
using UnityEngine.UI;
namespace Paradox
{
    // Native UI geometry: bevelled fantasy frame, inset panel, corner diamonds.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuButtonGraphic : MaskableGraphic
    {
        // Optional state driven only by the title animator; zero leaves all other menus unchanged.
        [System.NonSerialized] public float TitleEmphasis;
        [System.NonSerialized] public float TitleSheen=-1;
        private readonly Vector2[] points=new Vector2[6], inner=new Vector2[6];
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();Rect r=rectTransform.rect;
            Hex(vh,r,color);Hex(vh,new Rect(r.x+2,r.y+2,r.width-4,r.height-4),new Color(.035f,.025f,.065f,.96f));
            HexOutline(vh,new Rect(r.x+7,r.y+6,r.width-14,r.height-12),new Color(color.r,color.g,color.b,.45f));
            for(int side=0;side<2;side++){float x=side==0?r.x+28:r.xMax-28;
                Diamond(vh,new Vector2(x,r.center.y),6+TitleEmphasis,color);
                Diamond(vh,new Vector2(x,r.center.y),3,new Color(.07f,.04f,.1f));
            }
            if(TitleSheen>=0){float x=Mathf.Lerp(r.x+30,r.xMax-30,TitleSheen);Color c=new Color(.85f,.9f,1,Mathf.Sin(TitleSheen*Mathf.PI)*.14f);Tri(vh,new Vector2(x-14,r.y+8),new Vector2(x+10,r.yMax-8),new Vector2(x+32,r.yMax-8),c);Tri(vh,new Vector2(x-14,r.y+8),new Vector2(x+32,r.yMax-8),new Vector2(x+8,r.y+8),c);}
        }
        private static void Points(Rect r,Vector2[] p){p[0]=new Vector2(r.x,r.center.y);p[1]=new Vector2(r.x+22,r.yMax);p[2]=new Vector2(r.xMax-22,r.yMax);p[3]=new Vector2(r.xMax,r.center.y);p[4]=new Vector2(r.xMax-22,r.y);p[5]=new Vector2(r.x+22,r.y);}
        private void Hex(VertexHelper v,Rect r,Color c){Points(r,points);for(int i=1;i<5;i++)Tri(v,points[0],points[i],points[i+1],c);}
        private void HexOutline(VertexHelper v,Rect r,Color c){Points(r,points);Points(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),inner);for(int i=0;i<6;i++){int j=(i+1)%6;Tri(v,points[i],points[j],inner[j],c);Tri(v,points[i],inner[j],inner[i],c);}}
        private void Diamond(VertexHelper v,Vector2 p,float s,Color c){Tri(v,p+Vector2.left*s,p+Vector2.up*s,p+Vector2.right*s,c);Tri(v,p+Vector2.left*s,p+Vector2.right*s,p+Vector2.down*s,c);}
        private void Tri(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Color col){int i=v.currentVertCount;v.AddVert(a,col,Vector2.zero);v.AddVert(b,col,Vector2.zero);v.AddVert(c,col,Vector2.zero);v.AddTriangle(i,i+1,i+2);}
    }
}
