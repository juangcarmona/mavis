using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Reflection;

namespace MAVIS.Tests
{
    public class RootFolderWatcherTests
    {
        private static Random _random = new Random();

        [Fact]
        public void RootFolderWatcher_AddsCameraWatcher_WhenNewCameraFolderIsCreated()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<RootFolderWatcher>>();
            var uploaderMock = Substitute.For<AzureBlobUploader>();
            var rootWatcher = new RootFolderWatcher(loggerMock, uploaderMock);
            var currentPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), RandomString(6));
            Directory.CreateDirectory(currentPath);
            var timerInterval = 1000;

            rootWatcher.Watch(currentPath, timerInterval);

            Thread.Sleep(2000);

            // Act
            var newCameraPath = $"{currentPath}\\{RandomString(4)}";
            Directory.CreateDirectory(newCameraPath);
            Thread.Sleep(timerInterval + 2000);

            // Assert
            Assert.Contains(newCameraPath, rootWatcher.CameraWatchers.Keys);

            Directory.Delete(newCameraPath, true);
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
