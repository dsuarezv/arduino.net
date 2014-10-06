using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FastColoredTextBoxNS;

namespace arduino.net
{
    public class SyntaxHighlightApplier
    {
        static TextStyle BlueStyle = new TextStyle(new SolidBrush(Color.FromArgb(86, 156, 214)), null, FontStyle.Regular);
        static TextStyle BoldStyle = new TextStyle(null, null, FontStyle.Bold | FontStyle.Underline);
        static TextStyle GrayStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
        static TextStyle MagentaStyle = new TextStyle(new SolidBrush(Color.FromArgb(189, 99, 197)), null, FontStyle.Regular);
        static TextStyle GreenStyle = new TextStyle(new SolidBrush(Color.FromArgb(0, 0, 0)), null, FontStyle.Regular);
        static TextStyle BrownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Regular);
        static TextStyle MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);
        static TextStyle OrangeStyle = new TextStyle(new SolidBrush(Color.FromArgb(0, 0, 0)), null, FontStyle.Regular);
        static TextStyle RedStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
        static MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));



        public static void Cpp(FastColoredTextBox fctb, TextChangedEventArgs e)
        {
            fctb.LeftBracket = '(';
            fctb.RightBracket = ')';
            fctb.LeftBracket2 = '\x0';
            fctb.RightBracket2 = '\x0';
            fctb.CommentPrefix = "//";

            //clear style of changed range
            e.ChangedRange.ClearStyle(BlueStyle, BoldStyle, GrayStyle, MagentaStyle, GreenStyle, BrownStyle);

            //string highlighting
            e.ChangedRange.SetStyle(MagentaStyle, @"""""|@""""|''|@"".*?""|(?<!@)(?<range>"".*?[^\\]"")|'.*?[^\\]'");
            
            //comment highlighting
            e.ChangedRange.SetStyle(OrangeStyle, @"//.*$", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(OrangeStyle, @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
            e.ChangedRange.SetStyle(OrangeStyle, @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft);
            e.ChangedRange.SetStyle(OrangeStyle, @"(/\*.*?\*/)", RegexOptions.Multiline);

            // preprocessor
            e.ChangedRange.SetStyle(MagentaStyle, @"(#.*$)", RegexOptions.Multiline);

            //number highlighting
            e.ChangedRange.SetStyle(MagentaStyle, @"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b");
            
            //attribute highlighting
            e.ChangedRange.SetStyle(GrayStyle, @"^\s*(?<range>\[.+?\])\s*$", RegexOptions.Multiline);
            
            //class name highlighting
            e.ChangedRange.SetStyle(BlueStyle, @"\b(class|struct|enum|interface)\s+(?<range>\w+?)\b");
            
            //keyword highlighting
            e.ChangedRange.SetStyle(BlueStyle, @"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|add|alias|ascending|descending|dynamic|from|get|global|group|into|join|let|orderby|partial|remove|select|set|value|var|where|yield)\b|#region\b|#endregion\b");

            //clear folding markers
            e.ChangedRange.ClearFoldingMarkers();

            //set folding markers
            e.ChangedRange.SetFoldingMarkers("{", "}");                         //allow to collapse brackets block
            //e.ChangedRange.SetFoldingMarkers(@"#region\b", @"#endregion\b");    //allow to collapse #region blocks
            e.ChangedRange.SetFoldingMarkers(@"/\*", @"\*/");                   //allow to collapse comment block
        }

        
        public static Regex ErrorRegex = new Regex(@"\s*(?<file>.*):(?<line>[0-9]+): error: .*$", RegexOptions.Singleline | RegexOptions.Compiled);
        public static Regex WarningRegex = new Regex(@"\s*(?<file>.*):(?<line>[0-9]+): warning: .*$", RegexOptions.Singleline | RegexOptions.Compiled);


        public static void Compiler(FastColoredTextBox fctb, TextChangedEventArgs e)
        {
            //clear style of changed range
            e.ChangedRange.ClearStyle(BlueStyle, BoldStyle, GrayStyle, MagentaStyle, GreenStyle, BrownStyle);


            // Error highlight
            // Debugger\Debugger.ino: In function 'void loop()':
            // Debugger\Debugger.ino:24: error: 'myfunfc' was not declared in this scope
            e.ChangedRange.SetStyle(MaroonStyle, @"(.*: In .*$)", RegexOptions.Singleline);     // error context
            e.ChangedRange.SetStyle(RedStyle, ErrorRegex);

            // Warning highlight
            // Debugger\Debugger.ino:24: warning: unused variable 'test'
            e.ChangedRange.SetStyle(OrangeStyle, WarningRegex);

        }
    }
}
