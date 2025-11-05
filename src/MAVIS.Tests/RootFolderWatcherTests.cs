using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Reflection;

namespace MAVIS.Tests;

public class RootFolderWatcherTests
{
    [Fact]
    public async Task HandleNewImage_ShouldInvokeAllUploaders()
    {
        var uploader1 = Substitute.For<IImageUploader>();
        var uploader2 = Substitute.For<IImageUploader>();
        var logger = NullLogger<RootFolderWatcher>.Instance;

        var uploaders = new[] { uploader1, uploader2 };
        var watcher = new RootFolderWatcher(logger, uploaders);

        var tmpRoot = Path.Combine(Path.GetTempPath(), "mavis-test");
        var camFolder = Path.Combine(tmpRoot, "cam1");
        Directory.CreateDirectory(camFolder);
        var testFile = Path.Combine(camFolder, "frame.jpg");
        await File.WriteAllTextAsync(testFile, "fake");

        // initialize internal state
        watcher.Watch(tmpRoot, 1000);

        // invoke private method
        var method = typeof(RootFolderWatcher)
            .GetMethod("HandleNewImage", BindingFlags.NonPublic | BindingFlags.Instance);
        method!.Invoke(watcher, new object[] { testFile });

        await uploader1.Received(1).UploadAsync(
            Arg.Is<string>(p => p.EndsWith("frame.jpg")),
            Arg.Any<string>(),
            Arg.Is("cam1"),
            Arg.Any<bool>());

        await uploader2.Received(1).UploadAsync(
            Arg.Is<string>(p => p.EndsWith("frame.jpg")),
            Arg.Any<string>(),
            Arg.Is("cam1"),
            Arg.Any<bool>());
    }
}
