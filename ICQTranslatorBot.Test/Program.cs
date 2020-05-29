using ICQTranslatorBot.Logic;
using System;
using System.Net;

namespace ICQTranslatorBot.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //https://cloud.google.com/translate/docs/basic/setup-basic
            //https://stackoverflow.com/questions/43474871/using-google-cloud-bigquery-v2-api-in-azure-functions
            string content = @"[Content of your Google Cloud Service Account json]";

            var englishToRussianBot = new TranslatorBot("...", LanguageType.English, LanguageType.Russian, content);
            var russianToEnglishBot = new TranslatorBot("...", LanguageType.Russian, LanguageType.English, content);

            var e2r = englishToRussianBot.GetMeAsync().Result;
            var r2e = russianToEnglishBot.GetMeAsync().Result;

            englishToRussianBot.StartReceiving();
            Console.WriteLine($"Start listening for @{e2r.Nick}");

            russianToEnglishBot.StartReceiving();
            Console.WriteLine($"Start listening for @{r2e.Nick}");

            //https://sandervandevelde.wordpress.com/2016/07/19/turning-a-console-app-into-a-long-running-azure-webjob/
            for (; ; )
            {
                System.Threading.Thread.Sleep(5000);
            }
        }
    }
}
