using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using MasterMemory;
using UnityEditor;
using UnityEngine;

namespace SmbcApp.LearnGame.Data.Editor
{
    internal static class MasterDataBinaryGenerator
    {
        private const string StockDataCsvPath = "Assets/Projects/MasterData/stock_data.csv";

        [MenuItem("Tools/Master Data/Generate Binary")]
        private static void GenerateBinary()
        {
            Debug.Log("Generating Binary");
            SaveBinary().Forget();
        }

        private static async UniTask SaveBinary()
        {
            var (stocks, organizations) = await LoadStockData();
            var builder = new DatabaseBuilder();

            builder.Append(stocks);
            builder.Append(organizations);

            await using var stream = new FileStream(Path.Combine(MasterData.BinaryDirectory, MasterData.BinaryFileName),
                FileMode.Create);
            builder.WriteToStream(stream);
            Debug.Log("Binary generated and saved");
        }

        private static async UniTask<(List<StockData> Stocks, List<OrganizationData> Organizations)> LoadStockData()
        {
            var stocks = new List<StockData>();
            var organizations = new List<OrganizationData>();

            await using var fileStream = new FileStream(Path.Combine(StockDataCsvPath), FileMode.Open);
            using var reader = new StreamReader(fileStream);

            var header = await reader.ReadLineAsync();
            foreach (var (orgName, i) in header.Split(",")[1..].Select((org, i) => (org, i)))
                organizations.Add(new OrganizationData
                {
                    Id = i,
                    Name = orgName
                });

            while (reader.Peek() > -1)
            {
                var line = await reader.ReadLineAsync();
                var data = line.Split(",");

                if (!DateTime.TryParse(data[0], out var date))
                    throw new Exception($"Invalid date format: {data[0]}");
                foreach (var org in organizations)
                {
                    if (!int.TryParse(data[org.Id + 1], out var stockPrice))
                        throw new Exception("Invalid stock price format");

                    stocks.Add(new StockData
                    {
                        Date = date,
                        StockPrice = stockPrice,
                        OrganizationId = org.Id,
                        OrganizationName = org.Name
                    });
                }
            }

            return (stocks, organizations);
        }
    }
}