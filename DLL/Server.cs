using System;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Telegram.Bot.Types;

namespace NetworkDiscovery
{
    /// <summary>
    /// NetworkDiscoveryServer class that receives data from the multicast group network
    /// IMPORTANT Windows firewall must be open on UDP port 5040
    /// </summary>
    public class Server : IDisposable
    {
        /// <summary>
        /// ListeningCond is a variable for enabling/disabling NetworkDiscoveryServer's listening     
        /// </summary>
        public delegate void EventCast(Message cC);
        /// <summary>
        /// MessageReceived it is event which will be called by the delegate of the class subscribed to it   
        /// </summary>
        public event EventCast MessageReceived;
        private const Int32 DEFAULT_SERVICE_PORT = 5040;
        private Int32 _servicePort;
        private IPAddress _multicastGropuHost;
        private UdpClient _udpServer;
        private IPEndPoint _remoteEndPoint;
        //Dispose flag
        private bool _disposed = false;
        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        [DllImport("user32")]
        public static extern void LockWorkStation();
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                //disposition all unconrolling resources
            }
            _disposed = true; //помечаем флаг что метод Dispose уже был вызван
        }

        private String GetCheckedValue(NameValueCollection values, String key, Dictionary<String, String> defaultConfigValues)
        {
            if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(values[key]))
            {
                return values[key];
            }
            else
            {
                if (!String.IsNullOrEmpty(key) && !values.AllKeys.Contains(key))
                {
                    String defValueOutput;
                    if (defaultConfigValues.TryGetValue(key, out defValueOutput))
                    {
                        MessageBox.Show($"SERVER: Warning, key <{key}> is not defined in App.config and will be initialized with default value {defaultConfigValues[key]}");

                        return defValueOutput;
                    }
                    else
                    {
                        MessageBox.Show($"SERVER: Error, specific key <{key}> is not defined in App.config");
                    }
                }
                return null;
            }
        }

        private void Init()
        {

            try
            {
                var appConfig = ConfigurationManager.GetSection("main") as NameValueCollection;
                var defaultConfigValues = new Dictionary<String, String>
                                        {
                                            {"Port", DEFAULT_SERVICE_PORT.ToString()},
                                            {"multicastGropuHost", "0.0.0.0"},
                                        };
                if (!Int32.TryParse(GetCheckedValue(appConfig, "Port", defaultConfigValues), out _servicePort))
                    _servicePort = DEFAULT_SERVICE_PORT;

                IPAddress.TryParse(GetCheckedValue(appConfig, "multicastGropuHost", defaultConfigValues), out _multicastGropuHost);

                ///Уточнить IPAddress.Any или _multicastGropuHost
                ///_remoteEndPoint = new IPEndPoint(_multicastGropuHost, _servicePort);
                _remoteEndPoint = new IPEndPoint(IPAddress.Any, _servicePort);
                if (_remoteEndPoint == null)
                    MessageBox.Show($"SERVER: Error, failed on initialization <_endPoint>");
                _udpServer = new UdpClient(_remoteEndPoint);
                if (_udpServer == null)
                    MessageBox.Show($"SERVER: Error, failed on initialization <_udpServer>");

                _udpServer.JoinMulticastGroup(_multicastGropuHost);
            }
            catch (System.Net.Sockets.SocketException)

            {


               // Dispose();
                MessageBox.Show(
                "Поскольку прослуживающий порт занят, программа-агент выполнит перезагрузку приложения",
                "Сброс стэка протокола",
                 MessageBoxButtons.OK,
                MessageBoxIcon.Error

                );

                foreach (Process thisproc in Process.GetProcessesByName("Server"))
                {
                    // если просто закрыть нельзя, то убить!
                    if (!thisproc.CloseMainWindow())
                    {
                        
                        thisproc.Kill();
                        MessageBox.Show(
                "Перезапустите приложение еще раз",
                "Программа уничтожена",
                 MessageBoxButtons.OK,
                MessageBoxIcon.Information

                );

                    }
                    // await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId - 1, cancellationToken: cancellationToken);
                }
                /*
                System.Diagnostics.Process process2 = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo2 = new
                System.Diagnostics.ProcessStartInfo();
                startInfo2.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                startInfo2.FileName = "cmd.exe";
                startInfo2.Arguments = "netsh winsock reset && netsh int ip reset && ipconfig /release && ipconfig /renew && ipconfig /flushdns";
                startInfo2.Verb = "runas";
                process2.StartInfo = startInfo2;
                process2.Start();

                MessageBox.Show(
                 "Для завершения сброса сети требуется перезагрузка, она будет через 10 секунд",
                    "Обязательная перезагрузка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error

                );
                var psi = new ProcessStartInfo("shutdown", "/r /t 10");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);
                */

            }

            finally { 
            
            
            
            
            }

            
        }

        private T XmlDeserializeFromBytes<T>(Byte[] data) where T: class, new()
        {
            if (data == null || data.Length == 0)
            {
                throw new InvalidOperationException();
            }

            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

                using (XmlReader xmlReader = XmlReader.Create(memoryStream))
                {
                    return (T)xmlSerializer.Deserialize(xmlReader);
                }
            }
        }

        public async Task ReceiveData()
        {
            var datagramReceived = await _udpServer.ReceiveAsync();
            var receiveData = XmlDeserializeFromBytes<Message>(datagramReceived.Buffer);
            MessageReceived(receiveData);
            string message = "TEST";
            Console.WriteLine($"SERVER: Received  message ({message}) from {_remoteEndPoint.Address} port {_remoteEndPoint.Port}");
        }

        /// <summary>
        /// Listening for activity on all network interfaces
        /// </summary>
        public void Listening()
        {
            Init();
            
            Task.Run(async () =>
            {
                while (!_disposed)
                {

                    //for(; ; )  // решение адекватное, ибо бывает временами сокет не принимается сервером принудительно бесконечный цикл
                    //{
                        await ReceiveData();
                        Console.WriteLine("Server listening...");
                  //  }
                   
                }
                
            });


           
        }
    }
}
