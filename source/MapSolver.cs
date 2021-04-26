using System.Collections.Generic;
using System.Linq;
using Iron;

namespace IronCustom
{
    public partial class Block
    {
        public float UnhandledMass = 0;
        public Map.Figure Figure = null;
        public bool InQueue = false;
        public bool Passed = false;
        public bool Handled = false;
        public bool CanEnterQueueAgain = true;
        
        public bool Locker = false;
        
        public float PressureStrengthMargin = 0;
        public float TopMountMargin = 0;
        public float LeftMountMargin = 0;
        public float RightMountMargin = 0;

        public float MountMargin => TopMountMargin + LeftMountMargin + RightMountMargin;
    }
    
    public partial class Map
    {
        public delegate void MapSimulated(List<Figure> figures);
        public event MapSimulated OnMapSimulated;
        
        private const int SIMULATION_STEPS = 1;
        
        private void Simulate()
        {
            Clear();

            List<Figure> figures = GetFigures();
            foreach (Figure figure in figures)
            {
                for (int i = 0; i < SIMULATION_STEPS; i++)
                    SimulateFigure(figure);
            }
            
            OnMapSimulated?.Invoke(figures);
        }

        private void Clear()
        {
            for (int j = 0; j < blocks.GetLength(1); j++)
            {
                for (int i = 0; i < blocks.GetLength(0); i++)
                {
                    Block block = blocks[i, j];
                    if (block == null) continue;
                    
                    block.Figure = null;
                    block.Passed = false;
                    block.Handled = false;
                    block.InQueue = false;
                    
                    block.UnhandledMass = block.Stats.Mass;
                    block.TopMountMargin = block.Stats.MountStrength;
                    block.LeftMountMargin = block.Stats.MountStrength;
                    block.RightMountMargin = block.Stats.MountStrength;
                    block.PressureStrengthMargin = block.Stats.PressureStrength;
                }
            }
        }

        private List<Figure> GetFigures()
        {
            List<Figure> figures = new List<Figure>();
            
            for (int j = blocks.GetLength(1) - 1; j >= 0; j--)
            {
                for (int i = 0; i < blocks.GetLength(0); i++)
                {
                    Block block = blocks[i, j];
                    if (block == null || block.Stats.IsStatic) continue;

                    if (block.Figure == null)
                    {
                        Figure figure = new Figure();
                        figure.Blocks.Add(block);
                        if (BlockIsMount(block))
                            figure.Mounts.Add(block);
                        
                        figures.Add(figure);
                        block.Figure = figure;
                    }
                    
                    AddAllNeighboursToFigure(block);
                }
            }

            //Log.LogInfo($"Found {figures.Count} figures: {string.Join(", ", figures)}");
            return figures;
        }

        private bool BlockIsMount(Block block)
        {
            return TryGetTop(block, out Block topBlock) && topBlock != null && topBlock.Stats.Attachable
                || TryGetBottom(block, out Block bottomBlock) && bottomBlock != null && bottomBlock.Stats.Attachable
                || TryGetLeft(block, out Block leftBlock) && leftBlock != null && leftBlock.Stats.Attachable
                || TryGetRight(block, out Block rightBlock) && rightBlock != null && rightBlock.Stats.Attachable;
        }

        private void AddAllNeighboursToFigure(Block block)
        {
            if (TryGetBottom(block, out Block bottomBlock) && bottomBlock != null && !bottomBlock.Stats.IsStatic && bottomBlock.Figure == null)
            {
                bottomBlock.Figure = block.Figure;
                block.Figure.Blocks.Add(bottomBlock);
                if (BlockIsMount(bottomBlock))
                    block.Figure.Mounts.Add(bottomBlock);
                
                AddAllNeighboursToFigure(bottomBlock);
            }
            if (TryGetTop(block, out Block topBlock) && topBlock != null && !topBlock.Stats.IsStatic && topBlock.Figure == null)
            {
                topBlock.Figure = block.Figure;
                block.Figure.Blocks.Add(topBlock);
                if (BlockIsMount(topBlock))
                    block.Figure.Mounts.Add(topBlock);
                
                AddAllNeighboursToFigure(topBlock);
            }
            if (TryGetLeft(block, out Block leftBlock) && leftBlock != null && !leftBlock.Stats.IsStatic && leftBlock.Figure == null)
            {
                leftBlock.Figure = block.Figure;
                block.Figure.Blocks.Add(leftBlock);
                if (BlockIsMount(leftBlock))
                    block.Figure.Mounts.Add(leftBlock);
                
                AddAllNeighboursToFigure(leftBlock);
            }
            if (TryGetRight(block, out Block rightBlock) && rightBlock != null && !rightBlock.Stats.IsStatic && rightBlock.Figure == null)
            {
                rightBlock.Figure = block.Figure;
                block.Figure.Blocks.Add(rightBlock);
                if (BlockIsMount(rightBlock))
                    block.Figure.Mounts.Add(rightBlock);
                
                AddAllNeighboursToFigure(rightBlock);
            }
        }

