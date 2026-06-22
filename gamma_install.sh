#!/bin/bash

# Gamma installation script
# This script clones the Gamma project from GitHub and installs it system-wide

echo "[*] Installing Gamma..."

# 1. Check if .NET 6.0 is installed
if ! command -v dotnet &> /dev/null; then
    echo "[*] .NET SDK is not installed. Installing dotnet-sdk-6.0..."
    sudo apt-get update && sudo apt-get install -y dotnet-sdk-6.0
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
    echo "[-] Build failed. DLL not found."
    exit 1
fi

# 4. Install to system
echo "[*] Installing to system..."
INSTALL_PATH="/opt/gamma"
sudo mkdir -p "$INSTALL_PATH"
sudo cp -r ./publish/* "$INSTALL_PATH/"

# Create wrapper script
echo "#!/bin/bash" | sudo tee /usr/local/bin/gamma > /dev/null
echo "exec dotnet $INSTALL_PATH/Gamma.dll \"\$@\"" | sudo tee -a /usr/local/bin/gamma > /dev/null
sudo chmod +x /usr/local/bin/gamma

# 5. Cleanup
rm -rf "$TEMP_DIR"

echo "[+] Installation complete! You can now run 'gamma' from anywhere in the terminal."