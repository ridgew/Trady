using System;
using System.Threading;

namespace MT4ZmqSingal
{
    public class ThreadTask
    {
        Thread myThread = null;

        public ThreadTask(string threadName, Action action)
        {
            myThread = new Thread(new ThreadStart(action));
            myThread.Name = threadName;
            myThread.IsBackground = true;
        }

        public void Start()
        {
            myThread.Start();
        }

    }
}
