namespace IronCustom
{
    public class Ore
    {
        public BlockType BlockType;
        public Resources Resource;

        public int MinDepthToSpawn = 0;
        public float SpawnChance = 0.1f;
        public int MinSpawnAmount = 1;
        public int MaxSpawnAmount = 4;
        public int ResourcesInOneBlock = 1;
    }
}