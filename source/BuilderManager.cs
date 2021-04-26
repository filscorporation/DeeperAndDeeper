using System.Collections.Generic;
using System.Linq;
using Iron;

namespace IronCustom
{
    public class BuilderManager : ScriptComponent
    {
        private Map map;
        private Player player;

        private AsepriteData warningSprite;
        private AsepriteData explosionSprite;

        private readonly LinkedList<WarningBlock> blocksToDestroy = new LinkedList<WarningBlock>();
        
        private class WarningBlock
        {
            public float TimePassed;
            public float Delay;
            public Block Block;
            public Entity Warning;

            public bool Found;
        }
        
        public void Init(Map map, Player player)
        {
            this.map = map;
            this.player = player;

            warningSprite = ResourcesManager.LoadAsepriteData("warning_sign.aseprite", true);
            explosionSprite = ResourcesManager.LoadAsepriteData("explosion.aseprite");
            explosionSprite.Animations.First().EndWithEmptyFrame();

            map.OnMapSimulated += HandleSimulationResult;
        }

        public void Clear()
        {
            StopAllCoroutines();
            foreach (WarningBlock warningBlock in blocksToDestroy)
            {
                warningBlock.Warning?.Destroy();
            }
            blocksToDestroy.Clear();
        }

        public override void OnUpdate()
        {
            List<Block> removeFromMap = new List<Block>();
            
            LinkedListNode<WarningBlock> currentNode = blocksToDestroy.First;
            while (currentNode != null)
            {
                currentNode.Value.TimePassed += Time.DeltaTime;
                if (currentNode.Value.TimePassed > currentNode.Value.Delay)
                {
                    LinkedListNode<WarningBlock> temp = currentNode.Next;
                    
                    if (!map.IsFrozen(currentNode.Value.Block.Y))
                    {
                        DestroyBlock(currentNode.Value.Block);
                        removeFromMap.Add(currentNode.Value.Block);
                        blocksToDestroy.Remove(currentNode);
                    }
                    currentNode.Value.Warning.Destroy();

                    currentNode = temp;
                    continue;
                }
                
                currentNode = currentNode.Next;
            }

            if (removeFromMap.Any())
            {
                map.RemoveBlocks(removeFromMap);
            }
        }

        private void DestroyBlock(Block block)
        {
            Entity entity = map.CreateBlockObject(block.X, block.Y, block.Type);
            Entity explosionEntity = explosionSprite.CreateEntityFromAsepriteData();
            explosionEntity.Transformation.Position = entity.Transformation.Position + new Vector3(0, 0, 0.5f);
            explosionEntity.GetComponent<Animator>().Play(explosionSprite.Animations.First());

            StartCoroutine(Coroutine.WaitForSeconds(() => explosionEntity.Destroy(), 3f));
            StartCoroutine(Coroutine.WaitForSeconds(() => entity.Destroy(), 5f));
        }

        private void DestroyAndRemove(Block block)
        {
            LinkedListNode<WarningBlock> currentNode = blocksToDestroy.First;
            while (currentNode != null)
            {
                LinkedListNode<WarningBlock> temp = currentNode.Next;
                if (currentNode.Value.Block == block)
                {
                    blocksToDestroy.Remove(currentNode);
                    currentNode.Value.Warning.Destroy();
                }
                
                currentNode = temp;
            }
            
            Entity entity = map.CreateBlockObject(block.X, block.Y, block.Type);
            Entity explosionEntity = explosionSprite.CreateEntityFromAsepriteData();
            explosionEntity.Transformation.Position = entity.Transformation.Position + new Vector3(0, 0, 0.5f);
            explosionEntity.GetComponent<Animator>().Play(explosionSprite.Animations.First());

            StartCoroutine(Coroutine.WaitForSeconds(() => explosionEntity.Destroy(), 3f));
            StartCoroutine(Coroutine.WaitForSeconds(() => entity.Destroy(), 5f));
        }

        private void AddBlockToDestroy(Block block)
        {
            WarningBlock found = blocksToDestroy.FirstOrDefault(b => b.Block == block);
            if (found == null)
            {
                Entity entity = warningSprite.CreateEntityFromAsepriteData();
                entity.GetComponent<Animator>().Play(warningSprite.Animations.First());
                entity.Transformation.Position = block.Transformation.Position + new Vector3(0, 0, 0.25f);

                blocksToDestroy.AddLast(new WarningBlock
                {
                    Block = block,
                    Delay = player.DelayBeforeBlockDestroyed,
                    TimePassed = 0,
                    Warning = entity,
                    Found = true,
                });
            }
            else
            {
                found.Found = true;
            }
        }

        private void RemoveNotFound()
        {
            LinkedListNode<WarningBlock> currentNode = blocksToDestroy.First;
            while (currentNode != null)
            {
                LinkedListNode<WarningBlock> temp = currentNode.Next;
                if (!currentNode.Value.Found)
                {
                    blocksToDestroy.Remove(currentNode);
                    currentNode.Value.Warning.Destroy();
                }
                
                currentNode = temp;
            }
        }

        private void HandleSimulationResult(List<Map.Figure> figures)
        {
            if (!figures.Any())
            {
                GameManager.Lose();
                return;
            }
            
            foreach (Map.Figure figure in figures)
            {
                if (figure.FreeFall)
                {
                    foreach (Block block in figure.Blocks)
                    {
                        DestroyAndRemove(block);
                    }
                    map.RemoveBlocks(figure.Blocks);
                }
                else
                {
                    foreach (WarningBlock warningBlock in blocksToDestroy)
                        warningBlock.Found = false;
                    foreach (Block unhandledBlock in figure.UnhandledBlocks)
                    {
                        AddBlockToDestroy(unhandledBlock);
                    }

                    RemoveNotFound();
                }
            }
        }
    }
}