using Microsoft.Extensions.Logging;

namespace MAVIS;

public class RootFolderWatcher : FolderWatcher
{
    private readonly Dictionary<string, CameraFolderWatcher> _cameraWatchers = new();
    private readonly AzureBlobUploader _uploader;
    private string _subFolderName;


    public RootFolderWatcher(ILogger<RootFolderWatcher> logger, AzureBlobUploader uploader) : base(logger)
    {
        _uploader = uploader;
    }

    public Dictionary<string, CameraFolderWatcher> CameraWatchers => _cameraWatchers;

    public override void Watch(string folder, int timerInterval, Action<string> onCreatedAction = null,
        Action<string> onDeletedAction = null, bool verbose = false, int createdActionTriggerDelay = 30,
        string subFolderName = null, bool saveHistory = false)
    {
        _subFolderName = subFolderName;
        base.Watch(folder, timerInterval, HandleNewCamera, HandleCameraFolderDeleted, verbose, createdActionTriggerDelay);

        // Agregar watchers para las carpetas existentes
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
            var subFolderPath = string.IsNullOrEmpty(_subFolderName)
                ? cameraPath
                : Path.Combine(cameraPath, _subFolderName);

            if (!Directory.Exists(subFolderPath))
            {
                _logger.LogWarning($"Subfolder '{_subFolderName}' does not exist in camera path: {cameraPath}");
                return;
            }

            var cameraWatcher = new CameraFolderWatcher(_logger);
            cameraWatcher.Watch(subFolderPath, 1000, HandleNewImage);
            CameraWatchers[cameraPath] = cameraWatcher;
            _logger.LogInformation($"Added watcher for new camera subfolder: {subFolderPath}");
        }
    }


    private void HandleNewImage(string imagePath)
    {
        _logger.LogInformation($"New image detected: {imagePath}");

        var relativePath = Path.GetRelativePath(_pathBeingMonitored, imagePath);
        var segments = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        // Asume that first segment corresponds to camera name
        var cameraName = segments.Length > 1 ? segments[0] : "root";

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
