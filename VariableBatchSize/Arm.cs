using System;

namespace VariableBatchSize
{
    class Arm
    {
        private readonly Random random = new();

        public readonly double Expectation;
        public readonly double Dispersion;

        public Arm(double expectation, double dispersion)
        {
            Expectation = expectation;
            Dispersion = dispersion;
        }

        public int Counter { private set; get; }
        public double Income { private set; get; }
        public double UCB { private set; get; }

        public void Reset()
        {
            Counter = 0;
            Income = 0;
            UCB = 0;
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
        }

        public void SetUCB(int n, double a)
        {
            UCB = Income / Counter + a * Math.Sqrt(Dispersion * Math.Log(n) / Counter);
        }
    }
}
