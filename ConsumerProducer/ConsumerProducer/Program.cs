using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ConsumerProducer
{
    // How to: Synchronize a Producer and a Consumer Thread (C# Programming Guide)
    // https://msdn.microsoft.com/en-us/library/yy12yx1f(v=vs.90).aspx
    class Program
    {
        static void Main(string[] args)
        {
            var queue = new Queue<int>();
            var syncEvents = new SyncEvents();

            Console.WriteLine("Configuring worker threads...");
            var producer = new Producer(queue, syncEvents);
            var consumer = new Consumer(queue, syncEvents);
            var producerThread = new Thread(producer.ThreadRun);
            var consumerThread = new Thread(consumer.ThreadRun);

            Console.WriteLine("Launching producer and consumer threads...");
            producerThread.Start();
            consumerThread.Start();

            for (int i = 0; i < 4; i += 1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2.5));
                ShowQueueContents(queue);
            }

            Console.WriteLine("Signaling threads to terminate...");
            syncEvents.ExitThreadEvent.Set();

            producerThread.Join();
            consumerThread.Join();
        }

        private static void ShowQueueContents(Queue<int> q)
        {
            // Remarks
            // https://docs.microsoft.com/en-us/dotnet/api/system.collections.icollection.syncroot?view=netframework-4.7.2#remarks
            lock (((ICollection)q).SyncRoot)
            {
                foreach (int item in q)
                {
                    Console.WriteLine($"{item} ");
                }
            }
            Console.WriteLine();
        }
    }
}
