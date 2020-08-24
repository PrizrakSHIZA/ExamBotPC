using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExamBotPC.Commands
{
    class StatsMenuCmd : Command
    {
        public override string Name => "Моя статистика 📊";

        public override bool forAdmin => false;

        public async override void Execute(MessageEventArgs e)
        {
            User user = Program.GetCurrentUser(e);
            ReplyKeyboardMarkup menu = new ReplyKeyboardMarkup();
            menu.Keyboard = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Мої життя ♥"),
                    new KeyboardButton("Статистика помилок ❗")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Мій рейтинг 📈"),
                    new KeyboardButton("Головне меню ◀"),
                },
            };
            menu.ResizeKeyboard = true;
            menu.OneTimeKeyboard = false;
            await Program.bot.SendTextMessageAsync(user.id, "Моя статистика:", replyMarkup: menu);
        }
    }
}
