#!/bin/bash

# Gamma installation script
# This script clones the Gamma project from GitHub and installs it system-wide

echo "[*] Installing Gamma..."

# 1. Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "[*] .NET SDK is not installed. Installing dotnet-sdk..."
    sudo apt-get update && sudo apt-get install -y dotnet-sdk-10.0
fi

# 2. Clone the Gamma repository
TEMP_DIR=$(mktemp -d)
echo "[*] Cloning Gamma from GitHub..."
git clone https://github.com/lofi1130/gamma.git "$TEMP_DIR/gamma"

if [ ! -d "$TEMP_DIR/gamma" ]; then
    echo "[-] Failed to clone repository."
    exit 1
fi

cd "$TEMP_DIR/gamma/Gamma"

# 3. Build the project
echo "[*] Building Gamma..."
dotnet publish -c Release -o ./publish

if [ ! -f "./publish/Gamma" ]; then
    echo "[-] Build failed. Executable not found."
    exit 1
fi

# 4. Copy to PATH (requires sudo)
echo "[*] Installing to system..."
sudo cp ./publish/Gamma /usr/local/bin/gamma
sudo chmod +x /usr/local/bin/gamma

# 5. Cleanup
rm -rf "$TEMP_DIR"

echo "[+] Installation complete! You can now run 'gamma' from anywhere in the terminal."