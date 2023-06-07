using MagicNumbersSimulation;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

//var resultsManager = new ScenarioManager(new ConfigScenarioManager());
//resultsManager.Execute();

var analysisManager = new AnalysisManager(new ConfigAnalysisManager());
analysisManager.CalculateAndWriteFrequencies();
//analysisManager.CalculateAndWriteBalance();



///This method generates the results
//ScenarioManager.Execute();

//Run this command first to make sure that all the directories has been created
//AnalysisManager.CreateDirectories();

//Run this method to calculate the frequencies of rightGuesses
//AnalysisManager.CalculateAndWriteFrequencies();



//app.Run();

