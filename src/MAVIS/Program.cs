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
                Console.WriteLine("Usage: mavis -r <root_folder_path>");
                return;
            }

            var option = args[0];
            var rootFolderPath = args.Length > 1 ? args[1] : null;

            if (option != "-r" || rootFolderPath == null)
            {
                Console.WriteLine("Invalid option. Use -r to specify the root folder to monitor.");
                return;
            }

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
                    configure.AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                        options.TimestampFormat = "hh:mm:ss ";
                        options.IncludeScopes = false;
                    }).SetMinimumLevel(LogLevel.Information);
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
                    await uploader.UploadFileAsync(path, relativePath, "root");
                }
                else
                {
                    // File belongs to a specific camera
                    await uploader.UploadFileAsync(path, relativePath, cameraName);
                }
            }, verbose: false);

            Console.WriteLine("Monitoring root folder. Press Ctrl+C to exit.");
            await Task.Delay(Timeout.Infinite);
        }
    }
}
