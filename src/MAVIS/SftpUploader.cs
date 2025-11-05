using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace MAVIS
{
    public class SftpUploader : IImageUploader
    {
        private readonly ILogger<SftpUploader> _logger;
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _basePath;

        public SftpUploader(IConfiguration configuration, ILogger<SftpUploader> logger)
        {
            _logger = logger;

            var section = configuration.GetSection("SftpStorage");
            _host = section["Host"] ?? throw new ArgumentNullException("SftpStorage:Host");
            _port = int.TryParse(section["Port"], out var p) ? p : 22;
            _username = section["Username"] ?? throw new ArgumentNullException("SftpStorage:Username");
            _password = section["Password"] ?? throw new ArgumentNullException("SftpStorage:Password");
            _basePath = section["BasePath"] ?? "/htdocs/wp-content/uploads/mavis";
        }

        public async Task UploadAsync(string filePath, string relativePath, string cameraName, bool saveHistory = false)
        {
            await Task.Run(() =>
            {
                using var client = new SftpClient(_host, _port, _username, _password);
                try
                {
                    client.Connect();
                    if (!client.IsConnected)
                        throw new InvalidOperationException($"Could not connect to SFTP host {_host}");

                    // Base: /htdocs/wp-content/uploads/mavis/<camera>
                    var cameraFolder = $"{_basePath}/{cameraName}";
                    EnsureDirectories(client, cameraFolder);

                    var extension = Path.GetExtension(filePath).ToLower();
                    var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    var timestampedName = $"{timestamp}{extension}";
                    var timestampedPath = $"{cameraFolder}/{timestampedName}";
                    var latestPath = $"{cameraFolder}/latest{extension}";

                    if (saveHistory)
                    {
                        // Always upload timestamped file
                        using (var fs = File.OpenRead(filePath))
                        {
                            client.UploadFile(fs, timestampedPath, true);
                        }
                        _logger.LogInformation($"[{cameraName}] Uploaded timestamped file: {timestampedPath}");
                    }
                    // Then upload (overwrite) latest.ext
                    using (var fs = File.OpenRead(filePath))
                    {
                        client.UploadFile(fs, latestPath, true);
                    }
                    _logger.LogInformation($"[{cameraName}] Updated latest image: {latestPath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error uploading {filePath} via SFTP");
                    throw;
                }
                finally
                {
                    if (client.IsConnected) client.Disconnect();
                }
            });
        }

        private void EnsureDirectories(SftpClient client, string remotePath)
        {
            var parts = remotePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = "";
            foreach (var part in parts)
            {
                current += "/" + part;
                if (!client.Exists(current))
                {
                    client.CreateDirectory(current);
                    _logger.LogDebug($"Created remote directory: {current}");
                }
            }
        }
    }
}
