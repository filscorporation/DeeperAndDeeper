using System;
using System.Collections.Generic;
using System.Linq;
using Iron;
using Math = Iron.Math;
using Random = Iron.Random;

namespace IronCustom
{
    public enum BlockType
    {
        None = 0,
        Stone = 1,
        Basalt = 2,
        
        Lava = 10,
        Bottom = 11,
        Ice = 12,
        
        IronOre = 20,
        TitaniumOre = 21,
        UraniumOre = 22,
        DiamondOre = 23,
        
        BuildingBlock = 100,
        Carcass = 101,
        Miner = 102,
        Drill = 103,
    }
    
    public partial class Map
    {
        public class Params
        {
            public int Width = 48;
            public int Height = 144;
            
            public int GroundLevel = 16;
            public int CAPasses = 2;
            public float InitialWallChance = 0.4f;
            public int NullToWallValue = 4;
            public int WallToNullValue = 3;
            public int TargetIronOreAmount = 20;
        }

        public readonly Params MapParams;
        
        private readonly Dictionary<BlockType, AsepriteData> sprites = new Dictionary<BlockType, AsepriteData>();
        public readonly Dictionary<BlockType, BlockStats> BlocksStats = new Dictionary<BlockType, BlockStats>();
        public readonly List<Ore> Ores = new List<Ore>();
        private Block[,] blocks;
        
        private Sprite frozenSprite;

        public void CreateMap()
        {
            GenerateMap(MapParams.Width, MapParams.Height);
            //Simulate();

            // TODO: remove
            //Camera.Main.Height = MapParams.Height + 4;
            //Camera.Main.Transformation.Position = new Vector3(
            //    0,
            //    GetPosition(MapParams.Width / 2, MapParams.Height / 2).Y,
            //    Camera.Main.Transformation.Position.Z
            //);
        }

        public void DeleteMap()
        {
            for (int j = 0; j < blocks.GetLength(1); j++)
            {
                for (int i = 0; i < blocks.GetLength(0); i++)
                {
                    blocks[i, j]?.Entity.Destroy();
                }
            }
        }
        
        private void LoadSprites()
        {
            frozenSprite = ResourcesManager.LoadAsepriteData("frozen_environment.aseprite").Sprites.First();
            
            sprites[BlockType.None] = null;
            sprites[BlockType.Stone] = ResourcesManager.LoadAsepriteData("dirt.aseprite");
            sprites[BlockType.Basalt] = ResourcesManager.LoadAsepriteData("basalt.aseprite");
            
            sprites[BlockType.Lava] = ResourcesManager.LoadAsepriteData("lava.aseprite", true);
            sprites[BlockType.Bottom] = ResourcesManager.LoadAsepriteData("bottom.aseprite");
            sprites[BlockType.Ice] = ResourcesManager.LoadAsepriteData("frozen.aseprite");
            
            sprites[BlockType.IronOre] = ResourcesManager.LoadAsepriteData("iron_ore.aseprite");
            sprites[BlockType.TitaniumOre] = ResourcesManager.LoadAsepriteData("titanium_ore.aseprite");
            sprites[BlockType.UraniumOre] = ResourcesManager.LoadAsepriteData("uranium_ore.aseprite");
            sprites[BlockType.DiamondOre] = ResourcesManager.LoadAsepriteData("diamond_ore.aseprite");
            
            sprites[BlockType.BuildingBlock] = ResourcesManager.LoadAsepriteData("block.aseprite");
            sprites[BlockType.Carcass] = ResourcesManager.LoadAsepriteData("carcass.aseprite");
            sprites[BlockType.Miner] = ResourcesManager.LoadAsepriteData("miner.aseprite", true);
            sprites[BlockType.Drill] = ResourcesManager.LoadAsepriteData("drill.aseprite", true);
        }

