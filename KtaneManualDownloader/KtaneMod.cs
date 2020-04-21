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

        public KtaneMod(string name, string id)
        {
            ModName = name;
            SteamID = id;
            IsSelected = true;
            IsEnabled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
