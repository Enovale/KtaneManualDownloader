using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CefSharp;
using CefSharp.OffScreen;
using PdfSharp.Pdf;
using System.IO;
using PdfSharp.Pdf.IO;
using System.Net;
using System.Diagnostics;

namespace KtaneManualDownloader
{
    public class Scraper
    {

        public static Scraper Instance;

        public enum ModuleType
        {
            Regular = 0,
            Needy = 1,
            Boss = 2,
            Uncategorized = 3
        }

        public enum ModuleDifficulty
        {
            VeryEasy = 0,
            Easy = 1,
            Medium = 2,
            Hard = 3,
            VeryHard = 4
        }

        // Make this configurable
        public static string ScraperURL = "https://ktane.timwi.de/";

        private ChromiumWebBrowser browser;
        private IFrame frame
        {
            get
            {
                return browser.GetBrowser().MainFrame;
            }
        }
        private bool browserReady = false;

        public Scraper()
        {
            Instance = this;
            browser = new ChromiumWebBrowser(ScraperURL);
            //Wait for the page to finish loading (all resources will have been loaded, rendering is likely still happening)
            browser.LoadingStateChanged += (sender, args) =>
            {
                //Wait for the Page to finish loading
                if (args.IsLoading == false)
                {
                    browserReady = true;
                }
            };
            EnsureBrowserInitialized();
        }

        public void Search(string term)
        {
            string script = "" + 
                $"$('#search-field').val('{term}');" +
                $"$('#search-field').trigger('keyup');";

            JavascriptResponse _ = frame.EvaluateScriptAsync(script).Result;
        }

        // Need to fix this to get difficulty as well for expert, not defuser
        public List<KtaneModule> GetSearchResults(string searchTerm = null)
        {
            if (searchTerm != null) Search(searchTerm);

            string gatherResults = "" +
            @"var searchResults = [];

            function handleMod(mod, i)
            {
                if (mod.style.display != 'none' && i > 1)
                {
                    searchResults.push(mod);
                }
            }

            document.getElementById('main-table').childNodes[0].childNodes.forEach(handleMod); 
            ";

            string extractModInfo = "" +
            @"var modNames = [];
            var modPDFs = [];
            var modTypes = [];
            var modDiffs = [];

            function extractInfo(mod, i) {

                var modName = '';
                if(mod.childNodes[3].class != 'infos - 1') {
                    modName = mod.childNodes[4].childNodes[0].childNodes[0].childNodes[1].innerHTML;
                } else {
                    modName = mod.childNodes[3].childNodes[0].childNodes[0].childNodes[1].innerHTML;
                }
                modNames.push(modName);

                var modHTML = mod.childNodes[0].childNodes[0].href;
                var modHTMLAbs = new URL(modHTML, document.baseURI).href;
                var modPDF = modHTMLAbs.replace('/HTML/', '/PDF/').replace('.html', '.pdf');
                modPDFs.push(modPDF);
    
                var modType = '';
                if(mod.childNodes[3].class != 'infos-1') {
                    modType = mod.childNodes[4].childNodes[1].childNodes[0].innerHTML;
                } else {
                    modType = mod.childNodes[3].childNodes[1].childNodes[0].innerHTML;
                }
                modTypes.push(modType);

                var modDiff = '';
                var diffDiv;
                if(mod.childNodes[3].class != 'infos - 1') {
                    diffDiv = mod.childNodes[5].childNodes[0].childNodes[2];
                } else {
                    diffDiv = mod.childNodes[4].childNodes[0].childNodes[2];
                }
                if(diffDiv.childNodes.length > 1) {
                    modDiff = diffDiv.childNodes[2].innerHTML;
                } else {
                    modDiff = diffDiv.childNodes[0].innerHTML;
                }
                modDiffs.push(modDiff);
            }

            searchResults.forEach(extractInfo);
            ";

            JavascriptResponse _ = frame.EvaluateScriptAsync(gatherResults).Result;
            JavascriptResponse __ = frame.EvaluateScriptAsync(extractModInfo).Result;
            //browser.ScreenshotAsync().Result.Save("test.png");

            List<object> modNamesObj = (List<object>)frame.EvaluateScriptAsync("modNames;").Result.Result;
            List<object> modPDFsObj = (List<object>)frame.EvaluateScriptAsync("modPDFs;").Result.Result;
            List<object> modTypesObj = (List<object>)frame.EvaluateScriptAsync("modTypes;").Result.Result;
            List<object> modDiffsObj = (List<object>)frame.EvaluateScriptAsync("modDiffs;").Result.Result;

            List<string> modNames = modNamesObj.Cast<string>().ToList();
            List<string> modPDFs = modPDFsObj.Cast<string>().ToList();
            List<string> modTypes = modTypesObj.Cast<string>().ToList();
            List<string> modDiffs = modDiffsObj.Cast<string>().ToList();

            List<KtaneModule> finalList = new List<KtaneModule>();

            foreach(string name in modNames)
            {
                int i = modNames.IndexOf(name);

                finalList.Add(new KtaneModule(name, modPDFs[i], TypeStringToEnum(modTypes[i]), DiffStringToEnum(modDiffs[i])));
            }

            return finalList;
        }

        public PdfDocument DownloadManual(string url)
        {
            PdfDocument inputDocument = null;

            using (WebClient wc = new WebClient())
            {
                using (MemoryStream stream = new MemoryStream(wc.DownloadData(url)))
                {
                    inputDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                    int _ = inputDocument.PageCount;
                }
            }

            Debug.Assert(inputDocument != null);
            return inputDocument;
        }

        public ModuleType TypeStringToEnum(string type)
        {
            switch(type.ToLower().Trim())
            {
                case "regular":
                    return ModuleType.Regular;
                case "needy":
                    return ModuleType.Needy;
                case "boss":
                    return ModuleType.Boss;
                default:
                    return ModuleType.Uncategorized;
            }
        }

        public ModuleDifficulty DiffStringToEnum(string difficulty)
        {
            switch(difficulty.ToLower().Trim())
            {
                case "very easy":
                    return ModuleDifficulty.VeryEasy;
                case "easy":
                    return ModuleDifficulty.Easy;
                case "medium":
                    return ModuleDifficulty.Medium;
                case "hard":
                    return ModuleDifficulty.Hard;
                case "very hard":
                    return ModuleDifficulty.VeryHard;
                default:
                    return ModuleDifficulty.Medium;
            }
        }

        public void EnsureBrowserInitialized()
        {
            while (!browserReady)
            {
                Thread.Sleep(100);
            }
        }

    }
}
