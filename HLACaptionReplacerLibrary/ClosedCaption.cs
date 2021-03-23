using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionReplacer
{
    public class ClosedCaption
    {
        private string definition;
        private string soundEvent;

        public ushort Length { get; private set; }
        public uint SoundEventHash { get; set; }
        // Previous UnknownV2
        public uint DefinitionHash { get; set; } = 0;

        public uint Blocknum { get; set; }

        public ushort Offset { get; set; }

        internal ushort ReadLength { get; set; }

        public string SoundEvent
        {
            get => soundEvent;
            set
            {
                soundEvent = value;
                SoundEventHash = ValveResourceFormat.Crc32.Compute(Encoding.UTF8.GetBytes(soundEvent));
            }
        }

        public string Definition
        {
            get => definition;
            set
            {
                definition = value + '\0';
                Length = (ushort)Encoding.Unicode.GetByteCount(definition);
                DefinitionHash = ValveResourceFormat.Crc32.Compute(Encoding.Unicode.GetBytes(value));
                RawDefinition = value;
            }
        }

        public string RawDefinition { get; private set; }


        public ClosedCaption():this("", "")
        {
        }

        // How do you solve this warning? The backing field is definitely being set.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ClosedCaption(string sndevt, string definition)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            SoundEvent = sndevt;
            Definition = definition;
        }

        public bool IsBlank
        {
            get
            {
                return Definition == "\0" || Definition == "";
            }
        }
    }

    
}
