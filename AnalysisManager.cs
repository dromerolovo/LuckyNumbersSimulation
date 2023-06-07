using CsvHelper;
using System.Globalization;
using MagicNumbersSimulation.Models;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.IO;

namespace MagicNumbersSimulation
{
    public class AnalysisManager
    {
        private readonly ushort scenarios = 10;
        private readonly string[] subDir = { "Analysis/Frequency/1m" };
        private readonly ushort[] prizeLimits = { 0, 0, 1, 1, 1, 2, 2, 3, 3, 3 };
        private readonly ConfigAnalysisManager _configAnalysisManager;
        private string subSubDirectory;
        private string subSubDirectoryWithoutRoot;
        private string analysisTypeSubDirectory;

        public AnalysisManager(ConfigAnalysisManager configAnalysisManager)
        {
            _configAnalysisManager = configAnalysisManager;
            InitializeValuesAndDirectories();
        }

        private void InitializeValuesAndDirectories()
        {
            string subDirectory;

            if (!Directory.Exists("Analysis"))
            {
                Directory.CreateDirectory("Analysis");
            }

            if (_configAnalysisManager.SimulationType == SimulationType.RealWorld)
            {
                subDirectory = "RealWorld";
                if (Directory.Exists($"Analysis/{subDirectory}"))
                {
                    Directory.CreateDirectory($"Analysis/{subDirectory}");
                }
            }
            else
            {
                subDirectory = "ByScenarios";
                if (Directory.Exists($"Analysis/{subDirectory}"))
                {
                    Directory.CreateDirectory($"Analysis/{subDirectory}");
                }
            }

            switch (_configAnalysisManager.Iterations)
            {
                case Iterations.M1:
                    subSubDirectory = $"Analysis/{subDirectory}/M1";
                    subSubDirectoryWithoutRoot = $"{subDirectory}/M1";
                    break;
                case Iterations.M2:
                    subSubDirectory = $"Analysis/{subDirectory}/M2";
                    subSubDirectoryWithoutRoot = $"{subDirectory}/M2";
                    break;
                case Iterations.M5:
                    subSubDirectory = $"Analysis/{subDirectory}/M5";
                    subSubDirectoryWithoutRoot = $"{subDirectory}/M5";
                    break;
                case Iterations.M7:
                    subSubDirectory = $"Analysis/{subDirectory}/M7";
                    subSubDirectoryWithoutRoot = $"{subDirectory}/M7";
                    break;
                case Iterations.M10:
                    subSubDirectory = $"Analysis/{subDirectory}/M10";
                    subSubDirectoryWithoutRoot = $"{subDirectory}/M10";
                    break;
            }

            if (Directory.Exists(subSubDirectory))
            {
                Directory.CreateDirectory(subSubDirectory);
            }
        }

        public void CalculateAndWriteFrequencies()
        {

            if(!Directory.Exists($"{subSubDirectory}/Frequency"))
            {
                Directory.CreateDirectory($"{subSubDirectory}/Frequency");
            }

            for(var i = 0; i < scenarios; i++)
            {

                if (!File.Exists($"Results/{subSubDirectoryWithoutRoot}/Scenario{i + 1}"))
                {
                    throw new Exception("Results not found");
                }

                var total = (uint)_configAnalysisManager.Iterations;
                uint[] count = new uint[11];

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";"
                };

                using (var reader = new StreamReader($"Results/{subSubDirectoryWithoutRoot}/Scenario{i+1}"))
                using (var csv = new CsvReader(reader, config))
                {
                    while (csv.Read())
                    {
                        var record = csv.GetRecord<ResultModel>();
                        var rightGueses = record!.RightGuesses;
                        count[rightGueses] += 1;
                    }
                }

                using (var writer = new StreamWriter($"{subSubDirectory}/Frequency/Scenario{i + 1}"))
                using (var csv = new CsvWriter(writer, config))
                {
                    csv.WriteHeader<FrequencyModel>();
                    csv.NextRecord();
                    
                    for(var y = 0; y < i + 2; y++)
                    {
                        var frequencyModel = new FrequencyModel
                        {
                            Scenario = (ushort)i,
                            Case = (ushort)y,
                            Count = count[y],
                            Frequency = (decimal)count[y] / total,
                        };

                        csv.WriteRecord(frequencyModel);
                        csv.NextRecord();
                    }


                }
            }
        }

        public void CalculateAndWriteBalance()
        {

            if (!Directory.Exists($"{subSubDirectory}/Balance"))
            {
                Directory.CreateDirectory($"{subSubDirectory}/Balance");
            }

            for (var i = 0; i < scenarios; i++)
            {
                if (!File.Exists($"Results/{subSubDirectoryWithoutRoot}/Scenario{i + 1}"))
                {
                    throw new Exception("Results not found");
                }

                double balance = 0;
                double lowestPoint = 0;

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";"
                };

                using (var reader = new StreamReader($"Results/{subSubDirectoryWithoutRoot}/Scenario{i + 1}"))
                using (var csv = new CsvReader(reader, config))
                {
                    while (csv.Read())
                    {
                        var record = csv.GetRecord<ResultModel>();
                        var balanceRecord = record!.Prize;
                        balance += -balanceRecord + 0.01;
                        if(balance < lowestPoint)
                        {
                            lowestPoint = balance;
                        }
                    }
                }


                using (var writer = new StreamWriter($"{subSubDirectory}/Balance/Scenario{i + 1}"))
                using (var csv = new CsvWriter(writer, config))
                {
                    csv.WriteHeader<BalanceModel>();
                    csv.NextRecord();

                    var balanceModel = new BalanceModel
                    {
                        Scenario = (ushort)i,
                        Balance = balance,
                        LowestBalancePoint = lowestPoint,
                    };

                    csv.WriteRecord(balanceModel);
                }
            }
        }
    }
}
