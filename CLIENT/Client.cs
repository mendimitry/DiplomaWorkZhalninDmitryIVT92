using NetworkDiscovery;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot.Polling;
using Telegram.Bot;
using System.IO;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using TeleSharp.TL;
using WTelegram;
using static System.Net.Mime.MediaTypeNames;

namespace ClientPing
{
    internal class Client
    {
        static ITelegramBotClient bot = new TelegramBotClient("5831461438:AAHWz5vFbCWGlNh8_4bsk2SYFZSpET6xRpg");
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const int SW_Min = 2;
        const int SW_Max = 3;
        const int SW_Norm = 4;
        private int lastMessageId = 1;
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        private void label1_Click(object sender, EventArgs e)
        {

        }
       
        [STAThread]
        static void Main(string[] args)
        {

          
            MessageBox.Show("Запущен бот " + bot.GetMeAsync().Result.FirstName + "\n" + "Запущен фоновый режим работы приложения");
            
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

            NetworkDiscovery.Client nDC = new NetworkDiscovery.Client();
            nDC.Init();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            var localIp = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
            DialogResult dialogResult = MessageBox.Show("Запуск приложения-агента", "Вы хотите запустить приложение-агент!?", MessageBoxButtons.YesNo,
        MessageBoxIcon.Information,
        MessageBoxDefaultButton.Button1,
        MessageBoxOptions.DefaultDesktopOnly);
            if (dialogResult == DialogResult.Yes)
            {


            }
            else if (dialogResult == DialogResult.No)
            {
                Process.GetCurrentProcess().Kill();
            }


            NetworkDiscovery.Message nDM = new NetworkDiscovery.Message("Тестовое сообщение", localIp, int.Parse("5040"));

            Int32 nDMIndex = 0;

            while (true)
            {
                nDM.Index = Interlocked.Increment(ref nDMIndex);
                nDC.Sending(nDM);

                Thread.Sleep(300);

            }


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
                        //var chatID = update.Message.Chat.Id;
                        //var fromID = update.Message.From.Id;
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



                        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                        {
                            var message = update.Message;

                           // var fromID = update.Message.From.Id;
                            if (message.Text.Contains("startinit2"))
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Запущен сервис отслеживания Telegram", replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);

                                 await botClient.RestrictChatMemberAsync(chatId: 5831461438, userId: 1001576515778,
                                    permissions: new ChatPermissions()
                                    {
                                        CanSendMessages = true
                                    },
                                    untilDate: DateTime.Now.AddMinutes(1)
                                    );

                                //Process.Start("C:\\Users\\DmitryDima\\Desktop\\Диплом\\NetworkDiscoveryWatcherF\\bin\\Debug\\NetworkDiscoveryWatcherF");
                                return;

                            }

                            if (message.Text.Contains("tracking"))
                            {


                                // await botClient.SendTextMessageAsync(message.Chat.Id, "Введите фамилию преподавателя: ");
                                //await botClient.SendChatActionAsync(update.Message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);

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


                            if (message.Text.Contains("ipaddress"))
                            {
                                
                                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                                var localIp = host.AddressList.FirstOrDefault(ip1 => ip1.AddressFamily == AddressFamily.InterNetwork).ToString();


                                string ipAddress = "";
                                if (Dns.GetHostAddresses(Dns.GetHostName()).Length > 0)
                                {
                                    ipAddress = Dns.GetHostAddresses(Dns.GetHostName())[0].ToString();
                                   await botClient.SendTextMessageAsync(message.Chat, (string.Format("{0} : {1}", ipAddress, localIp)), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);
                                }
       





                                return;
                                }

                                if (message.Text.Contains("killclient"))
                                {


                                try
                                {
                                    // убить все процессы с заданным именем:
                                    foreach (Process thisproc in Process.GetProcessesByName("ClientPing"))
                                    {
                                        // если просто закрыть нельзя, то убить!
                                        if (!thisproc.CloseMainWindow())

                                        {
                                            await botClient.SendTextMessageAsync(message.Chat, ($"Процесс клиента найден и завершен"), replyToMessageId: update.Message.MessageId, cancellationToken: cancellationToken);
                                           thisproc.Kill();

                                        }
                                       // await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId - 1, cancellationToken: cancellationToken);
                                    }
                                }
                                
                                    catch
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, ($" Ошибка : Процесс клиента ненайден"));
                                        await botClient.SendTextMessageAsync(message.Chat, ($" Автоматическая очиcтка сообщения"));
                                    await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId - 1, cancellationToken: cancellationToken);
                                }
                                    

                                }

                            if (message.Text.ToLower().Contains("ddd") )
                            {
                                await botClient.SendTextMessageAsync(message.Chat, ($" Сообщение не принято, напишите команду"));
                                return;
                            }




                            }


                        }

                    }




                
            }

            catch (System.Net.WebException e)
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    MessageBox.Show(Convert.ToString(e));
                    var message = update.Message;
                    await botClient.SendTextMessageAsync(message.Chat, ($" Ошибка : Telegram по какой то причине не принимает сообщения! Удаление последнего сообщения!"));
                    await bot.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId - 1, cancellationToken: cancellationToken);
                }
            }
            catch (Telegram.Bot.Exceptions.RequestException e)
            {

                MessageBox.Show(Convert.ToString(e));
                //Process.GetCurrentProcess().Kill();
               // Process.GetCurrentProcess().Kill();
            }


        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }


    }
}

