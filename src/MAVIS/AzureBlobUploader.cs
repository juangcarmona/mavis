using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MAVIS
{
    public class AzureBlobUploader
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly string _keyPrefix;
        private readonly ILogger<AzureBlobUploader> _logger;

        public AzureBlobUploader(IConfiguration configuration, ILogger<AzureBlobUploader> logger)
        {
            _logger = logger;

            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            _containerName = configuration["AzureBlobStorage:ContainerName"];
            _keyPrefix = configuration["AzureBlobStorage:MavisKey"];

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task UploadFileAsync(string filePath, string relativePath)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient($"{_keyPrefix}/{relativePath}");

                await using var fileStream = File.OpenRead(filePath);
                await blobClient.UploadAsync(fileStream, overwrite: true);

                _logger.LogInformation($"File {filePath} uploaded to {blobClient.Uri}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file {filePath}");
            }
        }
    }
}
