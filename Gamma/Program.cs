using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Console = Colorful.Console;

namespace GammaTool
{
    class Program
    {
        // 언어 설정을 위한 정적 변수 (기본값: "en")
        static string currentLanguage = "en";

        static void Main(string[] args)
        {
            // 1. 루트 권한 확인
            if (ExecuteCommand("id -u").Trim() != "0")
            {
                Console.WriteLine(" ERROR: GammaTool requires root privileges.", Color.Red);
                Console.WriteLine(" Please run with 'sudo gamma'", Color.White);
                return;
            }

            // 2. 종속성 체크 및 설치
            CheckAndInstallDependencies();

            // 3. 로딩 창
            ShowLoadingScreen();

            // 4. 메인 메뉴 루프
            bool running = true;
            while (running)
            {
                Console.Clear();
                ShowMenu();

                string choice = Console.ReadLine();
                if (choice?.ToLower() == "exit") break;

                switch (choice)
                {
                    case "1":
                        Console.Clear();
                        ShowIpAddress();
                        WaitForKey();
                        break;
                    case "2":
                        Console.Clear();
                        ShowSystemInfo();
                        WaitForKey();
                        break;
                    case "3":
                        Console.Clear();
                        string prompt = (currentLanguage == "kr") ? "타겟 IP 입력 (exit 시 메뉴): " : "Enter target IP (exit to return): ";
                        Console.Write(prompt);
                        string target = Console.ReadLine();
                        if (target?.ToLower() != "exit")
                        {
                            CheckPort(target, 80);
                            WaitForKey();
                        }
                        break;
                    case "4":
                        ChangeLanguage();
                        break;
                }
            }
        }

        static void ShowMenu()
        {
            string asciiArt = @"
    _____                                   _     _           _            
  / ____|                                 | |   | |         | |           
 | |  __  __ _ _ __ ___  _ __ ___   __ _  | |__ | | __ _ ___| |_ ___ _ __ 
 | | |_ |/ _` | '_ ` _ \| '_ ` _ \ / _` | | '_ \| |/ _` / __| __/ _ \ '__/
 | |__| | (_| | | | | | | | | | | | (_| | | |_) | | (_| \__ \ ||  __/ |   
  \_____|\__,_|_| |_| |_|_| |_| |_|\__,_| |_.__/|_|\__,_|___/\__\___|_|   
                                      ______                              
                                     |______|                             ";

            Console.WriteLine(asciiArt, Color.Blue);
            Console.WriteLine("Gamma v1.0 - Security Tool | code by joaru02", Color.White);
            Console.WriteLine("\n=== Menu ===", Color.Gray);

            if (currentLanguage == "kr")
            {
                Console.WriteLine("[1] 내 IP 확인", Color.Green);
                Console.WriteLine("[2] 시스템 정보", Color.Yellow);
                Console.WriteLine("[3] 네트워크 포트 스캔", Color.Yellow);
                Console.WriteLine("[4] 언어 설정", Color.Gray);
                Console.WriteLine("\n(종료하려면 'exit' 입력)", Color.DarkGray);
                Console.Write("\n선택: ");
            }
            else
            {
                Console.WriteLine("[1] Check My IP Address", Color.Green);
                Console.WriteLine("[2] PC Information", Color.Yellow);
                Console.WriteLine("[3] Network Port Scan", Color.Yellow);
                Console.WriteLine("[4] Change Language", Color.Gray);
                Console.WriteLine("\n(Type 'exit' to quit)", Color.DarkGray);
                Console.Write("\nSelect: ");
            }
        }

        static void ChangeLanguage()
        {
            Console.Clear();
            Console.WriteLine("Select Language / 언어 선택:");
            Console.WriteLine("1. English");
            Console.WriteLine("2. 한국어");
            Console.Write("Choice: ");
            string langChoice = Console.ReadLine();
            if (langChoice == "1") currentLanguage = "en";
            else if (langChoice == "2") currentLanguage = "kr";
        }

        static void ShowLoadingScreen()
        {
            Console.Clear();
            Console.WriteLine("\nInitializing Gamma Tool...", Color.White);
            for (int i = 0; i <= 20; i++)
            {
                Console.Write("\r[");
                string progress = new string('=', i) + new string(' ', 20 - i);
                Console.Write(progress);
                Console.Write($"] {i * 5}%", Color.Cyan);
                Thread.Sleep(100);
            }
            Console.WriteLine("\n\nWelcome to Gamma Security Tool!", Color.Cyan);
            Thread.Sleep(1000);
        }

        static void CheckAndInstallDependencies()
        {
            string[] dependencies = { "pciutils", "procps" };
            foreach (var pkg in dependencies)
            {
                if (ExecuteCommand($"which {pkg}") == "Error" || string.IsNullOrWhiteSpace(ExecuteCommand($"which {pkg}")))
                {
                    Console.WriteLine($"[!] {pkg} not found. Installing...", Color.Orange);
                    ExecuteCommand("apt-get install -y " + pkg);
                }
            }
        }

        static void WaitForKey()
        {
            string msg = (currentLanguage == "kr") ? "\n아무 키나 누르면 메뉴로 돌아갑니다..." : "\nPress any key to return to the menu...";
            Console.WriteLine(msg, Color.Gray);
            Console.ReadKey();
        }

        static void ShowIpAddress()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            Console.WriteLine($"\n[Host Name]: {hostName}", Color.Cyan);
            foreach (var ip in addresses)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    Console.WriteLine($"[IP Address]: {ip}", Color.Green);
            }
        }

        static void ShowSystemInfo()
        {
            Console.WriteLine("\n=== System Information ===", Color.Magenta);
            Console.WriteLine($"[OS]: {RuntimeInformation.OSDescription}", Color.Yellow);
            Console.WriteLine($"[CPU]: {ExecuteCommand("lscpu | grep 'Model name' | cut -f 2 -d ':'").Trim()}", Color.Yellow);
            Console.WriteLine($"[GPU]: {ExecuteCommand("lspci | grep -i 'vga\\|3d' | cut -d ':' -f 3").Trim()}", Color.Yellow);
            Console.WriteLine($"[RAM]: {ExecuteCommand("grep MemTotal /proc/meminfo | cut -f 2 -d ':'").Trim()}", Color.Yellow);
            Console.WriteLine($"[SSD]: {ExecuteCommand("lsblk -d -o MODEL | grep -v 'MODEL' | head -n 1").Trim()}", Color.Yellow);
        }

        static void CheckPort(string host, int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                    string msg = (currentLanguage == "kr") ? (success ? "포트가 열려 있습니다!" : "포트가 닫혀 있거나 응답이 없습니다.") : (success ? "Port is open!" : "Port is closed.");
                    Console.WriteLine(msg, success ? Color.Green : Color.Red);
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}", Color.Red); }
        }

        static string ExecuteCommand(string command)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo { FileName = "/bin/bash", Arguments = $"-c \"{command}\"", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
                using (Process? process = Process.Start(startInfo))
                {
                    if (process == null) return "Error";
                    string output = process.StandardOutput.ReadToEnd();
                    if (!process.WaitForExit(3000)) { process.Kill(); return "Timeout"; }
                    return string.IsNullOrWhiteSpace(output) ? "N/A" : output.Replace("\n", "").Trim();
                }
            }
            catch { return "Error"; }
        }
    }
}