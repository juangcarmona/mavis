using Microsoft.Extensions.Logging;

namespace MAVIS;

public class RootFolderWatcher : FolderWatcher
{
    private readonly Dictionary<string, CameraFolderWatcher> _cameraWatchers = new();
    private readonly AzureBlobUploader _uploader;

    public RootFolderWatcher(ILogger<RootFolderWatcher> logger, AzureBlobUploader uploader) : base(logger)
    {
        _uploader = uploader;
    }

    public Dictionary<string, CameraFolderWatcher> CameraWatchers => _cameraWatchers;

    public override void Watch(string folder, int timerInterval, Action<string> onCreatedAction = null,
        Action<string> onDeletedAction = null, bool verbose = false, int createdActionTriggerDelay = 30)
    {
        // Watch for new and deleted camera folders
        base.Watch(folder, timerInterval, HandleNewCamera, HandleCameraFolderDeleted, verbose, createdActionTriggerDelay);
        // Add watchers for existing camera folders
        var existingFolders = Directory.GetDirectories(folder);
        foreach (var cameraPath in existingFolders)
        {
            HandleNewCamera(cameraPath);
        }
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
        var cameraName = new DirectoryInfo(Path.GetDirectoryName(imagePath)).Name;
        _uploader.UploadFileAsync(imagePath, Path.GetFileName(imagePath), cameraName).Wait();
        _logger.LogInformation($"Uploaded image: {imagePath}");
    }

    private void HandleCameraFolderDeleted(string cameraPath)
    {
        if (CameraWatchers.ContainsKey(cameraPath))
        {
            CameraWatchers[cameraPath].Dispose();
            CameraWatchers.Remove(cameraPath);
            _logger.LogInformation($"Removed watcher for deleted camera: {cameraPath}");
        }
    }
}
