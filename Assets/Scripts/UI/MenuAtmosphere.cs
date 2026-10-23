using UnityEngine;
using UnityEngine.UI;
namespace Paradox
{
    // Lightweight decorative mesh in screen space; no gameplay objects or colliders.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuAtmosphere : MaskableGraphic
    {
        private VertexHelper mesh;
        private float clock;
        private static readonly Color Gold=new Color(.84f,.69f,.42f);
        private static readonly Color Silver=new Color(.63f,.61f,.9f);
        private void Update(){clock=Time.unscaledTime;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            mesh=vh;mesh.Clear();
            // Gentle edge shadows preserve the central text against the illustration.
            Quad(0,0,1,1,new Color(.035f,.02f,.09f,.12f));
            for(int side=0;side<2;side++){
                bool dark=side==1;Color stone=dark?new Color(.055f,.035f,.12f):new Color(.87f,.85f,.79f);
                Color accent=dark?Silver:Gold;
                for(int i=0;i<7;i++){
                    float x=dark?.97f-i*.045f:.03f+i*.045f;
                    float y=.08f+(i%3)*.12f;float w=.035f+(i%2)*.022f;
                    float floatY=Mathf.Sin(clock*.24f+i)*.003f;
                    y+=floatY;
                    Poly(new[]{new Vector2(x-w,y),new Vector2(x+w,y),new Vector2(x+w*.7f,y-.045f),new Vector2(x,y-.13f),new Vector2(x-w*.65f,y-.065f)},stone);
                    Poly(new[]{new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w*.7f,y-.045f),new Vector2(x,y-.13f)},dark?new Color(.11f,.07f,.2f):new Color(.68f,.7f,.73f));
                    Quad(x-w,y,2*w,.012f,accent);
                    float h=.13f+(i%3)*.065f;
                    Quad(x-w*.55f,y+.012f,w*.28f,h,stone);Quad(x+w*.27f,y+.012f,w*.28f,h,stone);
                    Quad(x-w*.55f,y+h,w*1.1f,.012f,accent);
                    Quad(x-w*.5f,y+.02f,.001f,h,accent);
                    Quad(x-w*.3f,y+.012f,w*.03f,h,dark?new Color(.3f,.19f,.47f):Color.white);
                    if(dark)for(int k=0;k<4;k++){
                        float yy=y+.035f+k*.035f;Line(new Vector2(x+w*.4f-.004f,yy),new Vector2(x+w*.4f+.004f,yy+.01f),.0012f,new Color(.65f,.36f,1,.8f));
                    }
                }
                // Two celestial armillary silhouettes framing the title.
                Vector2 centre=new Vector2(dark?.78f:.22f,.70f);
                float aspect=rectTransform.rect.width/Mathf.Max(1,rectTransform.rect.height);
                Ring(centre,.125f/aspect,.125f,0,accent,.002f);
                Ring(centre,.15f/aspect,.068f,.55f+clock*.018f,accent,.0018f);
                Ring(centre,.15f/aspect,.055f,-.8f-clock*.014f,accent,.0014f);
                Ring(centre,.17f/aspect,.17f,0,new Color(accent.r,accent.g,accent.b,.38f),.0007f);
                Disc(centre,.031f/aspect,.031f,dark?new Color(.025f,.015f,.08f):new Color(1,.94f,.75f));
                Ring(centre,.034f/aspect,.034f,0,dark?new Color(.63f,.42f,.95f):new Color(1,.85f,.55f),.002f);
                Line(new Vector2(centre.x,.88f),new Vector2(centre.x,.96f),.0008f,accent);
                for(int i=0;i<36;i++){
                    float x=(Hash(i+side*100)*.45f)+(dark?.53f:.02f);
                    float y=Mathf.Repeat(Hash(i+250)-clock*(dark?.004f:.016f),1);
                    float size=dark?.0012f:.0024f;
                    Diamond(new Vector2(x,y),size,dark?new Color(.75f,.68f,1,.6f):new Color(1,.7f,.79f,.65f));
                }
            }
            // A split foreground terrace anchors the otherwise weightless composition.
            Poly(new[]{new Vector2(.12f,0),new Vector2(.29f,.075f),new Vector2(.5f,.075f),new Vector2(.5f,0)},new Color(.78f,.79f,.81f));
            Poly(new[]{new Vector2(.5f,0),new Vector2(.5f,.075f),new Vector2(.71f,.075f),new Vector2(.88f,0)},new Color(.055f,.04f,.13f));
            Line(new Vector2(.29f,.075f),new Vector2(.5f,.075f),.002f,Gold);
            Line(new Vector2(.5f,.075f),new Vector2(.71f,.075f),.002f,Silver);
            for(int i=0;i<9;i++){float x=.3f+i*.05f;Line(new Vector2(x,.073f),new Vector2(.5f+(x-.5f)*1.9f,0),.0007f,i<4?Gold:Silver);}
            // Several fine strands form a quiet luminous rift, never a flashing pulse.
            for(int band=5;band>=0;band--){
                Color c=band==0?new Color(.89f,.93f,1,.8f):new Color(.57f,.48f,1,.035f);
                for(int i=0;i<40;i++){
                    float y=i/40f, yn=(i+1)/40f;
                    Line(new Vector2(Rift(y),y),new Vector2(Rift(yn),yn),band==0?.0008f:.003f*band,c);
                }
            }
        }
        private float Rift(float y)=>.5f+.012f*Mathf.Sin(y*9+clock*.1f)+.004f*Mathf.Sin(y*24-clock*.13f);
        private float Hash(int i)=>Mathf.Repeat(Mathf.Sin(i*127.1f+13.7f)*43758.54f,1);
        private void Quad(float x,float y,float w,float h,Color c)=>Poly(new[]{new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h)},c);
        private void Diamond(Vector2 p,float s,Color c)=>Poly(new[]{p+new Vector2(-s,0),p+new Vector2(0,s*1.7f),p+new Vector2(s,0),p-new Vector2(0,s*1.7f)},c);
        private void Disc(Vector2 p,float rx,float ry,Color c){for(int i=0;i<48;i++){float a=i*Mathf.PI/24,b=(i+1)*Mathf.PI/24;Tri(p,p+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry),p+new Vector2(Mathf.Cos(b)*rx,Mathf.Sin(b)*ry),c);}}
        private void Ring(Vector2 p,float rx,float ry,float angle,Color c,float width){Vector2 previous=Ellipse(p,rx,ry,angle,0);for(int i=1;i<=96;i++){Vector2 next=Ellipse(p,rx,ry,angle,i*Mathf.PI/48);Line(previous,next,width,c);previous=next;}}
        private Vector2 Ellipse(Vector2 p,float rx,float ry,float angle,float t){float aspect=rectTransform.rect.width/Mathf.Max(1,rectTransform.rect.height);float x=Mathf.Cos(t)*rx*aspect,y=Mathf.Sin(t)*ry;return p+new Vector2((x*Mathf.Cos(angle)-y*Mathf.Sin(angle))/aspect,x*Mathf.Sin(angle)+y*Mathf.Cos(angle));}
        private void Line(Vector2 a,Vector2 b,float width,Color c){Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;Poly(new[]{a+n,b+n,b-n,a-n},c);}
        private void Poly(Vector2[] p,Color c){for(int i=1;i<p.Length-1;i++)Tri(p[0],p[i],p[i+1],c);}
        private void Tri(Vector2 a,Vector2 b,Vector2 c,Color tint){int i=mesh.currentVertCount;Rect r=rectTransform.rect;foreach(var p in new[]{a,b,c})mesh.AddVert(new Vector3(r.x+p.x*r.width,r.y+p.y*r.height),tint,Vector2.zero);mesh.AddTriangle(i,i+1,i+2);}
    }
}
