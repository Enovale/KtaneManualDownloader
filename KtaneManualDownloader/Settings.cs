using System.ComponentModel;
using KtaneManualDownloader.Enums;

namespace KtaneManualDownloader
{
    public class Settings : INotifyPropertyChanged
    {
        public static Settings Instance;

        #region CheckBoxes

        public bool MergePDFs { get; set; }
        public bool ReverseOrder { get; set; }
        public bool VanillaMerge { get; set; }
        public bool ForceRedownload { get; set; }
        public bool GroupByType { get; set; }

        #endregion

        #region Paths

        private string modsFolderLocation;

        public string ModsFolderLocation
        {
            get => modsFolderLocation;
            set
            {
                modsFolderLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModsFolderLocation)));
            }
        }

        private string manualDownloadsFolder;

        public string ManualDownloadsFolder
        {
            get => manualDownloadsFolder;
            set
            {
                manualDownloadsFolder = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ManualDownloadsFolder)));
            }
        }

        private string mergedManualOutputPath;

        public string MergedManualOutputPath
        {
            get => mergedManualOutputPath;
            set
            {
                mergedManualOutputPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MergedManualOutputPath)));
            }
        }

        #endregion

        public SortMode SortingChoice => DetermineSortMode();

        /// <summary>
        /// Don't access directly!
        /// </summary>
        public bool ModMerge { get; set; }

        /// <summary>
        /// Don't access directly!
        /// </summary>
        public bool ModuleMerge { get; set; }

        /// <summary>
        /// Don't access directly!
        /// </summary>
        public bool DifficultyMerge { get; set; }

        public Settings()
        {
            Instance = this;
        }

        private SortMode DetermineSortMode()
        {
            if (ModMerge)
                return SortMode.Mod;
            else if (ModuleMerge)
                return SortMode.Module;
            else
                return SortMode.Difficulty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}