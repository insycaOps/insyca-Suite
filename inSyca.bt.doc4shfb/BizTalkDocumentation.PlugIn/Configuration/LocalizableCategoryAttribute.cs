using System;
using System.ComponentModel;

using BizTalkDocumentation.PlugIn.Properties;

namespace BizTalkDocumentation.PlugIn
{
    public sealed class LocalizableCategoryAttribute : CategoryAttribute
    {
        public LocalizableCategoryAttribute(string category)
            : base(category)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            return Resources.ResourceManager.GetString(value);
        }
    }
}