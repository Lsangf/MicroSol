using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TG_Bot_Microcontrollers;
using UserStateManager;
using BotUser = TG_Bot_Microcontrollers.UserManager.User;

namespace UpdateHandler
{
    public class UpdateManager
    {
        static long ADMIN_CHAT_ID = 0;
        static Dictionary<long, UserState> userStates = new Dictionary<long, UserState>();
        static Dictionary<long, BotUser> users = new Dictionary<long, BotUser>();
        static Esp32Client esp32Client = new Esp32Client();
        static Dictionary<long, int> menuMessageIds = new Dictionary<long, int>();
        static bool greenOn = false;
        static bool redOn = false;
        static bool yellowOn = false;

        static InlineKeyboardButton turnOnGreenLedButton =
                    InlineKeyboardButton.WithCallbackData("🟢 Turn on green led", "turn_on_green_led");

        static InlineKeyboardButton turnOnRedLedButton =
            InlineKeyboardButton.WithCallbackData("🔴 Turn on red led", "turn_on_red_led");

        static InlineKeyboardButton turnOnYellowLedButton =
            InlineKeyboardButton.WithCallbackData("🟡 Turn on yellow led", "turn_on_yellow_led");

        static InlineKeyboardMarkup keyboard =
            new InlineKeyboardMarkup(
                new[]
                {
                    new[] { turnOnGreenLedButton },
                    new[] { turnOnRedLedButton },
                    new[] { turnOnYellowLedButton }
                }
            );

        public static async Task HandleUpdate(
            ITelegramBotClient bot,
            Update update,
            CancellationToken cancellationToken)
        {
            if (update.Message != null)
            {
                long chatId = update.Message.Chat.Id;
                User telegramUser = update.Message.From;

                if (!users.ContainsKey(chatId))
                {
                    BotUser newUser = new BotUser();

                    newUser.Id = chatId;
                    newUser.FirstName = telegramUser.FirstName;
                    newUser.LastName = telegramUser.LastName ?? "";
                    newUser.Username = telegramUser.Username ?? "";
                    newUser.RegisteredAt = DateTime.Now;
                    newUser.IsApproved = chatId == ADMIN_CHAT_ID;

                    users.Add(chatId, newUser);

                    Console.WriteLine($"New user: {newUser.FirstName}");
                    Console.WriteLine($"Chat ID: {newUser.Id}");
                    Console.WriteLine($"Approved: {newUser.IsApproved}");

                    if (chatId != ADMIN_CHAT_ID)
                    {
                        InlineKeyboardButton allowButton =
                            InlineKeyboardButton.WithCallbackData("✅ Allow", $"allow_user:{newUser.Id}");

                        InlineKeyboardButton denyButton =
                            InlineKeyboardButton.WithCallbackData("❌ Deny", $"deny_user:{newUser.Id}");

                        InlineKeyboardMarkup adminKeyboard =
                            new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[] { allowButton, denyButton }
                                }
                            );

                        await bot.SendMessage(
                            ADMIN_CHAT_ID,
                            $"""
                            🔔 New access request

                            Name: {newUser.FirstName}
                            Last name: {newUser.LastName}
                            Username: @{newUser.Username}
                            Chat ID: {newUser.Id}

                            Allow access?
                            """,
                            replyMarkup: adminKeyboard
                        );
                    }
                }

                BotUser user = users[chatId];

                

                switch (update.Message.Text)
                {
                    case "/start":

                        await bot.SendMessage(
                            chatId,
                            $"Hi, {user.FirstName}!"
                        );

                        await Task.Delay(500);

                        Message menuMessage = await bot.SendMessage(
                            chatId,
                            """
                            ———————— ESP32 BOT ————————

                            Capabilities:

                            🟢 Turn on green led
                            🔴 Turn on red led
                            🟡 Turn on yellow led
                            """,
                            replyMarkup: keyboard);

                        menuMessageIds[chatId] = menuMessage.Id;
                        break;

                    case "/menu":

                        menuMessage = await bot.SendMessage(
                            chatId,
                            $"""
                            ———————— ESP32 BOT ————————

                            🟢 Green LED:  {(greenOn ? "ON" : "OFF")}
                            🔴 Red LED:    {(redOn ? "ON" : "OFF")}
                            🟡 Yellow LED: {(yellowOn ? "ON" : "OFF")}
                            """,
                            replyMarkup: keyboard);

                        menuMessageIds[chatId] = menuMessage.Id;
                        break;
                }
            }


