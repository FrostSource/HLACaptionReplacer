using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tests
{
    class Tests
    {
        static void Main(string[] args)
        {

            // Why did I do it like this?
            var exePath = Path.GetFullPath(@"../../../../HLACaptionReplacer/bin/Debug/net5.0/HLACaptionReplacer.exe");
            var captionPath = Path.GetFullPath(@"../../../Files/closecaption_english.dat");
            var captionPathReplica = Path.GetFullPath(@"../../../Files/closecaption_replica.dat");
            var captionPathNew = Path.Combine(Path.GetDirectoryName(captionPath), $"{Path.GetFileNameWithoutExtension(captionPath)}_new{Path.GetExtension(captionPath)}");
            var customCaptionPath = Path.GetFullPath(@"../../../Files/closecaption_custom.dat");
            //var captionPathNew = string.Format("{0}{1}_new{2}", Path.GetDirectoryName(captionPath),
            //                                                    Path.GetFileNameWithoutExtension(captionPath),
            //                                                    Path.GetExtension(captionPath));
            var modifierPath = Path.GetFullPath(@"../../../Files/closecaption_own_modifier.txt");

            // should be 45 deletions
            //var proc = System.Diagnostics.Process.Start(exePath, $"-e {captionPath} {modifierPath}");
            //var proc = System.Diagnostics.Process.Start(exePath, $"{captionPath}");
            Console.OutputEncoding = Encoding.UTF8;
            //TestClosedCaptionsFileTokenizer();
            //TestClosedCaptionFileParser();
            //TestHLACaptionCompiler();
            TestCaptionTextTokenizer();

            //QuickAdesiJsonToSource();
            //EnglishCatagoriesToOtherLanguages();

        }

        private static void TestCaptionTextTokenizer()
        {
            var text = "<clr:25.8,255,255>A pachinko machine, in <clr:14,104,89>Vinny's<clr:255,255,255> very own basement!<sb>Who could have put this here?";
            var tokenizer = new HLACaptionCompiler.Parsing.CaptionTextTokenizer(text);
            tokenizer.Tokenize();
            foreach (var token in tokenizer.Tokens)
            {
                Console.WriteLine("{0,-12}{1,0}", Enum.GetName(typeof(HLACaptionCompiler.Parsing.TokenType), token.TokenType), token.Value);
            }
        }
        private static void TestHLACaptionCompiler()
        {
            //var path = Console.ReadLine().Trim('"');
            var path = Path.GetFullPath(@"../../../Files/closecaption_testparser.txt");
            if (!File.Exists(path))
            {
                Console.WriteLine("Doesn't exist " + path);
                return;
            }

            var exePath = Path.GetFullPath(@"../../../../HLACaptionCompiler/bin/Debug/net5.0/HLACaptionCompiler.exe");
            //var proc = System.Diagnostics.Process.Start(exePath, $"-v {path}");
            HLACaptionCompiler.Program.Main(new string[] { "-vs", path });
        }

        private static void TestClosedCaptionsFileTokenizer()
        {
            //var path = Console.ReadLine().Trim('"');
            var path = @"C:\Program Files (x86)\SteamLibrary\steamapps\common\Half-Life Alyx\content\hlvr_addons\vinny_christmas\resource\subtitles\english\closecaption_english-vinny.txt";
            var tokenizer = new HLACaptionCompiler.Parsing.ClosedCaptionsFileTokenizer(File.ReadAllText(path));
            tokenizer.Tokenize();
            foreach (var token in tokenizer.Tokens)
            {
                Console.WriteLine("{0,-12}{1,0}", Enum.GetName(typeof(HLACaptionCompiler.Parsing.TokenType), token.TokenType), token.Value);
            }
        }
        public static void TestClosedCaptionFileParser()
        {
            /*var str1 = "lang\n{\n                Language \"English\" //(or \"French\", etc)\n\n    Tokens\n\n    {\n                    // Captions defined here.\n                    nameofsound \"This is the caption.\"\n\n\n        barn.chatter    \"We're picking up radio chatter. They're looking for your car.\"\n        // etc...\n                }\n            }";
            var str2 = "\"lang\"\n" +
                "{ \n" +
                "\"Language\" \"English\" \n" +
                "\"Tokens\" \n" +
                "{\n" +
                "	\"levi.health01\"		\"Take this medkit.\"\n" +
                "	\"levi.health02\"		\"Here, have a medkit.\"\n" +
                "	\"levi.health03\"     \"Here, path yourself up.\"\n" +
                "    \"levi.health04\"     \"Heal up.\"\n" +
                "    \"levi.health05\"     \"Have a medkit.\"\n" +
                "    \"caption1\"      \"caption 1\"\n" +
                "    \"caption2\"      \"caption 2\"\n" +
                "    \"nothingtodo\"		\"Just never be seen.\"\n" +
                "    \"captain.captain_01\"    \"<clr:222,67,0>Hello! My name is Captain Dickhead and I want to tell you about custom captions.\"\n" +
                "    \"captain.captain_02\"    \"<clr:222,67,0>You can have cool colors and other markup I guess.\"\n" +
                "    \"padding1\"      \"padding 1\"\n" +
                "    \"padding2\"      \"padding 2\"\n" +
                "\n" +
                "}\n" +
                "}\n";
            var strUsed = str1;*/
            var path = @"C:\Program Files (x86)\SteamLibrary\steamapps\common\Half-Life Alyx\content\hlvr_addons\vinny_christmas\resource\subtitles\english\closecaption_english-vinny.txt";
            var parser = new HLACaptionCompiler.Parsing.ClosedCaptionsFileParser(File.ReadAllText(path));

            parser.Parse();

            Console.WriteLine(parser.CaptionLanguage);
            foreach (KeyValuePair<string, string> token in parser.CaptionTokens)
            {
                Console.WriteLine($"{token.Key}\t{token.Value}\n");
            }
        }

        private static void EnglishCatagoriesToOtherLanguages()
        {
            var path = Console.ReadLine().Trim('"');
            if (!File.Exists(path))
            {
                Console.WriteLine("Doesn't exist");
                return;
            }
            var json = File.ReadAllText(path);
            var captions = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<Dictionary<string, dynamic>>>>>(json);
            //var lines = captions.Select(kvp => kvp.Key + ": " + kvp.Value.ToString());
            //Console.WriteLine(string.Join(Environment.NewLine, lines));


            // string = catagory, string = sound event token
            var englishCatagories = new Dictionary<string, List<string>>();
            {
                var captionDictionary = captions["english"];
                var catagoryName = "";

                foreach (var caption in captionDictionary["GetCaptions"])
                {
                    var captionToken = ((System.Text.Json.JsonElement)caption["GetRealName"]).ToString();
                    var captionText = ((System.Text.Json.JsonElement)caption["GetText"]).ToString();
                    var newCatagory = GetCatagory(captionToken);
                    if (newCatagory != "")
                    {
                        catagoryName = newCatagory;
                        if (!englishCatagories.ContainsKey(catagoryName))
                        {
                            englishCatagories.Add(catagoryName, new List<string>());
                        }
                    }
                    englishCatagories[catagoryName].Add(captionToken);
                }
                //captions.Remove("english");
            }


            var languageStrings = new Dictionary<string, Dictionary<string, StringBuilder>>();
            foreach (var language in captions)
            {
                var dict = new Dictionary<string, StringBuilder>();
                var catagoryName = "";
                languageStrings.Add(language.Key, dict);

                foreach (var caption in language.Value["GetCaptions"])
                {
                    var name = ((System.Text.Json.JsonElement)caption["GetRealName"]).ToString();
                    var text = ((System.Text.Json.JsonElement)caption["GetText"]).ToString();
                    
                    var newCatagory = "";
                    if (language.Key == "english")
                        newCatagory = GetCatagory(name);
                    else
                        newCatagory = GetCatagoryFromCatagoryDictionary(englishCatagories, name);

                    if (newCatagory != "")
                    {
                        catagoryName = newCatagory;
                        if (!languageStrings[language.Key].ContainsKey(catagoryName))
                        {
                            languageStrings[language.Key].Add(catagoryName, new StringBuilder($"\"lang\"\n{{\n\t\"Language\"\t\"{language.Key}\"\n\t\"Tokens\"\n\t{{\n\t\t// {catagoryName}\n"));
                        }
                    }

                    languageStrings[language.Key][catagoryName].Append($"\t\t\"{name}\"\t\"{ConvertEscapedToUnescaped(text)}\"\n");
                }
            }

            WriteLanguageCatagories(languageStrings, path);


            // private helper method
            string GetCatagoryFromCatagoryDictionary(Dictionary<string, List<string>> catagories, string name)
            {
                foreach (var catagory in catagories)
                {
                    if (catagory.Value.Contains(name))
                    {
                        return catagory.Key;
                    }
                }
                return "";
            }


        }

        private static string GetCatagory(string captionToken)
        {
            const string generic = "Generic";
            const string alyx_generic = "Alyx Generic";
            const string alyx_choreo = "Alyx Choreo";
            const string eli_choreo = "Eli Choreo";
            const string gman_choreo = "Gman Choreo";
            const string russel_choreo = "Russel Choreo";
            const string gary_choreo = "Gary Choreo";
            const string overwatch = "Overwatch";
            const string larry_choreo = "Larry Choreo";
            const string olga_choreo = "Olga Choreo";
            const string drone_choreo = "Drone Choreo";
            const string combine_choreo = "Combine Choreo";
            const string combine_grunt = "Combine Grunt";
            const string combine_officer = "Combine Officer";
            const string combine_suppressor = "Combine Suppressor";
            const string combine_charger = "Combine Charger";
            const string contractor_choreo = "Contractor Choreo";
            const string sfx = "SFX";
            const string commentary = "Commentary";

            return captionToken switch
            {
                "3952443579" => generic,

                "vo.01_01104" => alyx_choreo,
                "vo.01_20000" => alyx_generic,
                "vo.01_20001" => alyx_choreo,
                "vo.01_20011" => alyx_generic,
                "vo.01_20031" => alyx_choreo,
                "vo.01_12754" => alyx_generic,
                "vo.01_12766" => alyx_choreo,
                "vo.01_20179" => alyx_generic,
                "vo.01_20271" => alyx_choreo,
                "vo.01_13146" => alyx_generic,
                "vo.01_13200" => alyx_choreo,
                "vo.01_60021" => alyx_generic,
                "vo.01_60030" => alyx_choreo,
                "vo.01_70649" => alyx_generic,
                "vo.01_70859" => alyx_choreo,
                "vo.01_71000" => alyx_generic,
                "vo.01_71021" => alyx_choreo,
                "vo.01_71055" => alyx_generic,
                "vo.01_71075" => alyx_choreo,
                "vo.01_90007" => alyx_generic,
                "vo.01_99958" => alyx_choreo,
                "vo.01_00001" => alyx_generic,

                "vo.02_01103" => eli_choreo,
                "vo.04_00200" => gman_choreo,
                "vo.05_00003" => russel_choreo,
                "vo.06_00001" => gary_choreo,
                "vo.07_10000" => overwatch,
                "vo.12_00001" => larry_choreo,
                "vo.13_00101" => olga_choreo,
                "vo.25_1000" => drone_choreo,
                "vo.27_1107" => combine_choreo,
                "vo.31_00002" => contractor_choreo,
                "vo.combine.charger.advancing_on_target_01" => combine_charger,
                "341443817" => combine_choreo,
                "vo.combine.grunt.advancing_on_target_01" => combine_grunt,
                "vo.combine.officer.advancing_on_target_01" => combine_officer,
                "vo.combine.suppressor.advancing_on_target_01" => combine_suppressor,
                "combine.radiooff" => sfx,
                "commentary.a1_01_intro1" => commentary,

                _ => ""
            };
        }

        static string ConvertEscapedToUnescaped(string str)
        {
            var sb = new StringBuilder(str);

            sb.Replace("\n", "\\n");
            sb.Replace("\t", "\\t");
            sb.Replace("\"", "\\\"");
            sb.Replace("\u0001", "\\u0001");
            sb.Replace("\u0002", "\\u0002");
            sb.Replace("\u0003", "\\u0003");
            sb.Replace("\u0005", "\\u0005");

            return sb.ToString();
        }

        private static void WriteLanguageCatagories(Dictionary<string, Dictionary<string, StringBuilder>>  languageStrings, string path)
        {
            foreach (var language in languageStrings)
            {
                foreach (var catagory in language.Value)
                {
                    var source = catagory.Value;
                    source.Append("\t}\n}");
                    var newPath = Path.Combine(Path.GetDirectoryName(path), "languages", language.Key.ToLower(), $"closecaption_{language.Key.ToLower()}_{catagory.Key.ToLower().Replace(" ", "_")}.txt");
                    FileInfo file = new FileInfo(newPath);
                    file.Directory.Create();
                    File.WriteAllText(file.FullName, source.ToString());
                }
            }
        }

        public static void QuickAdesiJsonToSource()
        {
            var path = Console.ReadLine().Trim('"');
            if (!File.Exists(path))
            {
                Console.WriteLine("Doesn't exist");
                return;
            }
            var json = File.ReadAllText(path);
            var captions = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<Dictionary<string, dynamic>>>>>(json);
            var lines = captions.Select(kvp => kvp.Key + ": " + kvp.Value.ToString());
            //Console.WriteLine(string.Join(Environment.NewLine, lines));

            var languageStrings = new Dictionary<string, Dictionary<string, StringBuilder>>();

            foreach (var language in captions)
            {
                var dict = new Dictionary<string, StringBuilder>();
                var catagoryName = "UI";
                var source = new StringBuilder($"\"lang\"\n{{\n\t\"Language\"\t\"{language.Key}\"\n\t\"Tokens\"\n\t{{\n\t\t// {catagoryName}\n");
                dict.Add(catagoryName, source);
                languageStrings.Add(language.Key, dict);

                foreach (var caption in language.Value["GetCaptions"])
                {
                    var name = ((System.Text.Json.JsonElement)caption["GetRealName"]).ToString();
                    var text = ((System.Text.Json.JsonElement)caption["GetText"]).ToString();
                    var newCatagory = GetCatagory(name);
                    if (newCatagory != "")
                    {
                        catagoryName = newCatagory;
                        if (!languageStrings[language.Key].ContainsKey(catagoryName))
                        {
                            languageStrings[language.Key].Add(catagoryName, new StringBuilder($"\"lang\"\n{{\n\t\"Language\"\t\"{language.Key}\"\n\t\"Tokens\"\n\t{{\n\t\t// {catagoryName}\n"));
                        }
                    }

                    languageStrings[language.Key][catagoryName].Append($"\t\t\"{name}\"\t\"{ConvertEscapedToUnescaped(text)}\"\n");
                }
                //source.Append("\t}\n}");
                //File.WriteAllText(Path.Combine(Path.GetDirectoryName(path), $"closecaption_{language.Key}.txt"), source.ToString());
            }

            WriteLanguageCatagories(languageStrings, path);
        }

        
    }
}
