using System;
using System.Collections.Generic;
using System.Linq;
using Iron;
using Math = Iron.Math;

namespace IronCustom
{
    public partial class Map
    {
        public Map()
        {
            MapParams = new Params();
        }
        
        private Block hoveredBlock;
        private UIBlockInfo uiInfo;
        private Entity selection;
        
        public int CurrentFrozenLevel = -1;
        private readonly List<Entity> freezeEffect = new List<Entity>();

        public void Initialize()
        {
            LoadSprites();
            LoadBlockStats();
            LoadOres();
            
            selection = ResourcesManager.LoadAsepriteData("selection.aseprite").CreateEntityFromAsepriteData();
            selection.IsActiveSelf = false;
        }

        public void OnBlockHovered(Block block)
        {
            if (UI.IsPointerOverUI())
                return;
            
            hoveredBlock = block;
            uiInfo = UIBlockInfo.Create(hoveredBlock);
            selection.IsActiveSelf = true;
            selection.Transformation.Position = hoveredBlock.Transformation.Position + new Vector3(0, 0, 1);
        }

        public void OnBlockExited()
        {
            hoveredBlock = null;
            uiInfo?.Entity.Destroy();
            selection.IsActiveSelf = false;
        }

        public void SpawnFirstBlock()
        {
            (int x, int y) = FindPositionToStart();
            blocks[x, y] = PlaceBlock(x, y, BlockType.BuildingBlock);
        }

        private Tuple<int, int> FindPositionToStart()
        {
            for (int j = blocks.GetLength(1) - MapParams.GroundLevel; j >= 2; j--)
            {
                int middle = (blocks.GetLength(0) - 8) / 2;
                for (int i = 0; i < middle; i++)
                {
                    if (CanSpawn(middle - i, j))
                        return new Tuple<int, int>(middle - i, j);
                    if (CanSpawn(middle + i, j))
                        return new Tuple<int, int>(middle + i, j);
                }
            }
            
            // We should never be there
            return null;
        }

        private bool CanSpawn(int x, int y)
        {
            return blocks[x, y] == null && blocks[x, y - 1] != null;
        }

        public bool TryPlacePlayersBlock(Vector2 position, BlockType type)
        {
            (int x, int y) = GetIndexes(position);

            if (!TryGet(x, y, out Block block) || block != null || IsFrozen(y))
                return false;

            if (!PlayersBlockInRange(x, y))
                return false;
            
            Block newBlock = PlaceBlock(x, y, type);
            blocks[x, y] = newBlock;

            CheckIfWon(y);

            Simulate();

            return true;
        }

        private void CheckIfWon(int y)
        {
            if (y <= Player.WINNING_HEIGHT)
            {
                GameManager.Win();
            }
        }

