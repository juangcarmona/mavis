using Microsoft.Extensions.Logging;

namespace MAVIS;

public class RootFolderWatcher : FolderWatcher
{
    private readonly Dictionary<string, CameraFolderWatcher> _cameraWatchers = new();
    private readonly AzureBlobUploader _uploader;

    public RootFolderWatcher(ILogger logger, AzureBlobUploader uploader) : base(logger)
    {
        _uploader = uploader;
    }

    public Dictionary<string, CameraFolderWatcher> CameraWatchers => _cameraWatchers;

    public override void Watch(string folder, int timerInterval, Action<string> onCreatedAction = null,
        Action<string> onDeletedAction = null, bool verbose = false, int createdActionTriggerDelay = 30)
    {
        base.Watch(folder, timerInterval, HandleNewCamera, onDeletedAction, verbose, createdActionTriggerDelay);
    }

    private void HandleNewCamera(string cameraPath)
    {
        if (!CameraWatchers.ContainsKey(cameraPath))
        {
            var cameraWatcher = new CameraFolderWatcher(_logger);
            cameraWatcher.Watch(cameraPath, 1000, HandleNewImage);
            CameraWatchers[cameraPath] = cameraWatcher;
            _logger.LogInformation($"Added watcher for new camera: {cameraPath}");
        }
    }

    private void HandleNewImage(string imagePath)
    {
        _logger.LogInformation($"New image detected: {imagePath}");
        _uploader.UploadFileAsync(imagePath, Path.GetFileName(imagePath)).Wait();
        _logger.LogInformation($"Uploaded image: {imagePath}");
    }
}
