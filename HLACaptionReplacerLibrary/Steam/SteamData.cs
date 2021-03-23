using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionReplacer.Steam
{
    public static class SteamData
    {
        public const string SoundEventsExtension = "vsndevts";
        public const string AlyxWorkshopListURL = "https://steamcommunity.com/app/546560/workshop/";
        const string SteamRegistryKey = @"SOFTWARE\Valve\Steam";
        const string SteamRegistrySteamPathValue = "SteamPath";
        const string SteamAppsRelativeFolder = "steamApps";
        const string SteamLibraryFolderList = "libraryFolders.vdf";
        const string SteamAddOnListURL = "https://steamcommunity.com/workshop/browse/?appid=546560&browsesort=trend&section=readytouseitems&days=90&numperpage=30&p={0}";
        //<span class="pagebtn disabled">&lt;</span>&nbsp;1&nbsp;&nbsp;<a class="pagelink" href="https://steamcommunity.com/workshop/browse/?appid=546560&browsesort=trend&section=readytouseitems&days=90&numperpage=30&actualsort=trend&p=2">2</a>&nbsp;&nbsp;<a class="pagelink" href="https://steamcommunity.com/workshop/browse/?appid=546560&browsesort=trend&section=readytouseitems&days=90&numperpage=30&actualsort=trend&p=3">3</a>&nbsp;...&nbsp;<a class="pagelink" href="https://steamcommunity.com/workshop/browse/?appid=546560&browsesort=trend&section=readytouseitems&days=90&numperpage=30&actualsort=trend&p=26">26</a>&nbsp;<a class='pagebtn' href="https://steamcommunity.com/workshop/browse/?appid=546560&browsesort=trend&section=readytouseitems&days=90&numperpage=30&actualsort=trend&p=2">&gt;</a>					</div>
        const string HLAWorkshopPath = @"steamapps\workshop\" + Content;
        const string HLAInstallFolder = @"steamapps\common\Half-Life Alyx";
        const string Content = "content";
        const string Game = "game";
        const string hlvr = "hlvr";
        const string HLAExecutable = HLAInstallFolder + @"\"+ Game + @"\bin\win64\hlvr.exe";
        const string HLAWIPAddonPath = Content + @"\hlvr_addons";
        public const string HLAWIPAddonGamePath = Game + @"\hlvr_addons";
        public const string CaptionFolder = @"resource\subtitles";
        public const string CaptionFormat = "closecaption_{0}.dat";
        public const string SoundFileExtension = "wav";
        public const string SoundEventsFolder = "soundevents";
        public const string SoundEventFileStart = "<!-- kv3 encoding:text:version{e21c7f3c-8a33-41c5-9977-a76d3a32aa0d} format:generic:version{7412167c-06e9-4698-aff2-e63eb59037e7} -->";
        const string HLAKey = "546560";
        const string WorkshopInfoFile = "publish_data.txt";
        public static string[] GetSupportedLanguages()
        {
            List<string> retVal = new List<string>();
            string captionFolder = GetGameCaptionFolder();
            const string prefix = "closecaption_";
            foreach (var captionFile in new DirectoryInfo(captionFolder).GetFiles(prefix + "*.dat"))
            {
                retVal.Add(captionFile.Name.Substring(prefix.Length, captionFile.Name.Length - 4 - prefix.Length));
            }
            return retVal.ToArray();
        }
        public static string GetGameCaptionFolder()
        {
            return System.IO.Path.Combine(GetHLAInstallFolder(), Game, hlvr, CaptionFolder);
        }
        public static string GetAddOnCaptionFolder(string addOn)
        {
            return System.IO.Path.Combine(GetAddOnGameFolder(addOn), CaptionFolder);
        }
        public static string GetAddOnGameFolder(string addOn)
        {
            return Path.Combine(GetHLAInstallFolder(), HLAWIPAddonGamePath, addOn);
        }
        public static string[] GetAddOnList()
        {

            var AddOns = new List<string>();
            foreach (var folder in new System.IO.DirectoryInfo(SteamData.GetHLAMainAddOnFolder()).GetDirectories())
            {
                AddOns.Add(folder.Name);
            }
            return AddOns.ToArray();
        }
        public static string GetHLAAddOnFolder(string addOn)
        {
            return Path.Combine(GetHLAMainAddOnFolder(), addOn);
        }
        public static string GetHLAMainAddOnFolder()
        {
            string retVal = Path.Combine(GetHLAInstallFolder(), HLAWIPAddonPath);
            return retVal;
        }
        static string HLAInstallation = null;
        public static string GetHLAInstallFolder()
        {
            string retVal = null;
            if (string.IsNullOrEmpty(HLAInstallation))
            {
                foreach (var folder in GetSteamLibraryFolders())
                {
                    if (File.Exists(Path.Combine(folder, HLAExecutable)))
                    {
                        HLAInstallation = Path.Combine(folder, HLAInstallFolder);
                        break;
                    }
                }
                
            }
            retVal = HLAInstallation;
            return retVal;
        }

        public static string GetSteamInstallFolder()
        {
            LastErrorMessage = "";
            string retVal;

            try
            {
                var reg = Registry.CurrentUser.OpenSubKey(SteamRegistryKey);
                if (reg == null)
                {
                    retVal = null;
                }
                else
                {
                    var steamPath = reg.GetValue(SteamRegistrySteamPathValue);
                    retVal = steamPath as string;
                }
            }
            catch (Exception ex)
            {
                retVal = null;
                LastErrorMessage = ex.Message;
            }

            return retVal;
        }
        public static string LastErrorMessage { get; private set; }
        public static string[] GetSteamLibraryFolders()
        {
            List<string> retVal = new List<string>();


            var steamFolder = GetSteamInstallFolder();
            if (Directory.Exists(steamFolder))
            {
                retVal.Add(steamFolder);
                string libraryListFileName = Path.Combine(steamFolder, SteamAppsRelativeFolder, SteamLibraryFolderList);

                if (File.Exists(libraryListFileName))
                {
                    var info = new SteamInfo(libraryListFileName);

                    foreach (var key in info.GetKeys())
                    {
                        int libraryNumber = 0;
                        if (int.TryParse(key, out libraryNumber))
                        {
                            retVal.Add(info.GetValue(key));
                        }
                    }
                }
            }
            return retVal.ToArray();
        }
        public static string GetWorkshopWebpageURL(string key)
        {
            return string.Format(@"https://steamcommunity.com/sharedfiles/filedetails/?id={0}&result=1", key);
        }
        static string HLAWorkshopFolder = null;
        public static string GetHLAWorkshopFolder()
        {

            if (string.IsNullOrEmpty(HLAWorkshopFolder) || !Directory.Exists(HLAWorkshopFolder))
            {
                var folderList = GetSteamLibraryFolders();

                foreach (var path in folderList)
                {
                    string testFolder = Path.Combine(path, HLAWorkshopPath, HLAKey);
                    if (Directory.Exists(testFolder))
                    {
                        HLAWorkshopFolder = testFolder;
                        break;
                    }
                }
            }

            return HLAWorkshopFolder;
        }

    }
}
