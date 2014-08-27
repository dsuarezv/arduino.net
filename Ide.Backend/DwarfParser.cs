using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ArduinoIDE.net
{
    public class DwarfParser
    {
        private DwarfNode mCurrentNode;

        private string mElfFile;

        public List<DwarfNode> TopNodes = new List<DwarfNode>();
        public List<DwarfNode> AllNodes = new List<DwarfNode>();

        public DwarfParser(string elfFile)
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
            var node = DwarfNode.Get(s);
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

        private void AddNodeToParent(DwarfNode node)
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

        private static void AddChildToParent(DwarfNode parent, DwarfNode node)
        {
            if (parent == null) return;

            if (parent.Children == null) parent.Children = new List<DwarfNode>();

            parent.Children.Add(node);
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Id} {TagType}")]
    public class DwarfNode
    {
        private static Regex RegExpr = new Regex(@"<([0-9a-f]+)><([0-9a-f]+)>: Abbrev Number: ([0-9]+) \((DW_TAG_[a-z_]+)\)");

        public int Id;
        public string TagType;
        public int Depth;
        public int AbbrevNumber;
        public List<DwarfNode> Children;
        public Dictionary<string, DwarfAttribute> Attributes = new Dictionary<string, DwarfAttribute>();

        public static DwarfNode Get(string s)
        {
            var match = RegExpr.Match(s);
            if (!match.Success) return null;

            var groups = match.Groups;

            return new DwarfNode()
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
        private static Regex RegExpr = new Regex(@"<[ ]*([0-9a-f]+)> + DW_AT_([a-z_]+)[ \t]*: *([<>\(\)A-z, \t_\.\\:;0-9]+)");

        public int Id;
        public string Name;
        public string RawValue;

        public static DwarfAttribute Get(string s)
        {
            var match = RegExpr.Match(s);
            if (!match.Success) return null;

            var groups = match.Groups;

            return new DwarfAttribute()
            {
                Id = groups[1].GetIntValue(),
                Name = groups[2].Value,
                RawValue = groups[3].Value
            };
        }
    }




    public static class DwarfExtensionMethods
    { 
        public static int GetIntValue(this Group group)
        {
            int result;
            if (Int32.TryParse(group.Value, out result)) return result;

            try
            {
                return Convert.ToInt32(group.Value, 16);
            }
            catch
            {
                return -1;
            }

            //if (Int32.TryParse(group.Value, System.Globalization.NumberStyles.AllowHexSpecifier, null, out result)) return result;
            //return -1;
        }
    }
}
