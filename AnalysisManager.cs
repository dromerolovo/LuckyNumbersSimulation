using CsvHelper;
using System.Globalization;
using MagicNumbersSimulation.Models;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace MagicNumbersSimulation
{
    public static class AnalysisManager
    {
        private static readonly ushort scenarios = 10;
        private static readonly string[] subDir = { "Analysis/1m" };
        private static readonly ushort[] prizeLimits = { 0, 0, 1, 1, 1, 2, 2, 3, 3, 3 };
        
        static AnalysisManager()
        {
            CreateDirectories();
        }
        private static void CreateDirectories()
        {
            string mainDir = "Analysis";
            if(!Directory.Exists(mainDir))
            {
                Directory.CreateDirectory(mainDir);
            }

            for (var i = 0; i < subDir.Length; i++)
            {
                if (!Directory.Exists(subDir[i]))
                {
                    Directory.CreateDirectory($"{subDir[i]}");
                }
            }
        }

        public static void CalculateAndWriteFrequencies()
        {
            for(var i = 0; i < scenarios; i++)
            {
                var total = 1000000;
                uint[] count = new uint[11];

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";"
                };

                using (var reader = new StreamReader($"Results/1m/scenario{i+1}"))
                using (var csv = new CsvReader(reader, config))
                {
                    while (csv.Read())
                    {
                        var record = csv.GetRecord<ResultModel>();
                        var rightGueses = record!.RightGuesses;
                        count[rightGueses] += 1;
                    }
                }

                using (var writer = new StreamWriter($"Analysis/1m/scenario{i + 1}"))
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


    }
}
