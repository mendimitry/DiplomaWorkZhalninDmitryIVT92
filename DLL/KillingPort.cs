using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkDiscovery
{
    public class PRC
    {
        public int PID { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
    }
    public class ProcManager
    {
        public void Main()
        {

            Console.WriteLine("Port number you want to clear");
            var input = Console.ReadLine();
            var port = int.Parse(input);
            var prc = new ProcManager();
            prc.KillByPort(port); //prc.KillbyPort(port);

        }
        public void KillByPort(int port)
        {
            var processes = GetAllProcesses();
            if (processes.Any(p => p.Port == port))
                try
                {
                    Process.GetProcessById(processes.First(p => p.Port == port).PID).Kill();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            else
            {
                MessageBox.Show("No process to kill!");
            }
        }

        public List<PRC> GetAllProcesses()
        {
            var pStartInfo = new ProcessStartInfo();
            pStartInfo.FileName = "netstat.exe";
            pStartInfo.Arguments = "-a -n -o";
            pStartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            pStartInfo.UseShellExecute = false;
            pStartInfo.RedirectStandardInput = true;
            pStartInfo.RedirectStandardOutput = true;
            pStartInfo.RedirectStandardError = true;

            var process = new Process()
            {
                StartInfo = pStartInfo
            };
            process.Start();

            var soStream = process.StandardOutput;

            var output = soStream.ReadToEnd();
            if (process.ExitCode != 0)
                throw new Exception("somethign broke");

            var result = new List<PRC>();

            var lines = Regex.Split(output, "\r\n");
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("Proto"))
                    continue;

                var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var len = parts.Length;
               


            }
            return result;
        }
    }
}
