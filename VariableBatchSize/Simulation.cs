using System;
using System.IO;
using System.Threading;

namespace VariableBatchSize
{
    class Simulation
    {
        private Thread[] threads;
        private Bandit[] bandits;
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

        private bool CheckArraysLength(params Array[] arrays)
        {
            for (int i = 1; i < arrays.Length; i++)
                if (arrays[0].Length != arrays[i].Length)
                    return false;

            return true;
        }

        public void Run(int[] arms, int[] horizon, int[] batchSize, double[] a, double[] possDev)
        {
            if (!CheckArraysLength(arms, horizon, batchSize, a, possDev))
                throw new ArgumentException("The size of the arrays does not match.");

            bandits = new Bandit[arms.Length];
            threads = new Thread[arms.Length];

            for (int i = 0; i < bandits.Length; i++)
            {
                bandits[i] = new Bandit(arms[i], horizon[i], batchSize[i], a[i], possDev[i]);

                bandits[i].PointProcessed += UpdateProgress;
                bandits[i].Finished += StartNextThread;

                threads[i] = new Thread(bandits[i].RunSimulation);
            }

            countPoints = bandits.Length * Bandit.NumberDeviations;

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

            using StreamWriter writer = new(@$"{path}\result_({time}).txt");

            writer.Write("d");

            foreach (var b in bandits)
                writer.Write(" " + b.Parameter);

            writer.WriteLine();

            for (int d = 0; d < Bandit.NumberDeviations; d++)
            {
                writer.Write(Bandit.GetDeviation(d));

                foreach (var b in bandits)
                    writer.Write(" " + b.GetRegrets(d));

                writer.WriteLine();
            }
        }
    }
}
