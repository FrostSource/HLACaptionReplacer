using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValveResourceFormat
{
    public enum ClosedCaptionLanguage
    {
        Brazilian,
        Bulgarian,
        Czech,
        Danish,
        Dutch,
        English,
        Finnish,
        French,
        German,
        Greek,
        Hungarian,
        Italian,
        Japanese,
        Koreana,
        Latam,
        Norwegian,
        Polish,
        Portuguese,
        Romanian,
        Russian,
        Schinese,
        Spanish,
        Swedish,
        Tchinese,
        Thai,
        Turkish,
        Ukranian,
        Vietnamese
    }
    class ClosedCaptionFile : IEnumerable<KeyValuePair<string, string>>
    {
        //TODO: Should enforce from list of specific languages so file can't choose anything? (see enum above)
        /// <summary>
        /// Gets or sets the language this file represents.
        /// </summary>
        public string Language { get; set; }

        private IDictionary<string, string> Tokens { get; set; } = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                return Tokens[key];
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Tokens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Read(string filename)
        {
            //var parser = new ClosedCaptionFileParser("");
            //var result = parser.Parse();

            //Language = result["Language"];
            //Tokens = result["Tokens"];
        }

        
    }
}
