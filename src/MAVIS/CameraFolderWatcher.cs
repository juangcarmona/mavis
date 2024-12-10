using Microsoft.Extensions.Logging;

namespace MAVIS;
public class CameraFolderWatcher : FolderWatcher
{
    private readonly List<string> _knownFiles = new();

    public CameraFolderWatcher(ILogger logger) : base(logger)
    {
    }

    public override void Watch(string folder, int timerInterval, Action<string> onCreatedAction = null,
        Action<string> onDeletedAction = null, bool verbose = false, int createdActionTriggerDelay = 30,
        string subFolderName = null)
    {
        // Inicializar la lista de archivos conocidos con los archivos existentes
        var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
        var existingFiles = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
            .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower())).ToList();

        _knownFiles.AddRange(existingFiles);

        base.Watch(folder, timerInterval, onCreatedAction, onDeletedAction, verbose, createdActionTriggerDelay);
    }

    protected override void PollFolder()
    {
        var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" }; // Filtra solo im�genes

        var currentFiles = Directory.GetFiles(_pathBeingMonitored, "*.*", SearchOption.TopDirectoryOnly)
            .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower())).ToList();

        foreach (var file in currentFiles)
        {
            if (!_knownFiles.Contains(file))
            {
                _knownFiles.Add(file);
                _onCreatedAction?.Invoke(file);
                _logger.LogInformation($"New image detected and handled: {file}");
            }
        }
    }
}

