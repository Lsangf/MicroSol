using Telegram.Bot;
using Telegram.Bot.Polling;
using UpdateHandler;

namespace TG_Bot_Microcontrollers
{
    public class StartBot
    {
        const string BOT_TOKEN = "-";

        static async Task Main()
        {
            TelegramBotClient bot = new(BOT_TOKEN);
            ReceiverOptions receiverOptions = new ReceiverOptions
            {
                DropPendingUpdates = true
            };
            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;

            bot.StartReceiving(
                UpdateManager.HandleUpdate,
                UpdateManager.HandleError,
                receiverOptions,
                cancellationToken);

            Console.WriteLine("Bot launched.");
            Console.ReadLine();
        }
    }
}
