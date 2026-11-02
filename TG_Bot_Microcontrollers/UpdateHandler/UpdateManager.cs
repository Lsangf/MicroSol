using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TG_Bot_Microcontrollers;
using BotUser = TG_Bot_Microcontrollers.UserManager.User;

namespace UpdateHandler
{
    public class UpdateManager
    {
        private static readonly long ADMIN_CHAT_ID = 0;
        private static readonly Dictionary<long, BotUser> users = new();
        private static readonly Dictionary<long, int> menuMessageIds = new();
        private static readonly Dictionary<long, string> menuTexts = new();
        private static readonly Esp32Client esp32Client = new();
        private static bool greenOn = false;
        private static bool redOn = false;
        private static bool yellowOn = false;
        private static readonly int servoStop = 90;
        private static readonly int servoLeft = 80;
        private static readonly int servoRight = 100;
        private static bool servoStartBool = false;
        private static bool servoLeftBool = false;
        private static bool servoRightBool = false;
        private static readonly InlineKeyboardMarkup keyboard =
            new InlineKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("🟢 Toggle green led", "toggle_green_led")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("🔴 Toggle red led", "toggle_red_led")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("🟡 Toggle yellow led", "toggle_yellow_led")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("⬅️25 Servo Control left 25", "servo_control_left_25")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("➡️25 Servo Control right 25", "servo_control_right_25")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("🔄️ Servo Control start", "servo_control_start")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("⛔ Servo Control stop", "servo_control_stop")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("❇️ Servo Control reset", "servo_control_reset")
                    }

                }
            );

        private static string BuildMenuText()
        {
            return 
                $"""
                ———————— ESP32 BOT ————————

                Capabilities:

                🟢 Green LED  | {(greenOn ? "ON" : "OFF")}
                🔴 Red LED    | {(redOn ? "ON" : "OFF")}
                🟡 Yellow LED | {(yellowOn ? "ON" : "OFF")}

                Servo Control:

                ⬅️25 Servo left        | {(servoLeftBool ? "ON" : "OFF")}
                ➡️25 Servo right       | {(servoRightBool ? "ON" : "OFF")}
                🔄️ Servo around start: | {(servoStartBool ? "YES" : "NO")}
                ⛔ Servo stop:         | {(servoStartBool ? "NO" : "YES")}
                                ❇️ Servo reset   
                """;
        }

        private static async Task ShowOrUpdateMenu(ITelegramBotClient bot, long chatId)
        {
            string text = BuildMenuText();

            if (menuMessageIds.ContainsKey(chatId))
            {
                if (menuTexts.ContainsKey(chatId) && menuTexts[chatId] == text)
                {
                    return;
                }

                int messageId = menuMessageIds[chatId];

                await bot.EditMessageText(
                    chatId,
                    messageId,
                    text,
                    replyMarkup: keyboard);

                menuTexts[chatId] = text;
                return;
            }

            Message menuMessage = await bot.SendMessage(
                chatId,
                text,
                replyMarkup: keyboard);

            menuMessageIds[chatId] = menuMessage.Id;
            menuTexts[chatId] = text;
        }

        public static async Task HandleUpdate(
            ITelegramBotClient bot,
            Update update,
            CancellationToken cancellationToken)
        {
            if (update.Message != null)
            {
                long chatId = update.Message.Chat.Id;
                User telegramUser = update.Message.From;
                string messageText = update.Message.Text ?? "";

                if (!users.ContainsKey(chatId))
                {
                    BotUser newUser = new BotUser
                    {
                        Id = chatId,
                        FirstName = telegramUser.FirstName,
                        LastName = telegramUser.LastName ?? "",
                        Username = telegramUser.Username ?? "",
                        RegisteredAt = DateTime.Now,
                        IsApproved = chatId == ADMIN_CHAT_ID
                    };

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
                                });

                        await bot.SendMessage(ADMIN_CHAT_ID,
                            $"""
                            🔔 New access request

                            Name: {newUser.FirstName}
                            Last name: {newUser.LastName}
                            Username: @{newUser.Username}
                            Chat ID: {newUser.Id}

                            Allow access?
                            """,
                            replyMarkup: adminKeyboard);
                    }
                }

                BotUser user = users[chatId];

                switch (messageText)
                {
                    case "/start":
                        await bot.SendMessage(
                            chatId,
                            $"Hi, {user.FirstName}!"
                        );

                        await ShowOrUpdateMenu(bot, chatId);
                        break;

                    case "/menu":
                        await ShowOrUpdateMenu(bot, chatId);
                        break;
                }
                return;
            }

            if (update.CallbackQuery != null)
            {
                string data = update.CallbackQuery.Data ?? "";
                await bot.AnswerCallbackQuery(update.CallbackQuery.Id);

                if (update.CallbackQuery.Message == null)
                {
                    return;
                }

                long chatId = update.CallbackQuery.Message.Chat.Id;

                if (data.StartsWith("allow_user:"))
                {
                    if (chatId != ADMIN_CHAT_ID)
                    {
                        await bot.SendMessage(
                            chatId,
                            "⛔ You are not the administrator.");
                        return;
                    }

                    string userIdText = data.Replace("allow_user:", "");
                    long userId = long.Parse(userIdText);

                    if (!users.ContainsKey(userId))
                    {
                        await bot.SendMessage(
                            chatId,
                            "❌ User not found.");
                        return;
                    }

                    BotUser user = users[userId];
                    user.IsApproved = true;

                    Console.WriteLine($"Access approved: {user.FirstName} ({user.Id})");

                    await bot.SendMessage(
                        user.Id,
                        "✅ Access approved!\n\nYou can now use ESP32 controls.");

                    await bot.SendMessage(
                        chatId,
                        $"✅ Access granted to {user.FirstName}.");
                    return;
                }

                if (data.StartsWith("deny_user:"))
                {
                    if (chatId != ADMIN_CHAT_ID)
                    {
                        await bot.SendMessage(
                            chatId,
                            "⛔ You are not the administrator.");
                        return;
                    }

                    string userIdText = data.Replace("deny_user:", "");
                    long userId = long.Parse(userIdText);

                    if (!users.ContainsKey(userId))
                    {
                        await bot.SendMessage(
                            chatId,
                            "❌ User not found.");
                        return;
                    }

                    BotUser user = users[userId];
                    user.IsApproved = false;

                    Console.WriteLine($"Access denied: {user.FirstName} ({user.Id})");

                    await bot.SendMessage(
                        user.Id,
                        "❌ Access denied.");

                    await bot.SendMessage(
                        chatId,
                        $"❌ Access denied for {user.FirstName}.");

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

                string result;

                switch (data)
                {
                    case "toggle_green_led":
                        result = await esp32Client.ToggleGreenLed();
                        greenOn = !greenOn;
                        break;

                    case "toggle_red_led":
                        result = await esp32Client.ToggleRedLed();
                        redOn = !redOn;
                        break;

                    case "toggle_yellow_led":
                        result = await esp32Client.ToggleYellowLed();
                        yellowOn = !yellowOn;
                        break;

                    case "servo_control_left_25":
                        result = await esp32Client.TurnLeft25Servo();
                        servoLeftBool = !servoLeftBool;
                        servoRightBool = !servoLeftBool;
                        break;

                    case "servo_control_right_25":
                        result = await esp32Client.TurnRight25Servo();
                        servoRightBool = !servoRightBool;
                        servoLeftBool = !servoRightBool;
                        break;

                    case "servo_control_start":
                        result = await esp32Client.StartServo();
                        servoStartBool = !servoStartBool;
                        if (result.Contains("Servo started left"))
                        {
                            servoLeftBool = true;
                            servoRightBool = false;
                        }
                        else if (result.Contains("Servo started right"))
                        {
                            servoLeftBool = false;
                            servoRightBool = true;
                        }
                        break;

                    case "servo_control_stop":
                        result = await esp32Client.StopServo();
                        servoStartBool = !servoStartBool;
                        servoLeftBool = false;
                        servoRightBool = false;
                        break;

                    case "servo_control_reset":
                        result = await esp32Client.ResetServo();
                        servoStartBool = false;
                        servoLeftBool = false;
                        servoRightBool = false;
                        break;

                    default:
                        return;
                }

                Console.WriteLine(result);
                
                await ShowOrUpdateMenu(bot, chatId);
            }
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
