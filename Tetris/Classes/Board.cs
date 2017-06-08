using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tetris.Classes;

namespace Tetris
{
    public class Board
    {
        public int Width { get; }
        public int Height { get; }

        public bool[,] Map { get; }

        public List<Block> BlockList { get; private set; } = new List<Block>();

        public Board(int width, int height)
        {
            Width = width;
            Height = height + 2;

            Map = new bool[Height, Width];
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    Map[i, j] = false;
                }
            }
        }

        public bool IsEmptyAt(int x, int y)
        {
            if((y < 0) || (x < 0) || (y >= Height) || (x >= Width))
            {
                return false;
            }

            return !Map[y, x];
        }

        public void AddBlock(Block block)
        {
            BlockList.Add(block);
        }

        public void RemoveBlock(Block block)
        {
            BlockList.Remove(block);
        }
    }
}
