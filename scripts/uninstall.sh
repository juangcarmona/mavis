#!/bin/bash

# Project name
PROJECT_NAME="MAVIS"

# Base directory for the installation
INSTALL_DIR="/usr/local/share/$PROJECT_NAME"

# Remove the installation directory
if [ -d "$INSTALL_DIR" ]; then
    sudo rm -rf "$INSTALL_DIR"
    echo "Deleted directory: $INSTALL_DIR"
else
    echo "Installation directory not found: $INSTALL_DIR"
fi

# Remove the executable script in /usr/local/bin
if [ -f "/usr/local/bin/timer" ]; then
    sudo rm /usr/local/bin/timer
    echo "Deleted executable: /usr/local/bin/timer"
else
    echo "Executable not found: /usr/local/bin/timer"
fi

echo "Uninstallation completed."

hash -r

echo "You might need to restart your terminal to refresh the PATH environment variable."