            if (update.CallbackQuery != null)
            {
                string data = update.CallbackQuery.Data;
                long chatId = update.CallbackQuery.Message.Chat.Id;

                await bot.AnswerCallbackQuery(
                    update.CallbackQuery.Id
                );

                if (data.StartsWith("allow_user:"))
                {
                    if (chatId != ADMIN_CHAT_ID)
                    {
                        await bot.SendMessage(
                            chatId,
                            "⛔ You are not the administrator."
                        );
                        return;
                    }

                    string userIdText = data.Replace("allow_user:", "");
                    long userId = long.Parse(userIdText);

                    if (!users.ContainsKey(userId))
                    {
                        await bot.SendMessage(
                            chatId,
                            "❌ User not found."
                        );
                        return;
                    }

                    BotUser user = users[userId];

                    user.IsApproved = true;

                    Console.WriteLine($"Access approved: {user.FirstName} ({user.Id})");

                    await bot.SendMessage(
                        user.Id,
                        "✅ Access approved!\n\nYou can now use ESP32 controls."
                    );

                    await bot.SendMessage(
                        chatId,
                        $"✅ Access granted to {user.FirstName}."
                    );

                    return;
                }

                if (data.StartsWith("deny_user:"))
                {
                    if (chatId != ADMIN_CHAT_ID)
                    {
                        await bot.SendMessage(
                            chatId,
                            "⛔ You are not the administrator."
                        );
                        return;
                    }

                    string userIdText = data.Replace("deny_user:", "");
                    long userId = long.Parse(userIdText);

                    if (!users.ContainsKey(userId))
                    {
                        await bot.SendMessage(
                            chatId,
                            "❌ User not found."
                        );
                        return;
                    }

                    BotUser user = users[userId];

                    user.IsApproved = false;

                    Console.WriteLine($"Access denied: {user.FirstName} ({user.Id})"
                    );

                    await bot.SendMessage(
                        user.Id,
                        "❌ Access denied."
                    );

                    await bot.SendMessage(
                        chatId,
                        $"❌ Access denied for {user.FirstName}."
                    );

                    return;
                }

                if (!users.ContainsKey(chatId))
                {
                    return;
                }

                BotUser currentUser = users[chatId];

                if (!currentUser.IsApproved)
                {
                    await bot.SendMessage(
                        chatId,
                        "⛔ Access has not been approved yet.");
                    return;
                }

                string result = "";

                switch (data)
                {
                    case "turn_on_green_led":
                        Console.WriteLine("The green led button was pressed");

                        result = await esp32Client.TurnOnGreenLed();
                        Console.WriteLine(result);
                        greenOn = true;
                        if (menuMessageIds.ContainsKey(chatId))
                        {
                            int menuMessageId = menuMessageIds[chatId];

                            await bot.EditMessageText(
                                chatId,
                                menuMessageId,
                                $"""
                                ———————— ESP32 BOT ————————

                                🟢 Green LED:  {(greenOn ? "ON" : "OFF")}
                                🔴 Red LED:    {(redOn ? "ON" : "OFF")}
                                🟡 Yellow LED: {(yellowOn ? "ON" : "OFF")}
                                """,
                                replyMarkup: keyboard);
                        }
                        break;

                    case "turn_on_red_led":
                        Console.WriteLine("The red led button was pressed");

                        result = await esp32Client.TurnOnRedLed();
                        Console.WriteLine(result);
                        redOn = true;
                        if (menuMessageIds.ContainsKey(chatId))
                        {
                            int menuMessageId = menuMessageIds[chatId];

                            await bot.EditMessageText(
                                chatId,
                                menuMessageId,
                                $"""
                                ———————— ESP32 BOT ————————

                                🟢 Green LED:  {(greenOn ? "ON" : "OFF")}
                                🔴 Red LED:    {(redOn ? "ON" : "OFF")}
                                🟡 Yellow LED: {(yellowOn ? "ON" : "OFF")}
                                """,
                                replyMarkup: keyboard);
                        }
                        break;

                    case "turn_on_yellow_led":
                        Console.WriteLine("The yellow led button was pressed");

                        result = await esp32Client.TurnOnYellowLed();
                        Console.WriteLine(result);
                        yellowOn = true;
                        if (menuMessageIds.ContainsKey(chatId))
                        {
                            int menuMessageId = menuMessageIds[chatId];

                            await bot.EditMessageText(
                                chatId,
                                menuMessageId,
                                $"""
                                ———————— ESP32 BOT ————————

                                🟢 Green LED:  {(greenOn ? "ON" : "OFF")}
                                🔴 Red LED:    {(redOn ? "ON" : "OFF")}
                                🟡 Yellow LED: {(yellowOn ? "ON" : "OFF")}
                                """,
                                replyMarkup: keyboard);
                        }
                        break;
                }
            }
        }

        public static Task SendMessageTo(ITelegramBotClient bot, long chatIdOut, long chatIdIn, string message)
        {
            return bot.SendMessage(chatIdIn, message);
        }

        public static Task HandleError(
            ITelegramBotClient bot,
            Exception exception,
            CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);

            return Task.CompletedTask;
        }
    }
}
