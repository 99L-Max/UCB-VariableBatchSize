using System;

namespace VariableBatchSize
{
    class Program
    {
        static void Main(string[] args)
        {
            int horizon = 5000;
            double dispersion = 0.25d;

            BatchProcessing.MathExp = 0.5;
            BatchProcessing.MaxDispersion = dispersion;
            BatchProcessing.NumberSimulations = 100000;
            BatchProcessing.SetDeviationBorders(1.5, 0.3, 5);
            BatchProcessing.PossibleDevition = 1.8 * Math.Sqrt(dispersion / horizon);

            Simulation simulation = new Simulation(5);
            simulation.Run(2, horizon, 10, 0.85, 0.01, 30);
            simulation.Save(@"E:\НовГУ\2) Магистратура\1 курс\Научная деятельность\Результаты\10) Переменный размер пакета");
        }
    }
}
