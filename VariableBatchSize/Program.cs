namespace VariableBatchSize
{
    class Program
    {
        static void Main(string[] args)
        {
            BatchProcessing.MathExp = 0.5;
            BatchProcessing.MaxDispersion = 0.25d;
            BatchProcessing.NumberSimulations = 100000;
            BatchProcessing.SetDeviation(1.5, 0.3, 5);
            BatchProcessing.PossibleDevition = 10.5;

            Simulation simulation = new Simulation(6);
            simulation.Run(2, 5000, 10, 0.35, 0.01, 30);
            simulation.Save(@"E:\НовГУ\2) Магистратура\1 курс\Научная деятельность\Результаты\10) Переменный размер пакета");
        }
    }
}
