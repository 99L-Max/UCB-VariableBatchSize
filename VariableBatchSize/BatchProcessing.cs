using System;
using System.Linq;

namespace VariableBatchSize
{
    class BatchProcessing
    {
        private readonly Arm[] arms;
        private readonly double[] ucb;
        private readonly double sqrtDivDN, sqrtMulDN;

        private int sumCountData;
        private double[] regrets;

        private static double mathExp;
        private static double[] deviation;

        public readonly int Horizon;
        public readonly double Parameter;

        public static int NumberSimulations;
        public static double MaxDispersion;
        public static double PossibleDevition;

        public delegate void EventUpdateData();
        public event EventUpdateData PointProcessed;
        public event EventUpdateData Finished;

        public BatchProcessing(int arms, int horizon, int batchSize, double parameter)
        {
            if (arms * batchSize > horizon)
                throw new ArgumentException("Incorrect parameters");

            this.arms = new Arm[arms];
            ucb = new double[arms];

            Horizon = horizon;
            BatchSize = batchSize;
            Parameter = parameter;

            sqrtDivDN = Math.Sqrt(MaxDispersion / horizon);
            sqrtMulDN = Math.Sqrt(MaxDispersion * horizon);
        }

        public int BatchSize { private set; get; }

        public double MaxDeviation { protected set; get; }

        public double MaxRegrets { protected set; get; } = 0d;

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

        public static void SetDeviationBorders(double d0, double dd, int count)
        {
            deviation = new double[count];
            DeltaDevition = dd;
            deviation[0] = d0;

            for (int i = 1; i < count; i++)
                deviation[i] = Math.Round(deviation[i - 1] + dd, 1);
        }

        private bool CheckDeviation(ref int index)
        {
            Arm[] arr = arms.OrderByDescending(a => a.AvgIncome).ToArray();
            index = arr[0].Index;
            return arr[0].AvgIncome - arr[1].AvgIncome > PossibleDevition;
        }

        private void UCB(ref int indexBestArm)
        {
            double maxUCB = double.MinValue;

            for (int i = 0; i < arms.Length; i++)
            {
                ucb[i] = arms[i].AvgIncome + Parameter * Math.Sqrt(MaxDispersion * Math.Log(sumCountData) / arms[i].Counter);

                if (maxUCB < ucb[i])
                {
                    maxUCB = ucb[i];
                    indexBestArm = i;
                }
            }
        }

        public void RunSimulation()
        {
            regrets = new double[deviation.Length];
            double maxIncome;
            int horizon;
            int indexBestArm = 0;

            for (int mainIndex = 0; mainIndex < deviation.Length; mainIndex++)
            {
                if (deviation[mainIndex] == 0d)
                {
                    PointProcessed.Invoke();
                    continue;
                }

                for (int i = 0; i < arms.Length; i++)
                    arms[i] = new Arm(i, MathExp + (i == 0 ? deviation[mainIndex] : -deviation[mainIndex]) * sqrtDivDN, MaxDeviation);

                maxIncome = arms.Select(a => a.Expectation).Max() * Horizon;

                for (int num = 0; num < NumberSimulations; num++)
                {
                    sumCountData = 0;
                    horizon = Horizon;

                    foreach (var arm in arms)
                    {
                        arm.Reset();
                        arm.Select(BatchSize, ref sumCountData, ref horizon);
                    }

                    while (horizon > 0)
                    {
                        if (CheckDeviation(ref indexBestArm))
                            BatchSize <<= 1;
                        else
                            UCB(ref indexBestArm);

                        arms[indexBestArm].Select(BatchSize, ref sumCountData, ref horizon);
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
