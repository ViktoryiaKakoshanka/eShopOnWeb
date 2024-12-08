using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

public static class UploadOrderRequest
{
    [FunctionName("UploadOrderRequest")]
    public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
    {
        // Parse request body
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic orderDetails = JsonConvert.DeserializeObject(requestBody);

        if (orderDetails == null || orderDetails[0].ItemId == null || orderDetails[0].Quantity == null)
        {
            return new BadRequestObjectResult("Invalid order details.");
        }

        // Generate a unique filename
        string fileName = $"order-{Guid.NewGuid()}.json";

        // Save to Azure Blob Storage
        string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("orders");

        // Ensure container exists
        await containerClient.CreateIfNotExistsAsync();

        // Upload the JSON file
        BlobClient blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(new BinaryData(requestBody), true);

        return new OkObjectResult($"Order uploaded successfully: {fileName}");
    }
}
