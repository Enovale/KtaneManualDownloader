using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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

        public MainWindow()
        {
            new KMD_Main(this);
            InitializeComponent();
        }

        public void MainWindow_Loaded(object sender, EventArgs e)
        {
            ModListPanel.DataContext = KMD_Main.Instance;
            ProgressBar.DataContext = KMD_Main.Instance;

            MergedPDFPathBox.DataContext = Settings.Instance;
            ManualDownloadsBox.DataContext = Settings.Instance;
            ModsFolderBox.DataContext = Settings.Instance;

            RedownloadCheck.DataContext = Settings.Instance;
            VanillaMergeCheck.DataContext = Settings.Instance;
            ReverseOrderCheck.DataContext = Settings.Instance;
            MergeCheck.DataContext = Settings.Instance;

            ModMergeRadio.DataContext = Settings.Instance;
            ModuleMergeRadio.DataContext = Settings.Instance;
            DiffMergeRadio.DataContext = Settings.Instance;
            ModuleGroupCheck.DataContext = Settings.Instance;
        }

        public void ToggleControlsDuringDownload(bool state)
        {
            KMD_Main.Instance.downloading = !state;
            if (state)
            {
                DownloadBtn.Content = "Download";
            }
            else
            {
                DownloadBtn.Content = "Cancel";
            }

            foreach(KtaneMod mod in ModListPanel.Items)
            {
                mod.IsEnabled = state;
            }

            //downloadBtn.Enabled = state;
            MergeBtn.IsEnabled = state;
            RedownloadCheck.IsEnabled = state;
            ModsFolderBox.IsEnabled = state;
            SelectModsDirBtn.IsEnabled = state;
            MergeCheck.IsEnabled = state;
            ReverseOrderCheck.IsEnabled = state;
            VanillaMergeCheck.IsEnabled = state;
            ModMergeRadio.IsEnabled = state;
            ModuleMergeRadio.IsEnabled = state;
            DiffMergeRadio.IsEnabled = state;
            ModuleGroupCheck.IsEnabled = state;
            MergedPDFPathBox.IsEnabled = state;
            MergedPDFPathBtn.IsEnabled = state;
            ManualDownloadsBox.IsEnabled = state;
            ManualDownloadsBtn.IsEnabled = state;
            if (state)
                ToggleMergeControls(Settings.Instance.MergePDFs);
        }

        public void ToggleMergeControls(bool state)
        {
            ReverseOrderCheck.IsEnabled = state;
            VanillaMergeCheck.IsEnabled = state;
            ModMergeRadio.IsEnabled = state;
            ModuleMergeRadio.IsEnabled = state;
            DiffMergeRadio.IsEnabled = state;
            ModuleGroupCheck.IsEnabled = state;
            MergedPDFPathBox.IsEnabled = state;
            MergedPDFPathBtn.IsEnabled = state;
        }

        private void DownloadBtn_Click(object sender, EventArgs e)
        {
            if (KMD_Main.Instance.downloading)
            {
                KMD_Main.Instance.cancelWork = true;
            }
            else
            {
                ToggleControlsDuringDownload(false);
                Task.Run(KMD_Main.Instance.DownloadManuals);
            }
        }

        private void MergeBtn_Click(object sender, EventArgs e)
        {
            if (KMD_Main.Instance.downloading)
            {
                KMD_Main.Instance.cancelWork = true;
            }
            else
            {
                ToggleControlsDuringDownload(false);
                Task.Run(KMD_Main.Instance.MergeManuals);
            }
        }

        private void MergeCheck_Checked(object sender, EventArgs e) => ToggleMergeControls(true);
        private void MergeCheck_Unchecked(object sender, EventArgs e) => ToggleMergeControls(false);

        private void SelectModsDirBtn_Click(object sender, EventArgs e)
        {
            if(!CommonOpenFileDialog.IsPlatformSupported)
            {
                using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        Settings.Instance.ModsFolderLocation = fbd.SelectedPath;
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
                        Settings.Instance.ModsFolderLocation = fbd.FileName;
                    }
                }
            }
            KMD_Main.Instance.LoadMods();
        }

        private void ManualDownloadsBtn_Click(object sender, EventArgs e)
        {
            if (!CommonFileDialog.IsPlatformSupported)
            {
                using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        Settings.Instance.ManualDownloadsFolder = fbd.SelectedPath;
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
                        Settings.Instance.ManualDownloadsFolder = fbd.FileName;
                    }
                }
            }
        }

        private void MergedPDFPathBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog folderBrowser = new OpenFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                Title = "Selected where to output the merged PDF",
                InitialDirectory = Path.GetDirectoryName(Application.ResourceAssembly.Location),
                FileName = Path.GetFileName(Settings.Instance.MergedManualOutputPath),
                Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
        };
            if (folderBrowser.ShowDialog() == true)
            {
                Settings.Instance.MergedManualOutputPath = folderBrowser.FileName;

                Properties.Settings.Default.MergedPDFPath = Settings.Instance.MergedManualOutputPath;
                Properties.Settings.Default.Save();
            }
        }

        private void MergedPDFPathBox_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Settings.Instance.MergedManualOutputPath = MergedPDFPathBox.Text;

                Properties.Settings.Default.MergedPDFPath = Settings.Instance.MergedManualOutputPath;
                Properties.Settings.Default.Save();
            }
        }

        private void ManualDownloadsBox_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Settings.Instance.ManualDownloadsFolder = ManualDownloadsBox.Text;

                Properties.Settings.Default.ManualFolderPath = Settings.Instance.ManualDownloadsFolder;
                Properties.Settings.Default.Save();
            }
        }

        private void ModsFolderBox_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Settings.Instance.ModsFolderLocation = ModsFolderBox.Text;
                KMD_Main.Instance.LoadMods();

                Properties.Settings.Default.ModFolderPath = Settings.Instance.ModsFolderLocation;
                Properties.Settings.Default.Save();
            }
        }

        private void DeselectBtn_Click(object sender, EventArgs e)
        {
            foreach (KtaneMod mod in ModListPanel.Items)
            {
                mod.IsSelected = false;
            }
        }

        private void SelectBtn_Click(object sender, EventArgs e)
        {
            foreach (KtaneMod mod in ModListPanel.Items)
            {
                mod.IsSelected = true;
            }
        }

        private void ResetSettingsBtn_Click(object sender, EventArgs e)
        {
            KMD_Main.Instance.ResetSettings();
        }
    }
}
