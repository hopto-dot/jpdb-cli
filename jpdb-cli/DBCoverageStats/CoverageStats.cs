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

namespace jpdb_cli.DBCoverageStats
{
    internal class CoverageStats
    {        
        public static void genCovStats(string contType)
        {
            bool all = false;
            if (contType.ToLower() == "all")
            {
                all = true;
            }

            string urlString = "";
            if (all)
            {
                urlString = $"https://jpdb.io/prebuilt_decks#a";
            }
            else
            {
                urlString = $"https://jpdb.io/prebuilt_decks?show_only={contType}#a";
            }
            int contentNo = noOfContent(urlString);

            List<int> coverages = new List<int>();  //50, 70, 80, 90, 95, 96, 97, 98
            
            for (int content = 0; content < contentNo - 49; content += 50)
            {
                if (all)
                {
                    urlString = $"https://jpdb.io/prebuilt_decks?sort_by=known_word_count&order=reverse&offset={content}#a";
                }
                else
                {
                    urlString = $"https://jpdb.io/prebuilt_decks?sort_by=known_word_count&order=reverse&show_only={contType}&offset={content}#a";
                }

                int coverage = covStatsContent(urlString);

                if (coverage < 98 && coverages.Count == 0) { coverages.Add((int)(((float)(content + 5) / contentNo)*100) + 1); }
                if (coverage < 97 && coverages.Count == 1) { coverages.Add((int)(((float)(content + 5) / contentNo)*100) + 1); }
                if (coverage < 96 && coverages.Count == 2) { coverages.Add((int)(((float)(content + 5) / contentNo)*100) + 1); }
                if (coverage < 95 && coverages.Count == 3) { coverages.Add((int)(((float)(content + 5) / contentNo)*100) + 1); }
                if (coverage < 90 && coverages.Count == 4) { coverages.Add((int)(((float)(content + 5) / contentNo)*100) + 1); }
                if (coverage < 80 && coverages.Count == 5) { coverages.Add((int)(((float)(content + 5) / contentNo)*100) + 1); }
                if (coverage < 70 && coverages.Count == 6) { coverages.Add((int)(((float)(content + 5) / contentNo)*100) + 1); }
                if (coverage < 50 && coverages.Count == 7) { coverages.Add((int)(((float)(content + 5) / contentNo)*100) + 1); }



                if (coverages.Count == 8)
                {
                    content = contentNo - 49; continue;
                }
            }

            try { Console.WriteLine($"{coverages[0]}% of content has a coverage of 98%"); } catch { }
            try { Console.WriteLine($"{coverages[1]}% of content has a coverage of 97%"); } catch { }
            try { Console.WriteLine($"{coverages[2]}% of content has a coverage of 96%"); } catch { }
            try { Console.WriteLine($"{coverages[3]}% of content has a coverage of 95%"); } catch { }
            try { Console.WriteLine($"{coverages[4]}% of content has a coverage of 90%"); } catch { }
            try { Console.WriteLine($"{coverages[5]}% of content has a coverage of 80%"); } catch { }
            try { Console.WriteLine($"{coverages[6]}% of content has a coverage of 70%"); } catch { }
            try { Console.WriteLine($"{coverages[7]}% of content has a coverage of 50%"); } catch { }

            Program.printSuccess("Finished scrape!");
        }
        
        
        static int covStatsContent(string url)
        {
            var handler = new HttpClientHandler();
            string html = "";

            // If you are using .NET Core 3.0+ you can replace `~DecompressionMethods.None` to `DecompressionMethods.All`
            handler.AutomaticDecompression = ~DecompressionMethods.None;

            // In production code, don't destroy the HttpClient through using, but better reuse an existing instance
            // https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    request.Headers.TryAddWithoutValidation("authority", "jpdb.io");
                    request.Headers.TryAddWithoutValidation("cache-control", "max-age=0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                    request.Headers.TryAddWithoutValidation("dnt", "1");
                    request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                    request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                    request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                    request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                    request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                    request.Headers.TryAddWithoutValidation("referer", "https://jpdb.io/prebuilt_decks?sort_by=known_word_count&order=reverse");
                    request.Headers.TryAddWithoutValidation("accept-language", "ja,en-GB;q=0.9,en;q=0.8");
                    request.Headers.TryAddWithoutValidation("cookie", Program.loginCookie.ToString());
                    request.Headers.TryAddWithoutValidation("if-none-match", "^^");

                    var response = httpClient.SendAsync(request);
                    var result = response.Result;
                    html = result.Content.ReadAsStringAsync().Result;
                }
            }

            int snipIndex = -1;
            snipIndex = html.IndexOf("Known words (%)") + 24;
            html = html.Substring(snipIndex);

            snipIndex = html.IndexOf("%");

            int coverage = int.Parse(html.Substring(0, snipIndex));
            return coverage;

            Console.WriteLine();
        }

        static int noOfContent(string url)
        {
            var handler = new HttpClientHandler();
            string html = "";

            // If you are using .NET Core 3.0+ you can replace `~DecompressionMethods.None` to `DecompressionMethods.All`
            handler.AutomaticDecompression = ~DecompressionMethods.None;

            // In production code, don't destroy the HttpClient through using, but better reuse an existing instance
            // https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    request.Headers.TryAddWithoutValidation("authority", "jpdb.io");
                    request.Headers.TryAddWithoutValidation("cache-control", "max-age=0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                    request.Headers.TryAddWithoutValidation("dnt", "1");
                    request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                    request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                    request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                    request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                    request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                    request.Headers.TryAddWithoutValidation("referer", url);
                    request.Headers.TryAddWithoutValidation("accept-language", "ja,en-GB;q=0.9,en;q=0.8");
                    request.Headers.TryAddWithoutValidation("cookie", Program.loginCookie.ToString());
                    request.Headers.TryAddWithoutValidation("if-none-match", "^^");

                    var response = httpClient.SendAsync(request);
                    var result = response.Result;
                    html = result.Content.ReadAsStringAsync().Result;
                }

                int snipIndex = -1;

                snipIndex = html.IndexOf("Showing");
                html = html.Substring(snipIndex);
                snipIndex = html.IndexOf("from ");
                html = html.Substring(snipIndex + 5);

                snipIndex = html.IndexOf(" ");


                int contentNo = int.Parse(html.Substring(0, snipIndex));
                return contentNo;
            }
        }
    }
}
