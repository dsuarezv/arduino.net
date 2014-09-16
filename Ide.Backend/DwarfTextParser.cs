using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace arduino.net
{
    public class DwarfTextParser
    {
        private DwarfParserNode mCurrentNode;
        private string mElfFile;
        private DwarfParserSection mSection = new DwarfParserSection();


        public List<DwarfParserNode> TopNodes = new List<DwarfParserNode>();
        public List<DwarfParserNode> AllNodes = new List<DwarfParserNode>();
        public Dictionary<int, List<string>> Locations = new Dictionary<int, List<string>>();

        public DwarfTextParser(string elfFile)
        {
            mElfFile = elfFile;

            Parse();
        }

        private void Parse()
        {
            int lineNo = 1;

            foreach (var line in ObjectDumper.GetDwarf(mElfFile))
            {
                ParseDwarfTreeLine(line, lineNo++);
            }
        }

        private bool ParseDwarfTreeLine(string line, int lineNo)
        {
            mSection.Update(line);

            switch (mSection.Section)
            {
                case DwarfSection.DebugInfo: return ParseDebugInfoLine(line, lineNo);
                case DwarfSection.LocationTable: return ParseLocationInfoLine(line, lineNo);
                case DwarfSection.LineInfoTable: break;
            }

            return false;
        }


        // __ Location table parsing __________________________________________


        private bool ParseLocationInfoLine(string line, int lineNo)
        {
            var loc = DwarfParserLocation.Get(line);
            if (loc != null)
            { 
                if (!Locations.ContainsKey(loc.Id))
                {
                    Locations.Add(loc.Id, new List<string>());
                }

                Locations[loc.Id].AddRange(DwarfParserLocation.GetProgramEntries(loc.Program));
                
                return true;
            }

            return false;  // no match here
        }


        // __ Debug Info parsing ______________________________________________


        private bool ParseDebugInfoLine(string line, int lineNo)
        {
            var node = DwarfParserNode.Get(line);
            if (node != null)
            {
                node.LineNumber = lineNo;
                AllNodes.Add(node);
                AddNodeToParent(node);
                mCurrentNode = node;
                return true;
            }

            var att = DwarfParserAttribute.Get(line);
            if (att != null)
            {
                if (mCurrentNode == null) throw new Exception("Orphan attribute");
                mCurrentNode.Attributes.Add(att.Name, att);
                return true;
            }

            return false;  // No match here.
        }

        private void AddNodeToParent(DwarfParserNode node)
        {
            if (node.Depth == 0)
            {
                TopNodes.Add(node);
                return;
            }

            if (node.Depth == mCurrentNode.Depth + 1)
            {
                AddChildToParent(mCurrentNode, node);
                return;
            }

            // find immediate parent: last node with depth - 1

            for (int i = AllNodes.Count - 1; i >= 0; i--)
            {
                var current = AllNodes[i];

                if (current.Depth == node.Depth - 1)
                {
                    AddChildToParent(current, node);
                    return;
                }
            }

            throw new Exception("No parent found for node");
        }

        private static void AddChildToParent(DwarfParserNode parent, DwarfParserNode node)
        {
            if (parent == null) return;

            if (parent.Children == null) parent.Children = new List<DwarfParserNode>();

            parent.Children.Add(node);
        }
    }


    public class DwarfParserSection
    {
        private static Regex RegExpr = new Regex(@"section (?<section>\.debug_line)|Contents of the (?<section>\.debug_loc) section|section (?<section>\.debug_info) contains");

        public DwarfSection Section;

        public bool Update(string s)
        {
            var m = RegExpr.Match(s);
            if (!m.Success) return false;

            switch (m.Groups["section"].Value)
            {
                case ".debug_info": Section = DwarfSection.DebugInfo; break;
                case ".debug_loc": Section = DwarfSection.LocationTable; break;
                case ".debug_line": Section = DwarfSection.LineInfoTable; break;
            }

            return true;
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Id} {TagType}")]
    public class DwarfParserNode
    {
        private static Regex RegExpr = new Regex(@"<([0-9a-f]+)><([0-9a-f]+)>: Abbrev Number: ([0-9]+) \(DW_TAG_([a-z_]+)\)", RegexOptions.Compiled);

        public int LineNumber;
        public int Id;
        public string TagType;
        public int Depth;
        public int AbbrevNumber;
        public List<DwarfParserNode> Children;
        public Dictionary<string, DwarfParserAttribute> Attributes = new Dictionary<string, DwarfParserAttribute>();

        public DwarfParserAttribute GetAttr(string name)
        {
            if (Attributes.ContainsKey(name)) return Attributes[name];

            return DwarfParserAttribute.Empty;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Id, TagType);
        }

        public static DwarfParserNode Get(string s)
        {
            var m = RegExpr.Match(s);
            if (!m.Success) return null;

            var groups = m.Groups;

            return new DwarfParserNode()
            {
                Id = groups[2].GetHexValue(),
                Depth = groups[1].GetHexValue(),
                TagType = groups[4].Value,
                AbbrevNumber = groups[3].GetIntValue()
            };
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Id} {Name}: {RawValue}")]
    public class DwarfParserAttribute
    {
        public static DwarfParserAttribute Empty = new DwarfParserAttribute();

        private static Regex RegExpr = new Regex(@"<[ ]*([0-9a-f]+)> + DW_AT_([a-z_]+)\s*: *(.+)", RegexOptions.Compiled);
        private static Regex IndirectStringRegEx = new Regex(@"\(indirect string, offset: 0x[0-9a-fA-F]+\): (.+)", RegexOptions.Compiled);

        public int Id;
        public string Name;
        public string RawValue;


        public static DwarfParserAttribute Get(string s)
        {
            var m = RegExpr.Match(s);
            if (!m.Success) return null;

            var groups = m.Groups;

            string rawValue = groups[3].Value;

            return new DwarfParserAttribute()
            {
                Id = groups[1].GetHexValue(),
                Name = groups[2].Value,
                RawValue = rawValue
            };
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Id, Name, RawValue);
        }

        public string GetStringValue()
        {
            if (RawValue == null) return null;

            var m = IndirectStringRegEx.Match(RawValue);
            if (!m.Success) return null;

            return m.Groups[1].Value.Trim(' ', '\t');
        }

        public int GetIntValue()
        {
            if (RawValue == null) return -1;

            return DwarfHelper.GetIntOrHex(RawValue);
        }

        public bool GetBoolValue()
        {
            if (RawValue == null) return false;

            return (RawValue == "1");
        }

        public int GetReferenceValue()
        {
            if (RawValue == null) return -1;

            if (!RawValue.StartsWith("<")) return -1;

            var cleanValue = RawValue.Trim('<', '>', '\t').Substring(2);

            return DwarfHelper.GetIntOrHex(cleanValue);
        }
    }

    public class DwarfParserLocation
    {
        public int Id;
        public string Program;

        private static Regex RegExpr = new Regex(@"(?<id>[0-9a-f]+) \w+ \w+ \((?<program>[\w; :]+)\)", RegexOptions.Compiled);
        
        public static DwarfParserLocation Get(string s)
        {
            var m = RegExpr.Match(s);
            if (!m.Success) return null;

            return new DwarfParserLocation()
            {
                Id = m.Groups[1].GetHexValue(),
                Program = m.Groups[2].Value
            };
        }

        public static List<string> GetProgramEntries(string program)
        {
            List<string> result = new List<string>();

            foreach (var s in program.Split(';')) result.Add(s.Trim());

            return result;
        }
    }



    public static class DwarfHelper
    { 
        public static int GetIntValue(this Group group)
        {
            int result;
            if (int.TryParse(group.Value, out result)) return result;

            return -1;
        }

        public static int GetHexValue(this Group group)
        {
            int result;
            if (int.TryParse(group.Value, NumberStyles.HexNumber, null, out result)) return result;

            return -1;
        }

        public static int GetIntOrHex(string s)
        {
            int result;
            if (int.TryParse(s, NumberStyles.HexNumber, null, out result)) return result;
            if (Int32.TryParse(s, out result)) return result;

            return -1;
        }
    }


    public enum DwarfSection { DebugInfo, LocationTable, LineInfoTable };

}
