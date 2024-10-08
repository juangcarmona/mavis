using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Collections.Concurrent;
using System.Reflection;

namespace MAVIS.Tests
{
    public class FolderWatcherTests
    {
        private string _pathArgumentInMethod;
        private bool _methodHasBeenExecuted;
        private static Random _random = new Random();

        [Fact]
        public void WhenFolderIsCreatedItDispatchesAnActionWithTheFolderPath()
        {
            // Arrange
            _methodHasBeenExecuted = false;
            _pathArgumentInMethod = string.Empty;
            var loggerMock = Substitute.For<ILogger>();
            var folderWatcher = new FolderWatcher(loggerMock);
            var currentPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), RandomString(6));
            Directory.CreateDirectory(currentPath);
            var timerInterval = 1000;
            folderWatcher.Watch(currentPath, timerInterval, MethodToBeExecuted, createdActionTriggerDelay: 0);

            // Allow the initial enumeration of directories to complete
            Thread.Sleep(2000);

            // Act
            var newFolderPath = $"{currentPath}\\{RandomString(4)}";
            Directory.CreateDirectory(newFolderPath);
            Thread.Sleep(timerInterval + 3000);

            // Assert
            Assert.True(_methodHasBeenExecuted);
            Assert.Equal(newFolderPath, _pathArgumentInMethod);

            Directory.Delete(newFolderPath, true);
            Directory.Delete(currentPath, true);
        }

        [Fact]
        public void WhenFolderIsDeletedItDispatchesAnActionWithTheFolderPath()
        {
            // Arrange
            _methodHasBeenExecuted = false;
            _pathArgumentInMethod = string.Empty;
            var loggerMock = Substitute.For<ILogger>();
            var folderWatcher = new FolderWatcher(loggerMock);
            var currentPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), RandomString(6));
            var timerInterval = 500;
            var newFolderPath = $"{currentPath}\\{RandomString(4)}";
            Directory.CreateDirectory(newFolderPath);

            // Act
            folderWatcher.Watch(currentPath, timerInterval, null, MethodToBeExecuted, createdActionTriggerDelay: 0);

            // Allow the initial enumeration of directories to complete
            Thread.Sleep(2000);

            Directory.Delete(newFolderPath, true);
            Thread.Sleep(timerInterval + 600);

            // Assert
            Assert.True(_methodHasBeenExecuted);
            Assert.Equal(newFolderPath, _pathArgumentInMethod);
        }

        [Fact]
        public void WhenMultipleFoldersAreCreatedTheyAreAllProcessed()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger>();
            var folderWatcher = new FolderWatcher(loggerMock);
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var timerInterval = 100;
            var folderCount = 5;

            var processedFolders = new ConcurrentBag<string>();

            // Create a method that adds the processed folder path to the list
            void MethodToBeExecuted(string folderPath)
            {
                // Simulate longer execution time
                Thread.Sleep(300);
                processedFolders.Add(folderPath);
            }

            folderWatcher.Watch(currentPath, timerInterval, MethodToBeExecuted, createdActionTriggerDelay: 0);

            // Allow the initial enumeration of directories to complete
            Thread.Sleep(1000);

            // Act
            var newFolders = new List<string>();

            // Create multiple folders simultaneously
            for (int i = 0; i < folderCount; i++)
            {
                var newFolderPath = $"{currentPath}\\{RandomString(4)}";
                newFolders.Add(newFolderPath);
                Directory.CreateDirectory(newFolderPath);
                Thread.Sleep(100);
            }

            // Wait for folders to be processed
            Thread.Sleep(4000);

            // Assert
            Assert.Equal(folderCount, processedFolders.Count);

            foreach (var newFolderPath in newFolders)
            {
                Assert.Contains(newFolderPath, processedFolders);
                Directory.Delete(newFolderPath, true);
            }
        }

        private void MethodToBeExecuted(string targetPath)
        {
            _pathArgumentInMethod = targetPath;
            _methodHasBeenExecuted = true;
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}