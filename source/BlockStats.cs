namespace IronCustom
{
    public class BlockStats
    {
        public BlockStats(bool isStatic)
        {
            IsStatic = isStatic;
            Attachable = IsStatic;

            Mass = 1.0f;
            PressureStrength = 5.0f;
            MountStrength = 2.0f;
            Description = "";
            
            Durability = 4.0f;
        }
        
        public bool IsStatic;
        public bool Attachable;
        
        public float Mass;
        public float PressureStrength;
        public float MountStrength;
        
        public float Durability;

        public string Description;
    }
}