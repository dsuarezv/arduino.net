﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class SymbolInfo
    {
        private IDebugger mDebugger;
        private List<SymbolInfo> mChildren;
        private byte[] mRawValue;
        private DwarfBaseType mType;
        private string mName;
        

        public bool HasChildren
        { 
            get
            {
                return (mType.Members != null);
            }
        }
        
        public IList<SymbolInfo> Children
        { 
            get
            {
                if (mChildren != null) return mChildren;
                if (mType.Members == null || mRawValue == null) return null;

                mChildren = new List<SymbolInfo>();

                foreach (var member in mType.Members)
                {
                    mChildren.Add(
                        new SymbolInfo(mDebugger, member.Name, member.Type, member.GetMemberRawValue(mRawValue)));
                }

                return mChildren;
            }
        }

        public string Value
        {
            get 
            {
                if (mRawValue == null) return "<no value>";

                return mType.GetValueRepresentation(mDebugger, mRawValue);
            }
        }

        public string TypeName
        { 
            get
            {
                return mType.ToString();
            }
        }

        public string SymbolName
        {
            get
            {
                return mName;
            }
        }


        public SymbolInfo(IDebugger debugger, string name, DwarfBaseType type, byte[] rawValue)
        {
            if (debugger == null) throw new ArgumentException("Debugger cannot be null");
            if (name == null) throw new ArgumentException("Name cannot be null");
            if (type == null) throw new ArgumentException("Type cannot be null");

            mDebugger = debugger;
            mName = name;
            mType = type;
            mRawValue = rawValue;
        }


        /// <summary>
        /// Returns a string replresenting the symbol, type and value. Also prints inmmediate children.
        /// </summary>
        /// <returns></returns>
        public string GetInspectRepresentation()
        {
            var result = GetString();

            if (HasChildren)
            { 
                foreach (var si in Children)
                {
                    result += "\n  " + si.GetString();
                }
            }

            return result;
        }

        private string GetString()
        {
            return string.Format("{0} ({1}): {2}", SymbolName, TypeName, Value);
        }
    }
}