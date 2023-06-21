using NetworkDiscovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using System.Threading;
using Telegram.Bot.Polling;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.IO;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Management;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Net.Mime.MediaTypeNames;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ClientPing;
using System.Net.Http;
using Application = System.Windows.Forms.Application;
using System.Drawing.Imaging;
using System.Drawing;
using Telegram.Bot.Types.InputFiles;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ServerPing
{
    internal class Server
    {
        [DllImport("user32.dll")]
        static extern bool SetProcessDPIAware();

        static Size GetMonitorSize()
        {
            IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Graphics g = Graphics.FromHwnd(hwnd);
            return new Size((int)g.VisibleClipBounds.Width, (int)g.VisibleClipBounds.Height);
        }

        static Bitmap TakeScreenshot(Size size)
        {
            Bitmap bmp = new Bitmap(size.Width, size.Height);
            Graphics graphics = Graphics.FromImage(bmp);
            graphics.CopyFromScreen(Point.Empty, Point.Empty, bmp.Size);
            return bmp;
        }
        static ITelegramBotClient bot = new TelegramBotClient("5888819132:AAFI0_ccd2bzK6-em-4qfHzoky8RSc2AuTQ");


        static ChatId PollChatID = null;
        static int PollMessageID = 0;

        static void Main(string[] args)
        {

            ///Server
            NetworkDiscovery.Server nDS = new NetworkDiscovery.Server();
            nDS.MessageReceived += NDS_MessageReceived;
            nDS.Listening();
 
            MessageBox.Show("Запущен бот " + bot.GetMeAsync().Result.Id);
            
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates =  {
                   
                },

                 ThrowPendingUpdates = true // receive all update types
            };

            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            var myForm = new MainFormServer();

            myForm.ShowDialog();

            Console.ReadLine();
           // Console.ReadKey();

        }


        public static string GetComputerInfo()
        {
            string info = string.Empty;
            string cpu = GetCPUInfo();
            string baseBoard = GetBaseBoardInfo();
            string bios = GetBIOSInfo();
            string mac = GetMACInfo();
            info = string.Concat("CPU Number" +"\t"+cpu + "\n "+ "SerialNumber MainBoard" + "\t" + baseBoard + "\n" + "SerialNumber Bios" + "\t" + bios + "\n "+ "SerialNumber Mac" + "\t" + mac);
            return info;
        }

        private static string GetCPUInfo()
        {
            string info = string.Empty;
            info = GetHardWareInfo("Win32_Processor", "ProcessorId");
            return info;
        }
        private static string GetBIOSInfo()
        {
            string info = string.Empty;
            info = GetHardWareInfo("Win32_BIOS", "SerialNumber");
            return info;
        }
        private static string GetBaseBoardInfo()
        {
            string info = string.Empty;
            info = GetHardWareInfo("Win32_BaseBoard", "SerialNumber");
            return info;
        }
        private static string GetMACInfo()
        {
            string info = string.Empty;
            info = GetHardWareInfo("Win32_BaseBoard", "SerialNumber");
            return info;
        }

        private static string NetworkInterfacesInformation()
        {
            string NetIntInfo = "";

            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            NetIntInfo = NetIntInfo + "Имя компьютера: " + Environment.MachineName + ", Домен: " + Environment.UserDomainName + ", Пользователь: " + SystemInformation.UserName + Environment.NewLine;

            NetIntInfo = NetIntInfo + "Информация по сетевым интерфейсам компьютера " + computerProperties.HostName + computerProperties.DomainName + Environment.NewLine;

            foreach (NetworkInterface adapter in nics)
            {

                IPInterfaceProperties properties = adapter.GetIPProperties();
                NetIntInfo = NetIntInfo + String.Empty.PadLeft(adapter.Description.Length, '=') + Environment.NewLine;
                NetIntInfo = NetIntInfo + adapter.Description + Environment.NewLine;

                NetIntInfo = NetIntInfo + " Тип интерфейса\t: " + adapter.NetworkInterfaceType + Environment.NewLine;

                foreach (UnicastIPAddressInformation uniIPInfo in properties.UnicastAddresses)
                {
                    NetIntInfo = NetIntInfo + " Адрес\t\t: " + uniIPInfo.Address.ToString() + Environment.NewLine;
                    NetIntInfo = NetIntInfo + "\t\t\tPreferred Lifetime: " + uniIPInfo.AddressPreferredLifetime + Environment.NewLine;
                    NetIntInfo = NetIntInfo + "\t\t\tValid Lifetime: " + uniIPInfo.AddressValidLifetime + Environment.NewLine;
                    NetIntInfo = NetIntInfo + "\t\t\tDHCP Lease Lifetime: " + uniIPInfo.DhcpLeaseLifetime + Environment.NewLine;
                    NetIntInfo = NetIntInfo + "\t\t\tPrefix Origin: " + uniIPInfo.PrefixOrigin.ToString() + Environment.NewLine;
                    NetIntInfo = NetIntInfo + "\t\t\tSuffix Origin: " + uniIPInfo.SuffixOrigin.ToString() + Environment.NewLine;
                }

                NetIntInfo = NetIntInfo + " MAC Адрес\t: " + adapter.GetPhysicalAddress().ToString() + Environment.NewLine;
                NetIntInfo = NetIntInfo + " Is receive only\t: " + adapter.IsReceiveOnly + Environment.NewLine;
                NetIntInfo = NetIntInfo + " Multicast\t: " + adapter.SupportsMulticast + Environment.NewLine;
            }

            return NetIntInfo;
        }

        private static string GetHardWareInfo(string typePath, string key)
        {
            try
            {
                ManagementClass managementClass = new ManagementClass(typePath);
                ManagementObjectCollection mn = managementClass.GetInstances();
                PropertyDataCollection properties = managementClass.Properties;
                foreach (PropertyData property in properties)
                {
                    if (property.Name == key)
                    {
                        foreach (ManagementObject m in mn)
                        {
                            return m.Properties[property.Name].Value.ToString();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                
            }
            return string.Empty;
        }
       


        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        
        }
        static List<string> GetHardwareInfo(string WIN32_Class, string ClassItemField)
        {
            List<string> result = new List<string>();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM " + WIN32_Class);

            try
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    result.Add(obj[ClassItemField].ToString().Trim());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

            [Obsolete]
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            

            try
            {
                if (update.Type != UpdateType.Unknown)
                {
                    foreach (System.Net.IPAddress ip in System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList)
                    {

                        String direction = "";
                        WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                        using (WebResponse response = request.GetResponse())
                        using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                        {
                            direction = stream.ReadToEnd();
                        }

                        //Search for the ip in the html
                        int first = direction.IndexOf("Address: ") + 9;
                        int last = direction.LastIndexOf("</body>");
                        direction = direction.Substring(first, last - first);







                        NetworkDiscovery.Message nDM = new NetworkDiscovery.Message();
                        // Некоторые действия
                        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
                        NetworkDiscovery.Server nDS = new NetworkDiscovery.Server();


                        //Console.WriteLine($"NOTIFIER: <server> message received {nDM}, {nDM.Index}, {nDM.Name}, {nDM.Ip}, {nDM.Port}");
                        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                        {
                            var message = update.Message;



                            if (message.Text.Contains("tracking"))
                            {


                                var keyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Go", $"{ip}"));

                                await bot.SendTextMessageAsync(message.Chat, "Вы можете зайти на ipaddress, если это маршрутизатор", replyMarkup: keyboard);


                                System.Net.NetworkInformation.Ping newping = new System.Net.NetworkInformation.Ping();
                                System.Net.NetworkInformation.PingReply reply = newping.Send(ip);
                                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, ("Сеть доступна - можете запустить клиент"));
                                }
                                if (reply.Status == System.Net.NetworkInformation.IPStatus.Unknown)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, ("Клиент недоступен - Отслеживание недоступно, перезапустите клиент"));
                                }
                            }
                            
                            if (message.Text.Contains("stopbot"))
                            {

                                await botClient.SendTextMessageAsync(message.Chat, ($"Программа-агент отслеживания {nDM} выключена. Перезапустите приложение"), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);
                                Application.Exit();
                                

                                return;

                            }
                            
                            if (message.Text.Contains("exit"))
                            {
                                await botClient.SendTextMessageAsync(message.Chat, ($"Программа-агент отслеживания {nDM} выключена. Перезапустите приложение"), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);
                                Application.Exit();
                                //Application.Restart();
                                return;
                            }
                            if (message.Text.Contains("info"))
                            {

                                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                                var localIp = host.AddressList.FirstOrDefault(ip1 => ip1.AddressFamily == AddressFamily.InterNetwork).ToString();

                               
                                string ipAddress = "";
                                if (Dns.GetHostAddresses(Dns.GetHostName()).Length > 0)
                                {
                                    ipAddress = Dns.GetHostAddresses(Dns.GetHostName())[0].ToString();
                                    await botClient.SendTextMessageAsync(message.Chat, (string.Format("Ip address сервера: "+"{0} : {1}", ipAddress, localIp)), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);

                                }

                                return;
                            }

                            if (message.Text.Contains("sysconfig"))
                            {

                                 ManagementObjectSearcher searcher5 =
                                 new ManagementObjectSearcher("root\\CIMV2",
                                    "SELECT * FROM Win32_OperatingSystem");
                                string CPU = GetHardwareInfo("Win32_Processor", "Name")[0], GPU = GetHardwareInfo("Win32_VideoController", "Name")[0];

                                string Description = GetHardwareInfo("Win32_Processor", "Description")[0];
                                string Video = GetHardwareInfo("Win32_VideoController", "Name")[0];

                                await botClient.SendTextMessageAsync(message.Chat, (string.Format("Процессор: " + "{0}  \"Видеокарта: \"  {1}", CPU, GPU)), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);
                                await botClient.SendTextMessageAsync(message.Chat, (string.Format("Описание процессора: " + "{0}  \"Название видеокарты: \"  {1}", Description, Video)), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);

                                foreach (DriveInfo dI in DriveInfo.GetDrives())
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, (string.Format($"\t Диск: {0}\n\t" +
                                          " Формат диска: {1}\n\t " +
                                          "Размер диска (ГБ): {2}\n\t Доступное свободное место (ГБ): {3}\n",
                                          dI.Name, dI.DriveFormat, (double)dI.TotalSize / 1024 / 1024 / 1024, (double)dI.AvailableFreeSpace / 1024 / 1024 / 1024)), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);
                                }

                                await botClient.SendTextMessageAsync(message.Chat, (string.Format("{0}", GetComputerInfo())), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);
                                await botClient.SendTextMessageAsync(message.Chat, (string.Format("{0}", NetworkInterfacesInformation())), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);

                            }

                            if (message.Text.Contains("deletebag"))
                            {
                                //присланное человеком сообщение
                                await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId - 5, cancellationToken: cancellationToken);


                            }
                            if (message.Text.Contains("otladka"))
                            {
                                await botClient.SendTextMessageAsync(message.Chat, (Newtonsoft.Json.JsonConvert.SerializeObject(update)), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);
                                return;
                            }
                            if (message.Text.Contains("cmd"))
                            {
                                await botClient.SendTextMessageAsync(message.Chat, ("/tracking, /stopbot, /exit, /info, /deletebag, /otladka, /cmd, /ipaddress, /photo"), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);
                                return;
                            }

                            if (message.Text.Contains("ipaddress"))
                            {

                                foreach (System.Net.IPAddress ip1 in System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList)
                                {
                                    nDS.MessageReceived += NDS_MessageReceived;
                                    Console.WriteLine($"NOTIFIER: <server> message received {nDM}, {nDM.Index}, {nDM.Name}, {nDM.Ip}, {nDM.Port}");

                                    UdpClient udpClient = new UdpClient(0);
                                    StringBuilder sb = new StringBuilder();
                                    StringWriter sw = new StringWriter(sb);

                                    StringReader sr = new StringReader(sb.ToString());
                                    string completeString = sr.ReadToEnd();

                                    await botClient.SendTextMessageAsync(message.Chat, ($"Локальный сетевой адрес: {ip}" + " : " + ((IPEndPoint)udpClient.Client.LocalEndPoint).Port.ToString()), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);
                                    await botClient.SendTextMessageAsync(message.Chat, ($"Локальный сетевой адрес: {sb}" + " : " + ((IPEndPoint)udpClient.Client.LocalEndPoint).Port.ToString()), replyToMessageId: update.Message.MessageId);
                                }
                                await botClient.SendTextMessageAsync(message.Chat, ($"Глобальный сетевой адрес: {direction}"), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);
                                return;
                            }

                            if (message.Text.Contains("photo"))
                            {
                                Bitmap memoryImage;
                                //Set full width, height for image
                                memoryImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                                       Screen.PrimaryScreen.Bounds.Height,
                                                       PixelFormat.Format32bppArgb);
                                Size s = new Size(memoryImage.Width, memoryImage.Height);
                                Graphics memoryGraphics = Graphics.FromImage(memoryImage);
                                memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);
                                string str = "";
                                try
                                {
                                    str = Application.ExecutablePath + @"screen.jpg";//Set folder to save image
                                }
                                catch { };
                                memoryImage.Save(str);


                                string screen = Application.ExecutablePath + @"screen.jpg";
                                    using (var fileStream = new FileStream(screen, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        await botClient.SendPhotoAsync(
                                            chatId: message.Chat.Id,
                                            photo: new InputOnlineFile(fileStream),
                                            caption: "Фотография сервера"
                                        );
                                    }
                                





                                await botClient.SendTextMessageAsync(message.Chat, ($"Глобальный сетевой адрес: {direction}"), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);
                                return;
                            }



                        }
                    }

                }
                
                
                
            }
            
            catch (System.Net.WebException)
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    var message = update.Message;
                    await botClient.SendTextMessageAsync(message.Chat, ($" Ошибка : Telegram по какой то причине не принимает сообщения! Удаление последнего сообщения!"));
                    await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId - 1, cancellationToken: cancellationToken);
                }
            }
            catch (Telegram.Bot.Exceptions.RequestException e )
            {


                //Process.GetCurrentProcess().Kill();
                //Process.GetCurrentProcess().Kill();
                MessageBox.Show(Convert.ToString(e));
            }
            catch(System.Runtime.InteropServices.ExternalException e)
            {
                MessageBox.Show(Convert.ToString(e));
            }
            
            
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
        private static void NDS_MessageReceived(NetworkDiscovery.Message nDM)
        {


            Console.WriteLine($"NOTIFIER: <server> message received {nDM}, {nDM.Index}, {nDM.Name}, {nDM.Ip}, {nDM.Port}");
        }

        public void Work()

        {
            //

            NetworkDiscovery.Message nDM = new NetworkDiscovery.Message();

            NetworkDiscovery.Server nDS = new NetworkDiscovery.Server();

            nDS.MessageReceived += NDS_MessageReceived;
            Console.WriteLine($"NOTIFIER: <server> message received {nDM}, {nDM.Index}, {nDM.Name}, {nDM.Ip}, {nDM.Port}");


        }
        
    }
}
