using ExamBotPC.Tests;
using ExamBotPC.Tests.Questions;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace ExamBotPC.Commands
{
    class A_TestList : Command
    {
        public override string Name => "/testlist";

        public override bool forAdmin => true;

        public async override void Execute(MessageEventArgs e)
        {
            Program.UpdateTests();
            User user = Program.GetCurrentUser(e);
            try
            {
                string text = "";
                int index = e.Message.Text.IndexOf(" ");
                if (index == -1)
                {
                    text = "Для перегляду питань оберить тест за номером (/testlist 'id')\n";
                    for (int i = 0; i < Program.testlist.Count; i++)
                    {
                        text += $"{i}: {Program.testlist[i].Text}\n";
                    }
                }
                else
                {
                    int id = Int32.Parse(e.Message.Text.Remove(0, Name.Length));
                    text = $"Тест {id}: {Program.testlist[id].Text}\n";
                    for (int i = 0; i < Program.testlist[id].questions.Count; i++)
                    {
                        text += "----------------------------\n";
                        Question question = Program.testlist[id].questions[i];
                        //Check question type
                        if (question.GetType() == typeof(TestQuestion))
                        {
                            TestQuestion q = (TestQuestion)question;
                            text += $"Питання {i + 1}, тип 'Тест':\n{q.text}\n";
                            text += $"Варіанти відповіді: \n";
                            foreach (string s in q.variants)
                            {
                                text += s + "\n";
                            }
                            text += $"Правильна відповідь: {q.answer}\n";
                            text += $"Кількіст стовпців: {q.columns}\n";
                            text += $"Кількість балів: {q.points}\n";
                        }
                        else if (question.GetType() == typeof(FreeQuestion))
                        {
                            FreeQuestion q = (FreeQuestion)question;
                            text += $"Питання {i + 1}, тип 'Вільна відповідь':\n{q.text}\n";
                            text += $"Правильна відповідь: {q.answer}\n";
                            text += $"Кількість балів: {q.points}\n";

                        }
                        else if (question.GetType() == typeof(ConformityQuestion))
                        {
                            ConformityQuestion q = (ConformityQuestion)question;
                            text += $"Питання {i + 1}, тип 'Відповідність':\n{q.text}\n";
                            text += $"Правильна відповідь: {q.answer}\n";
                            text += $"Кількість балів: {q.points}\n";

                        }
                    }
                }
                await Program.bot.SendTextMessageAsync(user.id, text);
            }
            catch (Exception exception)
            {
                await Program.bot.SendTextMessageAsync(user.id, $"Виникла помилка при виконанні програми. Текст помилки: '{exception.Message}'");
            }
        }
    }
}
