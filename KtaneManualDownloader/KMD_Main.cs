using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace KtaneManualDownloader
{
   public class KMD_Main : INotifyPropertyChanged
    {

        public static KMD_Main Instance;
        public static MainWindow Window;

        #region Path Vars
        public string ManualJSONPath
        {
            get
            {
                return Settings.Instance.ManualDownloadsFolder + "modules.json";
            }
        }
        public string VanillaDocsPath = Path.GetFullPath("./VanillaDocuments/");
        public string CoverPath
        {
            get
            {
                return VanillaDocsPath + "Cover.pdf";
            }
        }
        public string IntroPath
        {
            get
            {
                return VanillaDocsPath + "Intro.pdf";
            }
        }
        public string ModuleSpacerPath
        {
            get
            {
                return VanillaDocsPath + "ModuleSpacer.pdf";
            }
        }
        public string NeedySpacerPath
        {
            get
            {
                return VanillaDocsPath + "NeedySpacer.pdf";
            }
        }
        public string AppendixPath
        {
            get
            {
                return VanillaDocsPath + "VanillaAppendix.pdf";
            }
        }
        #endregion

        public ObservableCollection<KtaneMod> ModList { get; set; }

        public int _progress;
        public int DownloadProgress 
        {
            get { return _progress; } 
            set
            {
                if (value != _progress)
                {
                    _progress = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadProgress)));
                }
            }
        }

        public bool downloading = false;
        public bool cancelWork = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public KMD_Main(MainWindow uiWindow)
        {
            Instance = this;
            Window = uiWindow;
            new Settings();
            new Scraper();

            ModList = new ObservableCollection<KtaneMod>();

            LoadSettings();
            LoadMods();

            Settings.Instance.MergePDFs = true;
            Settings.Instance.ModuleMerge = true;
        }

        public void LoadSettings()
        {
            if(Settings.Instance.ModsFolderLocation == null)
            {
                Settings.Instance.ModsFolderLocation = "";
            }
            if (!string.IsNullOrEmpty(Properties.Settings.Default.ModFolderPath))
            {
                Settings.Instance.ModsFolderLocation = Properties.Settings.Default.ModFolderPath;
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.ManualFolderPath))
            {
                ResetSettings();
            }
            else
            {
                Settings.Instance.ManualDownloadsFolder = Properties.Settings.Default.ManualFolderPath;
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.MergedPDFPath))
            {
                ResetSettings();
            }
            else
            {
                Settings.Instance.MergedManualOutputPath = Properties.Settings.Default.MergedPDFPath;
            }

            Properties.Settings.Default.Save();
        }

        public void ResetSettings()
        {
            Settings.Instance.ModsFolderLocation = "";
            Settings.Instance.ManualDownloadsFolder = Path.GetFullPath("./Manuals/");
            Settings.Instance.MergedManualOutputPath = "./MergedDocument.pdf";

            Properties.Settings.Default.ModFolderPath = Settings.Instance.ModsFolderLocation;
            Properties.Settings.Default.ManualFolderPath = Settings.Instance.ManualDownloadsFolder;
            Properties.Settings.Default.MergedPDFPath = Settings.Instance.MergedManualOutputPath;
            Properties.Settings.Default.Save();

            ModList.Clear();
        }

        public void LoadMods()
        {
            if ((Settings.Instance.ModsFolderLocation).Trim() == "") return;
            if (!PathChecks.IsFolderKtane(Settings.Instance.ModsFolderLocation)) return;
            if (!Directory.Exists(Settings.Instance.ModsFolderLocation)) return;

            Properties.Settings.Default.ModFolderPath = Settings.Instance.ModsFolderLocation;
            Properties.Settings.Default.Save();

            string[] dirs = Directory.GetDirectories(Settings.Instance.ModsFolderLocation, "*", SearchOption.TopDirectoryOnly);
            ModList.Clear();
            var unsortedList = new List<KtaneMod>();
            foreach (string dir in dirs)
            {
                string steamID = new DirectoryInfo(dir).Name;
                JObject modInfo = JObject.Parse(File.ReadAllText(dir + "/modInfo.json"));
                unsortedList.Add(new KtaneMod(modInfo["title"].ToString(), steamID));
            }

            var sortedList = unsortedList.OrderBy(mod => mod.ModName);
            foreach(KtaneMod mod in sortedList)
            {
                ModList.Add(mod);
            }
        }

        public void SetProgressBar(int percentage)
        {
            DownloadProgress = percentage;
        }

        public void DownloadManuals()
        {
            if (ModList.Count <= 0 || !AreNoModsSelected())
            {
                MessageBox.Show("Please select at least 1 mod to download.");
                Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(true));
                return;
            }
            Directory.CreateDirectory(Settings.Instance.ManualDownloadsFolder);
            List<KtaneModule> moduleList = new List<KtaneModule>();
            for (int i = 0; i < ModList.Count; i++)
            {
                if (cancelWork)
                {
                    Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(true));
                    Window.Dispatcher.Invoke(() => SetProgressBar(0));
                    cancelWork = false;
                    return;
                }
                if (!ModList[i].IsSelected) continue;
                var searchResults = Scraper.Instance.GetSearchResults(ModList[i].SteamID);
                foreach (KtaneModule module in searchResults)
                {
                    module.ModName = ModList[i].ModName;
                    moduleList.Add(module);
                    if (!Settings.Instance.ForceRedownload)
                    {
                        if (File.Exists((Settings.Instance.ManualDownloadsFolder + module.ModuleName + ".pdf")))
                        {
                            continue;
                        }
                    }
                    PdfDocument manual = Scraper.Instance.DownloadManual(module.ManualURL);
                    manual.Save(Settings.Instance.ManualDownloadsFolder + module.ModuleName + ".pdf");
                }
                SetProgressBar((int)((float)i / ModList.Count * 100));
            }
            File.WriteAllText(ManualJSONPath,
                JsonConvert.SerializeObject(moduleList));
            Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(true));
            SetProgressBar(0);

            if (Settings.Instance.MergePDFs) MergeManuals();
        }

        public void MergeManuals()
        {
            Settings.SortMode sortMode = Settings.Instance.SortingChoice;

            if (ModList.Count <= 0 || !AreNoModsSelected())
            {
                MessageBox.Show("Please select at least 1 mod to merge.");
                Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(true));
                return;
            }

            DirectoryInfo manualDir = new DirectoryInfo(Settings.Instance.ManualDownloadsFolder);
            // If dir is empty or just modules.json
            if (manualDir.GetFiles().Length <= 1)
            {
                MessageBox.Show("No manuals have been downloaded, " +
                    "please redownload them or report this to the developer");
                Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(true));
                return;
            }
            if (!File.Exists(ManualJSONPath))
            {
                MessageBox.Show("You cannot merge manuals because you have not completed a download" +
                    "of the manuals yet. Starting it and cancelling it isn't enough");
                Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(true));
                return;
            }

            Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(false));

            string unformattedJSON = File.ReadAllText(KMD_Main.Instance.ManualJSONPath);
            JArray modulesJSON = JArray.Parse(unformattedJSON);
            List<KtaneModule> modules = modulesJSON.ToObject<List<KtaneModule>>();
            modules = SortModuleList(sortMode, modules);
            if (Settings.Instance.GroupByType)
            {
                List<List<KtaneModule>> groupedList = modules.GroupBy(mod => mod.Type)
                    .Select(grp => grp.ToList())
                    .ToList();
                List<KtaneModule> sortedList = new List<KtaneModule>();
                foreach (List<KtaneModule> list in groupedList)
                {
                    var sortedGroup = SortModuleList(sortMode, list);
                    sortedList.AddRange(sortedGroup);
                }
                modules = sortedList;
            }
            if (Settings.Instance.ReverseOrder) modules.Reverse();

            // Remove unchecked mods
            foreach (KtaneMod listedMod in ModList)
            {
                if (!listedMod.IsSelected)
                {
                    modules.RemoveAll(mod => mod.ModName == listedMod.ModName);
                }
            }

            PdfDocument mergedDocument = new PdfDocument();

            if (Settings.Instance.VanillaMerge)
            {
                if (File.Exists(KMD_Main.Instance.CoverPath))
                {
                    PdfDocument coverPage = PdfReader.Open(
                        KMD_Main.Instance.CoverPath,
                        PdfDocumentOpenMode.Import);
                    foreach (PdfPage page in coverPage.Pages)
                    {
                        mergedDocument.AddPage(page);
                    }
                    coverPage.Dispose();
                }
                if (File.Exists(KMD_Main.Instance.IntroPath))
                {
                    PdfDocument introPage = PdfReader.Open(
                        KMD_Main.Instance.IntroPath,
                        PdfDocumentOpenMode.Import);
                    foreach (PdfPage page in introPage.Pages)
                    {
                        mergedDocument.AddPage(page);
                    }
                    introPage.Dispose();
                }
            }


            bool addedModuleSpacer = false;
            bool addedNeedySpacer = false;
            foreach (KtaneModule module in modules)
            {
                if (Settings.Instance.GroupByType)
                {
                    if (module.Type == Scraper.ModuleType.Regular)
                    {
                        if (!addedModuleSpacer)
                        {
                            if (File.Exists(KMD_Main.Instance.ModuleSpacerPath))
                            {
                                PdfDocument moduleSpacerPage = PdfReader.Open(
                                    KMD_Main.Instance.ModuleSpacerPath,
                                    PdfDocumentOpenMode.Import);
                                foreach (PdfPage page in moduleSpacerPage.Pages)
                                {
                                    mergedDocument.AddPage(page);
                                }
                                moduleSpacerPage.Dispose();
                            }
                            addedModuleSpacer = true;
                        }
                    }

                    if (module.Type == Scraper.ModuleType.Needy)
                    {
                        if (!addedNeedySpacer)
                        {
                            if (File.Exists(KMD_Main.Instance.NeedySpacerPath))
                            {
                                PdfDocument needySpacerPage = PdfReader.Open(
                                    NeedySpacerPath,
                                    PdfDocumentOpenMode.Import);
                                foreach (PdfPage page in needySpacerPage.Pages)
                                {
                                    mergedDocument.AddPage(page);
                                }
                                needySpacerPage.Dispose();
                            }
                            addedNeedySpacer = true;
                        }
                    }
                }

                PdfDocument moduleManual = PdfReader.Open(
                    Settings.Instance.ManualDownloadsFolder + module.ModuleName + ".pdf",
                    PdfDocumentOpenMode.Import);
                foreach (PdfPage page in moduleManual.Pages)
                {
                    //string pageContent = page.Contents.Elements.GetDictionary(0).Stream.ToString();
                    //bool appendix = pageContent.FirstOrDefault(s => s.ToLower().Contains("appendix")) != null;
                    if (cancelWork)
                    {
                        Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(true));
                        SetProgressBar(0);
                        cancelWork = false;
                        return;
                    }
                    mergedDocument.AddPage(page);
                }
                moduleManual.Dispose();
                int i = modules.IndexOf(module);
                SetProgressBar((int)((float)i / modules.Count * 100));
            }

            if (Settings.Instance.VanillaMerge)
            {
                if (File.Exists(AppendixPath))
                {
                    PdfDocument appendixPages = PdfReader.Open(
                        AppendixPath,
                        PdfDocumentOpenMode.Import);
                    foreach (PdfPage page in appendixPages.Pages)
                    {
                        mergedDocument.AddPage(page);
                    }
                    appendixPages.Dispose();
                }
            }

            string errorMessage = "";
            if (mergedDocument.CanSave(ref errorMessage))
            {
                SetProgressBar(100);
                mergedDocument.Save(Settings.Instance.MergedManualOutputPath);
            }
            else
            {
                MessageBox.Show("An error occured trying save the merged PDF. Error: \n" + errorMessage);
            }

            mergedDocument.Dispose();
            modules = null;

            Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(true));
            SetProgressBar(0);
            cancelWork = false;
        }

        private static List<KtaneModule> SortModuleList(Settings.SortMode sortMode, List<KtaneModule> modules)
        {
            switch (sortMode)
            {
                case Settings.SortMode.Mod:
                    modules = modules.OrderBy(mod => mod.ModName).ToList();
                    break;
                case Settings.SortMode.Module:
                    modules = modules.OrderBy(mod => mod.ModuleName).ToList();
                    break;
                case Settings.SortMode.Difficulty:
                    List<List<KtaneModule>> groupedList = modules.GroupBy(mod => mod.Difficulty)
                        .Select(grp => grp.ToList())
                        .OrderBy(list => list.First().Difficulty)
                        .ToList();
                    List<KtaneModule> sortedList = new List<KtaneModule>();
                    foreach (List<KtaneModule> list in groupedList)
                    {
                        sortedList.AddRange(list.OrderBy(mod => mod.ModuleName).ToList());
                    }
                    modules = sortedList;
                    break;
            }

            return modules;
        }

        private bool AreNoModsSelected()
        {
            bool isOneChecked = false;
            foreach (KtaneMod mod in ModList)
            {
                if (mod.IsSelected)
                {
                    isOneChecked = true;
                    break;
                }
            }
            return isOneChecked;
        }
    }
}
