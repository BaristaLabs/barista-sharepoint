namespace Barista.TuesPechkin
{
    using System;
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class WkhtmltoxSettingAttribute : Attribute
    {
        public string Name { get; set; }

        public WkhtmltoxSettingAttribute(string name)
        {
            Name = name;
        }
    }
}
