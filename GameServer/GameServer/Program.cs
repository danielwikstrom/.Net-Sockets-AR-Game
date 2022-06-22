using System;
using System.Threading;

namespace GameServer
{
    class Program
    {
        public static bool isRunning = false;
        public const int TICKS_PER_SEC = 30;
        public const int MILLISECONDS_PER_TICK = 1000 / TICKS_PER_SEC;
        static void Main(string[] args)
        {
            Console.Title = "Game Server";
            isRunning = true;

            //The server will update on a new thread
            Thread serverThread = new Thread(new ThreadStart(ServerThread));
            serverThread.Start();

            Server.Start(4, 26950);

        }

        /// </summary>
        /// The main thread on which the server is run. It updates TICKS_PER_SEC times every second, which will determines the 
        /// send rate of the server
        /// </summary>
        private static void ServerThread()
        {
            DateTime timeForTick = DateTime.Now;
            while (isRunning)
            {
                while (timeForTick < DateTime.Now)
                {
                    Update();
                    //Determine when the next time a tick will happen 
                    timeForTick = timeForTick.AddMilliseconds(MILLISECONDS_PER_TICK);

                    //sleep the thread until the next sleep to reduce cpu usage
                    if (timeForTick > DateTime.Now)
                    { 
                        Thread.Sleep(timeForTick - DateTime.Now);
                    }
                }
            }
        }

        /// <summary>
        /// This method simulates the Unity Update, and is called a set amount of times per seconds
        /// </summary>
        public static void Update()
        {
            ThreadManager.UpdateMain();
        }
    }
}
