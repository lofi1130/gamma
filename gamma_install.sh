#!/bin/bash

# Gamma installation script for Kali Linux and Debian-based systems

echo "[*] Installing Gamma..."

# 1. Check if .NET 6.0 is installed
if ! command -v dotnet &> /dev/null; then
    echo "[*] .NET SDK is not installed. Installing dotnet-sdk-6.0..."
    sudo apt-get update
    sudo apt-get install -y dotnet-sdk-6.0
fi

# 2. Clone and build
TEMP_DIR=$(mktemp -d)
echo "[*] Cloning Gamma from GitHub..."
git clone https://github.com/lofi1130/gamma.git "$TEMP_DIR/gamma"

if [ ! -d "$TEMP_DIR/gamma" ]; then
    echo "[-] Failed to clone repository."
    exit 1
fi

cd "$TEMP_DIR/gamma/Gamma"

echo "[*] Building Gamma..."
dotnet publish -c Release -o ./publish

if [ ! -f "./publish/Gamma.dll" ]; then
    echo "[-] Build failed. DLL not found."
    exit 1
fi

# 3. Install to system
echo "[*] Installing to system..."
INSTALL_PATH="/opt/gamma"
sudo mkdir -p "$INSTALL_PATH"
sudo cp -r ./publish/* "$INSTALL_PATH/"

# Create a simple Python wrapper for better compatibility
sudo tee /usr/local/bin/gamma > /dev/null << 'WRAPPER'
#!/bin/bash
exec /usr/bin/dotnet /opt/gamma/Gamma.dll "$@"
WRAPPER

sudo chmod +x /usr/local/bin/gamma

# Verify installation
if [ -x /usr/local/bin/gamma ]; then
    echo "[+] Wrapper script created successfully"
else
    echo "[-] Failed to create wrapper script"
    exit 1
fi

# 4. Cleanup
rm -rf "$TEMP_DIR"

echo "[+] Installation complete!"
echo "[+] You can now run 'gamma' from anywhere in the terminal."