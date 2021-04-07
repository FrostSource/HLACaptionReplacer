using HLACaptionCompiler.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HLACaptionCompiler
{
    public class Program
    {
        private static CompilerSettings Settings { get; set; } = new();
        private static bool WriteSettingsFile { get; set; } = false;
        private static List<FileInfo> CaptionSourceFiles { get; set; } = new();
        private static List<string> AddonDirectories { get; set; } = new();
        public const string CaptionSourceFileFormat = "closecaption_{0}{1}.txt";
        public const string CaptionCompiledFileFormat = "closecaption_{0}.dat";
        public const string CaptionSourceFilePattern = "closecaption_*.txt";
        public const string CaptionCompiledFilePattern = "closecaption_{0}{1}*.dat";
        public static DirectoryInfo WorkingDirectory { get; private set; } = new DirectoryInfo(Directory.GetCurrentDirectory());
        public static string SettingsPath { get; set; } = Path.Combine(WorkingDirectory.FullName, "CompilerSettings.json");
        public static readonly string[] AvailableLanguages = { "Brazilian", "Bulgarian", "Czech", "Danish", "Dutch", "English", "Finnish", "French", "German", "Greek", "Hungarian", "Italian", "Japanese", "Koreana", "Latam", "Norwegian", "Polish", "Portuguese", "Romanian", "Russian", "Schinese", "Spanish", "Swedish", "Tchinese", "Thai", "Turkish", "Ukranian", "Vietnamese" };

        public static void Main(string[] args)
        {
            ParseArgs(args);

            if (WriteSettingsFile)
            {
                WriteSettings();
            }
            else if (File.Exists(SettingsPath))
            {
                ReadSettings();
            }
            
            if (CaptionSourceFiles.Count == 0 && AddonDirectories.Count == 0)
            {
                WriteLineVerbose("Compiling from current addon folder or chosen addon.\n");
                CompileWorkingOrChosenDirectory();
                return;
            }
            
            foreach (var file in CaptionSourceFiles)
            {
                if (file.Directory == null)
                {
                    WriteLineVerbose($"{file.FullName} had a null directory.");
                    continue;
                }
                CompileFile(file, file.Directory);
            }

            foreach (var addon in AddonDirectories)
            {
                CompileAddonCaptions(addon);
            }

            if (Settings.PauseOnCompletion)
            {
                Console.WriteLine("\nPress any key to exit the process...");
                Console.ReadKey();
            }
        }

        public static void CompileWorkingOrChosenDirectory()
        {
            var hlaMainAddonFolder = Steam.SteamData.GetHLAMainAddOnFolder();
            // Check if inside addon folder
            string addon = GetParentAddonName(WorkingDirectory.FullName);
            // Not inside an addon
            if (addon == "")
            {
                WriteLineVerbose("Not inside addon folder. Using list selection.");

                addon = GetUserAddonChoice();
                if (addon == "") return;
            }
            else
            {
                WriteLineVerbose($"Compiling captions for addon \"{addon}\".");
            }

            CompileAddonCaptions(addon);
        }

        private static void ParseArgs(string[] args)
        {
            //TODO: -a --addon followed by addon folder
            foreach (var arg in args)
            {
                if (arg.ToLower() == "help")
                {
                    PrintHelp();
                    continue;
                }
                // Fully qualified options
                if (arg.StartsWith("--"))
                {
                    var option = arg[2..];
                    if (option != string.Empty)
                    {
                        SetOption(option.ToLower());
                    }
                    continue;
                }
                // Single option(s)
                if (arg.StartsWith("-"))
                {
                    var options = arg[1..];
                    foreach (var option in options)
                    {
                        SetOption(option.ToString());
                    }
                    continue;
                }
                // Presumed files
                if (File.Exists(arg))
                {
                    try
                    {
                        CaptionSourceFiles.Add(new FileInfo(arg));
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine($"Not a valid filename: {arg}");
                    }
                    catch (PathTooLongException)
                    {
                        Console.WriteLine($"Path too long: {arg}");
                    }
                    catch (SystemException)
                    {
                        Console.WriteLine($"Program does not have access to file: {arg}");
                    }
                    continue;
                }
                // Presumed addons
                if (Directory.Exists(arg))
                {
                    if (IsAddonFolder(arg))
                    {
                        var addon = new DirectoryInfo(arg).Name;
                        AddonDirectories.Add(addon);
                    }
                }

                Console.WriteLine($"Unknown file or option: {arg}");
            }
        }
        public static void SetOption(string option)
        {
            switch (option)
            {
                case "help":
                case "h":
                    PrintHelp();
                    break;
                case "pause":
                case "p":
                    Settings.PauseOnCompletion = true;
                    break;
                case "verbose":
                case "v":
                    Settings.Verbose = true;
                    break;
                case "settings":
                case "S":
                    WriteSettingsFile = true;
                    break;
                case "strict":
                case "s":
                    Settings.Strict = true;
                    break;
                case "nodirective":
                case "D":
                    Settings.AllowDirectives = false;
                    break;

                default:
                    Console.WriteLine($"Unknown option '{option}'");
                    break;
            }
        }
        public static void SetOptions(string[] options)
        {
            foreach (var option in options)
            {
                SetOption(option);
            }
        }
        private static string GetUserAddonChoice(string message = "Please choose an addon to compile captions for, or leave blank to exit:\n")
        {
            var addons = Steam.SteamData.GetAddOnList();

            Console.WriteLine(message);
            for (int i = 0; i < addons.Length; i++)
            {
                Console.WriteLine($"{i+1}. {addons[i]}");
            }
            Console.Write("> ");
            var input = Console.ReadLine();

            int selection = 1;
            while (input != "" && !int.TryParse(input, out selection) && (selection < 1 || selection > addons.Length))
            {
                Console.WriteLine(" is not a valid selection.");
                Console.Write("> ");
                input = Console.ReadLine();
            }
            Console.WriteLine();

            if (input == "") return "";
            return addons[selection-1];
            
        }
        /// <summary>
        /// Compiles a single file to a given directory.
        /// </summary>
        /// <param name="file">The caption source file to compile.</param>
        /// <param name="dir">The directory to write the compiled file.</param>
        /// <returns>True if the file compiles successfully or false otherwise.</returns>
        public static bool CompileFile(FileInfo file, DirectoryInfo dir)
        {
            WriteVerbose($"Compiling {file.Name} ... ");
            var parser = new ClosedCaptionsFileParser(file, Settings.Strict);
            if (parser.TryParse())
            {
                var captions = new ClosedCaptions();
                foreach (KeyValuePair<string,string> token in parser.CaptionTokens)
                {
                    captions.Add(token.Key, token.Value);
                }
                captions.Write(Path.Combine(dir.FullName, Path.ChangeExtension(file.Name, ".dat")));
                WriteLineVerbose("DONE.");
                return true;
            }
            Console.WriteLine($"{file.Name} failed to compile...");
            return false;
        }
        /// <summary>
        /// Adds all captions from a given <paramref name="file"/> into a given <paramref name="languageDictionary"/>.
        /// If the language doesn't exist in the dictionary it adds a new <see cref="ClosedCaptions"/> object.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="captions"></param>
        /// <returns>Language name if successful, blank otherwise.</returns>
        public static string ParseFileIntoCaptions(FileInfo file, Dictionary<string, ClosedCaptions> languageDictionary)
        {
            WriteVerbose($"Examining {file.Name} ... ");
            var parser = new ClosedCaptionsFileParser(file, Settings.Strict);
            if (parser.TryParse())
            {
                ClosedCaptions captions;
                if (languageDictionary.ContainsKey(parser.CaptionLanguage))
                {
                    captions = languageDictionary[parser.CaptionLanguage];
                }
                else
                {
                    captions = new ClosedCaptions();
                    languageDictionary.Add(parser.CaptionLanguage, captions);
                }

                foreach (KeyValuePair<string, string> token in parser.CaptionTokens)
                {
                    if (captions.HasCaption(token.Key))
                    {
                        if (Settings.Verbose) WriteVerbose($"duplicate {token.Key} ... ");
                        continue;
                    }
                    captions.Add(token.Key, token.Value);
                }
                WriteLineVerbose("DONE.");
                return parser.CaptionLanguage.ToLower();
            }
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="dir"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static int CompileFilesToLanguage(List<FileInfo> files, DirectoryInfo dir, string language)
        {
            var captions = new ClosedCaptions();
            var numberOfGoodFiles = 0;
            foreach (var file in files)
            {
                WriteVerbose($"Examining {file.Name} ... ");
                var parser = new ClosedCaptionsFileParser(file, Settings.Strict);
                if (parser.TryParse())
                {
                    foreach (KeyValuePair<string, string> token in parser.CaptionTokens)
                    {
                        if (!captions.HasCaption(token.Key))
                           captions.Add(token.Key, token.Value);
                    }
                    WriteLineVerbose("DONE.");
                    numberOfGoodFiles++;
                }
            }
            if (numberOfGoodFiles == 0)
            {
                Console.WriteLine($"No valid files for language \"{language}\"\n");
                return 0;
            }

            var outputFileName = string.Format(CaptionCompiledFileFormat, language);
            WriteVerbose($"Compiling {numberOfGoodFiles}/{files.Count} files to \"{outputFileName}\" ...");
            dir.Create();
            captions.Write(Path.Combine(dir.FullName, outputFileName));
            WriteLineVerbose("DONE\n");
            return numberOfGoodFiles;
        }
        /// <summary>
        /// Compiles a folder of source caption files to a given folder.
        /// </summary>
        /// <param name="from">The directory that contains the source files.</param>
        /// <param name="to">The directory to compile to.</param>
        /// <returns>Tuple with number of files compiled and total found.</returns>
        public static (int filesCompiled, int filesTotal) CompileFolder(DirectoryInfo from, DirectoryInfo to)
        {
            if (!from.Exists)
            {
                Console.WriteLine($"\"{Steam.SteamData.CaptionFolder}\" does not not exist in addon \"{GetParentAddonName(from.FullName)}\".");
                return (0, 0);
            }
            int numberOfGoodFiles = 0;
            var files = from.GetFiles(CaptionSourceFilePattern, SearchOption.AllDirectories);
            var languageDictionary = new Dictionary<string, ClosedCaptions>();
            var languageFileSuccesses = new Dictionary<string, int>();

            // Back out if no files found
            if (files.Length <= 0)
            {
                Console.WriteLine($"No caption content files found in addon \"{GetParentAddonName(from.FullName)}\".");
                return (0, 0);
            }

            // Parse each file
            foreach (var file in files)
            {

                var language = ParseFileIntoCaptions(file, languageDictionary);
                if (language != "")
                {
                    numberOfGoodFiles++;
                    if (languageFileSuccesses.ContainsKey(language))
                        languageFileSuccesses[language]++;
                    else
                        languageFileSuccesses.Add(language, 1);
                }
            }

            // Compile/write each caption language
            foreach (var language in languageDictionary)
            {
                var outputFileName = string.Format(CaptionCompiledFileFormat, language.Key);
                WriteVerbose($"Compiling {languageFileSuccesses[language.Key]} files to \"{outputFileName}\" ...");
                to.Create();
                language.Value.Write(Path.Combine(to.FullName, outputFileName));
                WriteLineVerbose("DONE\n");
            }

            return (numberOfGoodFiles, files.Length);
        }
        public static void CompileAddonCaptions(string addon)
        {
            WriteLineVerbose($"Compiling captions for addon \"{addon}\"");
            var contentPath = Steam.SteamData.GetHLAAddOnFolder(addon);
            var gamePath = Steam.SteamData.GetAddOnGameFolder(addon);
            var contentCaptionDirectory = new DirectoryInfo(Path.Combine(contentPath, Steam.SteamData.CaptionFolder));
            var gameCaptionDirectory = new DirectoryInfo(Path.Combine(gamePath, Steam.SteamData.CaptionFolder));
            var (filesCompiled, filesTotal) = CompileFolder(contentCaptionDirectory, gameCaptionDirectory);
            //if (filesCompiled > 0)
            Console.WriteLine($"\n{filesCompiled}/{filesTotal} files compiled to \"{gameCaptionDirectory.FullName}\"");
            //else
            //    Console.WriteLine($"Failed to write captions.");
        }
        public static void WriteSettings()
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                var json = System.Text.Json.JsonSerializer.Serialize(Settings, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (SystemException e)
            {
                WriteLineVerbose($"[Error writing settings file: {e}");
            }
        }
        public static void ReadSettings()
        {
            try
            {
                var json = File.ReadAllText(SettingsPath);
                CompilerSettings? settings;
                if ((settings = System.Text.Json.JsonSerializer.Deserialize<CompilerSettings>(json)) != null)
                    Settings = settings;
            }
            catch (SystemException e)
            {
                WriteLineVerbose($"[Error reading settings file: {e}");
            }
            catch (System.Text.Json.JsonException e)
            {
                WriteLineVerbose($"[Error reading settings file: {e}");
            }
        }
        public static string GetParentAddonName(string folderPath)
        {
            if (!IsAddonFolder(folderPath)) return "";
            
            var folders = folderPath.Split(Path.DirectorySeparatorChar);
            for (int i = folders.Length - 1; i >= 0; i--)
            {
                if (folders[i] == "hlvr_addons")
                {
                    return folders[i + 1];
                }
            }

            return "";
            
        }
        public static bool IsAddonFolder(string folderPath)
        {
            var addonFolder = Steam.SteamData.GetHLAMainAddOnFolder();
            if (!folderPath.Contains(addonFolder) || addonFolder == folderPath)
                return false;

            return true;
        }
        private static void WriteLineVerbose(string message)
        {
            if (Settings.Verbose) Console.WriteLine(message);
        }
        private static void WriteVerbose(string message)
        {
            if (Settings.Verbose) Console.Write(message);
        }

        public static void PrintHelp()
        {
            var help = "https://github.com/FrostSource/HLACaptionReplacer for detailed help.\n" +
                "\n" +
                "Flags:\n" +
                "-S/--settings\t\tCreate a settings file in the current directory.\n" +
                "-v/--verbose\t\tVerbose console messages.\n" +
                "-p/--pause\t\tWait for input after finishing.\n" +
                "-s/--strict\t\tCompile with strict syntax checking.\n" +
                "-D/--nodirective\t\tDisables use of compiler directives.";
            Console.WriteLine(help);
        }

        private class CompilerSettings
        {
            public bool PauseOnCompletion { get; set; } = false;
            public bool Verbose { get; set; } = false;
            public bool Strict { get; set; } = false;
            public bool AllowDirectives { get; set; } = true;
        }
    }


    
}