        private void SimulateFigure(Figure figure)
        {
            //Log.LogInfo("=========== Simulate " + figure);
            
            Queue<Block> openList = new Queue<Block>(figure.Mounts);
            foreach (Block initialBlock in openList)
            {
                initialBlock.InQueue = true;
            }

            while (openList.Any())
            {
                Block block = openList.Dequeue();
                block.InQueue = false;
                block.Passed = true;
            
                if (TryGetBottom(block, out Block bottomBlock) && bottomBlock != null)
                {
                    if (!Math.Approximately(block.UnhandledMass, 0))
                    {
                        if (bottomBlock.Stats.IsStatic)
                        {
                            if (bottomBlock.Stats.Attachable)
                            {
                                block.UnhandledMass = 0;
                                block.PressureStrengthMargin = block.Stats.PressureStrength;
                            }
                        }
                        else if (bottomBlock.Passed)
                        {
                            if (bottomBlock.Handled && !Math.Approximately(bottomBlock.PressureStrengthMargin, 0))
                            {
                                float handled = PassUnhandledMass(block, bottomBlock, block.UnhandledMass);
                                block.UnhandledMass -= handled;
                            }
                        }
                        else if (bottomBlock.InQueue)
                        {
                            // Skip, this block will then use our data
                        }
                    }
                    if (!bottomBlock.Stats.IsStatic && !bottomBlock.Passed && !bottomBlock.InQueue)
                    {
                        openList.Enqueue(bottomBlock);
                        bottomBlock.InQueue = true;
                    }
                }
            
                if (TryGetTop(block, out Block topBlock) && topBlock != null)
                {
                    if (!Math.Approximately(block.UnhandledMass, 0))
                    {
                        if (topBlock.Stats.IsStatic)
                        {
                            if (topBlock.Stats.Attachable)
                            {
                                float handled = Math.Min(block.TopMountMargin, block.UnhandledMass);
                                block.UnhandledMass -= handled;
                                block.TopMountMargin -= handled;
                            }
                        }
                        else if (topBlock.Passed)
                        {
                            if (topBlock.Handled && !Math.Approximately(topBlock.MountMargin, 0))
                            {
                                float handled = PassUnhandledMass(block, topBlock, Math.Min(block.TopMountMargin, block.UnhandledMass));
                                block.UnhandledMass -= handled;
                                block.TopMountMargin -= handled;
                            }
                        }
                        else if (topBlock.InQueue)
                        {
                            // Skip, this block will then use our data
                        }
                    }
                    if (!topBlock.Stats.IsStatic && !topBlock.Passed && !topBlock.InQueue)
                    {
                        openList.Enqueue(topBlock);
                        topBlock.InQueue = true;
                    }
                }
            
                if (TryGetLeft(block, out Block leftBlock) && leftBlock != null)
                {
                    if (!Math.Approximately(block.UnhandledMass, 0))
                    {
                        if (leftBlock.Stats.IsStatic)
                        {
                            if (leftBlock.Stats.Attachable)
                            {
                                float handled = Math.Min(block.LeftMountMargin, block.UnhandledMass);
                                block.UnhandledMass -= handled;
                                block.LeftMountMargin -= handled;
                            }
                        }
                        else if (leftBlock.Passed)
                        {
                            if (leftBlock.Handled && !Math.Approximately(leftBlock.MountMargin, 0))
                            {
                                float handled = PassUnhandledMass(block, leftBlock, Math.Min(block.LeftMountMargin, block.UnhandledMass));
                                block.UnhandledMass -= handled;
                                block.LeftMountMargin -= handled;
                                leftBlock.RightMountMargin -= handled;
                            }
                        }
                        else if (leftBlock.InQueue)
                        {
                            // Skip, this block will then use our data
                        }
                    }
                    if (!leftBlock.Stats.IsStatic && !leftBlock.Passed && !leftBlock.InQueue)
                    {
                        openList.Enqueue(leftBlock);
                        leftBlock.InQueue = true;
                    }
                }
            
                if (TryGetRight(block, out Block rightBlock) && rightBlock != null)
                {
                    if (!Math.Approximately(block.UnhandledMass, 0))
                    {
                        if (rightBlock.Stats.IsStatic)
                        {
                            if (rightBlock.Stats.Attachable)
                            {
                                float handled = Math.Min(block.RightMountMargin, block.UnhandledMass);
                                block.UnhandledMass -= handled;
                                block.RightMountMargin -= handled;
                            }
                        }
                        else if (rightBlock.Passed)
                        {
                            if (rightBlock.Handled && !Math.Approximately(rightBlock.MountMargin, 0))
                            {
                                float handled = PassUnhandledMass(block, rightBlock, Math.Min(block.RightMountMargin, block.UnhandledMass));
                                block.UnhandledMass -= handled;
                                block.RightMountMargin -= handled;
                                rightBlock.LeftMountMargin -= handled;
                            }
                        }
                        else if (rightBlock.InQueue)
                        {
                            // Skip, this block will then use our data
                        }
                    }
                    if (!rightBlock.Stats.IsStatic && !rightBlock.Passed && !rightBlock.InQueue)
                    {
                        openList.Enqueue(rightBlock);
                        rightBlock.InQueue = true;
                    }
                }
                
                if (!Math.Approximately(block.UnhandledMass, 0))
                {
                    if (block.CanEnterQueueAgain)
                    {
                        openList.Enqueue(block);
                        block.CanEnterQueueAgain = false;
                        block.InQueue = true;
                        block.Passed = false;
                    }
                    else
                    {
                        figure.UnhandledBlocks.Add(block);
                        block.Handled = false;
                    }
                }
                else
                {
                    block.Handled = true;

                    // If any block got handled - give all others in queue second chance to get handled too
                    foreach (Block queuedBlock in openList)
                    {
                        queuedBlock.CanEnterQueueAgain = true;
                    }
                }
            }
            
            //Log.LogInfo("=========== End simulate " + figure);
        }

