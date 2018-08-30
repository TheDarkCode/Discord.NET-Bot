using System;

namespace ArcadesBot
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class UsageAttribute : Attribute
    {
        public string Text { get; }

        public UsageAttribute(string text) 
            => Text = text;
    }
}