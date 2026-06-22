#!/bin/bash

# 1. 루트 권한 확인
if [ "$EUID" -ne 0 ]; then
  echo "this script must be run with sudo privileges."
  exit
fi

echo "[*] installing Gamma..."

# 2. 필수 종속성 설치 (dotnet sdk 확인 및 설치)
if ! command -v dotnet &> /dev/null; then
    echo "[*] .NET SDK is not installed. Installing..."
    apt-get update && apt-get install -y dotnet-sdk-8.0
fi

# 3. 빌드 및 배포 파일 생성
echo "[*] building source code..."
dotnet publish -c Release -o ./publish

# 4. PATH에 등록 (시스템 어디서든 'gamma'라고 쳐서 실행 가능하게)
echo "[*] copying command to /usr/local/bin."
cp ./publish/GammaTool /usr/local/bin/gamma

echo "[+] installation complete! You can now run 'sudo gamma' from anywhere in the terminal."