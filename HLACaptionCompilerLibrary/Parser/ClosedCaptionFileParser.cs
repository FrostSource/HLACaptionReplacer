using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parser
{
    public class ClosedCaptionFileParser : GenericParser
    {

        public ClosedCaptionFileParser(string source):base(source)
        {

        }

        public IDictionary<string, dynamic> Parse()
        {
            throw new NotImplementedException();
            //var parsed = new Dictionary<string, dynamic>();
            
            
        }
    }
}
