using System.Collections.Generic;
using System.Linq;
using Iron;

namespace IronCustom
{
    public class Drill : ScriptComponent
    {
        private Player player;
        private Map map;
        private Block block;

        private Block currentDrilledBlock = null;
        private float drillingProgress = 0;
        private Entity currentDrillingEffect;

        private static Sprite _drillingEffect = null;
        private static Sprite drillingEffect => _drillingEffect
                                                ?? (_drillingEffect = ResourcesManager.LoadAsepriteData("drilling_effect.aseprite").Sprites.First());

        public void Init(Map map, Block block)
        {
            this.map = map;
            this.block = block;
            player = FindAllOfType<Player>().First();
        }

        public override void OnUpdate()
        {
            if (currentDrilledBlock != null)
            {
                if (currentDrilledBlock.Destroyed)
                {
                    currentDrilledBlock = null;
                    drillingProgress = 0;
                    currentDrillingEffect?.Destroy();
                    currentDrillingEffect = null;
                    return;
                }
                drillingProgress += Time.DeltaTime * player.DrillingSpeed;
                if (currentDrilledBlock.Stats.Durability > 0 && drillingProgress > currentDrilledBlock.Stats.Durability)
                {
                    map.RemoveBlocks(new List<Block> { currentDrilledBlock });
                    currentDrilledBlock = null;
                    drillingProgress = 0;
                    currentDrillingEffect?.Destroy();
                    currentDrillingEffect = null;
                }
                else if (currentDrillingEffect == null)
                {
                    currentDrillingEffect = new Entity("Drilling effect");
                    currentDrillingEffect.AddComponent<SpriteRenderer>().Sprite = drillingEffect;
                    currentDrillingEffect.Transformation.Position = currentDrilledBlock.Transformation.Position + new Vector3(0, 0, 0.1f);
                }
            }
            else
            {
                currentDrilledBlock = map.DestructibleNonPlayerBlockInRangeDown(block, player.DrillingRange);
            }
        }

        public override void OnDestroy()
        {
            currentDrillingEffect?.Destroy();
            currentDrillingEffect = null;
        }
    }
}