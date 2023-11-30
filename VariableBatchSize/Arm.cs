using System;

namespace VariableBatchSize
{
    class Arm
    {
        private readonly Random random = new();

        public readonly int Index;
        public readonly double Expectation;
        public readonly double Dispersion;

        public Arm(int index, double expectation, double dispersion)
        {
            Index = index;
            Expectation = expectation;
            Dispersion = dispersion;
        }

        public int Counter { private set; get; }
        public double Income { private set; get; }
        public double AvgIncome { private set; get; }

        public void Reset()
        {
            Counter = 0;
            Income = 0;
            AvgIncome = 0;
        }

        public void Select(int data, ref int sumData, ref int horizon)
        {
            data = Math.Min(data, horizon);

            horizon -= data;
            sumData += data;
            Counter += data;

            while (data-- > 0)
                if (random.NextDouble() < Expectation)
                    Income++;

            AvgIncome = Income / Counter;
        }
    }
}
