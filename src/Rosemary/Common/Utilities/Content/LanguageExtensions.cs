using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Localization;

namespace Rosemary.Common;

public static class LanguageExtensions
{
    extension(Language)
    {
        public static List<LocalizedText> GetAllText(string startsWith)
        {
            return Language.FindAll(Lang.CreateDialogFilter(startsWith)).ToList();
        }

        public static List<string> GetAllTextValues(string startsWith)
        {
            return Language.FindAll(Lang.CreateDialogFilter(startsWith)).Select(t => t.Value).ToList();
        }
    }
}
