using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace MAVIS;

public class FolderWatcher : IDisposable
{
    protected readonly ILogger _logger;
    protected bool _verbose;
    protected string _pathBeingMonitored;
    protected Timer _timer;
    protected Action<string> _onCreatedAction;
    protected Action<string> _onDeletedAction;
    protected List<string> _knownFolders;
    protected ConcurrentQueue<string> _directoriesToProcessQueue = new();
    protected int _createdActionTriggerDelay;

    public FolderWatcher(ILogger logger)
    {
        _logger = logger;
    }

    public virtual void Watch(string folder, int timerInterval, Action<string> onCreatedAction = null,
        Action<string> onDeletedAction = null, bool verbose = false, int createdActionTriggerDelay = 30,
        string subFolderName = null)
    {
        _verbose = verbose;
        _pathBeingMonitored = folder;
        _onCreatedAction = onCreatedAction;
        _onDeletedAction = onDeletedAction;
        _createdActionTriggerDelay = createdActionTriggerDelay;

        _knownFolders = Directory.EnumerateDirectories(_pathBeingMonitored).ToList();

        _timer = new Timer(_ => PollFolder(), null, 0, timerInterval);
    }

    protected virtual void PollFolder()
    {
        if (_verbose)
        {
            _logger.LogInformation($"Checking for changes at {_pathBeingMonitored}");
        }

        var currentFolders = Directory.EnumerateDirectories(_pathBeingMonitored).ToList();
        var newFolders = currentFolders.Where(x => !_knownFolders.Contains(x)).ToList();
        var deletedFolders = _knownFolders.Where(x => !currentFolders.Contains(x)).ToList();

        _knownFolders.AddRange(newFolders);
        _knownFolders.RemoveAll(deletedFolders.Contains);

        foreach (var folder in newFolders)
        {
            _logger.LogInformation($"New folder found: {folder}");
            _directoriesToProcessQueue.Enqueue(folder);
            _onCreatedAction?.Invoke(folder);
        }

        foreach (var folder in deletedFolders)
        {
            _logger.LogInformation($"Folder deleted: {folder}");
            _onDeletedAction?.Invoke(folder);
        }
    }

    public void Dispose()
    {
        _timer?.Change(Timeout.Infinite, 0);
    }
}