        private void LoadBlockStats()
        {
            BlocksStats[BlockType.None] = new BlockStats(true);

            BlocksStats[BlockType.Stone] = new BlockStats(true)
            {
                Durability = 3f,
                Description = "Basic ground block",
            };
            BlocksStats[BlockType.Basalt] = new BlockStats(true)
            {
                Durability = 10f,
                Description = "Durable ground block, harder to get through",
            };
            
            BlocksStats[BlockType.Lava] = new BlockStats(true)
            {
                Durability = -1f,
                Attachable = false,
                Description = "Unlimited source of heat!",
            };
            BlocksStats[BlockType.Bottom] = new BlockStats(true)
            {
                Durability = -1f,
                Attachable = false,
                Description = "Unlimited source of heat!",
            };
            BlocksStats[BlockType.Ice] = new BlockStats(true)
            {
                Durability = -1f,
                Attachable = false,
                Description = "It probably was something useful before..",
            };
            
            BlocksStats[BlockType.IronOre] = new BlockStats(true)
            {
                Durability = 3f,
                Description = "Iron used for building blocks",
            };
            BlocksStats[BlockType.TitaniumOre] = new BlockStats(true)
            {
                Durability = 6f,
                Description = "Titanium used to make blocks stronger",
            };
            BlocksStats[BlockType.UraniumOre] = new BlockStats(true)
            {
                Durability = 3f,
                Description = "Uranium used to upgrade miner",
            };
            BlocksStats[BlockType.DiamondOre] = new BlockStats(true)
            {
                Durability = 8f,
                Description = "Diamonds used to upgrade drill",
            };

            BlocksStats[BlockType.BuildingBlock] = new BlockStats(false)
            {
                Description = "Main building block",
            };
            BlocksStats[BlockType.Miner] = new BlockStats(false)
            {
                Mass = 2f,
                PressureStrength = 3f,
                Description = "Mines resources in close range",
            };
            BlocksStats[BlockType.Drill] = new BlockStats(false)
            {
                Mass = 2f,
                PressureStrength = 3f,
                Description = "Destroys up to 3 blocks underneath",
            };
        }

        private void LoadOres()
        {
            Ores.Add(new Ore
            {
                BlockType = BlockType.IronOre,
                Resource = Resources.Iron,
                
                MinDepthToSpawn = 0,
                MinSpawnAmount = 2,
                MaxSpawnAmount = 5,
                ResourcesInOneBlock = 3,
                SpawnChance = 0.075f,
            });
            
            Ores.Add(new Ore
            {
                BlockType = BlockType.TitaniumOre,
                Resource = Resources.Titanium,
                
                MinDepthToSpawn = 25,
                MinSpawnAmount = 1,
                MaxSpawnAmount = 3,
                ResourcesInOneBlock = 1,
                SpawnChance = 0.02f,
            });
            
            Ores.Add(new Ore
            {
                BlockType = BlockType.UraniumOre,
                Resource = Resources.Uranium,
                
                MinDepthToSpawn = 35,
                MinSpawnAmount = 1,
                MaxSpawnAmount = 2,
                ResourcesInOneBlock = 1,
                SpawnChance = 0.015f,
            });
            
            Ores.Add(new Ore
            {
                BlockType = BlockType.DiamondOre,
                Resource = Resources.Diamond,
                
                MinDepthToSpawn = 40,
                MinSpawnAmount = 1,
                MaxSpawnAmount = 1,
                ResourcesInOneBlock = 1,
                SpawnChance = 0.01f,
            });
        }

