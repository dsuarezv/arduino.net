﻿using System;
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
        static TextStyle BoldStyle = new TextStyle(null, null, FontStyle.Bold);
        static TextStyle BlackStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
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
            e.ChangedRange.ClearStyle(BlueStyle, GrayStyle, MagentaStyle, GreenStyle);

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
            e.ChangedRange.SetStyle(BlueStyle, @"\b(class|struct)\s+(?<range>\w+?)\b");
            
            //keyword highlighting
            e.ChangedRange.SetStyle(BlueStyle, @"\b(alignas|alignof|and|and_eq|asm|auto|bitand|bitor|bool|break|case|catch|char|char16_t|char32_t|class|compl|const|constexpr|const_cast|continue|decltype|default|delete|do|double|dynamic_cast|else|enum|explicit|export|extern|false|float|for|friend|goto|if|inline|int|long|mutable|namespace|new|noexcept|not|not_eq|nullptr|operator|or|or_eq|private|protected|public|register|reinterpret_cast|return|short|signed|sizeof|static|static_assert|static_cast|struct|switch|template|this|thread_local|throw|true|try|typedef|typeid|typename|union|unsigned|using|virtual|void|volatile|wchar_t|while|xor|xor_eq)\b|#region\b|#endregion\b");

            //clear folding markers
            e.ChangedRange.ClearFoldingMarkers();

            //set folding markers
            e.ChangedRange.SetFoldingMarkers("{", "}");                         //allow to collapse brackets block
            e.ChangedRange.SetFoldingMarkers(@"/\*", @"\*/");                   //allow to collapse comment block
        }

        

        public static void CppCompiler(FastColoredTextBox fctb, TextChangedEventArgs e)
        {
            //clear style of changed range
            e.ChangedRange.ClearStyle(MaroonStyle, RedStyle, OrangeStyle);


            // Error highlight
            // Debugger\Debugger.ino: In function 'void loop()':
            // Debugger\Debugger.ino:24: error: 'myfunfc' was not declared in this scope
            e.ChangedRange.SetStyle(MaroonStyle, @"(.*: In .*$)", RegexOptions.Singleline);     // error context
            e.ChangedRange.SetStyle(RedStyle, CompilerMsg.ErrorRegex);

            // Sketch size OK
            e.ChangedRange.SetStyle(GreenStyle, "^Binary sketch size: .*", RegexOptions.Multiline);

            // Sketch size off-limits
            e.ChangedRange.SetStyle(RedStyle, "^WARNING: Binary sketch size of .*", RegexOptions.Multiline);

            // Warning highlight
            // Debugger\Debugger.ino:24: warning: unused variable 'test'
            e.ChangedRange.SetStyle(OrangeStyle, CompilerMsg.WarningRegex);

        }


        public static void Disassembly(FastColoredTextBox fctb, TextChangedEventArgs e)
        {
            //clear style of changed range
            e.ChangedRange.ClearStyle(GrayStyle, BlackStyle);

            //e.ChangedRange.SetStyle(BoldStyle, @"^\s*(?<addr>[0-9a-f]+)(?<label>\s<[a-zA-Z_]+>)*:\s+(?<bytecode>([a-f0-9][a-f0-9] )+)\s+(?<range>[a-zA-Z0-9,\+\-\. ]*)(?<comment>;.+$)*", RegexOptions.Multiline);
            //e.ChangedRange.SetStyle(GreenStyle, @"^\s*(?<addr>[0-9a-f]+)(?<label>\s<[a-zA-Z_]+>)*:\s+(?<bytecode>([a-f0-9][a-f0-9] )+)\s+(?<insto>[a-zA-Z0-9,\+\-\. ]*)(?<range>;.+$)*", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(BlackStyle, @"^\s*(?<addr>[0-9a-f]+)(?<label>\s<[\.\-a-zA-Z_]+>)*:\s+(?<bytecode>[a-f0-9 ]+)(?<insto>.*)", RegexOptions.Multiline);

            e.ChangedRange.SetStyle(GrayStyle);

        }
    }
}
