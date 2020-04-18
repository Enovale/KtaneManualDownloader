﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KtaneManualDownloader
{
    public partial class MainForm : Form
    {

        public enum SortMode
        {
            Mod = 0,
            Module = 1,
            Difficulty = 2
        }

        private bool downloading = false;
        private bool cancelWork = false;

        public MainForm()
        {
            InitializeComponent();
            new Main();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            mergedPDFPathBox.Text = Main.Instance.MergedManualOutputPath;
            manualDownloadsBox.Text = Main.Instance.ManualDownloadsFolder;
            LoadMods();
        }

        public void LoadModList(string[] modList)
        {
            modListPanel.Controls.Clear();
            foreach(string mod in modList)
            {
                int i = Array.IndexOf(modList, mod);
                CheckBox modEntry = new CheckBox();
                modEntry.Text = mod;
                modEntry.Location = new Point(5, i * modEntry.Height);
                modEntry.Size = new Size(modListPanel.Width - 25, modEntry.Height);
                modEntry.Checked = true;
                modListPanel.Controls.Add(modEntry);
            }
        }

        public void LoadMods(string modsFolder = null)
        {
            if (modsFolder != null) Main.Instance.ModsFolderLocation = modsFolder;
            if ((Main.Instance.ModsFolderLocation ?? "").Trim() == "") return;
            if (!Main.Instance.IsFolderKtane(Main.Instance.ModsFolderLocation)) return;
            if (!Directory.Exists(Main.Instance.ModsFolderLocation)) return;

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
            Application.DoEvents();
        }

        private void downloadBtn_Click(object sender, EventArgs e)
        {
            if(downloading)
            {
                cancelWork = true;
            }
            else
            {
                ToggleControlsDuringDownload(false);
                Task.Run(DownloadManuals);
            }
        }

        private void DownloadManuals()
        {
            if (modListPanel.Controls.Count <= 0)
            {
                MethodInvoker enableControlsFailure = new MethodInvoker(() =>
                    ToggleControlsDuringDownload(true));
                Invoke(enableControlsFailure);
                return;
            }
            Directory.CreateDirectory(Main.Instance.ManualDownloadsFolder);
            List<KtaneModule> moduleList = new List<KtaneModule>();
            for (int i = 0; i < modListPanel.Controls.Count; i++)
            {
                if(cancelWork)
                {
                    MethodInvoker enableControlsCancelled = new MethodInvoker(() =>
                        ToggleControlsDuringDownload(true));
                    Invoke(enableControlsCancelled);
                    MethodInvoker clearProgress = new MethodInvoker(() =>
                        SetProgressBar(0));
                    progressBar.Invoke(clearProgress);
                    cancelWork = false;
                    return;
                }
                if (!((CheckBox)(modListPanel.Controls[i])).Checked) continue;
                var searchResults = Scraper.Instance.GetSearchResults(Main.Instance.CurrentModList[i]);
                foreach (KtaneModule module in searchResults)
                {
                    module.ModName = modListPanel.Controls[i].Text;
                    moduleList.Add(module);
                    if(File.Exists((Main.Instance.ManualDownloadsFolder + module.ModuleName + ".pdf"))) {
                        continue;
                    }
                    PdfDocument manual = Scraper.Instance.DownloadManual(module.ManualURL);
                    manual.Save(Main.Instance.ManualDownloadsFolder + module.ModuleName + ".pdf");
                }
                MethodInvoker progressUpdate = new MethodInvoker(() => 
                    SetProgressBar((int)((float)i / modListPanel.Controls.Count * 100)));
                progressBar.Invoke(progressUpdate);
            }
            File.WriteAllText(Main.Instance.ManualJSONPath, 
                JsonConvert.SerializeObject(moduleList));
            MethodInvoker reenableControls = new MethodInvoker(() =>
                ToggleControlsDuringDownload(true));
            Invoke(reenableControls);
            MethodInvoker clearProgressBar = new MethodInvoker(() =>
                SetProgressBar(0));
            progressBar.Invoke(clearProgressBar);

            if (mergeCheck.Checked) MergeManuals();
        }

        private void MergeManuals()
        {
            SortMode sortMode = DetermineSortMode();

            DirectoryInfo manualDir = new DirectoryInfo(Main.Instance.ManualDownloadsFolder);
            // If dir is empty or just modules.json
            if (manualDir.GetFiles().Length <= 1)
            {
                MessageBox.Show("No manuals have been downloaded, " +
                    "please redownload them or report this to the developer");
                return;
            }

            MethodInvoker disableControls = new MethodInvoker(() =>
                ToggleControlsDuringDownload(false));
            Invoke(disableControls);

            string unformattedJSON = File.ReadAllText(Main.Instance.ManualJSONPath);
            JArray modulesJSON = JArray.Parse(unformattedJSON);
            List<KtaneModule> modules = modulesJSON.ToObject<List<KtaneModule>>();
            modules = SortModuleList(sortMode, modules);
            if (moduleGroupCheck.Checked)
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
            if (reverseOrderCheck.Checked) modules.Reverse();

            PdfDocument mergedDocument = new PdfDocument();

            List<PdfPage> modulePages = new List<PdfPage>();
            List<PdfPage> appendixPages = new List<PdfPage>();
            foreach (KtaneModule module in modules)
            {
                PdfDocument moduleManual = PdfReader.Open(
                    Main.Instance.ManualDownloadsFolder + module.ModuleName + ".pdf",
                    PdfDocumentOpenMode.Import);
                foreach (PdfPage page in moduleManual.Pages)
                {
                    //string pageContent = page.Contents.Elements.GetDictionary(0).Stream.ToString();
                    //bool appendix = pageContent.FirstOrDefault(s => s.ToLower().Contains("appendix")) != null;
                    if (cancelWork)
                    {
                        MethodInvoker enableControlsCancelled = new MethodInvoker(() =>
                            ToggleControlsDuringDownload(true));
                        Invoke(enableControlsCancelled);
                        MethodInvoker clearProgress = new MethodInvoker(() =>
                            SetProgressBar(0));
                        progressBar.Invoke(clearProgress);
                        cancelWork = false;
                        return;
                    }
                    bool appendix = false;
                    if (!appendix)
                    {
                        modulePages.Add(page);
                    }
                    else
                    {
                        appendixPages.Add(page);
                    }
                }
                int i = modules.IndexOf(module);
                MethodInvoker progressUpdate = new MethodInvoker(() =>
                    SetProgressBar((int)((float)i / modules.Count * 100)));
                progressBar.Invoke(progressUpdate);
            }

            foreach (PdfPage page in modulePages)
            {
                mergedDocument.AddPage(page);
            }
            foreach (PdfPage page in appendixPages)
            {
                mergedDocument.AddPage(page);
            }

            string errorMessage = "";
            if (mergedDocument.CanSave(ref errorMessage))
            {
                mergedDocument.Save(Main.Instance.MergedManualOutputPath);
            }
            else
            {
                MessageBox.Show("An error occured trying save the merged PDF. Error: \n" + errorMessage);
            }

            mergedDocument.Dispose();
            modulePages = null;
            appendixPages = null;

            MethodInvoker reenableControls = new MethodInvoker(() =>
                ToggleControlsDuringDownload(true));
            Invoke(reenableControls);
            MethodInvoker clearProgressBar = new MethodInvoker(() =>
                SetProgressBar(0));
            progressBar.Invoke(clearProgressBar);
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
                    modules = modules.OrderBy(mod => mod.Difficulty).ToList();
                    break;
            }

            return modules;
        }

        private SortMode DetermineSortMode()
        {
            if(modMergeRadio.Checked)
            {
                return SortMode.Mod;
            } 
            else if(moduleMergeRadio.Checked)
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
            if(state)
            {
                downloadBtn.Text = "Download";
            }
            else
            {
                downloadBtn.Text = "Cancel";
            }

            //downloadBtn.Enabled = state;
            modsFolderBox.Enabled = state;
            selectModsDirBtn.Enabled = state;
            mergeCheck.Enabled = state;
            reverseOrderCheck.Enabled = state;
            appendAppendixesCheck.Enabled = state;
            modMergeRadio.Enabled = state;
            moduleMergeRadio.Enabled = state;
            diffMergeRadio.Enabled = state;
            moduleGroupCheck.Enabled = state;
            mergedPDFPathBox.Enabled = state;
            mergedPDFPathBtn.Enabled = state;
            manualDownloadsBox.Enabled = state;
            manualDownloadsBtn.Enabled = state;
            if (state) 
                mergeCheck_CheckedChanged(null, null);
        }

        private void ToggleMergeControls(bool state)
        {
            reverseOrderCheck.Enabled = state;
            appendAppendixesCheck.Enabled = state;
            modMergeRadio.Enabled = state;
            moduleMergeRadio.Enabled = state;
            diffMergeRadio.Enabled = state;
            moduleGroupCheck.Enabled = state;
            mergedPDFPathBox.Enabled = state;
            mergedPDFPathBtn.Enabled = state;
        }

        private void mergeCheck_CheckedChanged(object sender, EventArgs e)
        {
            if(mergeCheck.Checked)
            {
                ToggleMergeControls(true);
            } 
            else
            {
                ToggleMergeControls(false);
            }
        }

        private void selectModsDirBtn_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    modsFolderBox.Text = fbd.SelectedPath;
                    Main.Instance.ModsFolderLocation = fbd.SelectedPath;
                }
            }
            LoadMods();
        }

        private void manualDownloadsBtn_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    manualDownloadsBox.Text = fbd.SelectedPath + "\\";
                    Main.Instance.ManualDownloadsFolder = fbd.SelectedPath + "\\";
                }
            }
        }

        private void mergedPDFPathBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog folderBrowser = new OpenFileDialog();
            folderBrowser.CheckFileExists = false;
            folderBrowser.CheckPathExists = true;
            folderBrowser.Title = "Selected where to output the merged PDF";
            folderBrowser.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            folderBrowser.FileName = Path.GetFileName(Main.Instance.MergedManualOutputPath);
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                mergedPDFPathBox.Text = folderBrowser.FileName;
                Main.Instance.MergedManualOutputPath = folderBrowser.FileName;
            }
        }

        private void mergedPDFPathBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                Main.Instance.MergedManualOutputPath = mergedPDFPathBox.Text;
            }
        }

        private void manualDownloadsBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Main.Instance.ManualDownloadsFolder = manualDownloadsBox.Text;
            }
        }

        private void modsFolderBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                Main.Instance.ModsFolderLocation = modsFolderBox.Text;
                LoadMods();
            }
        }
    }
}