using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parsing
{
    public class GenericNode
    {
        public dynamic Value { get; private set; }
        public GenericNode Left { get; private set; }
        public GenericNode Right { get; private set; }

        public GenericNode(dynamic value = null, GenericNode left = null, GenericNode right = null)
        {
            Value = value;
            Left = left;
            Right = right;
        }
    }
}
