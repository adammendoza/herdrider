using System;
using System.Text;
using Microsoft.SPOT;
using Helpers.Imaging;
using Helpers;
namespace Helpers.Fonts {
    public abstract class FontDefinition {
        abstract public FontInfo GetFontInfo();
        public string GetFontName() {
            var name = GetType().ToString();
            return name.Substring(name.LastIndexOf('.') + 1);
        }
    }
}
