using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace KtaneManualDownloader
{
    public static class PathChecks
    {
        public static bool IsFolderKtane(string modFolder)
        {
            try
            {
                modFolder = MakeValidPath(modFolder);
                if (!IsValidPath(modFolder)) return false;
                var modDir = new DirectoryInfo(modFolder);
                if (modDir.Parent == null) return false;
                // If using local mods, this check should succeed (<KTANEDIR>/mods)
                if (modDir.Parent.GetFiles("ktane.exe").Length > 0) return true;
                // If using steam mods, this check should succeed (<workshop/content/ktane>)
                if (modDir.Parent.Parent != null)
                    if (modDir.Parent.Parent.Parent != null)
                    {
                        var steamApps = modDir.Parent.Parent.Parent;
                        var commonDir = steamApps.GetDirectories("common").FirstOrDefault();
                        if (commonDir != null)
                        {
                            var gameDir = commonDir.GetDirectories("Keep Talking and Nobody Explodes")[0];
                            if (gameDir != null)
                                if (gameDir.GetFiles("ktane.exe").Length > 0)
                                    return true;
                        }
                    }
            }
            catch (Exception e)
            {
                MessageBox.Show("This folder path is broken somehow. " +
                                "Please fix or report to the developer.\n" + e.Message);
            }

            return false;
        }

        public static string MakeValidPath(string filename)
        {
            return string.Concat(filename.Split(Path.GetInvalidPathChars()));
        }

        private static bool IsValidPath(string path)
        {
            var driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            if (!driveCheck.IsMatch(path.Substring(0, 3))) return false;
            var strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
            strTheseAreInvalidFileNameChars += @":/?*" + "\"";
            var containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
            if (containsABadCharacter.IsMatch(path.Substring(3, path.Length - 3)))
                return false;

            return true;
        }
    }
}