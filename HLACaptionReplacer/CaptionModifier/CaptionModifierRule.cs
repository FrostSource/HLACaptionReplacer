using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// DEPRECATED, DO NOT USED

namespace HLACaptionReplacer
{
    public enum ModificationType
    {
        Delete,
        Replace
    }

    class CaptionModifierRule
    {
        public uint Hash { get; set; }
        public string Text { get; set; }
        public ModificationType ModificationType { get; set; }
    }
}
