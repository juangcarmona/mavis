#!/bin/bash

# Project name
PROJECT_NAME="MAVIS"

# Base directory for the installation
INSTALL_DIR="/usr/local/share/$PROJECT_NAME"

# Create necessary directories
sudo mkdir -p "$INSTALL_DIR"

# Compile the project
sudo dotnet publish "../src/MAVIS/MAVIS.csproj" --configuration Debug --output "$INSTALL_DIR"

# Copy appsettings.json to the installation directory if it exists
if [ -f "../appsettings.json" ]; then
    sudo cp "../appsettings.json" "$INSTALL_DIR"
fi

# Create an executable script in /usr/local/bin
echo "#!/bin/bash
dotnet $INSTALL_DIR/$PROJECT_NAME.dll \"\$@\"" | sudo tee /usr/local/bin/mavis > /dev/null

# Make the script executable
sudo chmod +x /usr/local/bin/mavis

echo "Installation completed. You can now use the 'mavis' command from any location."
