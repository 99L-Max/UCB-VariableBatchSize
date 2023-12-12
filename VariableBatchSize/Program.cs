using System;
using System.Linq;

namespace VariableBatchSize
{
    class Program
    {
        static void Main(string[] args)
        {
            Bandit.MathExp = 0.5d;
            Bandit.MaxDispersion = 0.25d;
            Bandit.NumberSimulations = 100000;
            Bandit.SetDeviation(1.5d, 0.3d, 5);

            double a0 = 0.65d;
            double da = 0.01d;
            int count = 30;

            int[] arms = Enumerable.Repeat(2, count).ToArray();
            int[] horizon = Enumerable.Repeat(5000, count).ToArray();
            int[] batchSize = Enumerable.Repeat(10, count).ToArray();
            double[] a = Enumerable.Range(0, count).Select(i => Math.Round(a0 + i * da, 2)).ToArray();
            double[] possDev = Enumerable.Repeat(1.8d, count).ToArray();

            Simulation simulation = new Simulation(6);
            simulation.Run(arms, horizon, batchSize, a, possDev);
            simulation.Save(@"E:\НовГУ\2) Магистратура\1 курс\Научная деятельность\Результаты\10) Переменный размер пакета");
        }
    }
}
