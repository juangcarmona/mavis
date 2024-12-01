
# MAVIS (Multi-camera Azure Video Image Sync)

MAVIS is a scalable system for monitoring and uploading images from multiple webcams to Azure Blob Storage. It features smart synchronization and storage management, with the potential to generate continuous recordings and time-lapse videos.

## Features
- **Multi-camera Support**: Automatically detects new cameras and starts monitoring them.
- **Azure Integration**: Uploads images to Azure Blob Storage, including maintaining the latest snapshot.
- **Time-lapse Ready**: Stores all images with timestamps for future time-lapse or video generation.
- **Scalable and Flexible**: Designed to easily add and manage multiple cameras in an environment.

## How It Works
MAVIS continuously monitors a root folder for subdirectories, each representing a webcam. When a new image is detected, it is:
1. Uploaded to Azure Blob Storage with a unique timestamp.
2. The current image is also uploaded as `latest.jpg` to provide an easy link to the most recent snapshot.

This setup allows real-time updates while also creating a history of images that can be used for further analysis or fun compilations like time-lapse videos.

---

## Installation
### 1. Clone the Repository
```sh
git clone https://github.com/yourusername/MAVIS.git
cd MAVIS
```

### 2. Configuration
Update the `appsettings.json` file located in the root directory with your Azure connection details and the path to the folder you want to monitor.

Example `appsettings.json`:
```json
{
  "AzureBlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=mavisdev;AccountKey=***;EndpointSuffix=core.windows.net",
    "ContainerName": "mavis",
    "MavisKey": "unique_key_example"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### 3. Running the Application
You can start MAVIS manually by running:
```sh
dotnet MAVIS.dll -r "C:/path/to/cams"
```

---

## Make MAVIS Persistent
To ensure MAVIS starts automatically when the system restarts, you can use a simple batch script.

### Steps:
1. Create a `.bat` file:
   - Open a text editor (e.g., Notepad) and paste the following:
     ```batch
     @echo off
     start "" "C:\path\to\dotnet.exe" "C:\path\to\MAVIS.dll" -r "C:/path/to/cams"
     ```
   - Save the file as `start_mavis.bat`.

2. Add the `.bat` file to the Startup folder:
   - Press `Win + R`, type `shell:startup`, and press Enter.
   - Copy the `start_mavis.bat` file into the folder that opens.

Now, MAVIS will automatically start monitoring the specified folder whenever the system boots.

---

## Requirements
- **.NET Core 6.0** or higher
- **Azure Blob Storage Account** for storing images
- **Access to Cameras** that can save images to a local folder

---

## Usage
- **Real-time Monitoring**: MAVIS runs as a console application, monitoring specified folders for new images.
- **Flexible Configuration**: Adjust settings such as monitoring interval, Azure connection string, and folder paths in `appsettings.json`.

---

## Future Plans
- **Time-lapse Generation**: Automatically generate time-lapse videos from stored images.
- **Web Dashboard**: A simple dashboard to view the status of connected cameras and browse uploaded images.
- **Alerts**: Integrate alerts if a camera stops providing new images for a certain period.

---

## Contributing
Contributions are welcome! Feel free to open issues or submit pull requests to help improve MAVIS.

---

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Contact
For any questions or suggestions, please contact [your email].
