using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;
using ICQ.Bot;
using ICQ.Bot.Args;
using ICQ.Bot.Types;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ICQTranslatorBot.Logic
{
    public enum LanguageType
    {
        None,
        English,
        Russian
    }

    public class TranslatorBot
    {
        private readonly LanguageType from = LanguageType.None;
        private readonly LanguageType to = LanguageType.None;
        private readonly TranslationClient translationClient;
        private readonly IICQBotClient bot;

        public TranslatorBot(string botId, LanguageType from, LanguageType to)
            : this(botId, from, to, null)
        { }

        public TranslatorBot(string botId, LanguageType from, LanguageType to, string credentials)
        {
            this.from = from;
            this.to = to;

            bot = new ICQBotClient(botId);
            bot.OnMessage += BotOnMessageReceived;
            bot.OnMessageEdited += BotOnMessageReceived;
            bot.OnReceiveError += BotOnReceiveError;
            bot.OnReceiveGeneralError += OnReceiveGeneralError;

            if (string.IsNullOrWhiteSpace(credentials))
            {
                translationClient = TranslationClient.Create();
            }
            else
            {
                GoogleCredential credential = null;
                using (var credentialStream = new MemoryStream(Encoding.UTF8.GetBytes(credentials)))
                {
                    credential = GoogleCredential.FromStream(credentialStream);
                }

                translationClient = TranslationClient.Create(credential);
            }
        }

        public async Task<User> GetMeAsync()
        {
            return await bot.GetMeAsync();
        }

        public void StartReceiving()
        {
            bot.StartReceiving();
        }

        public void StopReceiving()
        {
            bot.StopReceiving();
        }

        private void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            ProcessMessage(message).Wait();
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode, receiveErrorEventArgs.ApiRequestException.Message);
        }

        private void OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
        {
            Console.WriteLine("Received error: {0}", e.Exception.ToString());
        }

        private async Task ProcessMessage(Message message)
        {
            if (message.Chat == null || message.From == null || message.Chat.ChatId != message.From.UserId.ToString())
            {
                return;
            }

            if (from == LanguageType.None || to == LanguageType.None || string.IsNullOrWhiteSpace(message.Text))
            {
                return;
            }

            string translatedText;
            if ("/start".Equals(message.Text, StringComparison.OrdinalIgnoreCase))
            {
                translatedText = "Type and send me the text for an easy and fast translation!";
                if (from != LanguageType.English)
                {
                    translatedText = TranslateText(translatedText, LanguageType.English, from);
                }
            }
            else
            {
                translatedText = TranslateText(message.Text);
            }

            await bot.SendTextMessageAsync(message.From.UserId, translatedText);
        }

        private string TranslateText(string text)
        {
            return TranslateText(text, from, to);
        }

        private string TranslateText(string text, LanguageType from, LanguageType to)
        {
            string fromCode = ConvertLanguageType(from);
            string toCode = ConvertLanguageType(to);

            //https://cloud.google.com/translate/docs/simple-translate-call
            var response = translationClient.TranslateText(text, toCode, fromCode);

            return response.TranslatedText;
        }

        private static string ConvertLanguageType(LanguageType type)
        {
            string result;
            if (type == LanguageType.English)
            {
                result = "en";
            }
            else if (type == LanguageType.Russian)
            {
                result = "ru";
            }
            else
            {
                throw new ArgumentException(string.Format("language type is not supported: {0}", type));
            }

            return result;
        }
    }
}
