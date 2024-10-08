using Microsoft.Extensions.Logging;

namespace MAVIS;
public class CameraFolderWatcher : FolderWatcher
{
    public CameraFolderWatcher(ILogger logger) : base(logger)
    {
    }

    public override void Watch(string folder, int timerInterval, Action<string> onCreatedAction = null,
        Action<string> onDeletedAction = null, bool verbose = false, int createdActionTriggerDelay = 30)
    {
        base.Watch(folder, timerInterval, onCreatedAction, onDeletedAction, verbose, createdActionTriggerDelay);
    }

    protected override void PollFolder()
    {
        base.PollFolder();
        // Aquí podrías añadir lógica específica para manejar la detección de imágenes nuevas
    }
}

