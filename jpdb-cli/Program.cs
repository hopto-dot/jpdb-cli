using jpdb_cli.Bulk_Upload;
using jpdb_cli.Reviews;
using jpdb_cli.DBCoverageStats;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace MyApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            reviews newReview = new reviews();

            if (args.Length == 0) { Console.WriteLine("Enter a command:"); args = Console.ReadLine().Split(" "); }
            if (args[0] == "exit")
            {
                Environment.Exit(0);
            }
            if (args[0] == "help")
            {
                help(); enterCommand();
            }
            if (args[0] == "logout")
            {
                if (loginCookie == null)
                {
                    Console.WriteLine("Already logged out."); 
                }
                else
                {
                    loginCookie = null;
                    Console.WriteLine("Logged out.");
                }
                
                enterCommand();
            }
            if (args[0] == "clear") { Console.Clear(); enterCommand(); }

            if ((args[0] == "review" || args[0] == "reviews") && args.Length == 1)
            {
                if (loginCookie == null)
                {
                    printError("You must be logged in!");
                    printError("Do 'login [username] [password]' first");
                    enterCommand();
                }
                newReview.startReviews();
                enterCommand();
                return;
            }
            if (args[0] == "login")
            {
                //if (args.Length == 1) {  }
                if (args.Length != 3) { printError("'login' command takes 3 arguments! 'login [username] [password]'"); enterCommand(); }
                Console.WriteLine();
                Console.WriteLine("Attempting to log in...");
                try { login(args[1], args[2]); newReview.homepage(); enterCommand(); } catch { printError("Failed to log in"); enterCommand(); }
            }
            if (args[0] == "statistics" && args.Length == 1)
            {
                if (loginCookie == null)
                {
                    printError("You must be logged in!");
                    printError("Do 'login [username] [password]' first");
                    enterCommand();
                }
                newReview.homepage();
                enterCommand();
            }
            if (args[0] == "deckfromtext")
            {
                if (args.Length != 3) { printError("'deckfromtext' command takes 3 arguments! 'deckfromtext [deckID] [file path]'"); enterCommand(); }
                bool fail = false;
                if (File.Exists(args[2]) == false)
                {
                    Program.printError($"The file '{args[2]}' doesn't exist.");
                    fail = true;
                }
                string x = args[1];
                if (int.TryParse(x, out int value) == false)
                {
                    Program.printError($"'{args[1]}' isn't a valid deck id. It must be a number.");
                    fail = true;
                }
                if (fail == false)
                {
                    BulkUpload.bulkUpload(args[1], args[2]);
                }
                enterCommand();
            }
            if (args[0] == "coverage")
            {
                if (loginCookie == null)
                {
                    printError("You must be logged in first!");
                    printError("Do 'login [username] [password]'");
                    enterCommand();
                }
                
                if (args.Length == 1)
                {
                    try { CoverageStats.genCovStats("all"); } catch { printError("Something went wrong, you may have entered an incorrect content type."); }
                } else
                {
                    try { CoverageStats.genCovStats(args[1]); } catch { printError("Something went wrong, you may have entered an incorrect content type."); }
                }
                
                enterCommand();
            }

            Console.WriteLine($"Command '{args[0]}' not recognised. Type 'help' for more information.");
            enterCommand();
        }

        public static Cookie? loginCookie = null;

        static void login(string username, string password)
        {
            var handler = new HttpClientHandler();
            CookieContainer cookies = new CookieContainer();
            handler.CookieContainer = cookies;

            // If you are using .NET Core 3.0+ you can replace `~DecompressionMethods.None` to `DecompressionMethods.All`
            handler.AutomaticDecompression = ~DecompressionMethods.None;

            // In production code, don't destroy the HttpClient through using, but better reuse an existing instance
            // https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://jpdb.io/login"))
                {
                    request.Headers.TryAddWithoutValidation("authority", "jpdb.io");
                    request.Headers.TryAddWithoutValidation("pragma", "no-cache");
                    request.Headers.TryAddWithoutValidation("cache-control", "no-cache");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                    request.Headers.TryAddWithoutValidation("origin", "https://jpdb.io");
                    request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                    request.Headers.TryAddWithoutValidation("dnt", "1");
                    request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.54 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                    request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                    request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                    request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                    request.Headers.TryAddWithoutValidation("referer", "https://jpdb.io/login");
                    request.Headers.TryAddWithoutValidation("accept-language", "ja,en-GB;q=0.9,en;q=0.8");

                    string encodedPassword = HttpUtility.UrlEncode(password);

                    request.Content = new StringContent($"username={username}&password={encodedPassword}&remember-me=on");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = httpClient.SendAsync(request);
                    var responseResult = response.Result;
                    Console.WriteLine($"Request {responseResult.ReasonPhrase}");
                    var responseCookies = cookies.GetCookies(new Uri("https://jpdb.io/login")).Cast<Cookie>();
                    Cookie cookie = responseCookies.ToList()[0];

                    if (responseResult.StatusCode == HttpStatusCode.OK)
                    {
                        loginCookie = cookie;
                    }
                    else
                    {
                        printError("Couldn't log you in");
                    }

                    printSuccess("Successfully logged in");
                }
            }
        }

        public static void printError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void printSuccess(string success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(success);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void enterCommand()
        {
            Console.WriteLine(); Main(new string[] { });
            return;
        }

        public static void help()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nCommands:\n" +
                              "login [username] [password]\n" +
                              "deckfromtext [deckID] [filepath]\n" +
                              "statistics\n" +
                              "logout\n" +
                              "coverage [content type]");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}