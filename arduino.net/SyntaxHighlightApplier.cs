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
        static TextStyle GreenStyle = new TextStyle(new SolidBrush(Color.FromArgb(87, 166, 74)), null, FontStyle.Regular);
        static TextStyle BrownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Regular);
        static TextStyle MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);
        static TextStyle OrangeStyle = new TextStyle(new SolidBrush(Color.FromArgb(255, 128, 0)), null, FontStyle.Regular);
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
            e.ChangedRange.SetStyle(GreenStyle, @"//.*$", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
            e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft);
            e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)", RegexOptions.Multiline);

            // preprocessor
            e.ChangedRange.SetStyle(MagentaStyle, @"(#.*$)", RegexOptions.Multiline);

            //number highlighting
            e.ChangedRange.SetStyle(MagentaStyle, @"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b");
            
            //attribute highlighting
            e.ChangedRange.SetStyle(GrayStyle, @"^\s*(?<range>\[.+?\])\s*$", RegexOptions.Multiline);
            
            //class name highlighting
            e.ChangedRange.SetStyle(BlueStyle, @"\b(class|struct|enum|interface)\s+(?<range>\w+?)\b");
            
            //keyword highlighting
            e.ChangedRange.SetStyle(BlueStyle, @"\b(alignas|alignof|and|and_eq|asm|auto|bitand|bitor|bool|break|case|catch|char|char16_t|char32_t|class|compl|const|constexpr|const_cast|continue|decltype|default|delete|do|double|dynamic_cast|else|enum|explicit|export|extern|false|float|for|friend|goto|if|inline|int|long|mutable|namespace|new|noexcept|not|not_eq|nullptr|operator|or|or_eq|private|protected|public|register|reinterpret_cast|return|short|signed|sizeof|static|static_assert|static_cast|struct|switch|template|this|thread_local|throw|true|try|typedef|typeid|typename|union|unsigned|using|virtual|void|volatile|wchar_t|while|xor|xor_eq)\b|#region\b|#endregion\b");

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
