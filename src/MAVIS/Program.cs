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
                Console.WriteLine("Usage: mavis -r <root_folder_path> [-s <sub_folder>] [-h (optional, to save history)]");
                return;
            }

            var option = args[0];
            var rootFolderPath = args.Length > 1 ? args[1] : null;
            var subFolderName = args.Length > 3 && args[2] == "-s" ? args[3] : null;
            var saveHistory = args.Contains("-h");

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

            Console.WriteLine($"Monitoring: {rootFolderPath}");
            if (!string.IsNullOrEmpty(subFolderName))
                Console.WriteLine($"Subfolder: {subFolderName}");
            Console.WriteLine($"Save history: {saveHistory}");

            // --- CONFIGURATION ----------------------------------------------------
            var exeDirectory = AppContext.BaseDirectory;
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

            var builder = new ConfigurationBuilder()
                .SetBasePath(exeDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            // --- DEPENDENCY INJECTION ---------------------------------------------
            var services = new ServiceCollection()
                .AddLogging(cfg => cfg.AddSimpleConsole())
                .AddSingleton<IConfiguration>(configuration);

            // Register uploaders based on config
            var uploaderTypes = configuration.GetSection("Uploaders").Get<string[]>() ?? Array.Empty<string>();

            foreach (var type in uploaderTypes)
            {
                switch (type.ToLowerInvariant())
                {
                    case "azureblob":
                        services.AddSingleton<IImageUploader, AzureBlobUploader>();
                        break;
                    case "wordpress":
                        services.AddSingleton<IImageUploader, WordPressUploader>();
                        break;
                    case "sftp":
                        services.AddSingleton<IImageUploader, SftpUploader>();
                        break;
                    default:
                        Console.WriteLine($"[WARN] Unknown uploader type: {type}");
                        break;
                }
            }

            // Register core watcher
            services.AddSingleton<RootFolderWatcher>();

            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            // --- RUNTIME DIAGNOSTICS ---------------------------------------------
            var uploaders = serviceProvider.GetServices<IImageUploader>().ToList();
            if (!uploaders.Any())
            {
                logger.LogWarning("No uploaders registered. Nothing will be uploaded.");
            }
            else
            {
                logger.LogInformation($"Loaded uploaders: {string.Join(", ", uploaders.Select(u => u.GetType().Name))}");
            }

            // --- WATCH EXECUTION --------------------------------------------------
            var rootWatcher = serviceProvider.GetRequiredService<RootFolderWatcher>();

            rootWatcher.Watch(
                folder: rootFolderPath,
                timerInterval: 1000,
                verbose: false,
                subFolderName: subFolderName,
                saveHistory: saveHistory
            );

            logger.LogInformation("Monitoring root folder. Press Ctrl+C to exit.");
            await Task.Delay(Timeout.Infinite);
        }
    }
}
