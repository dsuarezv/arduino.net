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
        private List<SymbolInfo> mChildren;
        private DwarfBaseType mType;
        private DwarfMember mMember;
        private IDebugger mDebugger;
        private byte[] mRawValue;

        
        public string Value { get { return (string)GetValue(ValueProperty); } }
        public string TypeName { get { return (string)GetValue(TypeNameProperty); } }
        public bool IsExpanded { get { return (bool)GetValue(IsExpandedProperty); } set { SetValue(IsExpandedProperty, value); } }
        public IList<SymbolInfo> Children { get { return (IList<SymbolInfo>)GetValue(ChildrenProperty); } }
        public string SymbolName { get; private set; }
        public bool IsRoot { get; set; }


        public static readonly DependencyProperty TypeNameProperty = DependencyProperty.Register("TypeName", typeof(string), typeof(SymbolInfo));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(SymbolInfo));
        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register("Children", typeof(IList<SymbolInfo>), typeof(SymbolInfo));
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(SymbolInfo));

        
        public SymbolInfo(IDebugger debugger, string name)
        {
            if (debugger == null) throw new ArgumentException("Debugger cannot be null");
            if (name == null) throw new ArgumentException("Name cannot be null");

            IsRoot = false;
            SymbolName = name;
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
            
            SetValue(ValueProperty, mType.GetValueRepresentation(mDebugger, mRawValue));
            SetValue(TypeNameProperty, mType.GetTypeRepresentation());

            RefreshChildren();
        }

        private void RefreshChildren()
        {
            if (mType != null && mType.Members != null)
            {
                if (mChildren == null) mChildren = new List<SymbolInfo>();

                if (mChildren.Count == 0)
                {
                    foreach (var member in mType.Members)
                    {
                        mChildren.Add(new SymbolInfo(mDebugger, member.Name) { mMember = member });
                    }
                }

                SetValue(ChildrenProperty, mChildren);
            }

            if (mChildren != null && IsExpanded)
            {
                foreach (var f in mChildren) f.RefreshMember(mRawValue);
            }
        }

        private void RefreshMember(byte[] parentRawValue)
        {
            if (parentRawValue == null) return;
            if (mMember == null || mMember.MemberType == null) return;

            mType = mMember.MemberType;
            mRawValue = mMember.GetMemberRawValue(mDebugger, parentRawValue);

            SetValue(ValueProperty, mType.GetValueRepresentation(mDebugger, mRawValue));
            SetValue(TypeNameProperty, mType.GetTypeRepresentation());

            RefreshChildren();
        }

        public string GetAsStringWithChildren()
        {
            IsExpanded = true;
            RefreshChildren();

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
            if (TypeName == null && Value == null)
            {
                return string.Format("{0} is not a symbol that can be inspected.", SymbolName);
            }

            var type = (TypeName == null) ? "" : " (" + TypeName + ')';
            var val = (Value == null) ? "" : ": " + Value;
            return string.Format("{0}{1}{2}", SymbolName, type, val);            
        }


        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == IsExpandedProperty)
            {
                bool val = (bool)GetValue(IsExpandedProperty);

                if (val)
                {
                    RefreshChildren();
                }
            }

            base.OnPropertyChanged(e);
        }
    }
}
