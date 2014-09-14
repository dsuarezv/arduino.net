using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace arduino.net
{
    public class DwarfTextParser
    {
        private DwarfParserNode mCurrentNode;

        private string mElfFile;

        public List<DwarfParserNode> TopNodes = new List<DwarfParserNode>();
        public List<DwarfParserNode> AllNodes = new List<DwarfParserNode>();

        public DwarfTextParser(string elfFile)
        {
            mElfFile = elfFile;

            Parse();
        }

        private void Parse()
        { 
            foreach (var line in ObjectDumper.GetDwarf(mElfFile))
            {
                ParseDwarfTreeLine(line);
            }
        }

        private bool ParseDwarfTreeLine(string s)
        {
            var node = DwarfParserNode.Get(s);
            if (node != null)
            {
                AllNodes.Add(node);
                AddNodeToParent(node);
                mCurrentNode = node;
                return true;
            }

            var att = DwarfAttribute.Get(s);
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

    [System.Diagnostics.DebuggerDisplay("{Id} {TagType}")]
    public class DwarfParserNode
    {
        private static Regex RegExpr = new Regex(@"<([0-9a-f]+)><([0-9a-f]+)>: Abbrev Number: ([0-9]+) \(DW_TAG_([a-z_]+)\)");

        public int Id;
        public string TagType;
        public int Depth;
        public int AbbrevNumber;
        public List<DwarfParserNode> Children;
        public Dictionary<string, DwarfAttribute> Attributes = new Dictionary<string, DwarfAttribute>();

        public DwarfAttribute GetAttr(string name)
        {
            if (Attributes.ContainsKey(name)) return Attributes[name];

            return DwarfAttribute.Empty;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Id, TagType);
        }

        public static DwarfParserNode Get(string s)
        {
            var match = RegExpr.Match(s);
            if (!match.Success) return null;

            var groups = match.Groups;

            return new DwarfParserNode()
            {
                Id = groups[2].GetIntValue(),
                Depth = groups[1].GetIntValue(),
                TagType = groups[4].Value,
                AbbrevNumber = groups[3].GetIntValue()
            };
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Id} {Name}: {RawValue}")]
    public class DwarfAttribute
    {
        public static DwarfAttribute Empty = new DwarfAttribute();

        private static Regex RegExpr = new Regex(@"<[ ]*([0-9a-f]+)> + DW_AT_([a-z_]+)\s*: *(.+)");
        private static Regex IndirectStringRegEx = new Regex(@"\(indirect string, offset: 0x[0-9a-fA-F]+\): (.+)");

        public int Id;
        public string Name;
        public string RawValue;
        public int ReferencedId = -1;


        public static DwarfAttribute Get(string s)
        {
            var match = RegExpr.Match(s);
            if (!match.Success) return null;

            var groups = match.Groups;

            string rawValue = groups[3].Value;

            return new DwarfAttribute()
            {
                Id = groups[1].GetIntValue(),
                Name = groups[2].Value,
                RawValue = rawValue,
                ReferencedId = GetReference(rawValue)
            };
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Id, Name, RawValue);
        }

        public string GetStringValue()
        {
            if (RawValue == null) return null;

            var match = IndirectStringRegEx.Match(RawValue);
            if (!match.Success) return null;

            return match.Groups[1].Value.Trim(' ', '\t');
        }

        public int GetIntValue()
        {
            if (RawValue == null) return -1;

            return DwarfHelper.GetInt(RawValue);
        }

        private static int GetReference(string s)
        {
            if (!s.StartsWith("<")) return -1;

            return DwarfHelper.GetInt(s.Trim('<', '>', '\t'));
        }
    }




    public static class DwarfHelper
    { 
        public static int GetIntValue(this Group group)
        {
            return GetInt(group.Value);
        }

        public static int GetInt(string s)
        {
            int result;
            if (Int32.TryParse(s, out result)) return result;

            try
            {
                return Convert.ToInt32(s, 16);
            }
            catch
            {
                return -1;
            }            
        }
    }
}
