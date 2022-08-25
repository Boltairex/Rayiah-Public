using System;

namespace Rayiah.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class InfoAttribute : Attribute
    {
        string desc;
        bool isProtected;

        public string Desc { get => desc; }
        public bool Protected { get => isProtected; }

        public InfoAttribute(string desc, bool isProtected = false)
        {
            this.desc = desc;
            this.isProtected = isProtected;
        }
    }
}
