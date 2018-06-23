using System;
using System.Threading;

namespace EventNullEx
{
    public delegate void Delegate(int i);

    public class EventWrapper
    {
        public event Delegate Subscriptions;

        public void CallSubscriptions(int i)
        {
            // This check can pass
            if (Subscriptions != null)
            {
                // When this thread is stoped by scheduler - another one can erase Subscribers

                // This line helps to reproduce the issue
                Thread.Sleep(100);

                Subscriptions(i);
            }
        }

    }

    class Program
    {
        private static Random rnd = new Random();
        private const int ThreadsCount = 10;

        private static void Method(int i) { Console.WriteLine(i); }

        static void Main(string[] args)
        {
            var ew = new EventWrapper();

            for (int i = 0; i < ThreadsCount; i++)
            {
                new Thread(() =>
                {
                    int j = i;
                    while (true)
                    {
                        // BTW, try 'i' here, explore capture issue
                        ew.CallSubscriptions(j);
                        ThreadSleep();
                    }
                }).Start();
            }

            while (true)
            {
                ew.Subscriptions += Method;
                ThreadSleep();
                ew.Subscriptions -= Method;
                ThreadSleep();
            }
        }

        private static void ThreadSleep() => Thread.Sleep(rnd.Next(20, 100));
    }
}
