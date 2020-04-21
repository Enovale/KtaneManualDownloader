using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KtaneManualDownloader.Scraper;

namespace KtaneManualDownloader
{
    public class KtaneModule
    {

        public string ModuleName;
        public string ModName;
        public string ManualURL;
        public ModuleType Type;
        public ModuleDifficulty Difficulty;

        public KtaneModule(string name, string downloadUrl, ModuleType type, ModuleDifficulty difficulty)
        {
            ModuleName = name;
            ManualURL = downloadUrl;
            Type = type;
            Difficulty = difficulty;
        }

    }
}
