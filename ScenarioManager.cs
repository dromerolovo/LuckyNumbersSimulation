using System.Xml.Serialization;
using System;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using MagicNumbersSimulation.Models;

namespace MagicNumbersSimulation
{
    public class ScenarioManager
    {
        private uint gameRuns;
        private readonly ushort[] scenarios = new ushort[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        private readonly ushort numberCeiling = 80;
        private readonly ushort lotteryUpperLimitResult = 20;
        public readonly float ticketPrize = 0.01f;
        private readonly uint[,] prizeTable = new uint[11, 11];
        private readonly ConfigScenarioManager _configScenarioManager;
        private string subSubDirectory;

        public ScenarioManager(ConfigScenarioManager configScenarioManager)
        {
            //[hits or right guesses, selected numbers count] 
            prizeTable[1, 1] = 3;
            prizeTable[1, 2] = 1;

            prizeTable[2, 2] = 6;
            prizeTable[2, 3] = 3;
            prizeTable[2, 4] = 2;
            prizeTable[2, 5] = 1;

            prizeTable[3, 3] = 25;
            prizeTable[3, 4] = 5;
            prizeTable[3, 5] = 2;
            prizeTable[3, 6] = 1;
            prizeTable[3, 7] = 1;

            prizeTable[4, 4] = 120;
            prizeTable[4, 5] = 10;
            prizeTable[4, 6] = 8;
            prizeTable[4, 7] = 4;
            prizeTable[4, 8] = 2;
            prizeTable[4, 9] = 1;
            prizeTable[4, 10] = 1;

            prizeTable[5, 5] = 380;
            prizeTable[5, 6] = 55;
            prizeTable[5, 7] = 20;
            prizeTable[5, 8] = 10;
            prizeTable[5, 9] = 5;
            prizeTable[5, 10] = 2;

            prizeTable[6, 6] = 2000;
            prizeTable[6, 7] = 150;
            prizeTable[6, 8] = 50;
            prizeTable[6, 9] = 30;
            prizeTable[6, 10] = 20;

            prizeTable[7, 7] = 5000;
            prizeTable[7, 8] = 1000;
            prizeTable[7, 9] = 200;
            prizeTable[7, 10] = 50;

            prizeTable[8, 8] = 20000;
            prizeTable[8, 9] = 4000;
            prizeTable[8, 10] = 500;

            prizeTable[9, 9] = 50000;
            prizeTable[9, 10] = 10000;

            prizeTable[10, 10] = 100000;

            _configScenarioManager = configScenarioManager;
            InitializeValuesAndDirectories();
        }

        private void InitializeValuesAndDirectories()
        {
            string subDirectory;

            if (!Directory.Exists("Results"))
            {
                Directory.CreateDirectory("Results");
            }

            if(_configScenarioManager.SimulationType == SimulationType.RealWorld)
            {
                subDirectory = "RealWorld";
                if (!Directory.Exists($"Results/{subDirectory}"))
                {
                    Directory.CreateDirectory($"Results/{subDirectory}");
                }
            }
            else
            {
                subDirectory = "ByScenarios";
                if (!Directory.Exists($"Results/{subDirectory}"))
                {
                    Directory.CreateDirectory($"Results/{subDirectory}");
                }
            }

            switch (_configScenarioManager.Iterations) {
                case Iterations.M1:
                    subSubDirectory = $"Results/{subDirectory!}/M1";
                    gameRuns = (uint)Iterations.M1;
                    break;
                case Iterations.M2:
                    subSubDirectory = $"Results/{subDirectory}/M2";
                    gameRuns = (uint)Iterations.M2;
                    break;
                case Iterations.M5:
                    subSubDirectory = $"Results/{subDirectory}/M5";
                    gameRuns = (uint)Iterations.M5;
                    break;
                case Iterations.M7:
                    subSubDirectory = $"Results/{subDirectory}/M7";
                    gameRuns = (uint)Iterations.M7;
                    break;
                case Iterations.M10:
                    subSubDirectory = $"Results/{subDirectory}/M10";
                    gameRuns = (uint)Iterations.M10;
                    break;  
            }

            if(!Directory.Exists(subSubDirectory))
            {
                Directory.CreateDirectory(subSubDirectory);
            }
        }

        public class Utils
        {
            internal static bool IsNumberSelected<T>(T[] array, T number)
            {
                IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
                foreach (var arrayElement in array)
                {
                    if (comparer.Equals(arrayElement, number))
                    {
                        return true;
                    }
                }

                return false;
            }

            internal static string ArrayToString(ushort[] array)
            {
                string baseString = "";
                var count = 0;
                foreach (var arrayElement in array)
                {
                    count++;
                    if(count == array.Length)
                    {
                        baseString += $"{arrayElement}";
                    }
                    else
                    {
                        baseString += $"{arrayElement},";
                    }
                    
                }

                return baseString;

            }
        }

        public void Execute()
        {
            uint counter = 0;
            for(ushort i = 1; i < scenarios.Length + 1; i++)
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";"
                };

                using (var writer = new StreamWriter($"{subSubDirectory}/Scenario{i}"))
                using (var csv = new CsvWriter(writer, config))
                {
                    csv.WriteHeader<ResultModel>();
                    csv.NextRecord();
                    for (var y = 0; y < gameRuns; y++)
                    {
                        counter++;
                        ushort[] lotteryResult = CreateLotteryResult();
                        if (y % _configScenarioManager.Cycles == 0)
                        {
                            lotteryResult = CreateLotteryResult();
                        }
                        ushort[] userResult = CreateUserResult(i);
                        var rightGuesses = CalculateRightGuesses(userResult, lotteryResult);
                        var prize = prizeTable[rightGuesses, i] * 0.01;
                        var resultModel = new ResultModel
                        {
                            Id = counter,
                            ScenarioNumber = i,
                            SelectedNumbers = Utils.ArrayToString(userResult),
                            RightGuesses = rightGuesses,
                            Prize = prize,
                        };
                        
                        csv.WriteRecord(resultModel);
                        csv.NextRecord();
                    }
                }

            }

        }

