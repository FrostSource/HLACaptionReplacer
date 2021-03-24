using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAT
{
    public enum ErrorCodes
    {
        UNKOWN,
        OK,
        INVALID,
        DUPLICATE,
        NOTFOUND
    }
    public class Soundevent
    {
        List<SoundeventProperty> properties = new List<SoundeventProperty>();

        string eventName;
        string baseEvent;
        string addon;


        public Soundevent(string SoundeventName, string baseEvent = null, string Addon = "BaseGame")
        {
            eventName = SoundeventName;
            this.baseEvent = baseEvent;
            this.addon = Addon;
        }

        public string EventName { get => eventName; }
        public string BaseEvent { get => baseEvent; }
        public string Addon { get => addon; }
        public List<SoundeventProperty> Properties { get => properties; }

        public SoundeventProperty GetProperty(SoundeventProperty property)
        {
            foreach (var item in properties)
            {
                if (item.TypeName == property.TypeName)
                {
                    return item;
                }
            }
            return null;
        }
        public SoundeventProperty GetProperty(string propertyName)
        {
            SoundeventProperty retVal = null;
            foreach (var item in properties)
            {
                if (item.TypeName == propertyName)
                {
                    retVal = item;
                    break;
                }
            }
            return retVal;
        }

        public ErrorCodes AddProperty(SoundeventProperty property)
        {
            foreach (var item in properties)
            {
                if (item.TypeName.Equals(property.TypeName))
                {
                    return ErrorCodes.DUPLICATE;
                }
            }
            Properties.Add(property);
            return ErrorCodes.OK;
        }
        public ErrorCodes RemoveProperty(string type)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                SoundeventProperty item = properties[i];
                if (item.TypeName == type)
                {
                    properties.RemoveAt(i);
                    return ErrorCodes.OK;
                }
            }
            return ErrorCodes.NOTFOUND;
        }

        public override string ToString()
        {
            return EventName.ToString();
        }
    }
}
