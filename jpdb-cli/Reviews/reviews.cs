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

namespace jpdb_cli.Reviews
{
    internal class reviews
    {
        
        public static void homepage()
        {
            if (Program.loginCookie == null) { Program.printError("You must be logged in for that!"); return; }
            
            var handler = new HttpClientHandler();
            var html = string.Empty;

            // If you are using .NET Core 3.0+ you can replace `~DecompressionMethods.None` to `DecompressionMethods.All`
            handler.AutomaticDecompression = ~DecompressionMethods.None;

            // In production code, don't destroy the HttpClient through using, but better reuse an existing instance
            // https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://jpdb.io/learn"))
                {
                    request.Headers.TryAddWithoutValidation("authority", "jpdb.io");
                    request.Headers.TryAddWithoutValidation("cache-control", "max-age=0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                    request.Headers.TryAddWithoutValidation("dnt", "1");
                    request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                    request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                    request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                    request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                    request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                    request.Headers.TryAddWithoutValidation("referer", "https://jpdb.io/");
                    request.Headers.TryAddWithoutValidation("accept-language", "ja,en-GB;q=0.9,en;q=0.8");
                    request.Headers.TryAddWithoutValidation("cookie", Program.loginCookie.ToString());
                    request.Headers.TryAddWithoutValidation("if-none-match", "^^");

                    var response = httpClient.SendAsync(request);
                    try
                    {
                        html = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (Exception ex)
                    {
                        Program.printError(ex.Message);
                        return;
                    }
                }
            }
            string snipTemp = html;
            int snipindex = -1;

            snipindex = snipTemp.IndexOf("/learn\">") + 8;
            snipTemp = snipTemp.Substring(snipindex); //snip to the start of '/learn">'

            snipindex = snipTemp.IndexOf(")") + 1;
            snipTemp = snipTemp.Substring(0, snipindex); //snip up the end of the '/learn">' information
            snipTemp = stringFunctions.removeBContents(snipTemp); //remove everything inside triangular brackets

            //word state statistics
            snipindex = html.IndexOf("Your learning progress");
            html = html.Substring(snipindex);

            for (int i = 1; i <= 2; i++) {
                snipindex = html.IndexOf("<tr>") + 4;
                html = html.Substring(snipindex);
            }

            for (int i = 1; i <= 2; i++)
            {
                snipindex = html.IndexOf("<td>") + 4;
                html = html.Substring(snipindex);
            }

            int totalWords = int.Parse( html.Substring(0, html.IndexOf("<")) );

            snipindex = html.IndexOf("<td>") + 4;
            html = html.Substring(snipindex); //snipping up to Learning word count

            int learningWords = int.Parse(html.Substring(0, html.IndexOf("<")));

            snipindex = html.IndexOf("<td>") + 4;
            html = html.Substring(snipindex); //snipping up to Known word count

            int knownWords = int.Parse(html.Substring(0, html.IndexOf("&")));

            snipindex = html.IndexOf("(") + 1;
            html = html.Substring(snipindex); //snipping up to Known word count

            int knownPercent = int.Parse(html.Substring(0, html.IndexOf(")") - 1));

            Console.WriteLine($"\nTotal: {totalWords}\nLearning: {learningWords}\nKnown: {knownWords} ({knownPercent}%)");
        }

    }
}
