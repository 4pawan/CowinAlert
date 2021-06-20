using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Cowin.Console20
{
    class Program
    {
        static ITelegramBotClient botClient;
        //https://api.telegram.org/bot12345678:ADAFEWQDLKDS*&^SD%FEWRRr/getUpdates  migrate_to_chat_id
        static ChatId Id = new ChatId(-123456789);

        static void Main()
        {
            botClient = new TelegramBotClient(CalendarByDistrictModel.Token);

            var me = botClient.GetMeAsync().Result;
            Console.WriteLine(
                $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );

            //var details = botClient.get().Result;

            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            botClient.StopReceiving();
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            string cmd = e.Message.Text;

            for (; !string.Equals(cmd, "done", StringComparison.InvariantCulture);)
            {
                if (cmd != "done")
                {

                    Console.WriteLine($"Received a text message in chat {e.Message.Chat.Id}.");
                    var msg = Printdata();
                    await botClient.SendTextMessageAsync(
                        chatId: Id,
                        text: msg, ParseMode.Html
                    );
                }
                else
                {
                    Console.WriteLine($"Thank you for using this service...I will not check now !");
                    await botClient.SendTextMessageAsync(
                        chatId: Id,
                        text: "Thank you for using this service...I will not check now"
                    );
                    break;
                }
                // sleep thread
                Thread.Sleep(TimeSpan.FromMinutes(CalendarByDistrictModel.TimeToSleepInMinute));
            }
        }

        static string Printdata()
        {
            var list = CalendarByDistrictModel.GetAvailableSlots();
            StringBuilder builder = new StringBuilder();
            if (list != null && list.Any())
            {
                int i = 1;
                foreach (CalendarByDistrictModel.Center c in list)
                {
                    foreach (var session in c.sessions)
                    {
                        if (session.available_capacity_dose1 <= 0) continue;
                        var msg = i++ + "]" + " name: " + c.name + " ,address:" + c.address +
                                  " ,block_name : " + c.block_name +
                                  " ,Pin code : <u>" + c.pincode + "</u>" +
                                  " ,vaccine : " + session.vaccine +
                                  " ,available_capacity_dose1 : <u>" + session.available_capacity_dose1 + "</u>" +
                                  " ,Available Date : <u>" + session.date + "</u>" +
                                  " ,vaccine fee : " + c.vaccine_fees?.FirstOrDefault()?.fee;


                        builder.Append(msg);
                        builder.Append(Environment.NewLine);
                        builder.Append(Environment.NewLine);
                    }

                }
            }
            else
            {
                builder.Append("Slots not available");
            }

            builder.Append($"===== executed at {DateTime.Now} ====");
            return builder.ToString();
        }

    }

}
