using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace TestServer2
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var pipe = new NamedPipeServerStream(
                "psexecsvc",
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Message))
            {
                Console.WriteLine("[*] Waiting for client connection...");
                pipe.WaitForConnection();
                Console.WriteLine("[*] Client connected.");
                while (true)
                {
                    var messageBytes = ReadMessage(pipe);
                    var line = Encoding.UTF8.GetString(messageBytes);
                    Console.WriteLine("[*] Received: {0}", line);
                    if (line.ToLower() == "exit") return;


                    if (line.ToLower() == "status")
                    {
                        //var response = Encoding.UTF8.GetBytes("this is server status !");
                        var response = Encoding.UTF8.GetBytes(AVStatus("Windows Defender"));
                        var response1 =Encoding.UTF8.GetBytes(WazuhStatus("Wazuh"));
                        var response2 = Encoding.UTF8.GetBytes(DeceptiveBytesStatus("Deceptive Bytes"));
                        var response3 = Encoding.UTF8.GetBytes(MicrosoftSysmonStatus("Microsoft Sysmon"));
                       // var response4 = Encoding.UTF8.GetBytes(DejavuStatus("Dejavu"));

                        pipe.Write(response, 0, response.Length);
                    }
                    else
                    {
                        var response = Encoding.UTF8.GetBytes("command not exist !");
                        pipe.Write(response, 0, response.Length);
                    }


                    //var processStartInfo = new ProcessStartInfo
                    //{
                    //    FileName = "cmd.exe",
                    //    Arguments = "/c " + line,
                    //    RedirectStandardOutput = true,
                    //    RedirectStandardError = true,
                    //    UseShellExecute = false
                    //};
                    //try
                    //{
                    //    var process = Process.Start(processStartInfo);
                    //    var output = process.StandardOutput.ReadToEnd();
                    //    output += process.StandardError.ReadToEnd();
                    //    process.WaitForExit();
                    //    if (string.IsNullOrEmpty(output))
                    //    {
                    //        output = "\n";
                    //    }
                    //    var response = Encoding.UTF8.GetBytes(output);
                    //    pipe.Write(response, 0, response.Length);
                    //}
                    //catch (Exception ex)
                    //{
                    //    Console.WriteLine(ex);
                    //    var response = Encoding.UTF8.GetBytes(ex.Message);
                    //    pipe.Write(response, 0, response.Length);
                    //}
                }
            }
        }

        private static byte[] ReadMessage(PipeStream pipe)
        {
            byte[] buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {
                do
                {
                    var readBytes = pipe.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, readBytes);
                }
                while (!pipe.IsMessageComplete);

                return ms.ToArray();
            }
        }

        private static string AVStatus(string avName)
        {
            string status = "unknown";
            //Windows Defender
            //393472 (060100) = disabled and up to date
            //397584 (061110) = enabled and out of date
            //397568 (061100) = enabled and up to date

            ManagementObjectSearcher wmiData = new ManagementObjectSearcher(@"root\SecurityCenter2", "SELECT * FROM AntiVirusProduct");
            ManagementObjectCollection data = wmiData.Get();

            foreach (ManagementObject virusChecker in data)
            {
                if (avName == virusChecker["displayName"].ToString())
                {
                    //Display defender name
                    Console.WriteLine(virusChecker["displayName"].ToString());
                    if (virusChecker["productState"].ToString() == "393472")
                    {
                        status = "disabled and up to date";
                        break;
                    }
                    else if (virusChecker["productState"].ToString() == "397584")
                    {
                        status = "enabled and out of date";
                        break;
                    }
                    else if (virusChecker["productState"].ToString() == "397568")
                    {
                        status = "enabled and up to date";
                        break;
                    }
                }
                //Console.WriteLine(virusChecker["displayName"]);
                //Console.WriteLine(virusChecker["instanceGuid"]);
                //Console.WriteLine(virusChecker["pathToSignedProductExe"]);
                //Console.WriteLine(virusChecker["productState"]);
                //Console.WriteLine(virusChecker["timestamp"]);
            }
            return status;
        }
        private static string WazuhStatus(string WazuhName)
        {
            string status = "unknown";
            string path = @"C:\Program Files (x86)\ossec-agent\wazuh-agent.exe";
            Console.WriteLine("Wazuh");
            if (Directory.Exists(path))
            {
                status = "enabled and up to date";
            }
            else
            {
                status = "disabled and up to date";
            }
            return status;
        }

        private static string DeceptiveBytesStatus(string DbName)
        {
            string status = "unknown";
            string path = @"C:\ProgramData\DBytes\EPS\DeceptiveBytes.EPS.Service.exe";
            Console.WriteLine("Deceptive Bytes");
            if (Directory.Exists(path))
            {
                status = "enabled and up to date";
            }
            else
            {
                status = "disabled and up to date";
            }
            return status;
        }

        private static string MicrosoftSysmonStatus(string MsName)
        {
            string status = "unknown";
            string path = @"C:\Windows\Sysmon64.exe";
            Console.WriteLine("Microsoft Sysmon");
            if (Directory.Exists(path))
            {
                status = "enabled and up to date";
            }
            else
            {
                status = "disabled and up to date";
            }
            return status;
        }

      /*  private static string DejavuStatus(string dName)
        {
            string status = "unknown";
            string path = @"C:\Windows\Sysmon64.exe";
            Console.WriteLine("Microsoft Sysmon");
            if (Directory.Exists(path))
            {
                status = "enabled and up to date";
            }
            else
            {
                status = "disabled and up to date";
            }
            return status;
        }*/
    }
}
