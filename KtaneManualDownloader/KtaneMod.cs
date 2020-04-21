using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KtaneManualDownloader
{
    public class KtaneMod : INotifyPropertyChanged
    {

        public KtaneModule[] Modules;
        public string ModName { get; set; }
        public string SteamID { get; set; }
        private bool _selected;
        public bool IsSelected
        {
            get => _selected;
            set
            {
                _selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
        private bool _enabled;
        public bool IsEnabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
            }
        }
        private bool _downloaded;
        public bool IsDownloaded
        {
            get 
            {
                if (NoManual) return false;
                return _downloaded;
            }
            set
            {
                _downloaded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDownloaded)));
            }
        }
        public bool NoManual
        {
            get
            {
                if (Modules.Length <= 0) return true;
                return false;
            }
        }

        public KtaneMod(string name, string id, KtaneModule[] modulesInMod)
        {
            Modules = modulesInMod;
            ModName = name;
            SteamID = id;
            IsSelected = true;
            IsEnabled = true;
            IsDownloaded = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
