using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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

namespace KtaneManualDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Atlas.UI.Window
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

            mergeCheck.IsChecked = true;
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
                    Margin = new Thickness(5, i * 16, modListPanel.ActualWidth - 25, 16),
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
                //DownloadManuals();
                Task.Run(() =>
                {
                    if (modListPanel.Children.OfType<CheckBox>().Count() <= 0 || !AreNoModsSelected())
                    {
                        progressBar.Dispatcher.Invoke(() => ToggleControlsDuringDownload(true));
                        return;
                    }
                    Directory.CreateDirectory(Main.Instance.ManualDownloadsFolder);
                    progressBar.Dispatcher.Invoke(() => ToggleControlsDuringDownload(true));
                    progressBar.Dispatcher.Invoke(() => SetProgressBar(100));
                });
                //Task.Run(DownloadManuals);
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
            if (modListPanel.Children.OfType<CheckBox>().Count() <= 0 || !AreNoModsSelected())
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
                if (!(bool)((CheckBox)modListPanel.Children[i]).IsChecked) continue;
                var searchResults = Scraper.Instance.GetSearchResults(Main.Instance.CurrentModList[i]);
                foreach (KtaneModule module in searchResults)
                {
                    module.ModName = ((CheckBox)(modListPanel.Children[i])).Content.ToString();
                    moduleList.Add(module);
                    if (!(bool)redownloadCheck.IsChecked)
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

            if ((bool)mergeCheck.IsChecked) MergeManuals();
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
            if ((bool)moduleGroupCheck.IsChecked)
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
            if ((bool)reverseOrderCheck.IsChecked) modules.Reverse();

            // Remove unchecked mods
            foreach (CheckBox modCheck in modListPanel.Children)
            {
                if (!(bool)modCheck.IsChecked)
                {
                    modules.RemoveAll(mod => mod.ModName == (string)modCheck.Content);
                }
            }

            PdfDocument mergedDocument = new PdfDocument();

            if ((bool)vanillaMergeCheck.IsChecked)
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
                if ((bool)moduleGroupCheck.IsChecked)
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

            if ((bool)vanillaMergeCheck.IsChecked)
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
            if ((bool)modMergeRadio.IsChecked)
            {
                return SortMode.Mod;
            }
            else if ((bool)moduleMergeRadio.IsChecked)
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
                ToggleMergeControls((bool)mergeCheck.IsChecked);
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
            foreach (CheckBox box in modListPanel.Children.OfType<CheckBox>())
            {
                if ((bool)box.IsChecked)
                {
                    isOneChecked = true;
                    break;
                }
            }
            return isOneChecked;
        }

        private void mergeCheck_Checked(object sender, EventArgs e) 
        {
            ToggleMergeControls(true); 
        }
        private void mergeCheck_Unchecked(object sender, EventArgs e) { ToggleMergeControls(false); }

        private void selectModsDirBtn_Click(object sender, EventArgs e)
        {
            if(!CommonOpenFileDialog.IsPlatformSupported)
            {
                using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        modsFolderBox.Text = fbd.SelectedPath;
                        Main.Instance.ModsFolderLocation = fbd.SelectedPath;
                    }
                }
            }
            else
            {
                using (var fbd = new CommonOpenFileDialog())
                {
                    fbd.IsFolderPicker = true;

                    CommonFileDialogResult result = fbd.ShowDialog();
                    if(result == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(fbd.FileName))
                    {
                        modsFolderBox.Text = fbd.FileName;
                        Main.Instance.ModsFolderLocation = fbd.FileName;
                    }
                }
            }
            LoadMods();
        }

        private void manualDownloadsBtn_Click(object sender, EventArgs e)
        {
            if (!CommonOpenFileDialog.IsPlatformSupported)
            {
                using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        manualDownloadsBox.Text = fbd.SelectedPath;
                        Main.Instance.ManualDownloadsFolder = fbd.SelectedPath;
                    }
                }
            }
            else
            {
                using (var fbd = new CommonOpenFileDialog())
                {
                    fbd.IsFolderPicker = true;

                    CommonFileDialogResult result = fbd.ShowDialog();
                    if (result == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(fbd.FileName))
                    {
                        manualDownloadsBox.Text = fbd.FileName;
                        Main.Instance.ManualDownloadsFolder = fbd.FileName;
                    }
                }
            }
        }

        private void mergedPDFPathBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog folderBrowser = new OpenFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                Title = "Selected where to output the merged PDF",
                InitialDirectory = Path.GetDirectoryName(Application.ResourceAssembly.Location),
                FileName = Path.GetFileName(Main.Instance.MergedManualOutputPath),
                Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
        };
            if (folderBrowser.ShowDialog() == true)
            {
                mergedPDFPathBox.Text = folderBrowser.FileName;
                Main.Instance.MergedManualOutputPath = folderBrowser.FileName;

                Properties.Settings.Default.MergedPDFPath = Main.Instance.MergedManualOutputPath;
                Properties.Settings.Default.Save();
            }
        }

        private void mergedPDFPathBox_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Main.Instance.MergedManualOutputPath = mergedPDFPathBox.Text;

                Properties.Settings.Default.MergedPDFPath = Main.Instance.MergedManualOutputPath;
                Properties.Settings.Default.Save();
            }
        }

        private void manualDownloadsBox_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Main.Instance.ManualDownloadsFolder = manualDownloadsBox.Text;

                Properties.Settings.Default.ManualFolderPath = Main.Instance.ManualDownloadsFolder;
                Properties.Settings.Default.Save();
            }
        }

        private void modsFolderBox_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Main.Instance.ModsFolderLocation = modsFolderBox.Text;
                LoadMods();
            }
        }

        private void deselectBtn_Click(object sender, EventArgs e)
        {
            foreach (CheckBox box in modListPanel.Children)
            {
                box.IsChecked = false;
            }
        }

        private void selectBtn_Click(object sender, EventArgs e)
        {
            foreach (CheckBox box in modListPanel.Children)
            {
                box.IsChecked = true;
            }
        }

        private void resetSettingsBtn_Click(object sender, EventArgs e)
        {
            ResetSettings();
        }
    }
}
