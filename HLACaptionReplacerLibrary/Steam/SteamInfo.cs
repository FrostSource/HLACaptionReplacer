using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionReplacer.Steam
{
    public class SteamInfo
    {
        public SteamInfo(string file)
        {
            Filename = file;
            using (var strm = new StreamReader(file))
            {
                rawData = strm.ReadToEnd();
            }
            FixRawData();

            ProcessRawData();
        }
        public SteamInfo()
        {

        }
        public void LoadData(string data)
        {
            rawData = data;
            FixRawData();
            ProcessRawData();
        }

        void FixRawData()
        {
            rawData = rawData.Replace("\r", string.Empty).Replace("\t"," ");
            StringBuilder sb = new StringBuilder();
            foreach (var line in rawData.Split('\n'))
            {
                sb.Append(line);
                if (line.EndsWith("\"") || line == "{" || line == "}")
                {
                    sb.Append('\n');
                }
                else
                {
                    sb.Append(" ");
                }
            }
            rawData = sb.ToString();
        }

        /*
         
"LibraryFolders"
{
	"TimeNextStatsReport"		"1611876812"
	"ContentStatsID"		"5688185081579020048"
	"1"		"D:\\StLibrary"
	"2"		"B:\\SteamLibrary"
	"3"		"G:\\Steam"
}

LibraryFolders
        Keys:
            1
            2
            3

publish_data
        Keys: 
            title
            publish_time_readable

        */
        void ProcessRawData()
        {
            //First line is the object name
            string[] lines = rawData.Split('\n');
            if (lines.Length > 0)
            {
                ObjectName = lines[0].Substring(1, lines[0].Length - 2);
                if (lines.Length > 2)
                {
                    for (int i = 2; i < lines.Length - 1; i++)
                    {
                        string line = lines[i].Replace("\t"," ").Trim();
                        int start = 1;
                        int end = 1;
                        if (line != "}")
                        {
                            do
                            {

                            } while (line.Substring(++end, 1) != "\"");
                            string key = line.Substring(start, end - start);

                            start = end + 1;
                            do
                            {

                            } while (line.Substring(++start, 1) != "\"");
                            start++;
                            end = start;
                            if (line.Substring(end, 1) != "\"")
                            {
                                do
                                {

                                } while (line.Substring(++end, 1) != "\"");
                            }
                            string value = string.Empty;
                            if (end > start)
                            {
                                value = line.Substring(start, end - start);
                                value = value.Replace(@"\\", @"\");
                            }
                            MainData.Add(key, value);
                        }

                    }
                }
            }

        }
        Dictionary<string, string> MainData = new Dictionary<string, string>();
        string rawData = null;
        public string Filename
        {
            get;
            private set;
        }
        public string GetValue(string key)
        {
            string retVal = null;
            if (MainData.ContainsKey(key))
            {
                retVal = MainData[key];
            }
            return retVal;
        }
        public string[] GetKeys()
        {
            List<string> retVal = new List<string>();
            foreach (var key in MainData.Keys)
            {
                retVal.Add(key);
            }
            return retVal.ToArray();
        }
        public bool HasKey(string key)
        {
            return MainData.ContainsKey(key);
        }
        public string ObjectName { get; private set; }
    }
}
