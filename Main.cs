using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KtaneManualDownloader
{
    class Main
    {

        public static Main Instance;

        #region Path Vars
        public string ModsFolderLocation;
        public string ManualDownloadsFolder = Path.GetFullPath("./Manuals/");
        public string ManualJSONPath
        {
            get
            {
                return ManualDownloadsFolder + "modules.json";
            }
        }
        public string MergedManualOutputPath = "./MergedDocument.pdf";

        public string VanillaDocsPath = Path.GetFullPath("./VanillaDocuments/");
        public string CoverPath
        {
            get
            {
                return VanillaDocsPath + "Cover.pdf";
            }
        }
        public string IntroPath
        {
            get
            {
                return VanillaDocsPath + "Intro.pdf";
            }
        }
        public string ModuleSpacerPath
        {
            get
            {
                return VanillaDocsPath + "ModuleSpacer.pdf";
            }
        }
        public string NeedySpacerPath
        {
            get
            {
                return VanillaDocsPath + "NeedySpacer.pdf";
            }
        }
        public string AppendixPath
        {
            get
            {
                return VanillaDocsPath + "VanillaAppendix.pdf";
            }
        }
        #endregion

        public List<string> CurrentModList = new List<string>();

        public Main()
        {
            Instance = this;
            new Scraper();
        }

        public bool IsFolderKtane(string modFolder)
        {
            DirectoryInfo modDir = null;
            try
            {
                modFolder = MakeValidPath(modFolder);
                if (!IsValidPath(modFolder)) return false;
                modDir = new DirectoryInfo(modFolder);
                if (modDir.Parent == null) return false;
                // If using local mods, this check should succeed (<KTANEDIR>/mods)
                if (modDir.Parent.GetFiles("ktane.exe").Length > 0) return true;
                // If using steam mods, this check should succeed (<workshop/content/ktane>)
                if (modDir.Parent.Parent != null)
                {
                    if (modDir.Parent.Parent.Parent != null)
                    {
                        DirectoryInfo steamApps = modDir.Parent.Parent.Parent;
                        DirectoryInfo commonDir = steamApps.GetDirectories("common").FirstOrDefault();
                        if (commonDir != null)
                        {
                            DirectoryInfo gameDir = commonDir.GetDirectories("Keep Talking and Nobody Explodes")[0];
                            if (gameDir != null)
                            {
                                if (gameDir.GetFiles("ktane.exe").Length > 0) return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("This folder path is broken somehow. " +
                    "Please fix or report to the developer.");
            }
            return false;
        }

        public string MakeValidPath(string filename)
        {
            return string.Concat(filename.Split(Path.GetInvalidPathChars()));
        }

        private bool IsValidPath(string path)
        {
            Regex driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            if (!driveCheck.IsMatch(path.Substring(0, 3))) return false;
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
            strTheseAreInvalidFileNameChars += @":/?*" + "\"";
            Regex containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
            if (containsABadCharacter.IsMatch(path.Substring(3, path.Length - 3)))
                return false;

            return true;
        }

    }
}
