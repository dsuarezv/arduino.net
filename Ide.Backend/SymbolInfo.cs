using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace arduino.net
{
    public class SymbolInfo: DependencyObject
    {
        private IDebugger mDebugger;
        private List<SymbolInfo> mChildren;
        private byte[] mRawValue;
        private DwarfBaseType mType;


        public bool IsRoot { get; set; }
        
        public string Value { get { return (string)GetValue(ValueProperty); } }
        public string TypeName { get { return (string)GetValue(TypeNameProperty); } }
        public string SymbolName { get; private set; }
        public bool IsExpanded { get { return (bool)GetValue(IsExpandedProperty); } set { SetValue(IsExpandedProperty, value); } }

        public IList<SymbolInfo> Children { get { return (IList<SymbolInfo>)GetValue(ChildrenProperty); } }


            //get
            //{
            //    if (mChildren != null) return mChildren;
            //    if (mType == null || mType.Members == null || mRawValue == null) return null;

            //    mChildren = new List<SymbolInfo>();

            //    foreach (var member in mType.Members)
            //    {
            //        mChildren.Add(
            //            new SymbolInfo(mDebugger, member.Name) { mType = member } );
            //    }

            //    return mChildren;
            //}


        public static readonly DependencyProperty TypeNameProperty = DependencyProperty.Register("TypeName", typeof(string), typeof(SymbolInfo));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(SymbolInfo));
        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register("Children", typeof(IList<SymbolInfo>), typeof(SymbolInfo));
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(SymbolInfo));

        
        public SymbolInfo(IDebugger debugger, string name)
        {
            if (debugger == null) throw new ArgumentException("Debugger cannot be null");
            if (name == null) throw new ArgumentException("Name cannot be null");
            
            SymbolName = name;
            IsRoot = false;
            mDebugger = debugger;
        }


        public void Refresh(DwarfSubprogram currentFunction, DwarfTree dwarf)
        {
            if (dwarf == null) return;

            var symbol = dwarf.GetSymbol(SymbolName, currentFunction);
            if (symbol == null || symbol.Type == null) return;

            if (mType != symbol.Type)
            { 
                mType = symbol.Type;
                mChildren = null;
            }            

            mRawValue = symbol.GetValue(mDebugger);
            if (mRawValue == null) return;

            var val = mType.GetValueRepresentation(mDebugger, mRawValue);
            
            SetValue(ValueProperty, val);
            SetValue(TypeNameProperty, mType.Name);

            if (mType.Members != null)
            {
                if (mChildren == null) mChildren = new List<SymbolInfo>();

                if (mChildren.Count == 0)
                {
                    foreach (var member in mType.Members)
                    {
                        mChildren.Add(new SymbolInfo(mDebugger, member.Name) { mType = member });
                    }
                }

                SetValue(ChildrenProperty, mChildren);
            }

            RefreshChildren();
        }

        private void RefreshChildren()
        {
            if (mChildren != null && IsExpanded)
            {
                foreach (var f in mChildren) f.Refresh(mRawValue);
            }
        }


        private void Refresh(byte[] parentRawValue)
        {
            if (mType == null) return;
            
            var member = mType as DwarfMember;
            if (member == null || member.Type == null) return;

            mRawValue = member.GetMemberRawValue(mDebugger, parentRawValue);
            var val = member.Type.GetValueRepresentation(mDebugger, mRawValue);

            SetValue(TypeNameProperty, member.Type.Name);
            SetValue(ValueProperty, val);

            RefreshChildren();
        }

        public string GetAsStringWithChildren()
        {
            var result = GetAsString();

            if (mChildren != null)
            {
                foreach (var si in Children)
                {
                    result += "\n  " + si.GetAsString();
                }
            }

            return result;
        }

        private string GetAsString()
        {
            return string.Format("{0} ({1}): {2}", SymbolName, TypeName, Value);
        }
    }
}
