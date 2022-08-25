using System;

namespace Rayiah.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HelpAttribute : Attribute 
    {
        readonly string helpType = "";
        readonly string helpdesc = "";
        readonly Discord.Color color;

        /// <summary>
        /// Description for command.
        /// </summary>
        public string Description { get => helpdesc; }
        /// <summary>
        /// Always in lowercase.
        /// </summary>
        public string Name { get => helpType; }
        /// <summary>
        /// Which color to use in embed.
        /// </summary>
        public Discord.Color Color { get => color; }

        public HelpAttribute(string type, string helpdesc, int color = 1) {
            this.helpType = type.ToLower();
            this.helpdesc = helpdesc;
            this.color = Tools.Utilites.GetColor(color);
        }

        public HelpAttribute(string type, string helpdesc, Discord.Color color) {
            this.helpType = type.ToLower();
            this.helpdesc = helpdesc;
            this.color = color;
        }
    }
}
