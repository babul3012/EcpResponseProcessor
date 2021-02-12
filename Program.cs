using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace ECPResponseProcessor
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Step 1: Reading the response file from Azure blob storage...");

            string connectionString = @"-------Blob storage connection string------";
            string containerName = "----Container name-----";
            string fileName = "------file name from blob-------";

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            BlobDownloadInfo download = await blobClient.DownloadAsync();

            string filePath = "DQTResponse.csv";
            using (FileStream downloadFileStream = File.OpenWrite(filePath))   
            {
                await download.Content.CopyToAsync(downloadFileStream);
                downloadFileStream.Close();
            }

            Console.WriteLine("Step 2: Process and move the data to database");

            using (var reader = new StreamReader(filePath))
            {
                using (var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    var records = csvReader.GetRecords<object>().ToList();
                    IDictionary<string, object> propertyValues = (ExpandoObject)records[0];

                    foreach (var property in propertyValues.Keys)
                    {
                        // Add logic to move the data to database
                        Console.WriteLine(String.Format("{0} : {1}", property, propertyValues[property]));
                    }
                }
            }

            Console.ReadLine();
        }
    }
}
