using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PdfSharp.Pdf;
using System.IO;
using PdfSharp.Pdf.IO;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System;

namespace KtaneManualDownloader
{
    public class RepoHandler
    {

        public static RepoHandler Instance;

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
        public static string BaseRepoURL = "https://ktane.timwi.de/";
        public static string RepoJsonUrl
        {
            get
            {
                return BaseRepoURL + "json/raw";
            }
        }

        private List<KtaneModule> rawModuleList;

        public RepoHandler()
        {
            Instance = this;
            GetRepoData();
        }

        public void GetRepoData()
        {
            string jsonString;
            using (WebClient wc = new WebClient())
            {
                jsonString = wc.DownloadString(RepoJsonUrl);
            }
            JObject rawJSON = JObject.Parse(jsonString);
            JArray moduleArrayJSON = (JArray)rawJSON["KtaneModules"];

            rawModuleList = new List<KtaneModule>();
            foreach (JObject mod in moduleArrayJSON)
            {
                rawModuleList.Add(new KtaneModule(
                    mod["Name"].ToString(),
                    Uri.EscapeUriString(Path.Combine(BaseRepoURL, "PDF/" + (mod["FileName"] ?? mod["Name"]) + ".pdf")),
                    mod["SteamID"].ToString(),
                    "" + (mod["FileName"] ?? mod["Name"]),
                    TypeStringToEnum(mod["Type"].ToString()),
                    DiffStringToEnum("" + (mod["ExpertDifficulty"] ?? "Regular"))));
            }
        }

        public List<KtaneModule> GetKtaneModulesBySteamID(string workshopID)
        {
            return rawModuleList.FindAll(module => module.SteamID == workshopID);
        }

        /// <summary>
        /// Download the PDF at the URL into memory and open it as a PdfDocument.
        /// </summary>
        /// <param name="url">Link to the PDF, likely a manual. (Must be a PDF.)</param>
        /// <returns>A PdfDocument in Import mode, opened from the URL</returns>
        public PdfDocument DownloadManual(string url)
        {
            PdfDocument inputDocument = null;

            using (WebClient wc = new WebClient())
            {
                using (MemoryStream stream = new MemoryStream(wc.DownloadData(url)))
                {
                    inputDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                    // Read the page count so PdfSharp doesn't think it's 0 /shrug
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
    }
}