        private ushort[] CreateUserResult(ushort scenarioNumber)
        {
            ushort[] userSelectedNumbers = new ushort[scenarioNumber];
            for (int i = 0; i < userSelectedNumbers.Length; i++)
            {
                Random random = new Random();
                ushort randomNumber = (ushort)random.Next(1, numberCeiling);
                if (Utils.IsNumberSelected<ushort>(userSelectedNumbers, randomNumber))
                {
                    i--;
                }
                else
                {
                    userSelectedNumbers[i] = randomNumber;
                }
            }

            return userSelectedNumbers;
        }

        private ushort[] CreateLotteryResult()
        {
            ushort[] lotterySelectedNumbers = new ushort[lotteryUpperLimitResult];
            for (int i = 0; i < lotterySelectedNumbers.Length; i++)
            {
                Random random = new Random();
                ushort randomNumber = (ushort)random.Next(1, numberCeiling);
                if (Utils.IsNumberSelected<ushort>(lotterySelectedNumbers, randomNumber))
                {
                    i--;
                }
                else
                {
                    lotterySelectedNumbers[i] = randomNumber;
                }
            }

            return lotterySelectedNumbers;
        }
        
        private ushort CalculateRightGuesses(ushort[] userSelectedNumbers, ushort[] lotterySelectedNumbers)
        {
            bool[] set = new bool[80];

            for(ushort i = 0; i < userSelectedNumbers.Length; i++)
            {
                set[userSelectedNumbers[i]] = true;
            }

            ushort rightGuesses = 0;
            for(ushort i = 0; i < lotterySelectedNumbers.Length; i++)
            {
                if(set[lotterySelectedNumbers[i]])
                {
                    rightGuesses++;
                }
            }

            return rightGuesses;
        }
    }
}
