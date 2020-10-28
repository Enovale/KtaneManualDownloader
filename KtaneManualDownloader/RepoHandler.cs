using System.Collections.Generic;
using PdfSharp.Pdf;
using System.IO;
using PdfSharp.Pdf.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System;
using System.Windows;
using System.Text;
using KtaneManualDownloader.Enums;

namespace KtaneManualDownloader
{
    public class RepoHandler
    {
        public static RepoHandler Instance;

        // Make this configurable
        public static string BaseRepoURL = "https://ktane.timwi.de/";
        public static string RepoJsonUrl => BaseRepoURL + "json/raw";

        private static WebClient _wc = new WebClient();

        private List<KtaneModule> _rawModuleList;

        public RepoHandler()
        {
            Instance = this;
            GetRepoData();
        }

        public void GetRepoData()
        {
            string jsonString;
            using (var wc = new WebClient())
            {
                jsonString = wc.DownloadString(RepoJsonUrl);
            }

            var rawJSON = JObject.Parse(jsonString);
            var moduleArrayJSON = (JArray) rawJSON["KtaneModules"];

            _rawModuleList = new List<KtaneModule>();
            foreach (JObject mod in moduleArrayJSON)
            {
                var fileName = DecodeJsonString((string) (mod["FileName"] ?? mod["Name"]));
                var manualName = "PDF/" +
                                 Uri.EscapeUriString(fileName) +
                                 ".pdf";

                _rawModuleList.Add(new KtaneModule(
                    DecodeJsonString((string) mod["Name"]),
                    Path.Combine(BaseRepoURL, manualName),
                    (string) mod["SteamID"],
                    fileName,
                    TypeStringToEnum((string) mod["Type"]),
                    DiffStringToEnum((string) (mod["ExpertDifficulty"] ?? "Regular"))));
            }
        }

        public List<KtaneModule> GetKtaneModulesBySteamId(string workshopID) 
            => _rawModuleList.FindAll(module => module.SteamID == workshopID);

        /// <summary>
        /// Download the PDF at the URL into memory and open it as a PdfDocument.
        /// </summary>
        /// <param name="url">Link to the PDF, likely a manual. (Must be a PDF.)</param>
        /// <returns>A PdfDocument in Import mode, opened from the URL</returns>
        public PdfDocument DownloadManual(string url)
        {
            PdfDocument inputDocument = null;

            try
            {
                using (var stream = new MemoryStream(_wc.DownloadData(url)))
                {
                    inputDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                    // Read the page count so PdfSharp doesn't think it's 0 /shrug
                    var _ = inputDocument.PageCount;
                }
            }
            catch (WebException e)
            {
                if (((HttpWebResponse) e.Response).StatusCode == HttpStatusCode.NotFound)
                    Console.WriteLine("Not found: " + url);
            }
            catch (Exception e)
            {
                MessageBox.Show("Downloading this manual failed. We might crash now, oops.\n" + e.Message);
            }

            return inputDocument;
        }

        public string DecodeJsonString(string text)
        {
            var textBytes = Encoding.Default.GetBytes(text);
            return Encoding.UTF8.GetString(textBytes);
        }

        public ModuleType TypeStringToEnum(string type)
        {
            switch (type.ToLower().Trim())
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
            switch (difficulty.ToLower().Trim())
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
    }
}