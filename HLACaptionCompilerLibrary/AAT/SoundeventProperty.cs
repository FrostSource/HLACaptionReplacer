using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAT
{
    public enum PropertyNames
    {
        type,
        volume,
        priority,
        mixgroup,
        volume_fade_out,
        volume_fade_in,
        vsnd_files,
        use_hrtf
    }
    public enum EventDisplays
    {
        FloatValue,
        SoundeventPicker,
        ArrayValue,
        StringValue,
        TypePicker
    }
    public class SoundeventProperty
    {
        private string typeName;
        private Type type;
        private string value;
        private EventDisplays disAs;

        public SoundeventProperty(PropertyNames PropertyName,Type property)
        {
            this.typeName = PropertyName.ToString();
            this.type = property;
        }
        public SoundeventProperty(string typeName,string v)
        {
            this.typeName = typeName;
            this.disAs = getDisplayType(v);
            this.value = v;
        }
        public SoundeventProperty(string typeName, Type type)
        {
            this.typeName = typeName;
            this.type = type;
        }
        public SoundeventProperty(string typeName,EventDisplays t = EventDisplays.StringValue, string v = "")
        {
            this.typeName = typeName;
            this.disAs = t;
            this.value = v;
        }

        public static EventDisplays getDisplayType(string val)
        {
            EventDisplays t = EventDisplays.StringValue;
            string orVal = val;
            if (float.TryParse(orVal, out _))
            {
                t = EventDisplays.FloatValue;
            }
            else if (orVal.Contains("["))
            {
                t = EventDisplays.ArrayValue;
            }
            return t;
        }

        public string TypeName { get => typeName; set => typeName = value; }
        public string Value { get => value; set => this.value = value; }
        public Type Type { get => type; set => type = value; }
        public EventDisplays DisAs { get => disAs; set => disAs = value; }
        public SortedDictionary<string, Type> TypeDictionary { get; } = new SortedDictionary<string, Type>();

        public override string ToString()
        {
            return TypeName+"; "+Value;
        }
    }
}
