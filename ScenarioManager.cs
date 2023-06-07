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
    public static class ScenarioManager
    {
        public static readonly int GameRuns = 1000000;
        private static readonly ushort[] Scenarios = new ushort[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        private static readonly ushort NumberCeiling = 80;
        private static readonly ushort LotteryUpperLimitResult = 20;
        public static readonly ushort Iteration = 100;
        public static readonly float TicketPrize = 0.01f;
        private static readonly uint[,] PrizeTable = new uint[11, 11];

        static ScenarioManager()
        {
            //[hits or right guesses, selected numbers count] 
            PrizeTable[1, 1] = 3;
            PrizeTable[1, 2] = 1;

            PrizeTable[2, 2] = 6;
            PrizeTable[2, 3] = 3;
            PrizeTable[2, 4] = 2;
            PrizeTable[2, 5] = 1;

            PrizeTable[3, 3] = 25;
            PrizeTable[3, 4] = 5;
            PrizeTable[3, 5] = 2;
            PrizeTable[3, 6] = 1;
            PrizeTable[3, 7] = 1;

            PrizeTable[4, 4] = 120;
            PrizeTable[4, 5] = 10;
            PrizeTable[4, 6] = 8;
            PrizeTable[4, 7] = 4;
            PrizeTable[4, 8] = 2;
            PrizeTable[4, 9] = 1;
            PrizeTable[4, 10] = 1;

            PrizeTable[5, 5] = 380;
            PrizeTable[5, 6] = 55;
            PrizeTable[5, 7] = 20;
            PrizeTable[5, 8] = 10;
            PrizeTable[5, 9] = 5;
            PrizeTable[5, 10] = 2;

            PrizeTable[6, 6] = 2000;
            PrizeTable[6, 7] = 150;
            PrizeTable[6, 8] = 50;
            PrizeTable[6, 9] = 30;
            PrizeTable[6, 10] = 20;

            PrizeTable[7, 7] = 5000;
            PrizeTable[7, 8] = 1000;
            PrizeTable[7, 9] = 200;
            PrizeTable[7, 10] = 50;

            PrizeTable[8, 8] = 20000;
            PrizeTable[8, 9] = 4000;
            PrizeTable[8, 10] = 500;

            PrizeTable[9, 9] = 50000;
            PrizeTable[9, 10] = 10000;

            PrizeTable[10, 10] = 100000;

            CreateDirectories();
        }

        private static void CreateDirectories()
        {
            if(!Directory.Exists("Results"))
            {
                Directory.CreateDirectory("Results");
            }

            if(!Directory.Exists("Results/1m"))
            {
                Directory.CreateDirectory("Results/1m");
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

        public static void Execute()
        {
            uint counter = 0;
            for(ushort i = 1; i < Scenarios.Length + 1; i++)
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";"
                };

                using (var writer = new StreamWriter($"Results/1m/scenario{i}"))
                using (var csv = new CsvWriter(writer, config))
                {
                    csv.WriteHeader<ResultModel>();
                    csv.NextRecord();
                    for (var y = 0; y < GameRuns; y++)
                    {
                        counter++;
                        ushort[] lotteryResult = CreateLotteryResult();
                        if (y % 100 == 0)
                        {
                            lotteryResult = CreateLotteryResult();
                        }
                        ushort[] userResult = CreateUserResult(i);
                        var rightGuesses = CalculateRightGuesses(userResult, lotteryResult);
                        var prize = PrizeTable[rightGuesses, i] * 0.01;
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

        public static ushort[] CreateUserResult(ushort scenarioNumber)
        {
            ushort[] userSelectedNumbers = new ushort[scenarioNumber];
            for (int i = 0; i < userSelectedNumbers.Length; i++)
            {
                Random random = new Random();
                ushort randomNumber = (ushort)random.Next(1, NumberCeiling);
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

        public static ushort[] CreateLotteryResult()
        {
            ushort[] lotterySelectedNumbers = new ushort[LotteryUpperLimitResult];
            for (int i = 0; i < lotterySelectedNumbers.Length; i++)
            {
                Random random = new Random();
                ushort randomNumber = (ushort)random.Next(1, NumberCeiling);
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
        
        public static ushort CalculateRightGuesses(ushort[] userSelectedNumbers, ushort[] lotterySelectedNumbers)
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

    public class ConfigScenarioManager
    {
        public ushort Iterator { get; set; }
        public float TicketPrize { get; set; } 
    }
}
