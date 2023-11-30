namespace VariableBatchSize
{
    class Program
    {
        static void Main(string[] args)
        {
            BatchProcessing.MathExp = 0.5;
            BatchProcessing.MaxDispersion = 0.25;
            BatchProcessing.NumberSimulations = 100000;
            BatchProcessing.PossibleDevition = 1.8;
            BatchProcessing.SetDeviationBorders(1.5, 0.3, 5);

            Simulation simulation = new Simulation(5);
            simulation.Run(2, 5000, 10, 0.85, 0.01, 30);
            simulation.Save(@"E:\НовГУ\2) Магистратура\1 курс\Научная деятельность\Результаты\10) Переменный размер пакета");
        }
    }
}
