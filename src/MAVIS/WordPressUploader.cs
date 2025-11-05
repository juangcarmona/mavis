using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace MAVIS;

public class WordPressUploader: IImageUploader
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WordPressUploader> _logger;
    private readonly string _baseUrl;
    private readonly string _uploadEndpoint;
    private readonly string _authHeader;

    public WordPressUploader(IConfiguration config, ILogger<WordPressUploader> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();

        _baseUrl = config["WordPress:BaseUrl"] ?? throw new ArgumentNullException("WordPress:BaseUrl");
        _uploadEndpoint = config["WordPress:UploadEndpoint"] ?? "/wp-json/wp/v2/media";

        var username = config["WordPress:Username"];
        var password = config["WordPress:Password"];
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        _authHeader = $"Basic {credentials}";
    }

    public async Task UploadAsync(string filePath, string relativePath, string cameraName, bool saveHistory = false)
    {
        try
        {
            using var fileStream = File.OpenRead(filePath);
            var fileName = Path.GetFileName(filePath);

            using var content = new MultipartFormDataContent();
            var imageContent = new StreamContent(fileStream);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            content.Add(imageContent, "file", fileName);
            content.Add(new StringContent(cameraName), "title");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}{_uploadEndpoint}");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(_authHeader);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation($"Image {fileName} uploaded to WordPress ({cameraName})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading image {filePath} to WordPress");
        }
    }
}
