using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MAVIS
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: mavis -r <root_folder_path> [-s <sub_folder] [-h (optional, to save history)]");
                return;
            }

            var option = args[0];
            var rootFolderPath = args.Length > 1 ? args[1] : null;
            var subFolderName = args.Length > 3 && args[2] == "-s" ? args[3] : null;
            var saveHistory = args.Contains("-h"); // To Save history of the files uploaded


            if (option != "-r" || rootFolderPath == null)
            {
                Console.WriteLine("Invalid option. Use -r to specify the root folder to monitor.");
                return;
            }

            if (!string.IsNullOrEmpty(subFolderName))
            {
                Console.WriteLine($"Subfolder specified: {subFolderName}");
            }

            Console.WriteLine($"Save history: {saveHistory}");

            if (!Directory.Exists(rootFolderPath))
            {
                Console.WriteLine($"The specified folder {rootFolderPath} does not exist.");
                return;
            }

            var exeDirectory = AppContext.BaseDirectory;
            var appSettingsPath = Path.Combine(exeDirectory, "appsettings.json");

            // Set up configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(exeDirectory)
                .AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true);

            IConfiguration configuration = builder.Build();

            // Set up dependency injection
            var serviceProvider = new ServiceCollection()
                .AddLogging(configure =>
                {
                    configure.AddSimpleConsole();
                })
                .AddSingleton(configuration)
                .AddSingleton<RootFolderWatcher>()
                .AddSingleton<AzureBlobUploader>()
                .BuildServiceProvider();

            var rootWatcher = serviceProvider.GetService<RootFolderWatcher>();
            var uploader = serviceProvider.GetService<AzureBlobUploader>();

            // Watch the specified root folder
            rootWatcher.Watch(rootFolderPath, 1000, async (path) =>
            {
                // Calculate the relative path and determine if the file belongs to a camera
                var relativePath = Path.GetRelativePath(rootFolderPath, path);

                // Determine camera name (if any)
                var directoryName = Path.GetDirectoryName(path);
                var cameraName = string.IsNullOrEmpty(directoryName) || directoryName == rootFolderPath
                    ? null
                    : new DirectoryInfo(directoryName).Name;

                // Upload the file, including the camera name if it exists
                if (string.IsNullOrEmpty(cameraName))
                {
                    // File is in the root folder, doesn't belong to any camera
                    await uploader.UploadFileAsync(path, relativePath, "root", saveHistory);
                }
                else
                {
                    // File belongs to a specific camera
                    await uploader.UploadFileAsync(path, relativePath, cameraName, saveHistory);
                }

            }, verbose: false, subFolderName: subFolderName, saveHistory: saveHistory);

            Console.WriteLine("Monitoring root folder. Press Ctrl+C to exit.");
            await Task.Delay(Timeout.Infinite);
        }
    }
}
