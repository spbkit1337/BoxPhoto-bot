using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot; //нужная библеотека для работы
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace BoxPhoto
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //api ключ (токен) от бота телеги
            var client = new TelegramBotClient("");
            client.StartReceiving(Update , Error); //принятие методов
            Console.ReadLine(); // чтобы не закрывалось
        }

        //async чтобы не зависало
        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
           var message = update.Message; //переменная

            //обычный ответ на текст
            if(message.Text != null)
            {    
                Console.WriteLine($"{message.Chat.FirstName} | {message.Text}");  //кто и что отправляет вывестив консоль (необязательно)

                if (message.Text.ToLower().Contains("здорова")) //если текст "здорова" то бот вернет текст "здоровей видали"
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Здоровей видали");
                    return;
                }
            }

            //ответ на присланную фотку
            if (message.Photo != null)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "норм фотка но отправь документом");
                return;
            }

            //обработка фотоографий в виде документа
            if (message.Document != null)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "сейчас сделаю лучше"); //ответ на присланную фотку

                //переменные которые предлагались в документации 
                var fileId = update.Message.Document.FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);
                var filePath = fileInfo.FilePath;

                //скачивает фотку отправленную боту
                string destinationFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{message.Document.FileName}"; //качает на рабочий стол
                await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                await botClient.DownloadFileAsync(filePath , fileStream);
                fileStream.Close();

                //сначало он тут берет дроплета в формате exe по указаному пути и выполняет экзешник  для фотки.
                Process.Start(@"C:\Users\Denis\Desktop\boxphoto.exe", $@"""{destinationFilePath}"""); //запуск дроплета и обработка отправленной фотки
                await Task.Delay(5000); //пауза

                //отдаем человеку обратно обработанную фотку
                await using Stream stream = System.IO.File.OpenRead(destinationFilePath);
                await botClient.SendDocumentAsync(message.Chat.Id, new InputOnlineFile(stream, message.Document.FileName.Replace(".jpg" , " (edit).jpg")));

                return;
            }

        }

        private static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }
    }
}
