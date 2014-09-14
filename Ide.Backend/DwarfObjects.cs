using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class DwarfObject
    {
        public int Id;
    }

    [System.Diagnostics.DebuggerDisplay("{Id} {Name}")]
    public class DwarfNamedObject: DwarfObject
    {
        public string Name;
    }

    public class DwarfBaseType: DwarfNamedObject
    {
        public int ByteSize;
        public int Encoding;
    }

    public class DwarfDeclaredObject: DwarfNamedObject
    {
        public int DeclarationFile;
        public int DeclarationLine;
    }

    public class DwarfCompileUnit: DwarfNamedObject
    {
        public string Ranges;
        public string StatementList;

        public List<DwarfSubprogram> Subprograms = new List<DwarfSubprogram>();
        public Dictionary<string, DwarfVariable> Variables = new Dictionary<string, DwarfVariable>();
        public Dictionary<string, DwarfLocatedObject> Types = new Dictionary<string, DwarfLocatedObject>();


        // Subprograms: dictionary <name, subprogram>
        // Typedefs
        // Variables
        
        // BaseTypes
        // StructureTypes
        // ArrayTypes
        // PointerTypes
        // ClassTypes
    }

    public class DwarfSubprogram: DwarfDeclaredObject
    {
        public string LinkageName;
        public bool External;
        public int LowPc;
        public int HighPc;

        public Dictionary<string, DwarfLocatedObject> Variables = new Dictionary<string, DwarfLocatedObject>();

        // Formal parameters
        // Variables
        // InlinedSubroutines
    }

    public class DwarfLocatedObject: DwarfDeclaredObject
    {
        public DwarfLocation Location;
        public DwarfBaseType Type;

        public virtual byte[] GetValue(Debugger debugger)
        {
            throw new NotImplementedException();

            // This method should interpret the location program and send the needed traceQueries 
            // to the debugger until it finds the described value.
        }

        public virtual string GetValueRepresentation(byte[] value)
        {
            throw new NotImplementedException();

            // using the Type, interpret the bytes and return a printable representation.
        }
    }

    public class DwarfFormalParameter : DwarfLocatedObject
    {
        
    }

    public class DwarfVariable: DwarfLocatedObject
    {
        public string ConstValue;
    }


    public class DwarfLocation: DwarfObject
    {
        public string[] LocationProgram;
    }
}
