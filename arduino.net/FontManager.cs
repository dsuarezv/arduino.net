using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class FontManager
    {
        private const string InternalFontFile = "fonts/SourceCodePro-Regular.otf";
        //private static Font mInternalFont = null;

        public static Font GetSourceCodeFont()
        {
            if (Configuration.Instance.EditorFontName == "Internal" && File.Exists(InternalFontFile))
            {
                var p = new PrivateFontCollection();
                p.AddFontFile(InternalFontFile);
                return new Font(p.Families[0], Configuration.Instance.EditorFontSize);
            }
                
            return new Font(Configuration.Instance.EditorFontName, Configuration.Instance.EditorFontSize);
        }

    }
}
