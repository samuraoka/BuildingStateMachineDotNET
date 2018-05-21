using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ConsumerProducer
{
    // How to: Synchronize a Producer and a Consumer Thread (C# Programming Guide)
    // https://msdn.microsoft.com/en-us/library/yy12yx1f(v=vs.90).aspx
    class Program
    {
        static void Main(string[] args)
        {
            var queue = new BlockingCollection<int>();
            var endTokenSource = new CancellationTokenSource();

            Console.WriteLine("Configuring worker threads...");
            // Chnage from original sample:
            // Adding an additional producer to be closer to our scenario
            var producer1 = new Producer(queue, endTokenSource.Token);
            var producer2 = new Producer(queue, endTokenSource.Token);
            var consumer = new Consumer(queue, endTokenSource.Token);
            // Create one consumer and two producers
            var tasks = new[]
            {
                new Task(producer1.ThreadRun),
                new Task(producer2.ThreadRun),
                new Task(consumer.ThreadRun),
            };

            Console.WriteLine("Launching producer and consumer threads...");
            Array.ForEach(tasks, t => t.Start());

            for (int i = 0; i < 4; i += 1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2.5));
                ShowQueueContents(queue);
            }

            Console.WriteLine("Signaling threads to terminate...");
            endTokenSource.Cancel();

            // Join all threads again to end gracefully
            Task.WaitAll(tasks);
        }

        private static void ShowQueueContents(BlockingCollection<int> q)
        {
            foreach (int item in q)
            {
                Console.Write($"{item} ");
            }
            Console.WriteLine();
        }
    }
}
