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

        public async Task UploadFileAsync(string filePath, string relativePath, string cameraName, bool saveHistory = false)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

                // Generate new file name based on timestamp
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var extension = Path.GetExtension(filePath).ToLower(); // Normalize extension to lowercase
                var fileName = $"image_{timestamp}{extension}";

                // Define the folder structure for the camera
                var cameraFolder = $"{_keyPrefix}/{cameraName}";

                if (saveHistory)
                {

                    // Upload the original file
                    var blobClient = containerClient.GetBlobClient($"{cameraFolder}/{fileName}");
                    await using var fileStream = File.OpenRead(filePath);
                    await blobClient.UploadAsync(fileStream, overwrite: true);
                    _logger.LogInformation($"File {filePath} uploaded to {blobClient.Uri}");
                }

                // Upload the file as the latest image for the camera
                var latestBlobClient = containerClient.GetBlobClient($"{cameraFolder}/latest.jpeg");
                await using (var fileStream = File.OpenRead(filePath)) // Ensure a fresh stream
                {
                    await latestBlobClient.UploadAsync(fileStream, overwrite: true);
                }

                // Determine MIME type and set HTTP headers
                var mimeType = GetMimeType(extension);
                await latestBlobClient.SetHttpHeadersAsync(new Azure.Storage.Blobs.Models.BlobHttpHeaders
                {
                    CacheControl = "no-store, no-cache, must-revalidate",
                    ContentType = mimeType
                });

                _logger.LogInformation($"File {filePath} uploaded as {latestBlobClient.Uri} with MIME type: {mimeType}");
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
