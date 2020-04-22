using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KtaneManualDownloader.RepoHandler;

namespace KtaneManualDownloader
{
    public class KtaneModule
    {

        public string ModuleName;
        public string ModName;
        public string ManualURL;
        public string SteamID;
        public string FileName;
        public ModuleType Type;
        public ModuleDifficulty Difficulty;

        public KtaneModule(string name, string downloadUrl, string steamID, string fileName, ModuleType type, ModuleDifficulty difficulty)
        {
            ModuleName = name;
            ManualURL = downloadUrl;
            SteamID = steamID;
            FileName = fileName;
            Type = type;
            Difficulty = difficulty;
        }

    }
}
