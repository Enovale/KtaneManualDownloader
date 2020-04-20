using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KtaneManualDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public enum SortMode
        {
            Mod = 0,
            Module = 1,
            Difficulty = 2
        }

        private bool downloading = false;
        private bool cancelWork = false;

        public MainWindow()
        {
            InitializeComponent();
            new Main();
        }

        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            LoadSettings();

            mergedPDFPathBox.Text = Main.Instance.MergedManualOutputPath;
            manualDownloadsBox.Text = Main.Instance.ManualDownloadsFolder;
            modsFolderBox.Text = Main.Instance.ModsFolderLocation;
            LoadMods();
        }

        public void LoadSettings()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.ModFolderPath))
            {
                Main.Instance.ModsFolderLocation = Properties.Settings.Default.ModFolderPath;
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.ManualFolderPath))
            {
                Properties.Settings.Default.ManualFolderPath = Main.Instance.ManualDownloadsFolder;
            }
            else
            {
                Main.Instance.ManualDownloadsFolder = Properties.Settings.Default.ManualFolderPath;
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.MergedPDFPath))
            {
                Properties.Settings.Default.MergedPDFPath = Main.Instance.MergedManualOutputPath;
            }
            else
            {
                Main.Instance.MergedManualOutputPath = Properties.Settings.Default.MergedPDFPath;
            }

            Properties.Settings.Default.Save();
        }

        public void ResetSettings()
        {
            modsFolderBox.Text = "";
            manualDownloadsBox.Text = Path.GetFullPath("./Manuals/");
            mergedPDFPathBox.Text = "./MergedDocument.pdf";

            Properties.Settings.Default.ModFolderPath = modsFolderBox.Text;
            Properties.Settings.Default.ManualFolderPath = manualDownloadsBox.Text;
            Properties.Settings.Default.MergedPDFPath = mergedPDFPathBox.Text;
            Properties.Settings.Default.Save();

            modListPanel.Children.Clear();
        }

        public void LoadModList(string[] modList)
        {
            modListPanel.Children.Clear();
            foreach (string mod in modList)
            {
                int i = Array.IndexOf(modList, mod);
                CheckBox modEntry = new CheckBox
                {
                    Content = mod,
                    Margin = new Thickness(5, i * Height, modListPanel.Width - 25, Height),
                    IsChecked = true
                };
                modListPanel.Children.Add(modEntry);
            }
        }

        public void LoadMods(string modsFolder = null)
        {
            if ((Main.Instance.ModsFolderLocation ?? "").Trim() == "") return;
            if (!Main.Instance.IsFolderKtane(Main.Instance.ModsFolderLocation)) return;
            if (!Directory.Exists(Main.Instance.ModsFolderLocation)) return;

            Properties.Settings.Default.ModFolderPath = Main.Instance.ModsFolderLocation;
            Properties.Settings.Default.Save();

            List<(string, string)> modList = new List<(string, string)>();
            string[] dirs = Directory.GetDirectories(Main.Instance.ModsFolderLocation, "*", SearchOption.TopDirectoryOnly);
            foreach (string dir in dirs)
            {
                string steamID = new DirectoryInfo(dir).Name;
                JObject modInfo = JObject.Parse(File.ReadAllText(dir + "/modInfo.json"));
                modList.Add((modInfo["title"].ToString(), steamID));
            }
            modList.Sort((x, y) => string.Compare(x.Item1, y.Item1));

            List<string> modNameList = new List<string>();
            Main.Instance.CurrentModList = new List<string>();
            foreach ((string, string) mod in modList)
            {
                modNameList.Add(mod.Item1);
                Main.Instance.CurrentModList.Add(mod.Item2);
            }

            LoadModList(modNameList.ToArray());
        }

        public void SetProgressBar(int percentage)
        {
            progressBar.Value = percentage;
        }

        private void downloadBtn_Click(object sender, EventArgs e)
        {
            if (downloading)
            {
                cancelWork = true;
            }
            else
            {
                ToggleControlsDuringDownload(false);
                Task.Run(DownloadManuals);
            }
        }

        private void mergeBtn_Click(object sender, EventArgs e)
        {
            if (downloading)
            {
                cancelWork = true;
            }
            else
            {
                ToggleControlsDuringDownload(false);
                Task.Run(MergeManuals);
            }
        }

        private void DownloadManuals()
        {
            if (modListPanel.Children.Count <= 0 || !AreNoModsSelected())
            {
                progressBar.Dispatcher.Invoke(() => ToggleControlsDuringDownload(true));
                return;
            }
            Directory.CreateDirectory(Main.Instance.ManualDownloadsFolder);
            List<KtaneModule> moduleList = new List<KtaneModule>();
            for (int i = 0; i < modListPanel.Children.Count; i++)
            {
                if (cancelWork)
                {
                    progressBar.Dispatcher.Invoke(() => ToggleControlsDuringDownload(true));
                    progressBar.Dispatcher.Invoke(() => SetProgressBar(0));
                    cancelWork = false;
                    return;
                }
                if (!((CheckBox)modListPanel.Children[i]).IsChecked.Value) continue;
                var searchResults = Scraper.Instance.GetSearchResults(Main.Instance.CurrentModList[i]);
                foreach (KtaneModule module in searchResults)
                {
                    module.ModName = ((CheckBox)(modListPanel.Children[i])).Content.ToString();
                    moduleList.Add(module);
                    if (!redownloadCheck.IsChecked)
                    {
                        if (File.Exists((Main.Instance.ManualDownloadsFolder + module.ModuleName + ".pdf")))
                        {
                            continue;
                        }
                    }
                    PdfDocument manual = Scraper.Instance.DownloadManual(module.ManualURL);
                    manual.Save(Main.Instance.ManualDownloadsFolder + module.ModuleName + ".pdf");
                }
                progressBar.Dispatcher.Invoke(() => SetProgressBar((int)((float)i / modListPanel.Children.Count * 100)));
            }
            File.WriteAllText(Main.Instance.ManualJSONPath,
                JsonConvert.SerializeObject(moduleList));
            progressBar.Dispatcher.Invoke(() => ToggleControlsDuringDownload(true));
            progressBar.Dispatcher.Invoke(() => SetProgressBar(0));

            if (mergeCheck.IsChecked) MergeManuals();
        }

        private void MergeManuals()
        {
            SortMode sortMode = DetermineSortMode();

            if (modListPanel.Children.Count <= 0 || !AreNoModsSelected())
            {
                progressBar.Dispatcher.Invoke(() => ToggleControlsDuringDownload(true));
                return;
            }

            DirectoryInfo manualDir = new DirectoryInfo(Main.Instance.ManualDownloadsFolder);
            // If dir is empty or just modules.json
            if (manualDir.GetFiles().Length <= 1)
            {
                MessageBox.Show("No manuals have been downloaded, " +
                    "please redownload them or report this to the developer");
                return;
            }
            if (!File.Exists(Main.Instance.ManualJSONPath))
            {
                MessageBox.Show("You cannot merge manuals because you have not completed a download" +
                    "of the manuals yet. Starting it and cancelling it isn't enough");
                return;
            }

            progressBar.Dispatcher.Invoke(() => ToggleControlsDuringDownload(false));

            string unformattedJSON = File.ReadAllText(Main.Instance.ManualJSONPath);
            JArray modulesJSON = JArray.Parse(unformattedJSON);
            List<KtaneModule> modules = modulesJSON.ToObject<List<KtaneModule>>();
            modules = SortModuleList(sortMode, modules);
            if (moduleGroupCheck.IsChecked)
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
            if (reverseOrderCheck.IsChecked) modules.Reverse();

            // Remove unchecked mods
            foreach (CheckBox modCheck in modListPanel.Children)
            {
                if (!modCheck.IsChecked.Value)
                {
                    modules.RemoveAll(mod => mod.ModName == (string)modCheck.Content);
                }
            }

            PdfDocument mergedDocument = new PdfDocument();

            if (vanillaMergeCheck.IsChecked)
            {
                if (File.Exists(Main.Instance.CoverPath))
                {
                    PdfDocument coverPage = PdfReader.Open(
                        Main.Instance.CoverPath,
                        PdfDocumentOpenMode.Import);
                    foreach (PdfPage page in coverPage.Pages)
                    {
                        mergedDocument.AddPage(page);
                    }
                    coverPage.Dispose();
                }
                if (File.Exists(Main.Instance.IntroPath))
                {
                    PdfDocument introPage = PdfReader.Open(
                        Main.Instance.IntroPath,
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
                if (moduleGroupCheck.IsChecked)
                {
                    if (module.Type == Scraper.ModuleType.Regular)
                    {
                        if (!addedModuleSpacer)
                        {
                            if (File.Exists(Main.Instance.ModuleSpacerPath))
                            {
                                PdfDocument moduleSpacerPage = PdfReader.Open(
                                    Main.Instance.ModuleSpacerPath,
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
                            if (File.Exists(Main.Instance.NeedySpacerPath))
                            {
                                PdfDocument needySpacerPage = PdfReader.Open(
                                    Main.Instance.NeedySpacerPath,
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
                    Main.Instance.ManualDownloadsFolder + module.ModuleName + ".pdf",
                    PdfDocumentOpenMode.Import);
                foreach (PdfPage page in moduleManual.Pages)
                {
                    //string pageContent = page.Contents.Elements.GetDictionary(0).Stream.ToString();
                    //bool appendix = pageContent.FirstOrDefault(s => s.ToLower().Contains("appendix")) != null;
                    if (cancelWork)
                    {
                        progressBar.Dispatcher.Invoke(() => ToggleControlsDuringDownload(true));
                        progressBar.Dispatcher.Invoke(() => SetProgressBar(0));
                        cancelWork = false;
                        return;
                    }
                    mergedDocument.AddPage(page);
                }
                moduleManual.Dispose();
                int i = modules.IndexOf(module);
                progressBar.Dispatcher.Invoke(() => SetProgressBar((int)((float)i / modules.Count * 100)));
            }

            if (vanillaMergeCheck.IsChecked)
            {
                if (File.Exists(Main.Instance.AppendixPath))
                {
                    PdfDocument appendixPages = PdfReader.Open(
                        Main.Instance.AppendixPath,
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
                progressBar.Dispatcher.Invoke(() => SetProgressBar(100));
                mergedDocument.Save(Main.Instance.MergedManualOutputPath);
            }
            else
            {
                MessageBox.Show("An error occured trying save the merged PDF. Error: \n" + errorMessage);
            }

            mergedDocument.Dispose();
            modules = null;

            progressBar.Dispatcher.Invoke(() => ToggleControlsDuringDownload(true));
            progressBar.Dispatcher.Invoke(() => SetProgressBar(0));
            cancelWork = false;
        }

        private static List<KtaneModule> SortModuleList(SortMode sortMode, List<KtaneModule> modules)
        {
            switch (sortMode)
            {
                case SortMode.Mod:
                    modules = modules.OrderBy(mod => mod.ModName).ToList();
                    break;
                case SortMode.Module:
                    modules = modules.OrderBy(mod => mod.ModuleName).ToList();
                    break;
                case SortMode.Difficulty:
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

        private SortMode DetermineSortMode()
        {
            if (modMergeRadio.IsChecked)
            {
                return SortMode.Mod;
            }
            else if (moduleMergeRadio.IsChecked)
            {
                return SortMode.Module;
            }
            else
            {
                return SortMode.Difficulty;
            }
        }

        private void ToggleControlsDuringDownload(bool state)
        {
            downloading = !state;
            if (state)
            {
                downloadBtn.Content = "Download";
            }
            else
            {
                downloadBtn.Content = "Cancel";
            }

            //downloadBtn.Enabled = state;
            mergeBtn.IsEnabled = state;
            redownloadCheck.IsEnabled = state;
            modsFolderBox.IsEnabled = state;
            selectModsDirBtn.IsEnabled = state;
            mergeCheck.IsEnabled = state;
            reverseOrderCheck.IsEnabled = state;
            vanillaMergeCheck.IsEnabled = state;
            modMergeRadio.IsEnabled = state;
            moduleMergeRadio.IsEnabled = state;
            diffMergeRadio.IsEnabled = state;
            moduleGroupCheck.IsEnabled = state;
            mergedPDFPathBox.IsEnabled = state;
            mergedPDFPathBtn.IsEnabled = state;
            manualDownloadsBox.IsEnabled = state;
            manualDownloadsBtn.IsEnabled = state;
            if (state)
                mergeCheck_CheckedChanged(null, null);
        }

        private void ToggleMergeControls(bool state)
        {
            reverseOrderCheck.IsEnabled = state;
            vanillaMergeCheck.IsEnabled = state;
            modMergeRadio.IsEnabled = state;
            moduleMergeRadio.IsEnabled = state;
            diffMergeRadio.IsEnabled = state;
            moduleGroupCheck.IsEnabled = state;
            mergedPDFPathBox.IsEnabled = state;
            mergedPDFPathBtn.IsEnabled = state;
        }

        private bool AreNoModsSelected()
        {
            bool isOneChecked = false;
            foreach (CheckBox box in modListPanel.Children)
            {
                if (box.IsChecked.Value)
                {
                    isOneChecked = true;
                    break;
                }
            }
            return isOneChecked;
        }
    }
}
