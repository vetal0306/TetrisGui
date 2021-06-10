using System;
using System.Threading;
using System.Timers;
using Microsoft.SmallBasic.Library;

namespace Tetris
{
    class Program
    {
        const int TIMER_INTERVAL = 500;
        static System.Timers.Timer timer;

        static private Object _lockObject = new object();

        static Figure currentFigure;

        static Object lockObj = new Object();

        static FigureGenerator factory = new FigureGenerator(Field.Widht / 2, 0);
        static bool gameOver = false;

        static void Main(string[] args)
        {
            DrawerProvider.Drawer.InitField();
            SetTimer();

            currentFigure = factory.GetNewFigure();
            currentFigure.Draw();

            GraphicsWindow.KeyDown += GraphicsWindow_KeyDown;
        }

        private static void GraphicsWindow_KeyDown()
        {
            Monitor.Enter(lockObj);
            var result = HandleKey(currentFigure, GraphicsWindow.LastKey);
            if (GraphicsWindow.LastKey == "Down")
                gameOver = ProcessResult(result, ref currentFigure);
            Monitor.Exit(lockObj);
        }

        private static bool ProcessResult(Result result, ref Figure currentFigure)
        {
            if (result == Result.Heap_Strike || result == Result.Down_Border_Strike)
            {
                Field.AddFigure(currentFigure);
                Field.TryDeleteLines();

                if (currentFigure.IsOnTop())
                {
                    DrawerProvider.Drawer.WriteGameOver();

                    timer.Elapsed -= OnTimedEvent;
                    return true;
                }
                else
                {
                    currentFigure = factory.GetNewFigure();
                    return false;
                }
            }
            else
                return false;
        }



        private static void SetTimer()
        {
            timer = new System.Timers.Timer(TIMER_INTERVAL);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private static void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            Monitor.Enter(lockObj);
            var result = currentFigure.TryMove(Direction.DOWN);
            gameOver = ProcessResult(result, ref currentFigure);
            if (gameOver)
                timer.Stop();
            Monitor.Exit(lockObj);
        }

        private static void Test()
        {
            DrawerProvider.Drawer.DrawPoint(5, 6);
        }

        private static Result HandleKey(Figure f, String key)
        {
            switch (key)
            {
                case "Left":
                    return f.TryMove(Direction.LEFT);
                case "Right":
                    return f.TryMove(Direction.RIGHT);
                case "Down":
                    return f.TryMove(Direction.DOWN);
                case "Space":
                    return f.TryRotate();
            }
            return Result.Success;
        }
    }
}
