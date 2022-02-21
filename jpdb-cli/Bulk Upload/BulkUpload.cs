using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Net;
using MyApp;
using System.Net.Http.Headers;
using System.Web;

namespace jpdb_cli.Bulk_Upload
{
    internal class BulkUpload
    {
        public static void bulkUpload(string deckID, string filePath)
        {
            if (Program.loginCookie == null) { Program.printError("You must be logged in for that! Use the command 'login [username] [password]'"); return; }

            string progressString = string.Empty;

            if (deckID == string.Empty || filePath == string.Empty)
            {
                return;
            }
            string fileText = File.ReadAllText(filePath);

            string sendString = string.Empty;
            for (int bottomIndex = 0; bottomIndex < fileText.Length; bottomIndex += 150000)
            {
                progressString = $"{(int)Math.Round((double)(100 * bottomIndex) / fileText.Length)}%";
                Console.WriteLine(progressString);
                try
                {
                    sendString = fileText.Substring(bottomIndex, 150000);
                }
                catch
                {
                    sendString = fileText.Substring(bottomIndex, fileText.Length - bottomIndex);
                }
                try { createDeckFromText(deckID, sendString); } catch { Program.printError("Something went wrong. Proceeding anyway."); }

                System.Threading.Thread.Sleep(200);

                if (bottomIndex >= fileText.Length) { Console.WriteLine("100%"); }
            }

            Program.printSuccess($"Successfully bulk uploaded words from '{filePath}' into deck with ID {deckID}");
        }


        static void createDeckFromText(string deckID, string textToAdd)
        {
            var handler = new HttpClientHandler();
            CookieContainer cookies = new CookieContainer();
            cookies.Add(Program.loginCookie);
            handler.CookieContainer = cookies;
            string toDeckID = string.Empty;

            // If you are using .NET Core 3.0+ you can replace `~DecompressionMethods.None` to `DecompressionMethods.All`

            // In production code, don't destroy the HttpClient through using, but better reuse an existing instance
            // https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://jpdb.io/add_to_deck_from_text"))
                {
                    request.Headers.TryAddWithoutValidation("authority", "jpdb.io");
                    request.Headers.TryAddWithoutValidation("cache-control", "max-age=0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                    request.Headers.TryAddWithoutValidation("origin", "https://jpdb.io");
                    request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                    request.Headers.TryAddWithoutValidation("dnt", "1");
                    request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                    request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                    request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                    request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                    request.Headers.TryAddWithoutValidation("referer", "https://jpdb.io/add_to_deck_from_text?id=93");
                    request.Headers.TryAddWithoutValidation("accept-language", "ja,en-GB;q=0.9,en;q=0.8");
                    request.Headers.TryAddWithoutValidation("cookie", Program.loginCookie.ToString());

                    request.Content = new StringContent($"id={deckID}&text={HttpUtility.UrlEncode(textToAdd)}");

                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = httpClient.Send(request);
                    toDeckID = response.RequestMessage.RequestUri.Query.Replace("?id=", "");
                }
            }

            handler = new HttpClientHandler();
            cookies.Add(Program.loginCookie);
            handler.AutomaticDecompression = ~DecompressionMethods.None;

            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://jpdb.io/add_to_deck_from_text_confirm"))
                {
                    request.Headers.TryAddWithoutValidation("authority", "jpdb.io");
                    request.Headers.TryAddWithoutValidation("cache-control", "max-age=0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                    request.Headers.TryAddWithoutValidation("origin", "https://jpdb.io");
                    request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                    request.Headers.TryAddWithoutValidation("dnt", "1");
                    request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                    request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                    request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                    request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                    request.Headers.TryAddWithoutValidation("referer", "https://jpdb.io/add_to_deck_from_text_confirm?id=4");
                    request.Headers.TryAddWithoutValidation("accept-language", "ja,en-GB;q=0.9,en;q=0.8");
                    request.Headers.TryAddWithoutValidation("cookie", Program.loginCookie.ToString());

                    request.Content = new StringContent($"id={toDeckID}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = httpClient.Send(request);
                }
            }
        }
    }
}
