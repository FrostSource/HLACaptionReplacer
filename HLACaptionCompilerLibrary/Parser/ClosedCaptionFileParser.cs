using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionReplacer.Parser
{
    class ClosedCaptionFileParser : AbstractParser
    {

        public ClosedCaptionFileParser(string source):base(source)
        {

        }

        public override IDictionary<string, dynamic> Parse()
        {
            throw new NotImplementedException();
            //var parsed = new Dictionary<string, dynamic>();
            
            
        }
    }
}
