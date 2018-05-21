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
            // Chnage from original sample:
            // Adding an additional producer to be closer to our scenario
            var producer1 = new Producer(queue, syncEvents);
            var producer2 = new Producer(queue, syncEvents);
            var consumer = new Consumer(queue, syncEvents);
            var producerThread1 = new Thread(producer1.ThreadRun)
            {
                Name = "Producer1",
            };
            var producerThread2 = new Thread(producer2.ThreadRun)
            {
                Name = "Producer2",
            };
            var consumerThread = new Thread(consumer.ThreadRun)
            {
                Name = "Consumer",
            };

            Console.WriteLine("Launching producer and consumer threads...");
            producerThread1.Start();
            producerThread2.Start();
            consumerThread.Start();

            for (int i = 0; i < 4; i += 1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2.5));
                ShowQueueContents(queue);
            }

            Console.WriteLine("Signaling threads to terminate...");
            syncEvents.ExitThreadEvent.Set();

            // Join all threads again to end gracefully
            producerThread1.Join();
            producerThread2.Join();
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
                    Console.Write($"{item} ");
                }
            }
            Console.WriteLine();
        }
    }
}
