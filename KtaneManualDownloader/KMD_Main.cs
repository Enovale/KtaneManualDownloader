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
using KtaneManualDownloader.Enums;

namespace KtaneManualDownloader
{
    public class KMD_Main : INotifyPropertyChanged
    {
        public static KMD_Main Instance;
        public static MainWindow Window;

        #region Path Vars

        public string ResourcesPath = Path.GetFullPath("./Resources/");
        public string VanillaDocsPath => Path.Combine(ResourcesPath, "VanillaDocuments/");

        public string CoverPath => VanillaDocsPath + "Cover.pdf";

        public string IntroPath => VanillaDocsPath + "Intro.pdf";
        public string ModuleSpacerPath => VanillaDocsPath + "ModuleSpacer.pdf";
        public string NeedySpacerPath => VanillaDocsPath + "NeedySpacer.pdf";

        public string AppendixPath => VanillaDocsPath + "VanillaAppendix.pdf";

        #endregion

        public ObservableCollection<KtaneMod> ModList { get; set; }

        private int _progress;
        public int DownloadProgress
        {
            get => _progress;
            set
            {
                if (value != _progress)
                {
                    _progress = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadProgress)));
                }
            }
        }

        public bool Downloading = false;
        public bool CancelWork = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public KMD_Main(MainWindow uiWindow)
        {
            Instance = this;
            Window = uiWindow;
            new Settings();
            new RepoHandler();

            ModList = new ObservableCollection<KtaneMod>();

            LoadSettings();
            LoadMods();

            Settings.Instance.MergePDFs = true;
            Settings.Instance.ModuleMerge = true;
        }

        public void LoadSettings()
        {
            if (Settings.Instance.ModsFolderLocation == null) Settings.Instance.ModsFolderLocation = "";
            if (!string.IsNullOrEmpty(Properties.Settings.Default.ModFolderPath))
                Settings.Instance.ModsFolderLocation = Properties.Settings.Default.ModFolderPath;

            if (string.IsNullOrEmpty(Properties.Settings.Default.ManualFolderPath))
                ResetSettings();
            else
                Settings.Instance.ManualDownloadsFolder = Properties.Settings.Default.ManualFolderPath;

            if (string.IsNullOrEmpty(Properties.Settings.Default.MergedPDFPath))
                ResetSettings();
            else
                Settings.Instance.MergedManualOutputPath = Properties.Settings.Default.MergedPDFPath;

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
            if (Settings.Instance.ModsFolderLocation.Trim() == "") return;
            if (!PathChecks.IsFolderKtane(Settings.Instance.ModsFolderLocation)) return;
            if (!Directory.Exists(Settings.Instance.ModsFolderLocation)) return;

            Properties.Settings.Default.ModFolderPath = Settings.Instance.ModsFolderLocation;
            Properties.Settings.Default.Save();

            var dirs = Directory.GetDirectories(Settings.Instance.ModsFolderLocation, "*",
                SearchOption.TopDirectoryOnly);
            ModList.Clear();
            var unsortedList = new List<KtaneMod>();
            foreach (var dir in dirs)
            {
                var steamID = new DirectoryInfo(dir).Name;
                var modInfo = JObject.Parse(File.ReadAllText(dir + "/modInfo.json"));

                var searchResults = RepoHandler.Instance.GetKtaneModulesBySteamId(steamID);
                unsortedList.Add(new KtaneMod(modInfo["title"].ToString(), steamID, searchResults.ToArray()));
            }

            var sortedList = unsortedList.OrderBy(mod => mod.ModName);
            foreach (var mod in sortedList) ModList.Add(mod);

            UpdateDownloadStatus();
        }

        public void UpdateDownloadStatus()
        {
            if (!Directory.Exists(Settings.Instance.ManualDownloadsFolder)) return;
            var manualDir = new DirectoryInfo(Settings.Instance.ManualDownloadsFolder);
            foreach (var mod in ModList)
            {
                var allDownloaded = mod.Modules.ToList().FindAll(module =>
                {
                    return File.Exists(
                        Settings.Instance.ManualDownloadsFolder + module.ModuleName + ".pdf");
                }).Count == mod.Modules.Length;

                if (allDownloaded) mod.IsDownloaded = true;
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
            var moduleList = new List<KtaneModule>();
            for (var i = 0; i < ModList.Count; i++)
            {
                if (CancelWork)
                {
                    Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(true));
                    Window.Dispatcher.Invoke(() => SetProgressBar(0));
                    CancelWork = false;
                    return;
                }

                if (!ModList[i].IsSelected) continue;
                var modModules = ModList[i].Modules;
                foreach (var module in modModules)
                {
                    module.ModName = ModList[i].ModName;
                    moduleList.Add(module);
                    if (!Settings.Instance.ForceRedownload)
                        if (File.Exists(Settings.Instance.ManualDownloadsFolder + module.FileName + ".pdf"))
                            continue;
                    var manual = RepoHandler.Instance.DownloadManual(module.ManualURL);
                    manual.Save(Settings.Instance.ManualDownloadsFolder + module.FileName + ".pdf");
                }

                ModList[i].IsDownloaded = true;
                SetProgressBar((int) ((float) i / ModList.Count * 100));
            }

            Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(true));
            SetProgressBar(0);

            if (Settings.Instance.MergePDFs) MergeManuals();
        }

        public void MergeManuals()
        {
            var sortMode = Settings.Instance.SortingChoice;

            if (ModList.Count <= 0 || !AreNoModsSelected())
            {
                MessageBox.Show("Please select at least 1 mod to merge.");
                Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(true));
                return;
            }

            var manualDir = new DirectoryInfo(Settings.Instance.ManualDownloadsFolder);
            // If dir is empty or just modules.json
            if (manualDir.GetFiles().Length <= 1)
            {
                MessageBox.Show("No manuals have been downloaded, " +
                                "please redownload them or report this to the developer");
                Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(true));
                return;
            }

            Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(false));

            var selectedMods = ModList.ToList().FindAll(mod => mod.IsSelected);
            var modules = new List<KtaneModule>();
            selectedMods.ForEach(mod => modules.AddRange(mod.Modules));
            modules = SortModuleList(sortMode, modules);
            if (Settings.Instance.GroupByType)
            {
                var groupedList = modules.GroupBy(mod => (int)mod.Type)
                    .Select(grp => grp.ToList())
                    .ToList();
                var sortedList = new List<KtaneModule>();
                foreach (var list in groupedList)
                {
                    var sortedGroup = SortModuleList(sortMode, list);
                    sortedList.AddRange(sortedGroup);
                }

                modules = sortedList;
            }

            if (Settings.Instance.ReverseOrder) modules.Reverse();

            // Remove unchecked mods
            foreach (var listedMod in ModList)
                if (!listedMod.IsSelected)
                    modules.RemoveAll(mod => mod.ModName == listedMod.ModName);

            var mergedDocument = new PdfDocument();

            if (Settings.Instance.VanillaMerge)
            {
                if (File.Exists(Instance.CoverPath))
                {
                    var coverPage = PdfReader.Open(
                        Instance.CoverPath,
                        PdfDocumentOpenMode.Import);
                    foreach (var page in coverPage.Pages) mergedDocument.AddPage(page);
                    coverPage.Dispose();
                }

                if (File.Exists(Instance.IntroPath))
                {
                    var introPage = PdfReader.Open(
                        Instance.IntroPath,
                        PdfDocumentOpenMode.Import);
                    foreach (var page in introPage.Pages) mergedDocument.AddPage(page);
                    introPage.Dispose();
                }
            }


            var addedModuleSpacer = false;
            var addedNeedySpacer = false;
            foreach (var module in modules)
            {
                if (Settings.Instance.GroupByType)
                {
                    if (module.Type == ModuleType.Regular)
                        if (!addedModuleSpacer)
                        {
                            if (File.Exists(Instance.ModuleSpacerPath))
                            {
                                var moduleSpacerPage = PdfReader.Open(
                                    Instance.ModuleSpacerPath,
                                    PdfDocumentOpenMode.Import);
                                foreach (var page in moduleSpacerPage.Pages) mergedDocument.AddPage(page);
                                moduleSpacerPage.Dispose();
                            }

                            addedModuleSpacer = true;
                        }

                    if (module.Type == ModuleType.Needy)
                        if (!addedNeedySpacer)
                        {
                            if (File.Exists(Instance.NeedySpacerPath))
                            {
                                var needySpacerPage = PdfReader.Open(
                                    NeedySpacerPath,
                                    PdfDocumentOpenMode.Import);
                                foreach (var page in needySpacerPage.Pages) mergedDocument.AddPage(page);
                                needySpacerPage.Dispose();
                            }

                            addedNeedySpacer = true;
                        }
                }

                var moduleManual = PdfReader.Open(
                    Settings.Instance.ManualDownloadsFolder + module.ModuleName + ".pdf",
                    PdfDocumentOpenMode.Import);
                foreach (var page in moduleManual.Pages)
                {
                    //string pageContent = page.Contents.Elements.GetDictionary(0).Stream.ToString();
                    //bool appendix = pageContent.FirstOrDefault(s => s.ToLower().Contains("appendix")) != null;
                    if (CancelWork)
                    {
                        Window.Dispatcher.Invoke(() => Window.ToggleControlsDuringDownload(true));
                        SetProgressBar(0);
                        CancelWork = false;
                        return;
                    }

                    mergedDocument.AddPage(page);
                }

                moduleManual.Dispose();
                var i = modules.IndexOf(module);
                SetProgressBar((int) ((float) i / modules.Count * 100));
            }

            if (Settings.Instance.VanillaMerge)
                if (File.Exists(AppendixPath))
                {
                    var appendixPages = PdfReader.Open(
                        AppendixPath,
                        PdfDocumentOpenMode.Import);
                    foreach (var page in appendixPages.Pages) mergedDocument.AddPage(page);
                    appendixPages.Dispose();
                }

            var errorMessage = "";
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
            CancelWork = false;
        }

        private static List<KtaneModule> SortModuleList(SortMode sortMode, List<KtaneModule> modules)
        {
            switch (sortMode)
            {
                case SortMode.Mod:
                    modules.Sort((x, y) => SmartCompare(x.ModName, y.ModName));
                    break;
                case SortMode.Module:
                    modules.Sort((x, y) => SmartCompare(x.ModuleName, y.ModuleName));
                    break;
                case SortMode.Difficulty:
                    var groupedList = modules.GroupBy(mod => (int)mod.Difficulty)
                        .Select(grp => grp.ToList())
                        .OrderBy(list => (int)list.First().Difficulty)
                        .ToList();
                    var sortedList = new List<KtaneModule>();
                    foreach (var list in groupedList)
                    {
                        list.Sort((x, y) => SmartCompare(x.ModuleName, y.ModuleName));
                        sortedList.AddRange(list);
                    }

                    modules = sortedList;
                    break;
            }

            return modules;
        }

        private bool AreNoModsSelected()
        {
            var isOneChecked = false;
            foreach (var mod in ModList)
                if (mod.IsSelected)
                {
                    isOneChecked = true;
                    break;
                }

            return isOneChecked;
        }

        private static Regex smartCompareExpression
            = new Regex(@"^(?:A |The )\s*",
                RegexOptions.Compiled |
                RegexOptions.CultureInvariant |
                RegexOptions.IgnoreCase);

        public static int SmartCompare(string x, string y)
        {
            x = smartCompareExpression.Replace(x, "");
            y = smartCompareExpression.Replace(y, "");

            return x.CompareTo(y);
        }
    }
}