        private void GenerateMap(int width, int height)
        {
            blocks = new Block[width, height];
            BlockType[,] types = GetMapTypes(width, height);
            
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    Block block = PlaceBlock(i, j, types[i, j]);
                    blocks[i, j] = block;
                }
            }
        }

        private BlockType[,] GetMapTypes(int width, int height)
        {
            BlockType[,] types = new BlockType[width, height];
            
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    BlockType type;
                    if (j >= height - MapParams.GroundLevel)
                        type = BlockType.None;
                    else
                        type = Random.NextFloat(0, 1) > MapParams.InitialWallChance ? BlockType.None : BlockType.Stone;
                    
                    types[i, j] = type;
                }
            }

            FillBorders(types);

            for (int i = 0; i < MapParams.CAPasses; i++)
            {
                CellularAutomatonPass(types);
            }

            AddLava(types);
            AddOres(types);
            RemoveUnwinnableStart(types);

            return types;
        }

        private void FillBorders(BlockType[,] types)
        {
            for (int j = 1; j < types.GetLength(1) - 1; j++)
            {
                if (j < types.GetLength(1) - MapParams.GroundLevel)
                {
                    types[0, j] = BlockType.Stone;
                    types[types.GetLength(0) - 1, j] = BlockType.Stone;
                }
            }
        }

        private void CellularAutomatonPass(BlockType[,] types)
        {
            for (int j = 1; j < types.GetLength(1) - 1; j++)
            {
                for (int i = 1; i < types.GetLength(0) - 1; i++)
                {
                    int n = NeighboursNumber(types, i, j);
                    if (types[i, j] != BlockType.None && n < MapParams.WallToNullValue)
                        types[i, j] = BlockType.None;
                    if (types[i, j] == BlockType.None && n > MapParams.NullToWallValue)
                        types[i, j] = BlockType.Stone;
                }
            }
        }

        private void AddOres(BlockType[,] types)
        {
            int depth = 0;
            for (int j = types.GetLength(1) - MapParams.GroundLevel; j >= 2; j--)
            {
                foreach (Ore ore in Ores)
                {
                    if (ore.MinDepthToSpawn <= depth)
                    {
                        int solidCount = SolidNonOreBlocksOnLevel(types, j);
                        float chance = 1 - Math.Pow(1 - ore.SpawnChance, solidCount);
                        if (Random.NextFloat(0, 1) < chance)
                        {
                            int x = SolidNonOreBlockX(types, j, Random.NextInt(0, solidCount - 1));
                            SpawnOreFromBlock(types, x, j, ore);
                        }
                    }
                }
                depth++;
            }
        }
        
        private void RemoveUnwinnableStart(BlockType[,] types)
        {
            int upperLevel = types.GetLength(1) - MapParams.GroundLevel;
            int lowerLevel = upperLevel - 8;

            int amount = 0;
            int left = (types.GetLength(0) - (int) Camera.Main.Width) / 2;
            int right = types.GetLength(0) - left;
            for (int j = upperLevel; j > lowerLevel; j--)
            {
                for (int i = left; i < right; i++)
                {
                    if (types[i, j] == BlockType.IronOre)
                        amount++;
                }
            }
            
            Ore ironOre = Ores.First(o => o.BlockType == BlockType.IronOre);
            int maxIterations = 20;
            while (amount < MapParams.TargetIronOreAmount)
            {
                for (int j = upperLevel; j > lowerLevel; j--)
                {
                    int solidCount = SolidNonOreBlocksOnLevel(types, j);
                    if (solidCount <= 0)
                        continue;
                    int x = SolidNonOreBlockX(types, j, Random.NextInt(0, solidCount - 1));
                    amount += SpawnOreFromBlock(types, x, j, ironOre, MapParams.TargetIronOreAmount - amount);
                }

                maxIterations--;
                if (maxIterations <= 0)
                    return;
            }
        }
        
        private static bool SolidNonOre(BlockType type) => type != BlockType.None && type < BlockType.Lava;
        private static bool IsOre(BlockType type) => type >= BlockType.IronOre && type < BlockType.BuildingBlock;
        private static bool IsPlayers(BlockType type) => type >= BlockType.BuildingBlock;
        private static bool IsDestructible(Block block) => block != null && block.Stats.Durability > 0;

        private int SolidNonOreBlocksOnLevel(BlockType[,] types, int y)
        {
            int result = 0;
            
            for (int i = 1; i < types.GetLength(0) - 1; i++)
            {
                if (SolidNonOre(types[i, y]))
                    result++;
            }

            return result;
        }

        private int SolidNonOreBlockX(BlockType[,] types, int y, int index)
        {
            int result = 0;
            int counter = -1;
            
            while (counter < index)
            {
                if (SolidNonOre(types[result, y]))
                    counter++;
                result++;
            }

            return result - 1;
        }

        private int SpawnOreFromBlock(BlockType[,] types, int x, int y, Ore ore, int maxChain = -1)
        {
            types[x, y] = ore.BlockType;
            int chain = Random.NextInt(ore.MinSpawnAmount, ore.MaxSpawnAmount);
            if (maxChain > 0)
                chain = chain > maxChain ? maxChain : chain;
            Tuple<int, int> pair = new Tuple<int, int>(x, y);
            for (int i = 1; i < chain; i++)
            {
                pair = RandomSolidNonOreNeighbour(types, pair.Item1, pair.Item2);
                if (pair == null)
                    break;

                types[pair.Item1, pair.Item2] = ore.BlockType;
            }

            return chain;
        }

        private Tuple<int, int> RandomSolidNonOreNeighbour(BlockType[,] types, int x, int y)
        {
            List<Tuple<int, int>> possibleResult = new List<Tuple<int, int>>();
            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    if (i != j && i + j != 0
                        && x + i >= 0 && x + i < types.GetLength(0)
                        && y + j >= 0 && y + j < types.GetLength(1)
                        && SolidNonOre(types[x + i, y + j]))
                        possibleResult.Add(new Tuple<int, int>(x + i, y + j));
                }
            }

            if (possibleResult.Count == 0)
                return null;

            int index = Random.NextInt(0, possibleResult.Count - 1);
            
            return possibleResult[index];
        }

        private void AddLava(BlockType[,] types)
        {
            for (int i = 0; i < types.GetLength(0); i++)
            {
                types[i, 0] = BlockType.Bottom;
                types[i, 1] = BlockType.Lava;
            }
        }

        private int NeighboursNumber(BlockType[,] types, int x, int y)
        {
            int result = 0;
            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    if ((i != 0 || j != 0) && types[x + i, y + j] != BlockType.None)
                        result++;
                }
            }

            return result;
        }
    }
}