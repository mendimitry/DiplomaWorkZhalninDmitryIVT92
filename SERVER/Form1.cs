using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Telegram.Bot;

namespace ServerPing
{
    public partial class Form1 : Form
    {
        static TraceEventSession m_EtwSession;

        public class PRC
        {
            public int PID { get; set; }
            public int Port { get; set; }
            public string Protocol { get; set; }
        }
        public class ProcManager
        {
            
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

        class P
        {
            int value;
            DateTime d;
            public P (int v, DateTime dd)
            {
                value = v;
                d = dd;
            }
        }
        
      List<P> fdfs = new List<P>();

        public Form1()
        {
            InitializeComponent();




           






            System.Console.SetOut(new Server_Frm.CSS.TextBoxWriter(textBox1));

           // StartPosition = FormStartPosition.CenterScreen;
            timer1.Enabled = true;

           // chart1.ChartAreas[0].AxisY.Maximum = 45;
          //  chart1.ChartAreas[0].AxisY.Minimum = 0;

            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "H:mm:ss";
            chart1.Series[0].XValueType = ChartValueType.DateTime;
            chart1.ChartAreas[0].AxisX.Minimum = DateTime.Now.ToOADate();
            chart1.ChartAreas[0].AxisX.Maximum = DateTime.Now.AddMinutes(0.2).ToOADate();
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;
            chart1.ChartAreas[0].AxisX.Interval = 5;
        }
        private int _countSeconds = 0;
      

        private void Form1_Load(object sender, EventArgs e)
        {
            

        }
        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://t.me/testtrackingt");
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

            Process process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = "/c chcp 65001 & netstat -a -n | find \":5040\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            //process.BeginOutputReadLine();
            //process.OutputDataReceived += (s, e) => { label5.Text = e.Data; };
             label5.Text = process.StandardOutput.ReadToEnd();


            


           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            PerformanceCounter cpuCounter;

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");


 

            double tanTemp = cpuCounter.NextValue();



          Random random = new Random();
            DateTime timeNow = DateTime.Now;

            int value = random.Next(0, 100) ;
            //chart1.Series[0].Points.AddXY(timeNow,value);


            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            UdpStatistics tcpstat = properties.GetUdpIPv4Statistics();

            
            chart1.Series[0].Points.AddXY(timeNow, properties.GetIPv4GlobalStatistics().ReceivedPackets);
            chart1.Series[1].Points.AddXY(timeNow, tcpstat.DatagramsReceived);





            fdfs.Add(new P((int)tanTemp, timeNow));
            _countSeconds++;

            if(_countSeconds  == 60)
            {
                _countSeconds = 0;
                chart1.ChartAreas[0].AxisX.Minimum = DateTime.Now.ToOADate();
                chart1.ChartAreas[0].AxisX.Maximum = DateTime.Now.AddMinutes(0.2).ToOADate();
                chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;
                chart1.ChartAreas[0].AxisX.Interval = 5;
            }
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            String connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=1234;Database=perfomancy;";
            NpgsqlConnection npgSqlConnection = new NpgsqlConnection(connectionString);
            npgSqlConnection.Open();
            MessageBox.Show("Данные внесены");
            NpgsqlCommand npgSqlCommand = new NpgsqlCommand("INSERT INTO perfomancy_ippackets( perfomancy_ipreceivedpackets) VALUES ('" + (textBox1) + "')", npgSqlConnection);
            npgSqlCommand.ExecuteNonQuery();
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            UdpStatistics tcpstat = properties.GetUdpIPv4Statistics();
            
            NpgsqlCommand npgSqlCommand1 = new NpgsqlCommand("INSERT INTO perfomancy_ippackets( perfomancy_ipreceivedpackets) VALUES ('" + (properties.GetIPv4GlobalStatistics().ReceivedPackets) + "')", npgSqlConnection);
            npgSqlCommand1.ExecuteNonQuery();
            NpgsqlCommand npgSqlCommand2 = new NpgsqlCommand("INSERT INTO perfomancy_ippackets( perfomancy_ipreceivedpackets) VALUES ('" + (tcpstat.DatagramsReceived) + "')", npgSqlConnection);
            npgSqlCommand2.ExecuteNonQuery();

            npgSqlConnection.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}
