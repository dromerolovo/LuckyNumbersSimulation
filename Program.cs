using MagicNumbersSimulation;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");


AnalysisManager.CreateDirectories();
AnalysisManager.CalculateAndWriteFrequencies();
///
///This command generates the results
//ScenarioManager.Execute();

//app.Run();