        private float PassUnhandledMass(Block fromBlock, Block toBlock, float mass)
        {
            fromBlock.Locker = true;
            float passed = PassUnhandledMassInner(toBlock, Math.Min(mass, toBlock.PressureStrengthMargin));
            fromBlock.Locker = false;
            if (!toBlock.Stats.IsStatic)
                toBlock.PressureStrengthMargin -= passed;

            return passed;
        }

        private float PassUnhandledMassInner(Block block, float mass)
        {
            if (Math.Approximately(mass, 0))
                return 0;

            if (block.Locker)
                return 0;

            if (block.Stats.IsStatic)
                return block.Stats.Attachable ? mass : 0;

            float massLeft = mass;
            if (!Math.Approximately(block.PressureStrengthMargin, 0) && TryGetBottom(block, out Block bottomBlock)
                && bottomBlock != null && (bottomBlock.Handled || bottomBlock.Stats.IsStatic))
            {
                float passed = PassUnhandledMass(block, bottomBlock, massLeft);
                massLeft -= passed;
            }
            if (!Math.Approximately(block.TopMountMargin, 0) && TryGetTop(block, out Block topBlock)
                 && topBlock != null && (topBlock.Handled || topBlock.Stats.IsStatic))
            {
                block.Locker = true;
                float passed = PassUnhandledMass(block, topBlock, Math.Min(massLeft, block.TopMountMargin));
                block.Locker = false;
                block.TopMountMargin -= passed;
                massLeft -= passed;
            }
            if (!Math.Approximately(block.LeftMountMargin, 0) && TryGetLeft(block, out Block leftBlock)
                && leftBlock != null && (leftBlock.Handled || leftBlock.Stats.IsStatic))
            {
                block.Locker = true;
                float passed = PassUnhandledMass(block, leftBlock, Math.Min(massLeft, block.LeftMountMargin));
                block.Locker = false;
                block.LeftMountMargin -= passed;
                leftBlock.RightMountMargin -= passed;
                massLeft -= passed;
            }
            if (!Math.Approximately(block.RightMountMargin, 0) && TryGetRight(block, out Block rightBlock)
                && rightBlock != null && (rightBlock.Handled || rightBlock.Stats.IsStatic))
            {
                block.Locker = true;
                float passed = PassUnhandledMass(block, rightBlock, Math.Min(massLeft, block.RightMountMargin));
                block.Locker = false;
                block.RightMountMargin -= passed;
                rightBlock.LeftMountMargin -= passed;
                massLeft -= passed;
            }

            return mass - massLeft;
        }
        
        public class Figure
        {
            public List<Block> Blocks = new List<Block>();
            public List<Block> Mounts = new List<Block>();
            
            public List<Block> UnhandledBlocks = new List<Block>();

            public bool FreeFall => !Mounts.Any();

            public override string ToString()
            {
                return $"Figure with {Blocks.Count} blocks and {Mounts.Count} mounts";
            }
        }
    }
}