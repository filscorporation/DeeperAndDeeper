using System.Collections.Generic;
using System.Linq;
using Iron;

namespace IronCustom
{
    public class Miner : ScriptComponent
    {
        private Player player;
        private Map map;
        private Block block;

        private Block currentMinedBlock = null;
        private float miningProgress = 0;
        private Entity currentMiningEffect;

        private static Sprite _miningEffect = null;
        private static Sprite miningEffect => _miningEffect
                                              ?? (_miningEffect = ResourcesManager.LoadAsepriteData("mining_effect.aseprite").Sprites.First());
        
        public void Init(Map map, Block block)
        {
            this.map = map;
            this.block = block;
            player = FindAllOfType<Player>().First();
        }

        public override void OnUpdate()
        {
            if (block == null)
                return;
            
            if (currentMinedBlock != null)
            {
                miningProgress += Time.DeltaTime * player.MiningSpeed;
                if (currentMinedBlock.Stats.Durability > 0 && miningProgress > currentMinedBlock.Stats.Durability)
                {
                    miningProgress = 0;
                    Ore ore = map.OreFromBlockType(currentMinedBlock.Type);
                    if (ore == null)
                    {
                        currentMinedBlock = null;
                        Log.LogError("Mined ore is null");
                        return;
                    }
                    player.AddResource(ore.Resource, ore.ResourcesInOneBlock);
                    map.ReplaceWithStone(currentMinedBlock);
                    currentMinedBlock = null;
                    currentMiningEffect?.Destroy();
                    currentMiningEffect = null;
                }
                else if (currentMiningEffect == null)
                {
                    currentMiningEffect = new Entity("Mining effect");
                    currentMiningEffect.AddComponent<SpriteRenderer>().Sprite = miningEffect;
                    currentMiningEffect.Transformation.Position = currentMinedBlock.Transformation.Position + new Vector3(0, 0, 0.1f);
                }
            }
            else
            {
                currentMinedBlock = map.OreBlockInRange(block, player.MiningRange);
            }
        }

        public override void OnDestroy()
        {
            currentMiningEffect?.Destroy();
            currentMiningEffect = null;
        }
    }
}