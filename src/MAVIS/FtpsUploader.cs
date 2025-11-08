using System;
using System.IO;
using System.Threading.Tasks;
using FluentFTP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MAVIS;

public sealed class FtpsUploader : IImageUploader
{
    private readonly ILogger<FtpsUploader> _logger;
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _basePath;
    private readonly FtpEncryptionMode _encryption;
    private readonly bool _passive;
    private readonly bool _validateAnyCert;
    private readonly bool _forceJpegName;

    public FtpsUploader(IConfiguration config, ILogger<FtpsUploader> logger)
    {
        _logger = logger;

        var s = config.GetSection("FtpsStorage");
        _host = s["Host"] ?? throw new ArgumentNullException("FtpsStorage:Host");
        _port = int.TryParse(s["Port"], out var p) ? p : 21;
        _username = s["Username"] ?? throw new ArgumentNullException("FtpsStorage:Username");
        _password = s["Password"] ?? throw new ArgumentNullException("FtpsStorage:Password");
        _basePath = (s["BasePath"] ?? "/public_html/wp-content/uploads/mavis").TrimEnd('/');

        var mode = (s["Mode"] ?? "Explicit").Trim().ToLowerInvariant();
        _encryption = mode switch
        {
            "implicit" => FtpEncryptionMode.Implicit,
            _ => FtpEncryptionMode.Explicit
        };

        _passive = bool.TryParse(s["Passive"], out var pasv) ? pasv : true;
        _validateAnyCert = bool.TryParse(s["ValidateAnyCertificate"], out var vac) ? vac : true;
        _forceJpegName = bool.TryParse(s["ForceJpegName"], out var fj) ? fj : true;
    }

    public async Task UploadAsync(string filePath, string relativePath, string cameraName, bool saveHistory = false)
    {
        // Client setup
        await using var client = new AsyncFtpClient(_host, _username, _password, _port)
        {
            Config =
            {
                EncryptionMode = _encryption,                                  // Explicit TLS on 21 (or Implicit on 990)
                DataConnectionType = _passive ? FtpDataConnectionType.PASV : FtpDataConnectionType.PORT,
                ValidateAnyCertificate = _validateAnyCert,                      // testing only
                UploadDataType = FtpDataType.Binary,                            // images: always binary
                DownloadDataType = FtpDataType.Binary
            }
        };

        // Remote layout: /public_html/wp-content/uploads/mavis/<camera>/(latest.jpg + optional history)
        var cameraFolder = $"{_basePath}/{cameraName}";
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        // Name “latest.jpg” as required by WordPress pattern (no format conversion performed)
        var latestName = _forceJpegName ? "latest.jpg" : $"latest{Path.GetExtension(filePath).ToLower()}";
        var latestRemotePath = $"{cameraFolder}/{latestName}";

        var histName = _forceJpegName ? $"{timestamp}.jpg" : $"{timestamp}{Path.GetExtension(filePath).ToLower()}";
        var histRemotePath = $"{cameraFolder}/{histName}";

        try
        {
            await client.Connect();
            await EnsureDir(client, cameraFolder);

            if (saveHistory)
            {
                await client.UploadFile(
                    localPath: filePath,
                    remotePath: histRemotePath,
                    existsMode: FtpRemoteExists.Overwrite,
                    createRemoteDir: true,
                    verifyOptions: FtpVerify.None
                );
                _logger.LogInformation("[{Camera}] Uploaded timestamped file: {Path}", cameraName, histRemotePath);
            }

            await client.UploadFile(
                localPath: filePath,
                remotePath: latestRemotePath,
                existsMode: FtpRemoteExists.Overwrite,
                createRemoteDir: true,
                verifyOptions: FtpVerify.None
            );
            _logger.LogInformation("[{Camera}] Updated latest image: {Path}", cameraName, latestRemotePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTPS upload failed for {File}", filePath);
            throw;
        }
        finally
        {
            if (client.IsConnected) await client.Disconnect();
        }
    }

    private static async Task EnsureDir(AsyncFtpClient client, string remotePath)
    {
        if (!await client.DirectoryExists(remotePath))
        {
            await client.CreateDirectory(remotePath, true);
        }
    }
}
