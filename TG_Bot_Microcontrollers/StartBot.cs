using System.Reflection.Metadata;
using Telegram.Bot;

namespace ArduinoUno
{
    internal class StartBot
    {
        static async Task Main()
        {
            var botClient = new TelegramBotClient("/");
            string chatId = "/"; 
            string messageText = "Hello, World!";

            await botClient.SendMessage(chatId, messageText);


        }
    }
}
