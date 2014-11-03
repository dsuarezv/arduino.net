using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace arduino.net
{

    // All line indices are zero-based.

    public class SketchFileParser
    {
        
        
        private static Regex mIncludeRegex = new Regex("^\\s*#include\\s*[<\\\"](?<file>\\S+)[\\\">]", RegexOptions.Multiline);
        private static Regex mFunctionRegEx = new Regex(@"(?<return_type>[\w\[\]\*]+)\s+(?<name>[&\[\]\*\w]+)\s*\((?<arguments>[&,\[\]\*\w\s]*)\)(?=\s*\{)", RegexOptions.Multiline);
        private static Regex mPrototypeRegEx = new Regex(@"(?<return_type>[\w\[\]\*]+)\s+(?<name>[&\[\]\*\w]+)\s*\((?<arguments>[&,\[\]\*\w\s]*)\)(?<attributes>\s__attribute__\s*\(\([\w,\s]+\)\))*(?=\s*\;)", RegexOptions.Multiline);
        private static Regex mCommentsRegEx = new Regex(
                                "('.')" +
                                "|(\"(?:[^\"\\\\]|\\\\.)*\")" +                  // double-quoted string
                                "|(//.*?$)|(/\\*[^*]*(?:\\*(?!/)[^*]*)*\\*/)" +  // single and multi-line comment
                                "|((^\\s*#define.*)?$)",                         // pre-processor directive
                                RegexOptions.Multiline);
        private static Regex mSetupFuncRegEx = new Regex(@"void\ssetup\s*\(\s*\)\s*{\s*", RegexOptions.Multiline);


        private string mFileName;
        private List<int> mLineEnds = new List<int>();
        private Dictionary<string, string> mFunctions = new Dictionary<string, string>();
        private Dictionary<string, string> mPrototypes = new Dictionary<string, string>();
        private List<string> mFunctionsWithoutPrototype = new List<string>();
        private int mLastIncludeLine = -1;
        private int mSetupFirstLine = -1;
        private List<string> mIncludeFiles = new List<string>();


        public int LastIncludeLineNumber
        {
            get { return mLastIncludeLine; }
        }

        public List<string> UniqueFunctionDeclarations
        {
            get { return mFunctionsWithoutPrototype; }
        }

        public bool HasSetupFunction
        {
            get { return mSetupFirstLine > -1; }
        }

        public int SetupFunctionFirstLine
        {
            get { return mSetupFirstLine; }
        }

        public IList<string> IncludedFiles
        {
            get { return mIncludeFiles; }
        }

        public SketchFileParser(string sketchFileName)
        {
            mFileName = sketchFileName;
        }


        public void Parse()
        {
            using (var r = new StreamReader(mFileName))
            {
                ProcessCode(r.ReadToEnd());
            }
        }



        private void ProcessCode(string content)
        { 
            var noComments = RemoveComments(content);
            var normalized = NormalizeLineEndings(noComments);
            
            CalculateLineNumbers(normalized);
            CalculateLastIncludeLine(normalized);
            BuildFunctionsLists(normalized);
            CalculateSetupFirstLine(normalized);
        }

        private string NormalizeLineEndings(string content)
        {
            return content.Replace("\r\n", "\n").Replace('\r', '\n');
        }

        private string RemoveComments(string content)
        {
            // Remove comments preserving lines

            return mCommentsRegEx.Replace(content, (m) => 
                    {
                        var r = new StringBuilder(m.Value);

                        for (int i = 0; i < r.Length; ++i)
                        {
                            if (r[i] != '\r' && r[i] != '\n') r[i] = ' ';
                        }

                        return r.ToString();
                    });
        }

        private int GetLineForCharIndex(int charIndex)
        { 
            for (int i = 0; i < mLineEnds.Count; ++i)
            {
                if (charIndex < mLineEnds[i]) return i;
            }

            return -1;
        }

        private void CalculateLineNumbers(string content)
        {
            int currentEndLine = 0;

            while ( (currentEndLine = content.IndexOf("\n", currentEndLine + 1)) > -1 )
            {
                mLineEnds.Add(currentEndLine);
            }
        }

        private void CalculateLastIncludeLine(string content)
        {
            var matches = mIncludeRegex.Matches(content);

            if (matches == null || matches.Count == 0)
            {
                mLastIncludeLine = 0;
                return;
            }

            BuildLibraryList(matches);

            var lastMatch = matches[matches.Count - 1];

            mLastIncludeLine = GetLineForCharIndex(lastMatch.Index);
        }

        private void BuildFunctionsLists(string content)
        {
            foreach (Match m in mFunctionRegEx.Matches(content))
            {
                mFunctions.Add(m.Groups["name"].Value, m.Value);
            }

            foreach (Match m in mPrototypeRegEx.Matches(content))
            {
                mPrototypes.Add(m.Groups["name"].Value, m.Value);
            }

            foreach (var func in mFunctions)
            { 
                bool found = false;

                foreach (var prot in mPrototypes)
                {
                    if (prot.Key == func.Key)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found) mFunctionsWithoutPrototype.Add(func.Value);
            }
        }

        private void BuildLibraryList(MatchCollection matches)
        { 
            mIncludeFiles.Clear();

            foreach (Match m in matches)
            {
                mIncludeFiles.Add(m.Groups["file"].Value);
            }
        }

        private void CalculateSetupFirstLine(string content)
        {
            Match m = mSetupFuncRegEx.Match(content);

            if (!m.Success) return;

            mSetupFirstLine = GetLineForCharIndex(m.Index + m.Length);
        }
    }
}
