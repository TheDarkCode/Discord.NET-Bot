using System;

namespace ArcadesBot.CommandExtensions.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class UsageAttribute : System.Attribute
    {
        public string Text { get; }

        public UsageAttribute(string text) 
            => Text = text;
    }
}