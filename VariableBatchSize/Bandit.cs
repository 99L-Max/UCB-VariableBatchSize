using System;
using System.Linq;

namespace VariableBatchSize
{
    class Bandit
    {
        private readonly Arm[] arms;
        private readonly double sqrtDivDN, sqrtMulDN;

        private double[] regrets;

        private static double mathExp;
        private static double[] deviation;

        public readonly int Horizon;
        public readonly double Parameter;
        public readonly double PossibleDevition;

        public static int NumberSimulations;
        public static double MaxDispersion;

        public delegate void EventUpdateData();
        public event EventUpdateData PointProcessed;
        public event EventUpdateData Finished;

        public Bandit(int arms, int horizon, int batchSize, double parameter, double possibleDevition)
        {
            if (arms * batchSize > horizon)
                throw new ArgumentException("Incorrect parameters");

            this.arms = new Arm[arms];

            Horizon = horizon;
            BatchSize = batchSize;
            Parameter = parameter;
            PossibleDevition = possibleDevition;

            sqrtDivDN = Math.Sqrt(MaxDispersion / horizon);
            sqrtMulDN = Math.Sqrt(MaxDispersion * horizon);
        }

        public int BatchSize { private set; get; }

        public double MaxDeviation { private set; get; }

        public double MaxRegrets { private set; get; } = 0d;

        public static int NumberDeviations => deviation.Length;

        public static double MathExp
        {
            set
            {
                if (value > 1d || value < 0d)
                    throw new ArgumentException("For the Bernoulli distribution expectation of p must be between 0 and 1 inclusive.");

                mathExp = value;
            }
            get => mathExp;
        }

        public static double DeltaDevition { private set; get; }

        public double GetRegrets(int i) => regrets[i];

        public static double GetDeviation(int i) => deviation[i];

        public static void SetDeviation(double dev0, double step, int count)
        {
            deviation = Enumerable.Range(0, count).Select(i => Math.Round(dev0 + i * step, 1)).ToArray();
        }

        public void RunSimulation()
        {
            regrets = new double[deviation.Length];

            Arm[] sortArms;

            double maxIncome;
            double normDevition = 2d * PossibleDevition * sqrtDivDN;

            int horizon, sumCountData, batchSize;

            for (int mainIndex = 0; mainIndex < deviation.Length; mainIndex++)
            {
                if (deviation[mainIndex] == 0d)
                {
                    PointProcessed.Invoke();
                    continue;
                }

                for (int i = 0; i < arms.Length; i++)
                    arms[i] = new Arm(MathExp + (i == 0 ? 1 : -1) * deviation[mainIndex] * sqrtDivDN, MaxDispersion);

                maxIncome = arms.Select(a => a.Expectation).Max() * Horizon;

                for (int num = 0; num < NumberSimulations; num++)
                {
                    sumCountData = 0;
                    horizon = Horizon;
                    batchSize = BatchSize;

                    foreach (var arm in arms)
                    {
                        arm.Reset();
                        arm.Select(batchSize, ref sumCountData, ref horizon);
                    }

                    while (horizon > 0)
                    {
                        foreach (var arm in arms)
                            arm.SetUCB(sumCountData, Parameter);

                        sortArms = arms.OrderByDescending(a => a.UCB).ToArray();

                        if (sortArms[0].UCB - sortArms[1].UCB > normDevition)
                            batchSize <<= 1;

                        sortArms[0].Select(batchSize, ref sumCountData, ref horizon);
                    }

                    regrets[mainIndex] += maxIncome - arms.Select(a => a.Income).Sum();
                }

                regrets[mainIndex] /= NumberSimulations * sqrtMulDN;

                if (MaxRegrets < regrets[mainIndex])
                {
                    MaxRegrets = regrets[mainIndex];
                    MaxDeviation = deviation[mainIndex];
                }

                PointProcessed.Invoke();
            }

            Finished.Invoke();
        }
    }
}
