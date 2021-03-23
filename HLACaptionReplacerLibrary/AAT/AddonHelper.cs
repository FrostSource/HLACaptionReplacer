using System;
using System.Collections.Generic;
using System.Text;
//using ValveKeyValue;

namespace AAT
{
   
    public class InvalidFileFormatException : Exception
    {
        internal InvalidFileFormatException() : base() { }
        internal InvalidFileFormatException(string message) : base(message) { }
        internal InvalidFileFormatException(string message, Exception inner) : base(message, inner) { }
        protected InvalidFileFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    public static class AddonHelper
    {
        public static string[] GetAddons(string AlyxInstallFolder)
        {
            string path = System.IO.Path.Combine(AlyxInstallFolder, "content", "hlvr_addons");
            List<string> retVal = new List<string>();

            foreach (var dir in new System.IO.DirectoryInfo(path).GetDirectories())
            {
                retVal.Add(dir.Name);
            }
            return retVal.ToArray();
        }
        public static string GetAddonPath(string AlyxInstallFolder, string addonName)
        {
            return System.IO.Path.Combine(AlyxInstallFolder, "content", "hlvr_addons", addonName);
        }
        const string SoundEventHeader = "<!-- kv3 encoding:text:version{e21c7f3c-8a33-41c5-9977-a76d3a32aa0d} format:generic:version{7412167c-06e9-4698-aff2-e63eb59037e7} -->";

        public static IEnumerable<Soundevent> Deserialize(System.IO.Stream stream)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                int bytesRead = 0;
                byte[] buffer = new byte[32768];
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }
                return Deserialize(System.Text.ASCIIEncoding.ASCII.GetString(ms.ToArray()));
            }
        }
        public static IEnumerable<Soundevent> DeserializeFile(string file)
        {
            using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
            {
                return Deserialize(sr.ReadToEnd());
            }
        }
        public static IEnumerable<Soundevent> Deserialize(string data)  //Change to data instead (no file opening).
        {
            List<Soundevent> events = new List<Soundevent>();
            
            List<string> Lines = new List<string>();
            
            bool propertyOpen = false;

            string[] AllLines = data.Replace("\n", string.Empty).Split('\r');

            for (int i = 2; i < AllLines.Length; i++)  //This expects line 1 to be the header (SoundEventHeader defined above), while line 2 is the opening brace ("{").  If it could be different additional logic is needed.
            {
                string line = AllLines[i];
                if (!propertyOpen && line.StartsWith("}"))
                {
                    break;
                }
                else
                {
                    int j = -1;
                    while (++j < line.Length && (line[j] == '\t' || line[j] == ' ')) { }
                    if (j < line.Length)
                    {
                        if (line[j] == '{')
                        {
                            propertyOpen = true;
                            Lines.Add(line.Substring(j).Trim());
                        }
                        else if (line[j] == '}')
                        {
                            propertyOpen = false;

                            var property = CreateSoundevent(Lines.ToArray());
                            events.Add(property);
                            Lines.Clear();
                        }
                        else
                        {
                            Lines.Add(line.Substring(j).Trim());
                        }
                    }
                }
            }
            StringBuilder builder = new StringBuilder();


            return events.ToArray();
        }
        private static Soundevent CreateSoundevent(string[] lines)
        {
            List<string> values = new List<string>();
            bool propertyOpen = false;
            string comment;
            bool valueOpen = false;
            string valueName = null;
            string valueValue;

            string eventName = null;
            Soundevent currentEvent = null;
            List<SoundeventProperty> properties = new List<SoundeventProperty>();
            foreach (var rawline in lines)
            {
                string line;
                int i = rawline.IndexOf("//");
                if (i > -1)
                {
                    if (i == 0)
                    {
                        //skip comment line.
                        comment = rawline.Substring(2);
                        continue;
                    }
                    else
                    {
                        //strip out comments;
                        line = rawline.Substring(0, i);
                        comment = rawline.Substring(i);
                    }
                }
                else
                {
                    line = rawline;
                }

                if (!propertyOpen)
                {
                    //looking for event name.
                    int j = line.IndexOf('=');
                    if (j > -1)
                    {
                        eventName = line.Substring(0, j).Replace("\"", string.Empty).Trim();
                    }

                }

                if (line.StartsWith("{"))
                {
                    if (string.IsNullOrEmpty(eventName))
                    {
                        throw new InvalidFileFormatException("Event name for a list of sound event properties was not found.");
                    }
                    propertyOpen = true;
                }
                else if (line.EndsWith("}"))
                {
                    propertyOpen = false;
                    break;
                }
                else if (propertyOpen)
                {
                    if (line.StartsWith('['))
                    {
                        valueOpen = true;
                    }
                    else if (line.StartsWith(']'))
                    {
                        if (string.IsNullOrEmpty(valueName))
                        {
                            throw new InvalidFileFormatException("Invalid file format.");
                        }

                        //TODO: The following line will need change depending on how arrays are handled.
                        SoundeventProperty bproperty = new SoundeventProperty(valueName, EventDisplays.ArrayValue, string.Join(',', values.ToArray()));
                        properties.Add(bproperty);
                        valueOpen = false;
                    }
                    else if (valueOpen)
                    {
                        //Is an array--load list of values.
                        string l = line;
                        if (l.EndsWith(','))
                        {
                            l = l.Substring(0, l.Length - 1);
                        }
                        values.Add(l.Replace("\"", string.Empty));
                    }
                    else
                    {
                        //either name = value or name =
                        //                            [
                        //                            ]
                        int k = line.IndexOf('=');
                        if (k > -1)
                        {
                            valueName = line.Substring(0, k).Trim();

                            while (++k < line.Length && line[k] < ' ') { }
                            if (k < line.Length)
                            {
                                if (line[k] == '[')
                                {
                                    valueOpen = true;
                                    k++;
                                }
                                if (k < line.Length && valueOpen)
                                {
                                    valueValue = line.Substring(k + 1).Trim();
                                    if (!string.IsNullOrEmpty(valueValue))
                                    {
                                        if (valueValue.StartsWith("\"") && valueValue.EndsWith("\""))
                                        {
                                            valueValue = valueValue.Substring(1, valueValue.Length - 2);
                                        }
                                        var aproperty = new SoundeventProperty(valueName, valueValue);
                                        properties.Add(aproperty);
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidFileFormatException("File format is invalid.");
                        }
                    }
                }
            }
            if (eventName == null)
            {
                throw new InvalidFileFormatException("Event name for a list of sound event properties was not found.");
            }
            currentEvent = new Soundevent(eventName);
            foreach (var prop in properties)
            {
                currentEvent.AddProperty(prop);
            }

            return currentEvent;
        }
       
        public static string Serialize(IEnumerable<Soundevent> soundevents)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(SoundEventHeader);
            sb.AppendLine("{");
            foreach (var soundevent in soundevents)
            {
                sb.Append("\t\"");

                sb.Append(soundevent.EventName);
                sb.AppendLine("\" = ");
                sb.AppendLine("\t{");
                foreach (var property in soundevent.Properties)
                {
                    sb.Append("\t\t");
                    sb.Append(property.TypeName);
                    sb.Append(" = ");

                    //TODO: The following lines will need change depending on how arrays are handled.
                    if (property.DisAs == EventDisplays.ArrayValue)
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t[");
                        sb.Append("\t\t\t\"");
                        sb.Append(property.Value);
                        sb.AppendLine("\"");
                        sb.AppendLine("\t\t]");
                    }
                    else
                    {
                        if(property.DisAs  == EventDisplays.StringValue )
                        {
                            sb.Append("\"");
                        }
                        sb.Append(property.Value);
                        if (property.DisAs == EventDisplays.StringValue)
                        {
                            sb.AppendLine("\"");
                        }
                        else
                        {
                            sb.AppendLine();
                        }
                    }
                }

                sb.AppendLine("\t}");
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
