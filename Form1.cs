using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Color = System.Drawing.Color;

namespace MyBot
{
    public partial class Form1 : Form
    {
        static string token = string.Empty;
        private Thread Thread;
        Telegram.Bot.TelegramBotClient client;
        private ReplyKeyboardMarkup ReplyKeyboardMarkup;
        private int positiveVotes = 0;
        private int negativeVotes = 0;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) // در اینجا ما دکمه های که کاربر برای تعامل با بات و انجام دستورات نیاز دارد را میسازیم
        {
            KeyboardButton[] row1 = { new KeyboardButton("درباره ما" + "\U00002764"), new KeyboardButton("تماس با ما" + "\U00002709") };
            KeyboardButton[] row2 = { new KeyboardButton("نظر سنجی") };


            ReplyKeyboardMarkup = new ReplyKeyboardMarkup(new[] { row1,row2 })
            {
                ResizeKeyboard = true
            };



        }

        private  void button1_Click(object sender, EventArgs e)//دکمه استارت برای ایجاد ترد و اجرای متد runbot
        {
            token = textBox1.Text;
            Thread = new Thread(new ThreadStart(RunBot));
            Thread.Start();
        }
         async void  RunBot()// در این متد ما دستوراتی که برای انها دکمه ساختیم را کنترل میکنیم
        {
            client = new Telegram.Bot.TelegramBotClient(token);// یک نمونه گیری برای ارتباط با بات

            this.Invoke(new Action(() =>//برای تغیرات فرم در حالت انلاین باید ان را بخاطر thred ها در invoke قرار دهیم.
            {
                toolStripStatusLabel1.Text = "Online";
                toolStripStatusLabel1.ForeColor = Color.Green;
            }));
            int offset = 0;
            while (true)//یک حلقه ایجاد میکنیم تا دستورات بطور مداوم پردازش شود
            {
                try
                {
                    Telegram.Bot.Types.Update[] updates = client.GetUpdatesAsync(offset).Result;
                    foreach (var up in updates) // این حلقه و نمونه را برای کنترل دستورات نیاز داریم
                    {

                        offset = up.Id + 1;
                        if (up.Type == UpdateType.CallbackQuery)//اگر پیام از نوع برگشتی از سوی کاربر بود
                        {
                            var callbackQuery = up.CallbackQuery;

                            // ثبت نوع رای برای اشکال‌زدایی
                            Console.WriteLine($"Callback received: {callbackQuery.Data}");

                            // پیام برای اطلاع‌رسانی رأی ثبت شده
                            string voteMessage = "";

                            if (callbackQuery.Data == "positive_vote")
                            {
                                positiveVotes++;
                                voteMessage = "شما رای مثبت ثبت کردید. 👍";
                                await client.AnswerCallbackQueryAsync(callbackQuery.Id, "رای شما ثبت شد: مثبت");
                            }
                            else if (callbackQuery.Data == "negative_vote")
                            {
                                negativeVotes++;
                                voteMessage = "شما رای منفی ثبت کردید. 👎";
                                await client.AnswerCallbackQueryAsync(callbackQuery.Id, "رای شما ثبت شد: منفی");
                            }

                            // ارسال پیام به کاربر برای اطلاع‌رسانی درباره نظر ثبت شده
                            await client.SendTextMessageAsync(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: voteMessage
                            );

                            // ساخت دوباره inline keyboard با نتایج به‌روز شده
                            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("👍 مثبت", "positive_vote"),
            InlineKeyboardButton.WithCallbackData("👎 منفی", "negative_vote")
        }
    });

                            // ویرایش پیام اصلی با نتایج جدید
                            await client.EditMessageTextAsync(
                                chatId: callbackQuery.Message.Chat.Id,
                                messageId: callbackQuery.Message.MessageId,
                                text: $"نتیجه فعلی:\n 👍 مثبت‌ها: {positiveVotes}\n 👎 منفی‌ها: {negativeVotes}",
                                replyMarkup: inlineKeyboard
                            );
                        }
                        if (up.Message == null)
                        {
                            continue;
                        }
                        var text = up.Message.Text.ToLower();
                        var from = up.Message.From;
                        var chatid = up.Message.Chat.Id;
                        if (text.Contains("/start"))
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine(from.Username + "به بات ما خوش آمدی");
                            sb.AppendLine("میتونی از تمام قابلیت ها استفاده کنی");
                            sb.AppendLine("درباره ما : /abut");
                            sb.AppendLine("تماس با ما:" + "/contacts");


                            client.SendTextMessageAsync(chatid, sb.ToString(), parseMode: default, disableNotification: false, disableWebPagePreview: false, replyToMessageId: 0, replyMarkup: ReplyKeyboardMarkup);// ساخت کیبورد و سایر تنظیمات برای پیام ها
                        }
                        else if (text.Contains("/abut") || text.Contains("درباره ما"))
                        {
                            StringBuilder sd = new StringBuilder();
                            sd.AppendLine("این یک پیغام تست است");
                            sd.AppendLine("برای عضویت در کانال ما، روی دکمه زیر کلیک کنید.");

                            // ساخت inline keyboard با لینک
                            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                 new []
                                        {
                                                InlineKeyboardButton.WithUrl("عضویت در کانال", "https://t.me/rmmkanal") // لینک به کانال شما
                                        }
                             });

                            await client.SendTextMessageAsync(
                                chatId: chatid,
                                text: sd.ToString(),
                                replyMarkup: inlineKeyboard
                            );
                        }
                        else if (text.Contains("/contacts") || text.Contains("تماس با ما"))
                        {
                            StringBuilder sb1 = new StringBuilder();
                            sb1.AppendLine("با کدام شماره تماس میگیرید؟");
                            KeyboardButton[] row2 = { new KeyboardButton("09194524545"), new KeyboardButton("09185802821") };

                            ReplyKeyboardMarkup ReplyKeyboardMarkup2 = new ReplyKeyboardMarkup(new[] { row2 })
                            {
                                ResizeKeyboard = true
                            };
                            client.SendTextMessageAsync(chatid, sb1.ToString(), parseMode: default, disableNotification: false, disableWebPagePreview: false, replyToMessageId: 0, replyMarkup: ReplyKeyboardMarkup2);// ساخت کیبورد و سایر تنظیمات برای پیام ها

                        }
                        else if (text.Contains("نظر سنجی"))
                        {
                            StringBuilder sb2 = new StringBuilder();
                            sb2.AppendLine("برای نظر سنجی یک گزینه را انتخاب کنید");

                            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                    new []
                                            {
                                                   InlineKeyboardButton.WithCallbackData("👍 رای مثبت",  "positive_vote"),
                                                   InlineKeyboardButton.WithCallbackData("👎 رای منفی", "negative_vote")
                                            }
                            });

                            await client.SendTextMessageAsync(
                                chatId: chatid,
                                text: sb2.ToString(),
                                replyMarkup: inlineKeyboard
                            );
                        }
                        
                        else
                        {
                            StringBuilder sb1 = new StringBuilder();
                            sb1.AppendLine("پیام غیر مفهوم ");
                            client.SendTextMessageAsync(chatid, sb1.ToString());
                        }
                        dataGridView1.Invoke(new Action(() =>// پر کردن با اطلاعات گرفته شده از کاربر درdatagridview
                        {
                            dataGridView1.Rows.Add(up.Message.Date.ToString("yyy/mm/dd - hh:mm"), up.Message.MessageId, chatid, from.Username, text);
                        }));


                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace);
                }


            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Thread.Abort();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int chatid = int.Parse(dataGridView1.CurrentRow.Cells[2].Value.ToString());
                client.SendTextMessageAsync(chatid, textBox2.Text, parseMode: ParseMode.Html);
                textBox2.Text = "";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = openFileDialog.FileName;

            }
        }

      
        private async void button5_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int chatId = int.Parse(dataGridView1.CurrentRow.Cells[2].Value.ToString()); // شناسه چت از دیتاگراید
                string filePath = textBox3.Text; // مسیر فایل از تکست‌باکس

                if (System.IO.File.Exists(filePath))
                {
                    using (FileStream imageFile = System.IO.File.OpenRead(filePath))
                    {
                        // ارسال عکس با استفاده از InputFileStream
                        await client.SendPhotoAsync(
                            chatId: chatId,
                            photo: new InputFileStream(imageFile, Path.GetFileName(filePath)),
                            caption: "تست"
                            

                        ) ;
                    }
                }
                else
                {
                    MessageBox.Show("فایل عکس مورد نظر یافت نشد. لطفا مسیر فایل را بررسی کنید.");
                }
            }
            else
            {
                MessageBox.Show("لطفا یک ردیف را از dataGridView انتخاب کنید.");
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int chatId = int.Parse(dataGridView1.CurrentRow.Cells[2].Value.ToString()); // شناسه چت از دیتاگرید
                string filePath = textBox3.Text; // مسیر فایل از تکست‌باکس

                if (System.IO.File.Exists(filePath))
                {
                    using (FileStream videoFile = System.IO.File.OpenRead(filePath))
                    {
                        // ارسال ویدیو با استفاده از InputFileStream
                        await client.SendVideoAsync(
                            chatId: chatId,
                            video: new InputFileStream(videoFile, Path.GetFileName(filePath)),
                            caption: "این یک ویدیوی نمونه است"
                        );
                    }
                }
                else
                {
                    MessageBox.Show("فایل ویدیو مورد نظر یافت نشد. لطفا مسیر فایل را بررسی کنید.");
                }
            }
            else
            {
                MessageBox.Show("لطفا یک ردیف را از dataGridView انتخاب کنید.");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            client.SendTextMessageAsync(textBox4.Text,textBox5.Text,parseMode:ParseMode.Html);
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            if (textBox4.Text != null)
            {
                string filePath = textBox3.Text; // مسیر فایل از تکست‌باکس

                if (System.IO.File.Exists(filePath))
                {
                    using (FileStream imageFile = System.IO.File.OpenRead(filePath))
                    {
                        // ارسال عکس با استفاده از InputFileStream
                        await client.SendPhotoAsync(
                            chatId: textBox4.Text,
                            photo: new InputFileStream(imageFile, Path.GetFileName(filePath)),
                            caption: "تست"


                        );
                    }
                }
                else
                {
                    MessageBox.Show("فایل عکس مورد نظر یافت نشد. لطفا مسیر فایل را بررسی کنید.");
                }
            }
            else
            {
                MessageBox.Show("لطفا یک ردیف را از dataGridView انتخاب کنید.");
            }

        }

        private async void button6_Click(object sender, EventArgs e)
        {
            if (textBox4.Text != null)
            {
                string filePath = textBox3.Text; // مسیر فایل از تکست‌باکس

                if (System.IO.File.Exists(filePath))
                {
                    using (FileStream videoFile = System.IO.File.OpenRead(filePath))
                    {
                        // ارسال ویدیو با استفاده از InputFileStream
                        await client.SendVideoAsync(
                            chatId: textBox4.Text,
                            video: new InputFileStream(videoFile, Path.GetFileName(filePath)),
                            caption: "این یک ویدیوی نمونه است"
                        );
                    }
                }
                else
                {
                    MessageBox.Show("فایل ویدیو مورد نظر یافت نشد. لطفا مسیر فایل را بررسی کنید.");
                }
            }
            else
            {
                MessageBox.Show("لطفا یک ردیف را از dataGridView انتخاب کنید.");
            }
        }
    }
}
    
        
    