        private bool PlayersBlockInRange(int x, int y)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    if (i != j && i + j != 0
                        && x + i >= 0 && x + i < blocks.GetLength(0)
                        && y + j >= 0 && y + j < blocks.GetLength(1)
                        && blocks[x + i, y + j] != null && IsPlayers(blocks[x + i, y + j].Type))
                        return true;
                }
            }

            Log.LogInfo("No PlayersBlockInRange");
            return false;
        }

        public void FreezeLevel(int level)
        {
            bool needToSimulate = false;
            
            if (level < 0 || level > blocks.GetLength(1))
                return;

            CurrentFrozenLevel = level;
            int y = blocks.GetLength(1) - level - 1;
            for (int i = 0; i < blocks.GetLength(0); i++)
            {
                Block block = blocks[i, y];
                if (block != null && !block.Stats.IsStatic)
                {
                    block.Entity.Destroy();
                    blocks[i, y] = PlaceBlock(i, y, BlockType.Ice);
                    
                    needToSimulate = true;
                }
                else
                {
                    Entity entity = new Entity("Effect");
                    entity.AddComponent<SpriteRenderer>().Sprite = frozenSprite;
                    entity.Transformation.Position = (Vector3)GetPosition(i, y) + new Vector3(0, 0, 0.1f);
                    freezeEffect.Add(entity);
                }
            }
            
            if (needToSimulate)
                Simulate();
        }

        public void ClearFreeze()
        {
            CurrentFrozenLevel = 0;
            foreach (Entity entity in freezeEffect)
            {
                entity.Destroy();
            }
            freezeEffect.Clear();
        }

        public bool IsFrozen(int y) => blocks.GetLength(1) - CurrentFrozenLevel - 1 <= y;

        public Entity CreateBlockObject(int x, int y, BlockType type)
        {
            if (type == BlockType.None)
                return null;
            
            Entity entity = sprites[type].CreateEntityFromAsepriteData();
            entity.Name = $"Block {x} {y}";
            entity.Transformation.Position = GetPosition(x, y);
            entity.AddComponent<BoxCollider>();
            RigidBody rb = entity.AddComponent<RigidBody>();
            rb.RigidBodyType = RigidBodyType.Dynamic;
            //rb.Mass = blocksStats[type].Mass;

            return entity;
        }

        public void RemoveBlocks(List<Block> blocksToRemove)
        {
            foreach (Block block in blocksToRemove)
            {
                if (block == null)
                {
                    Log.LogError("Block is null from " + blocksToRemove.Count);
                    continue;
                }
                
                blocks[block.X, block.Y].Destroyed = true;
                blocks[block.X, block.Y].Entity.Destroy();
                blocks[block.X, block.Y] = null;
            }

            Simulate();
        }

        public void ReplaceWithStone(Block blocksToReplace)
        {
            if (blocksToReplace == null)
            {
                Log.LogError("Block is null from replace");
                return;
            }
            blocks[blocksToReplace.X, blocksToReplace.Y].Entity.Destroy();
            blocks[blocksToReplace.X, blocksToReplace.Y] = PlaceBlock(blocksToReplace.X, blocksToReplace.Y, BlockType.Stone);

            Simulate();
        }

        public Block OreBlockInRange(Block origin, int range)
        {
            int x = origin.X, y = origin.Y;
            for (int r = 1; r <= range; r++)
            {
                for (int j = -r; j <= r; j++)
                {
                    for (int i = -r; i <= r; i++)
                    {
                        if ((i != 0 || j != 0)
                            && x + i >= 0 && x + i < blocks.GetLength(0)
                            && y + j >= 0 && y + j < blocks.GetLength(1)
                            && blocks[x + i, y + j] != null && IsOre(blocks[x + i, y + j].Type))
                            return blocks[x + i, y + j];
                    }
                }
            }

            return null;
        }

        public Block DestructibleNonPlayerBlockInRangeDown(Block origin, int range)
        {
            int x = origin.X, y = origin.Y;
            for (int r = 1; r <= range; r++)
            {
                if (y - r >= 0 && y - r < blocks.GetLength(1)
                    && blocks[x, y - r] != null && !IsPlayers(blocks[x, y - r].Type))
                {
                    if (!IsDestructible(blocks[x, y - r]))
                        return null;
                    return blocks[x, y - r];
                }
            }

            return null;
        }

        public Ore OreFromBlockType(BlockType type)
        {
            return ores.FirstOrDefault(o => o.BlockType == type);
        }

        public bool TryGet(int x, int y, out Block block)
        {
            block = null;
            if (x < 0 || x >= blocks.GetLength(0) || y < 0 || y >= blocks.GetLength(1))
                return false;

            block = blocks[x, y];
            return true;
        }

        public bool TryGetTop(Block block, out Block topBlock)
        {
            return TryGet(block.X, block.Y + 1, out topBlock);
        }

        public bool TryGetBottom(Block block, out Block bottomBlock)
        {
            return TryGet(block.X, block.Y - 1, out bottomBlock);
        }

        public bool TryGetLeft(Block block, out Block leftBlock)
        {
            return TryGet(block.X - 1, block.Y, out leftBlock);
        }
        
        public bool TryGetRight(Block block, out Block rightBlock)
        {
            return TryGet(block.X + 1, block.Y, out rightBlock);
        }

        private Block PlaceBlock(int x, int y, BlockType type)
        {
            if (type == BlockType.None)
                return null;
            
            Entity entity = sprites[type].CreateEntityFromAsepriteData();
            entity.Name = $"Block {x} {y}";
            Block block = entity.AddComponent<Block>();
            block.Map = this;
            block.Transformation.Position = GetPosition(x, y);
            block.X = x;
            block.Y = y;
            block.Type = type;
            block.Stats = BlocksStats[type];
            entity.AddComponent<BoxCollider>();
            entity.AddComponent<RigidBody>().RigidBodyType = RigidBodyType.Static;
            
            if (type == BlockType.Lava || type == BlockType.Miner || type == BlockType.Drill)
                entity.GetComponent<Animator>().Play(sprites[type].Animations.First());
            if (type == BlockType.Miner)
                entity.AddComponent<Miner>().Init(this, block);
            if (type == BlockType.Drill)
                entity.AddComponent<Drill>().Init(this, block);

            return block;
        }

        private Vector2 GetPosition(int x, int y)
        {
            return new Vector2(-blocks.GetLength(0) / 2 + x, y - blocks.GetLength(1) + MapParams.GroundLevel);
        }

        private Tuple<int, int> GetIndexes(Vector2 position)
        {
            Vector2 zero = GetPosition(0, 0);
            return new Tuple<int, int>((int)Math.Round(position.X - zero.X), (int)Math.Round(position.Y - zero.Y));
        }

        public Vector2 GetTopLeft()
        {
            return GetPosition(0, blocks.GetLength(1) - 1);
        }

        public Vector2 GetBottomRight()
        {
            return GetPosition(blocks.GetLength(1) - 1, 0);
        }
    }
}