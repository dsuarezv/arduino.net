using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ArduinoIDE.net
{
    public class DwarfParser
    {
        private DwarfNode mCurrentNode;
        private DwarfAttribute mCurrentAttribute;

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
                AddAttributeToNode(mCurrentNode, att);
                mCurrentAttribute = att;
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

        private void AddAttributeToNode(DwarfNode node, DwarfAttribute att)
        {
            if (node == null) return;

            if (node.Attributes == null) node.Attributes = new List<DwarfAttribute>();

            node.Attributes.Add(att);
        }
    }


    public class DwarfNode
    {
        private static Regex RegExpr = new Regex(@"<([0-9a-f]+)><([0-9a-f]+)>: Abbrev Number: ([0-9]+) \((DW_TAG_[a-z]+)\)");

        public int Id;
        public string TagType;
        public int Depth;
        public int AbbrevNumber;
        public List<DwarfAttribute> Attributes;
        public List<DwarfNode> Children;

        public static DwarfNode Get(string s)
        {
            var match = RegExpr.Match(s);
            if (!match.Success) return null;

            var groups = match.Groups;

            return new DwarfNode()
            {
                Id = groups[1].GetIntValue(),
                Depth = groups[0].GetIntValue(),
                TagType = groups[3].Value,
                AbbrevNumber = groups[2].GetIntValue()
            };
        }
    }

    public class DwarfAttribute
    {
        private static Regex RegExpr = new Regex(@"<([0-9a-f]+)> + (DW_AT_[a-z_]+)[ \t]*: *([<>\(\)A-z, \t_\.\\:;0-9]+)");

        public int Id;
        public string Name;
        public string RawValue;

        public virtual string Value
        {
            get { return RawValue; }
        }

        public static DwarfAttribute Get(string s)
        {
            DwarfAttribute result;

            result = DwarfNameAtt.Get(s);
            if (result != null) return result;

            result = DwarfLocationListAtt.Get(s);
            if (result != null) return result;

            result = DwarfLocationtAtt.Get(s);
            if (result != null) return result;

            result = DwarfFrameBasetAtt.Get(s);
            if (result != null) return result;

            return GetImpl(s);
        }


        private static DwarfAttribute GetImpl(string s)
        {
            var match = RegExpr.Match(s);
            if (!match.Success) return null;

            var groups = match.Groups;

            return new DwarfAttribute()
            {
                Id = groups[0].GetIntValue(),
                Name = groups[1].Value,
                RawValue = groups[2].Value
            };
        }
    }

    public class DwarfNameAtt: DwarfAttribute
    {
        private static Regex RegExpr = new Regex(@"<([a-f0-9]+)> +(DW_AT_name)[ \t]*: *\(indirect string, offset: 0x([a-f0-9]+)\): ([<>\(\)A-z, \t_:;0-9]+)");

        public string NameValue;
        public int Offset;

        public override string Value
        {
            get { return NameValue; }
        }

        public static DwarfAttribute Get(string s)
        {
            var match = RegExpr.Match(s);
            if (!match.Success) return null;

            var groups = match.Groups;

            return new DwarfNameAtt()
            {
                Id = groups[0].GetIntValue(),
                Name = groups[1].Value,
                Offset = groups[2].GetIntValue(),
                NameValue = groups[3].Value
            };
        }
    }


    public class DwarfLocationListAtt : DwarfAttribute
    {
        private static Regex RegExpr = new Regex(@"<([a-f0-9]+)> +(DW_AT_location)[ \t]+: +([a-z0-9 :]*)[ \t]+\(location list\)");

        public int LocationListIndex;
        
        public static DwarfAttribute Get(string s)
        {
            var match = RegExpr.Match(s);
            if (!match.Success) return null;

            var groups = match.Groups;

            return new DwarfLocationListAtt()
            {
                Id = groups[0].GetIntValue(),
                Name = groups[1].Value,
                RawValue = groups[2].Value,
                LocationListIndex = groups[2].GetIntValue()
            };
        }
    }


    public class DwarfLocationtAtt : DwarfAttribute
    {
        private static Regex RegExpr = new Regex(@"<([a-f0-9]+)> +(DW_AT_location)[ \t]+: +([a-z0-9 :]*)[ \t]+\(([_A-Za-z0-9: ]+)\)");

        public string LocationOpcodes;

        public override string Value
        {
            get { return LocationOpcodes; }
        }

        public static DwarfAttribute Get(string s)
        {
            var match = RegExpr.Match(s);
            if (!match.Success) return null;

            var groups = match.Groups;

            return new DwarfLocationtAtt()
            {
                Id = groups[0].GetIntValue(),
                Name = groups[1].Value,
                RawValue = groups[2].Value,
                LocationOpcodes = groups[3].Value
            };
        }
    }


    public class DwarfFrameBasetAtt : DwarfAttribute
    {
        private static Regex RegExpr = new Regex(@"<([a-f0-9]+)> +(DW_AT_frame_base)[ \t]+: +([a-z0-9 :]*)[ \t]+\(([_A-Za-z0-9: ]+)\)");

        public string FrameBaseOpcode;

        public override string Value
        {
            get { return FrameBaseOpcode; }
        }

        public static DwarfAttribute Get(string s)
        {
            var match = RegExpr.Match(s);
            if (!match.Success) return null;

            var groups = match.Groups;

            return new DwarfFrameBasetAtt()
            {
                Id = groups[0].GetIntValue(),
                Name = groups[1].Value,
                RawValue = groups[2].Value,
                FrameBaseOpcode = groups[3].Value
            };
        }
    }



    public static class DwarfExtensionMethods
    { 
        public static int GetIntValue(this Group group)
        {
            int result;
            if (Int32.TryParse(group.Value, out result)) return result;

            return -1;
        }
    }
}
