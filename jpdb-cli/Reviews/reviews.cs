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
        public void homepage()
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

        class review
        {
            public string definitionString = "";
            public string idString = "";
            public int rating = -1;
        }

        List<review> reviewSession = new List<review>();

        //Review URL Format:
        //c=vf%2C[wordID]&r=[id]
        string nextArgs = string.Empty;

        public void startReviews()
        {
            string firstReviewArgs = firstReview();
            while (nextReview(firstReviewArgs) == true) { }
        }

        public string firstReview()
        {
            review newReview = new review();
            
            var handler = new HttpClientHandler();
            string html = "";
            string snipString = html;
            int snipIndex = -1;
            string vocabulary;
            //start a new review session in jpdb, extract the next review args (nextArgs), and extract the vocabulary:
            using (var httpClient = new HttpClient(handler)) //GET: 
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://jpdb.io/review"))
                {
                    #region Add headers
                    request.Headers.TryAddWithoutValidation("authority", "jpdb.io");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                    request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                    request.Headers.TryAddWithoutValidation("dnt", "1");
                    request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                    request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                    request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                    request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                    //request.Headers.TryAddWithoutValidation("referer", "https://jpdb.io/learn");
                    request.Headers.TryAddWithoutValidation("accept-language", "ja,en-GB;q=0.9,en;q=0.8");
                    request.Headers.TryAddWithoutValidation("cookie", Program.loginCookie.ToString());
                    request.Headers.TryAddWithoutValidation("if-none-match", "^^");
                    #endregion
                    var response = httpClient.SendAsync(request);
                    var result = response.Result;
                    html = result.Content.ReadAsStringAsync().Result;
                }

                #region Extract form IDs
                snipIndex = html.IndexOf("<form action=");
                html = html.Substring(snipIndex);

                snipIndex = html.IndexOf("vf,");
                snipString = html.Substring(snipIndex);
                string vfValue = snipString.Substring(0, snipString.IndexOf("\""));

                html = snipString.Substring(snipString.IndexOf("value=") + 7);

                string rID = html.Substring(0, html.IndexOf("\""));
                #endregion
                nextArgs = $"c={vfValue}&r={rID}";

                newReview.idString = nextArgs;

                #region Extract vocabulary
                snipIndex = html.IndexOf("</div><div>");
                snipString = html.Substring(snipIndex);
                snipString = snipString.Substring(snipString.IndexOf("</div>") + 11);
                vocabulary = snipString.Substring(0, snipString.IndexOf("</div>"));
                #endregion
                Console.WriteLine(vocabulary);
            }
            #region Readkey
            System.ConsoleKey keyRead;
            keyRead = Console.ReadKey().Key;
            Console.WriteLine();
            if (keyRead == ConsoleKey.Backspace && reviewSession.Count > 0)
            {
                reviewWord(reviewSession[reviewSession.Count - 1].idString);
                Console.WriteLine();
                Console.WriteLine(vocabulary);
            }
            #endregion

            return nextArgs;
        }

        public bool nextReview(string args)
        {
            string html = "";
            string snipString = "";
            
            //go to the review page at nextArgs and extract the answers information (word type and definitions):
            var handler = new HttpClientHandler();
            string newHTML = "";
            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://jpdb.io/review?{nextArgs}"))
                {
                    request.Headers.TryAddWithoutValidation("cookie", Program.loginCookie.ToString());

                    var response = httpClient.SendAsync(request).Result;
                    newHTML = response.Content.ReadAsStringAsync().Result;

                }
            }
            #region Parse html
            //newHTML = "<!DOCTYPE html><html class=\"dark-mode\"><head><meta http-equiv=\"Content-type\" content=\"text/html; charset=utf-8\" /><meta http-equiv=\"Content-language\" content=\"en\" /><meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge\" /><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, maximum-scale=1.0\" /><title>Review vocabulary and kanji – jpdb</title><link rel=\"stylesheet\" media=\"screen\" href=\"/static/0486fe6bdad5.css\" /><link rel=\"stylesheet\" media=\"screen\" href=\"/static/88a1d4ad0790.css\" /><link rel=\"apple-touch-icon\" sizes=\"180x180\" href=\"/static/533228467534.png\" /><link rel=\"icon\" type=\"image/png\" sizes=\"32x32\" href=\"/static/414de0c1e6b5.png\" /><link rel=\"icon\" type=\"image/png\" sizes=\"16x16\" href=\"/static/8f8d7f6ca822.png\" /><link rel=\"manifest\" href=\"/static/9919db124702.webmanifest\" /><link rel=\"search\" type=\"application/opensearchdescription+xml\" title=\"jpdb\" href=\"/static/opensearch.xml\" /><script>function oneshot(e, n, f) { var g = function() { e.removeEventListener(n, g); f(); }; e.addEventListener(n, g); } document.addEventListener(\"DOMContentLoaded\", function() { (document.querySelectorAll(\"[autofocus][type='text'], .autofocus[type='text']\") || []).forEach(function(e){ e.addEventListener(\"focusin\", function() { e.setSelectionRange(0,e.value.length); }); });if ((location.hash === \"\" || location.hash === \"#a\") && (!window.performance || window.performance.navigation.type != window.performance.navigation.TYPE_BACK_FORWARD || window.scrollY === 0)) { (document.querySelectorAll(\".autofocus[type='text']\") || []).forEach(function(e){e.focus(); e.setSelectionRange(0,e.value.length);}); }}); window.addEventListener(\"load\", function() { setTimeout(function() { var es = Array.prototype.slice.call(document.querySelectorAll(\".bugfix\")); if (es.some(function(e) { return e.scrollHeight > e.clientHeight; })) { console.log(\"Working around broken layout on Firefox...\"); (es.forEach(function(e) { e.style.height = e.scrollHeight + \"px\"; setTimeout(function() { e.style.removeProperty(\"height\"); }, 0); })); } }, 0); });</script><script>document.addEventListener(\"keyup\", function(e) { if (e.key == \"1\" || e.key == \"2\" || e.key == \"3\" || e.key == \"4\" || e.key == \"5\") { var el = document.querySelector(\"#grade-\" + e.key); if (!el) { if (e.key == \"1\" || e.key == \"2\") { el = document.querySelector(\"#grade-f\"); } else { el = document.querySelector(\"#grade-p\"); } }el.click();} else if (e.key == \"b\") {document.querySelector(\"#grade-blacklist\").click();} else if (e.key == \"k\") {document.querySelector(\"#grade-permaknown\").click();} else if (e.key == \"r\") { var el = document.querySelector(\"audio\"); if (el) { el.play(); } } });</script></head><body data-instant-allow-query-string><div class=\"nav minimal\"><h1 class=\"nav-logo\"><a href=\"/\">jpdb</a> <span style=\"font-size: 50%\">beta</span></h1><input class=\"menu-btn\" type=\"checkbox\" id=\"menu-btn\" /><label class=\"menu-icon\" for=\"menu-btn\"><span class=\"navicon\"></span></label><div class=\"menu\"><a class=\"nav-item\" href=\"/learn\">Learn (<span style=\"color: red;\">10</span>)</a><a class=\"nav-item\" href=\"/prebuilt_decks\">Built-in decks</a><a class=\"nav-item\" href=\"/stats\">Stats</a><a class=\"nav-item\" href=\"/settings\">Settings</a><a class=\"nav-item\" href=\"/logout\" data-no-instant>Logout</a></div></div><div class=\"container bugfix\"><div class=\"container\" style=\"display: flex; flex-direction: column; align-items: center;\"><div class=\"review-button-group\"><div class=\"column\"><div class=\"main-row\"><label id=\"show-checkbox-1-label\" for=\"show-checkbox-1\" class=\"side-button\" onclick=\"this.classList.toggle('rot-180', !document.querySelector('#show-checkbox-1').checked)\"><div>⮟</div></label><div class=\"main column\"><div class=\"row row-2\"><form action=\"/review#a\" method=\"post\"><input type=\"hidden\" name=\"c\" value=\"vf,1194520,2859068334\" /><input type=\"hidden\" name=\"r\" value=\"19\" /><input type=\"hidden\" name=\"g\" value=\"1\" /><input type=\"submit\" value=\"✘ Nothing\" class=\"outline v1\" id=\"grade-1\" /></form><form action=\"/review#a\" method=\"post\"><input type=\"hidden\" name=\"c\" value=\"vf,1194520,2859068334\" /><input type=\"hidden\" name=\"r\" value=\"19\" /><input type=\"hidden\" name=\"g\" value=\"2\" /><input type=\"submit\" value=\"✘ Something\" class=\"outline v1\" id=\"grade-2\" /></form></div><div class=\"row row-3\"><form action=\"/review#a\" method=\"post\"><input type=\"hidden\" name=\"c\" value=\"vf,1194520,2859068334\" /><input type=\"hidden\" name=\"r\" value=\"19\" /><input type=\"hidden\" name=\"g\" value=\"3\" /><input type=\"submit\" value=\"✔ Hard\" class=\"outline v3\" id=\"grade-3\" /></form><form action=\"/review#a\" method=\"post\"><input type=\"hidden\" name=\"c\" value=\"vf,1194520,2859068334\" /><input type=\"hidden\" name=\"r\" value=\"19\" /><input type=\"hidden\" name=\"g\" value=\"4\" /><input type=\"submit\" value=\"✔ Okay\" class=\"outline v4\" id=\"grade-4\" autofocus /></form><form action=\"/review#a\" method=\"post\"><input type=\"hidden\" name=\"c\" value=\"vf,1194520,2859068334\" /><input type=\"hidden\" name=\"r\" value=\"19\" /><input type=\"hidden\" name=\"g\" value=\"5\" /><input type=\"submit\" value=\"✔ Easy\" class=\"outline\" id=\"grade-5\" /></form></div><input class=\"show-hide-checkbox\" type=\"checkbox\" id=\"show-checkbox-1\" hidden /><div class=\"hidden-body\" style=\"width: 100%;\"><div class=\"row row-2\"><form action=\"/review#a\" method=\"post\" style=\"margin: 0;\"><input type=\"hidden\" name=\"c\" value=\"vf,1194520,2859068334\" /><input type=\"hidden\" name=\"r\" value=\"19\" /><input type=\"hidden\" name=\"g\" value=\"-1\" /><input type=\"submit\" value=\"Blacklist\" class=\"outline v1\" id=\"grade-blacklist\" /></form><form action=\"/review#a\" method=\"post\"><input type=\"hidden\" name=\"c\" value=\"vf,1194520,2859068334\" /><input type=\"hidden\" name=\"r\" value=\"19\" /><input type=\"hidden\" name=\"g\" value=\"w\" /><input type=\"submit\" value=\"I'll never forget\" class=\"outline\" id=\"grade-permaknown\" /></form></div></div></div></div></div></div><div id=\"a\"></div><div class=\"review-reveal\"><div class=\"answer-box\"><div class=\"plain\" style=\"display: flex;\"><div style=\"flex: 1\"></div><a class=\"plain\" href=\"/vocabulary/1194520/花弁#a\"><ruby>花<rt>はな</rt></ruby><ruby>弁<rt>びら</rt></ruby></a><div style=\"flex: 1; display: flex; flex-direction: column; align-items: flex-end; font-size: 40%; opacity: 0.8;\"></div></div><div style=\"display: flex; justify-content: center;\"><div style=\"display: flex;\"><div style=\"display: flex; flex-direction: column;\"><div class=\"sentence\"><ruby>男<rt>おとこ</rt></ruby>は<ruby>目<rt>め</rt></ruby>の<ruby>前<rt>まえ</rt></ruby>に<ruby>振<rt>ふ</rt></ruby>ってきた<span class=\"highlight\"><ruby>花<rt>はな</rt></ruby><ruby>弁<rt>びら</rt></ruby></span>をそっと<ruby>掴<rt>つか</rt></ruby>んでみる。</div></div><a href=\"/edit-shown-sentence?v=1194520&amp;s=2859068334&amp;r=1525331267&amp;origin=%2Freview%3Fc%3Dvf%2C1194520%2C2859068334%26r%3D19\" style=\"border: 0; display: flex; padding-bottom: 0.3rem; align-items: center; margin-right: 0.5rem;\"><svg xmlns=\"http://www.w3.org/2000/svg\" class=\"icon icon-tabler icon-tabler-pencil\" width=\"36\" height=\"36\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\" style=\"width: 1.4rem; height: 1.4rem; opacity: 0.5; \"><path stroke=\"none\" d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M4 20h4l10.5 -10.5a1.5 1.5 0 0 0 -4 -4l-10.5 10.5v4\" /><line x1=\"13.5\" y1=\"6.5\" x2=\"17.5\" y2=\"10.5\" /></svg></a></div></div></div><div class=\"result vocabulary\"><div class=\"vbox gap\" style=\"width: 100%;\"><div class=\"hbox\" style=\"width: 100%; justify-content: space-between;\"><script async type=\"text/javascript\" src=\"/static/15d8be99ddf4.js\"></script><div class=\"subsection-meanings\"><h6 class=\"subsection-label\">Meanings&nbsp;<a href=\"/edit_shown_meanings?v=1194520&amp;s=2859068334&amp;r=1525331267&amp;origin=%2Freview%3Fc%3Dvf%2C1194520%2C2859068334%26r%3D19\" style=\"border: 0; display: flex; padding-bottom: 0.3rem;\"><svg xmlns=\"http://www.w3.org/2000/svg\" class=\"icon icon-tabler icon-tabler-pencil\" width=\"36\" height=\"36\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\" style=\"width: 1.4rem; height: 1.4rem; opacity: 0.5;\"><path stroke=\"none\" d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M4 20h4l10.5 -10.5a1.5 1.5 0 0 0 -4 -4l-10.5 10.5v4\" /><line x1=\"13.5\" y1=\"6.5\" x2=\"17.5\" y2=\"10.5\" /></svg></a></h6><div class=\"subsection\"><div class=\"part-of-speech\"><div>Noun</div></div><div class=\"description\">1.  (flower) petal</div></div></div><audio preload=\"none\" autoplay><source src=\"/static/voice/8ef4807c8310.mp3\" type=\"audio/mpeg\"></audio><a class=\"audio-link\" href=\"/static/voice/8ef4807c8310.mp3\" target=\"_blank\" onclick=\"window.event.preventDefault(); this.previousElementSibling.play();\"><svg xmlns=\"http://www.w3.org/2000/svg\" class=\"icon icon-tabler icon-tabler-volume\" width=\"44\" height=\"44\" viewBox=\"0 0 24 24\" stroke-width=\"0.8\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path stroke=\"none\" d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M15 8a5 5 0 0 1 0 8\" /><path d=\"M17.7 5a9 9 0 0 1 0 14\" /><path d=\"M6 15h-2a1 1 0 0 1 -1 -1v-4a1 1 0 0 1 1 -1h2l3.5 -4.5a0.8 .8 0 0 1 1.5 .5v14a0.8 .8 0 0 1 -1.5 .5l-3.5 -4.5\" /></svg></a></div><div class=\"subsection-composed-of-kanji\"><h6 class=\"subsection-label\">Kanji used</h6><div class=\"subsection\"><div><div class=\"spelling\"><a class=\"plain\" href=\"/kanji/花#a\">花</a></div><div class=\"description\">flower</div></div><div><div class=\"spelling\"><a class=\"plain\" href=\"/kanji/弁#a\">弁</a></div><div class=\"description\">dialect</div></div></div></div><div class=\"subsection-pitch-accent\"><h6 class=\"subsection-label\"\">Pitch accent</h6><div class=\"subsection\"><div style=\"display: flex; flex-direction: column; gap: 0.5rem;\"><div style=\"word-break: keep-all; display: flex;\"><div style=\"display: flex; background-image: linear-gradient(to top,var(--pitch-low-s),var(--pitch-low-e)); padding-bottom: 2px; margin-top: 2px; margin-right: -2px; padding-right: 2px;\"><div style=\"background-color: var(--background-color); padding-right: 2px;\">は</div></div><div style=\"display: flex; background-image: linear-gradient(to bottom,var(--pitch-high-s),var(--pitch-high-e)); padding-top: 2px; margin-bottom: 2px; margin-right: -2px; padding-left: 2px; padding-right: 2px;\"><div style=\"background-color: var(--background-color); padding-left: 1px; padding-right: 2px;\">なび</div></div><div style=\"display: flex; background-image: linear-gradient(to top,var(--pitch-low-s),var(--pitch-low-e)); padding-bottom: 2px; margin-top: 2px;  padding-left: 2px;\"><div style=\"background-color: var(--background-color); padding-left: 1px;\">ら</div></div></div></div></div></div></div></div><div style=\"width: 100%; display: flex; flex-direction: column;\"><label id=\"show-checkbox-examples-label\" for=\"show-checkbox-examples\" style=\"display: flex; justify-content: flex-end;\"><div style=\"opacity: 0.5; text-align: right; cursor: pointer; display: inline-block; padding-bottom: 0.5rem; margin-top: 0.5rem;\">Click to toggle examples...</div></label><input class=\"show-hide-checkbox\" type=\"checkbox\" id=\"show-checkbox-examples\" hidden /><div class=\"hidden-body\"><div class=\"subsection-examples\"><div class=\"subsection\"><div class=\"used-in\"><div class=\"jp\">この<span class=\"highlight\"><ruby>花<rt>はな</rt></ruby><ruby>弁<rt>びら</rt></ruby></span>とこの<ruby>花<rt>か</rt></ruby><ruby>弁<rt>べん</rt></ruby>、<ruby>色<rt>いろ</rt></ruby>が<ruby>違<rt>ちが</rt></ruby>うの。</div></div><div class=\"used-in\"><div class=\"jp\"><ruby>言<rt>こと</rt></ruby><ruby>葉<rt>ば</rt></ruby>と<ruby>共<rt>とも</rt></ruby>にそっと<span class=\"highlight\"><ruby>花<rt>はな</rt></ruby><ruby>弁<rt>びら</rt></ruby></span>に<ruby>触<rt>ふ</rt></ruby>れる。</div></div><div class=\"used-in\"><div class=\"jp\"><ruby>柔<rt>やわ</rt></ruby>らかく<ruby>美<rt>うつく</rt></ruby>しい<span class=\"highlight\"><ruby>花<rt>はな</rt></ruby><ruby>弁<rt>びら</rt></ruby></span>を、<ruby>撫<rt>な</rt></ruby>でる。</div></div><div class=\"used-in\"><div class=\"jp\"><ruby>指<rt>ゆび</rt></ruby><ruby>先<rt>さき</rt></ruby>でそっと<ruby>優<rt>やさ</rt></ruby>しく<span class=\"highlight\"><ruby>花<rt>はな</rt></ruby><ruby>弁<rt>びら</rt></ruby></span>に<ruby>触<rt>ふ</rt></ruby>れる。</div></div><div class=\"used-in\"><div class=\"jp\">いつもと<ruby>変<rt>か</rt></ruby>わらぬ<ruby>調<rt>ちょう</rt></ruby><ruby>子<rt>し</rt></ruby>で、<span class=\"highlight\"><ruby>花<rt>はな</rt></ruby><ruby>弁<rt>びら</rt></ruby></span>みたいな<ruby>口<rt>くち</rt></ruby>を<ruby>開<rt>ひら</rt></ruby>かせ。</div></div><div class=\"used-in\"><div class=\"jp\"><ruby>白<rt>しろ</rt></ruby>い<span class=\"highlight\"><ruby>花<rt>はな</rt></ruby><ruby>弁<rt>びら</rt></ruby></span>に、<ruby>手<rt>て</rt></ruby>を<ruby>伸<rt>の</rt></ruby>ばしてみる。</div></div><div class=\"used-in\"><div class=\"jp\"><ruby>青<rt>あお</rt></ruby>い<span class=\"highlight\"><ruby>花<rt>はな</rt></ruby><ruby>弁<rt>びら</rt></ruby></span>と<ruby>虫<rt>むし</rt></ruby>を<ruby>乗<rt>の</rt></ruby>せて、<ruby>風<rt>かぜ</rt></ruby>は<ruby>巨<rt>きょ</rt></ruby><ruby>大<rt>だい</rt></ruby>な<ruby>門<rt>もん</rt></ruby>へと<ruby>向<rt>む</rt></ruby>かう。</div></div><div class=\"used-in\"><div class=\"jp\"><ruby>手<rt>て</rt></ruby>を<ruby>伸<rt>の</rt></ruby>ばして<ruby>捕<rt>つか</rt></ruby>まえると、<span class=\"highlight\"><ruby>花<rt>はな</rt></ruby><ruby>弁<rt>びら</rt></ruby></span>だった。</div></div><div style=\"margin-top: 0.5rem;\"></div></div></div></div><div style=\"margin-top: 0.5rem;\"></div><div style=\"opacity: 0.5; margin-top: 1rem;\">Part of the <a href=\"/deck?id=99\">Tenkou-saki no Seiso Karen na Bishoujo ga, Mukashi Danshi to Omotte Issho ni Asonda Osananajimi datta Ken</a> deck (1x)</div><div style=\"margin-top: 0.5rem;\"></div></div></div></div></div><div class=\"with-bottom-padding-2\"></div><script src=\"/static/ae433897feb1.js\" type=\"module\" defer></script></body></html>";
            newHTML = newHTML.Substring(0, newHTML.IndexOf("Kanji used"));

            List<String> partsOfSpeech = new List<string>();
            html = newHTML;
            while (newHTML.Contains("part-of-speech") == true)
            {
                string partOfSpeech = "";
                newHTML = newHTML.Substring(newHTML.IndexOf("part-of-speech") + 16);
                snipString = newHTML.Substring(0, newHTML.IndexOf("</div></div>"));
                partOfSpeech = snipString.Replace("<div>", "").Replace("</div>", ", ");
                partsOfSpeech.Add(partOfSpeech);

                //new
                newHTML = newHTML.Replace(snipString, "");
            }

            newHTML = html;

            List<String> definitions = new List<string>();
            while (newHTML.Contains("<div class=\"description\">") == true)
            {
                string definition = "";
                newHTML = newHTML.Substring(newHTML.IndexOf("<div class=\"description\">"));
                snipString = newHTML.Substring(0, newHTML.IndexOf("</div><div "));
                if (newHTML.Contains("<audio") == true) { snipString = newHTML.Substring(0, newHTML.IndexOf("</div></div>")); }
                definition = snipString.Replace("<div class=\"description\">", "").Replace("</div>", ",\n").Replace("  ", " ").TrimEnd() + "\n";
                newHTML = newHTML.Substring(25);
                definitions.Add(definition);
            }

            string definitionsString = "";
            for (int i = 0; i < partsOfSpeech.Count; i++)
            {
                definitionsString += $"{partsOfSpeech[i]}\n{definitions[i]}";
            }

            newReview.definitionString = definitionsString;
            #endregion

            Console.WriteLine(definitionsString.Replace("&#39;", "'"));
            Console.Write("->");
            #region Readkey (grade)
            int rating = -1;
            while (rating == -1)
            {
                keyRead = Console.ReadKey().Key;
                switch (keyRead)
                {
                    case ConsoleKey.D1:
                        rating = 1;
                        break;
                    case ConsoleKey.D2:
                        rating = 2;
                        break;
                    case ConsoleKey.D3:
                        rating = 3;
                        break;
                    case ConsoleKey.D4:
                        rating = 4;
                        break;
                    case ConsoleKey.D5:
                        rating = 5;
                        break;

                    case ConsoleKey.Backspace:
                        reviewWord(reviewSession[reviewSession.Count - 1].idString);
                        Console.WriteLine();
                        Console.WriteLine(vocabulary);
                        Console.WriteLine(definitionsString);
                        Console.Write("->");
                        break;

                    case ConsoleKey.Escape:
                        return true;
                }
            }
            Console.WriteLine();
            #endregion
            newReview.rating = rating;

            //make a post request to send off the grade
            handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://jpdb.io/review"))
                {
                    #region Add headers
                    request.Headers.TryAddWithoutValidation("authority", "jpdb.io");
                    request.Headers.TryAddWithoutValidation("cache-control", "max-age=0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                    request.Headers.TryAddWithoutValidation("origin", "https://jpdb.io");
                    request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                    request.Headers.TryAddWithoutValidation("dnt", "1");
                    request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
                    request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                    request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                    request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                    //request.Headers.TryAddWithoutValidation("referer", "");
                    request.Headers.TryAddWithoutValidation("accept-language", "ja,en-GB;q=0.9,en;q=0.8");
                    request.Headers.TryAddWithoutValidation("cookie", Program.loginCookie.ToString());
                    #endregion
                    request.Content = new StringContent($"{nextArgs}&g={rating}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = httpClient.SendAsync(request).Result;
                    html = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"Graded {vocabulary} {rating} ({nextArgs})");
                }

            }

            //IMPORTANT:
            //html contains form IDs of next word

            reviewSession.Add(newReview);
            return true;


            return false;
        }

    }
}
