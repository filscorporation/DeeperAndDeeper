using Iron;

namespace IronCustom
{
    public partial class Block : ScriptComponent
    {
        public Map Map;
        
        public int X, Y;
        public BlockType Type;
        public BlockStats Stats;

        public bool Destroyed = false;

        public override void OnMouseEnter()
        {
            Map.OnBlockHovered(this);
        }

        public override void OnMouseExit()
        {
            Map.OnBlockExited();
        }

        public override void OnDestroy()
        {
            Map.OnBlockExited();
        }
    }
}