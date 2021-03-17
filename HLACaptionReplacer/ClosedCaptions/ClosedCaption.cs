using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionReplacer
{
    class ClosedCaption
    {
        private string definition;
        private string soundEvent;

        public ushort Length { get; private set; }
        public uint SoundEventHash { get; set; }
        // Previous UnknownV2
        public uint DefinitionHash { get; set; } = 0;
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
            }
        }


        public ClosedCaption()
        {
            Definition = "";
        }

        public ClosedCaption(string sndevt, string definition)
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
