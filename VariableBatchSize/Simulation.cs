using System;
using System.IO;
using System.Threading;

namespace VariableBatchSize
{
    class Simulation
    {
        private Thread[] threads;
        private BatchProcessing[] bandits;
        private int indexBandit = -1;
        private int countProcessedPoints = -1;
        private int countPoints;

        public readonly int MaxCountThreads;

        public Simulation(int maxCountThreads)
        {
            MaxCountThreads = maxCountThreads;
        }

        private void StartNextThread()
        {
            indexBandit++;

            if (indexBandit < bandits.Length)
                threads[indexBandit].Start();
        }

        private void UpdateProgress()
        {
            countProcessedPoints++;
            Console.Write($"\rВыполнено {countProcessedPoints} / {countPoints} ({countProcessedPoints * 100 / countPoints}%)");
        }

        public void Run(int arms, int horizon, int batchSize, double a, double da, int count)
        {
            bandits = new BatchProcessing[count];
            threads = new Thread[count];

            for (int i = 0; i < bandits.Length; i++)
            {
                bandits[i] = new BatchProcessing(arms, horizon, batchSize, a);

                bandits[i].PointProcessed += UpdateProgress;
                bandits[i].Finished += StartNextThread;

                threads[i] = new Thread(bandits[i].RunSimulation);

                a = Math.Round(a + da, 2);
            }

            countPoints = bandits.Length * BatchProcessing.NumberDeviations;

            int countThreads = Math.Min(MaxCountThreads, bandits.Length);

            while (countThreads-- > 0)
                StartNextThread();

            UpdateProgress();

            foreach (var th in threads)
                th.Join();
        }

        public void Save(string path)
        {
            string time = $"{DateTime.Now:d}_{DateTime.Now.Hour:d2}.{DateTime.Now.Minute:d2}.{DateTime.Now.Second:d2}";

            using StreamWriter writer = new(@$"{path}\d_is_{BatchProcessing.PossibleDevition:f1}_({time}).txt");

            writer.Write("d");

            foreach (var b in bandits)
                writer.Write(" " + b.Parameter);

            writer.WriteLine();

            for (int d = 0; d < BatchProcessing.NumberDeviations; d++)
            {
                writer.Write(BatchProcessing.GetDeviation(d));

                foreach (var b in bandits)
                    writer.Write(" " + b.GetRegrets(d));

                writer.WriteLine();
            }
        }
    }
}
