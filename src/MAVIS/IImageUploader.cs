namespace MAVIS;

public interface IImageUploader
{
    Task UploadAsync(string filePath, string relativePath, string cameraName, bool saveHistory = false);
}

