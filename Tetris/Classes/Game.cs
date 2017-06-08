using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace Tetris.Classes
{
    public class Game
    {
        public delegate void DrawEventHandler(Block block);

        public event DrawEventHandler DrawEvent;
        public event DrawEventHandler DrawNext;
        public event DrawEventHandler DrawHold;

        public Action<int> ScoreUpdated { get; set; }
        public Action GameOver { get; set; }

        public Controller Controller { get; set; }
        public Board Board { get; set; }

        private int score;
        public int Score
        {
            get
            {
                return score;
            }
            private set
            {
                score = value;
                ScoreUpdated?.Invoke(score);
            }
        }

        

        private bool[,] map;
        private Block currentBlock;
        private Block nextBlock;
        private Block holdBlock;

        private bool canHold = true; 

        private DispatcherTimer processTimer;

        private Random random;
        private double clk = 0;
        //private double clkDest = 0.1;
        private double clkDest = 0.15;

        public bool IsPlaying { get; private set; } = false;
        public bool IsPaused { get; private set; } = false;

        public Game()
        {
            processTimer = new DispatcherTimer();
            processTimer.Interval = TimeSpan.FromMilliseconds(33);
            processTimer.Tick += Process;

            random = new Random((int)DateTime.Now.Ticks);
        }

        private void RemoveBlock(Block block)
        {
            Board.RemoveBlock(block);

            for (int bx = 0; bx < BlockShape.Shapes.GetLength(2); bx++)
            {
                for (int by = 0; by < BlockShape.Shapes.GetLength(1); by++)
                {
                    if (currentBlock.Shape[by, bx] == 1)
                    {
                        int x = Math.Max(Math.Min(block.X + bx, Board.Width - 1), 0);
                        int y = Math.Max(0, Math.Min(Board.Height - 1, block.Y + by));

                        map[y, x] = false;
                    }
                }
            }
        }

        private void AddBlock(Block block)
        {
            Board.AddBlock(currentBlock);

            for (int bx = 0; bx < BlockShape.Shapes.GetLength(2); bx++)
            {
                for (int by = 0; by < BlockShape.Shapes.GetLength(1); by++)
                {
                    if (currentBlock.Shape[by, bx] == 1)
                    {
                        int x = Math.Max(Math.Min(block.X + bx, Board.Width - 1), 0);
                        int y = Math.Max(0, Math.Min(Board.Height - 1, block.Y + by));

                        map[y, x] = true;
                    }
                }
            }
        }

        private Block Rotate()
        {
            Block rotatedBlock = new Block(currentBlock.X, currentBlock.Y, currentBlock.ShapeNumber, currentBlock.Color, (currentBlock.Rotate + 1) % 4);

            for (int bx = 0; bx < BlockShape.Shapes.GetLength(2); bx++)
            {
                for (int by = 0; by < BlockShape.Shapes.GetLength(1); by++)
                {
                    if (rotatedBlock.Shape[by, bx] == 0)
                        continue;

                    int x = rotatedBlock.X + bx;
                    int y = rotatedBlock.Y + by;

                    if (!Board.IsEmptyAt(x, y))
                    {
                        return currentBlock;
                    }
                }
            }

            return rotatedBlock;
        }

        private bool CanMove(int dx, int dy)
        {
            for (int bx = 0; bx < BlockShape.Shapes.GetLength(2); bx++)
            {
                for (int by = 0; by < BlockShape.Shapes.GetLength(1); by++)
                {
                    if (currentBlock.Shape[by, bx] == 0)
                        continue;

                    int x = currentBlock.X + bx + dx;
                    int y = currentBlock.Y + by + dy;

                    if (!Board.IsEmptyAt(x, y))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void Process(object sender, EventArgs args)
        {
            if (IsPaused)
                return;

            map = Board.Map;

            if (Controller.InputKey != null)
            {
                int dx = 0;
                int dy = 0;
                bool doRotate = false;
                bool doInstant = false;
                bool doHold = false;

                switch (Controller.InputKey)
                {
                    case System.Windows.Input.Key.Left:
                        dx = -1;
                        break;
                    case System.Windows.Input.Key.Right:
                        dx = +1;
                        break;
                    case System.Windows.Input.Key.Up:
                        doRotate = true;
                        break;
                    case System.Windows.Input.Key.Down:
                        dy = +1;
                        break;
                    case System.Windows.Input.Key.Space:
                        doInstant = true;
                        clk = 8;
                        break;
                    case System.Windows.Input.Key.H:
                        doHold = true;
                        clk = 0;
                        break;
                    default:
                        return;
                }

                Controller.InputKey = null;

                if (doRotate)
                { // 회전
                    RemoveBlock(currentBlock);
                    currentBlock = Rotate();
                    AddBlock(currentBlock);
                }
                else if (doInstant)
                { // 즉시 내리기
                    RemoveBlock(currentBlock);
                    while (true)
                    {
                        if (CanMove(0, +1))
                            currentBlock.Y += 1;
                        else
                            break;
                    
                    }
                    AddBlock(currentBlock);
                }
                else if (doHold && holdBlock != null)
                { // 홀드 블록 가져오기
                    canHold = false;
                    RemoveBlock(currentBlock);
                    currentBlock = holdBlock;
                    holdBlock = null;
                    AddBlock(currentBlock);

                    DrawHold?.Invoke(holdBlock);
                }
                else if (doHold && canHold)
                { // 현재 블록 홀드
                    canHold = false;
                    holdBlock = CreateBlock();
                    holdBlock.Color = currentBlock.Color;
                    holdBlock.Shape = currentBlock.Shape;
                    holdBlock.ShapeNumber = currentBlock.ShapeNumber;

                    RemoveBlock(currentBlock);
                    currentBlock = NextBlock();
                    AddBlock(currentBlock);

                    DrawHold?.Invoke(holdBlock);
                }
                else
                { // 블록 조작
                    RemoveBlock(currentBlock);
                    if (CanMove(dx, dy))
                    {
                        currentBlock.X += dx;
                        currentBlock.Y += dy;
                    }
                    AddBlock(currentBlock);
                }

                // 화면 그리기
                DrawEvent?.Invoke(null);
            }

            if (clk > clkDest)
            {
                clk = 0;

                // 블록 이전 위치 지움
                RemoveBlock(currentBlock);

                // 블록 아래 이동 가능한지 확인 및
                // 블록 위치 갱신
                if (CanMove(0, +1))
                {
                    currentBlock.Y += 1;
                    AddBlock(currentBlock);
                }
                else
                {
                    // 블록이 바닥에 붙고 새로운 블록 생성
                    Score += 100;
                    canHold = true;

                    AddBlock(currentBlock);
                    Board.RemoveBlock(currentBlock);
                    currentBlock = NextBlock();
                    CheckGameOver();

                    SoundEffecter.Play(Properties.Resources.laydown);

                    ProcessBoom();
                }
            }

            clk += 0.01;
            clkDest -= 0.00002;
            //clkDest = Math.Max(0.016, clkDest);
            clkDest = Math.Max(0.024, clkDest);
            DrawEvent?.Invoke(null);
        }

        

        private void ProcessBoom()
        {
            bool[,] map = Board.Map;
            double combo = 1.0;

            List<int> lines = new List<int>();

            for (int line = 0; line < map.GetLength(0); ++line)
            {
                bool complete = true;
                for (int w = 0; w < map.GetLength(1); ++w)
                {
                    if (!map[line, w])
                    {
                        complete = false;
                        break;
                    }
                }

                if (complete)
                {
                    lines.Add(line);

                    Score += (int)(combo * 1000);
                    combo += 1.2;

                    for (int l = line; l >= 1; --l)
                    {
                        for (int w = 0; w < map.GetLength(1); ++w)
                        {
                            map[l, w] = map[l - 1, w];
                        }
                    }

                    for (int x = 0; x < map.GetLength(1); ++x)
                    {
                        map[0, x] = false;
                    }
                }
            }

            lines.Reverse();
        }

        private Block NextBlock()
        {
            //int SHAPE_LENGTH = Enum.GetNames(typeof(BlockShape.Shape)).Length;
            int SHAPE_LENGTH = BlockShape.Shapes.GetLength(0);
            int blockNumber = random.Next(SHAPE_LENGTH);

            Block retBlock = nextBlock;
            nextBlock = new Block(
                Board.Width / 2 - 1, 0,
                blockNumber,
                Color.FromRgb(
                    (byte)random.Next(256),
                    (byte)random.Next(256),
                    (byte)random.Next(256)
                )
                );

            DrawNext?.Invoke(nextBlock);

            return retBlock;
        }

        private Block CreateBlock()
        {
            int SHAPE_LENGTH = BlockShape.Shapes.GetLength(0);
            int blockNumber = random.Next(SHAPE_LENGTH);

            return new Block(
                Board.Width / 2 - 1, 0,
                blockNumber,
                Color.FromRgb(
                    (byte)random.Next(256),
                    (byte)random.Next(256),
                    (byte)random.Next(256)
                )
                );
        }


        private void CheckGameOver()
        {
            if(!CanMove(0, 0))
            {
                Stop();
                GameOver?.Invoke();
            }
        }

        public void Start()
        {
            IsPlaying = true;

            // 점수 초기화 및 랜덤 블록 생성
            Score = 0;
            currentBlock = CreateBlock();
            nextBlock = CreateBlock();
            DrawNext?.Invoke(nextBlock);

            processTimer.Start();
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }

        public void Stop()
        {
            IsPlaying = false;
            processTimer.Stop();
        }
    }
}
