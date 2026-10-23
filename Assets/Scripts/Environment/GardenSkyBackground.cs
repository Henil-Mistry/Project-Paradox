using UnityEngine;
using UnityEngine.Rendering;
namespace Paradox
{
    // Camera-local illustration, drawn before opaque geometry. No global sky or lighting mutation.
    [RequireComponent(typeof(Camera))]
    public sealed class GardenSkyBackground : MonoBehaviour
    {
        [SerializeField] private DimensionManager manager;
        [SerializeField] private Material template;
        [SerializeField] private Texture day;
        [SerializeField] private Texture night;
        private Camera owner;
        private Material material;
        private Mesh quad;
        private CommandBuffer commands;
        private CameraClearFlags previousClear;
        public Texture CurrentTexture => material ? material.mainTexture : null;
        public void Configure(DimensionManager dimensions, Material source, Texture white, Texture black)
        { manager=dimensions; template=source; day=white; night=black; }
        private void OnEnable()
        {
            if(!manager || !template || !day || !night) return;
            owner=GetComponent<Camera>(); previousClear=owner.clearFlags; owner.clearFlags=CameraClearFlags.SolidColor;
            material=new Material(template);
            quad=new Mesh { name="Garden background screen quad" };
            quad.vertices=new[]{new Vector3(-1,-1,0),new Vector3(1,-1,0),new Vector3(1,1,0),new Vector3(-1,1,0)};
            quad.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up}; quad.triangles=new[]{0,1,2,0,2,3};
            commands=new CommandBuffer { name="Garden illustrated sky" };
            commands.DrawMesh(quad,Matrix4x4.identity,material);
            owner.AddCommandBuffer(CameraEvent.BeforeForwardOpaque,commands);
            manager.Changed+=Apply; Apply(manager.Current);
        }
        private void OnPreRender()
        {
            if(material && owner)
                material.SetFloat("_ViewAspect", owner.targetTexture ? (float)owner.targetTexture.width/owner.targetTexture.height : (float)owner.pixelWidth/Mathf.Max(1,owner.pixelHeight));
        }
        private void Apply(Dimension dimension) { if(material) material.mainTexture=dimension==Dimension.White?day:night; }
        private void OnDisable()
        {
            if(manager) manager.Changed-=Apply;
            if(owner){if(commands!=null)owner.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque,commands); owner.clearFlags=previousClear;}
            commands?.Release(); commands=null;
            if(material)Destroy(material); if(quad)Destroy(quad);
        }
    }
}
