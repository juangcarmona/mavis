using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MAVIS
{
    public class AzureBlobUploader : IImageUploader
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

        public async Task UploadAsync(string filePath, string relativePath, string cameraName, bool saveHistory = false)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var extension = Path.GetExtension(filePath).ToLower();
                var fileName = $"image_{timestamp}{extension}";
                var cameraFolder = $"{_keyPrefix}/{cameraName}";

                if (saveHistory)
                {
                    var blobClient = containerClient.GetBlobClient($"{cameraFolder}/{fileName}");
                    await using var fileStream = File.OpenRead(filePath);
                    await blobClient.UploadAsync(fileStream, overwrite: true);
                    _logger.LogInformation($"File {filePath} uploaded to {blobClient.Uri}");
                }

                var latestBlobClient = containerClient.GetBlobClient($"{cameraFolder}/latest.jpeg");
                await using (var fileStream = File.OpenRead(filePath))
                    await latestBlobClient.UploadAsync(fileStream, overwrite: true);

                var mimeType = GetMimeType(extension);
                await latestBlobClient.SetHttpHeadersAsync(new Azure.Storage.Blobs.Models.BlobHttpHeaders
                {
                    CacheControl = "no-store, no-cache, must-revalidate",
                    ContentType = mimeType
                });

                _logger.LogInformation($"File {filePath} uploaded as {latestBlobClient.Uri}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file {filePath}");
            }
        }

        private string GetMimeType(string extension)
        {
            var mimeTypes = new Dictionary<string, string>
            {
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" },
                { ".gif", "image/gif" },
                { ".bmp", "image/bmp" },
                { ".webp", "image/webp" },
                { ".tiff", "image/tiff" },
                { ".svg", "image/svg+xml" }
            };

            return mimeTypes.TryGetValue(extension, out var mimeType) ? mimeType : "application/octet-stream";
        }

    }
}
