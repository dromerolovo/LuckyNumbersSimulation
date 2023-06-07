namespace MagicNumbersSimulation
{
    public class ConfigScenarioManager
    {
        public ushort Cycles { get; set; } = 100;
        public float TicketPrize { get; set; } = 0.01f;
        public SimulationType SimulationType { get; set; } = SimulationType.ByScenarios;
        public Iterations Iterations { get; set; } = Iterations.M1;
    }

    public class ConfigAnalysisManager
    {
        public SimulationType SimulationType { get; set; } = SimulationType.ByScenarios;
        public Iterations Iterations { get; set; } = Iterations.M1;
    }

    public enum AnalysisType { Frequency, Balance }
    public enum SimulationType { ByScenarios,  RealWorld}
    public enum Iterations 
    { 
        M1 = 1000000, 
        M2 = 2000000, 
        M5 = 5000000, 
        M7 = 7000000, 
        M10 = 10000000
    }
}
