using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoIDE.net
{
    public partial class Form1 : Form
    {
        const string ElfFile = @"C:\Users\dave\AppData\Local\Temp\build5092726759323454303.tmp\Debugger.cpp.elf";


        private DebuggerTransport mDebugger = new DebuggerTransport("COM3");
        private SymbolTableParser mSymbolTable;
        private DwarfParser mDwarfParser;

        public Form1()
        {
            InitializeComponent();
            
            mDebugger.BreakPoints.Add(new BreakpointInfo());
            mDebugger.BreakPointHit += BreakPointHit;
            mDebugger.SerialCharReceived += SerialCharReceived;

            SetState(false, "");

            InitSymbols();
            InitSource();
            InitDwarf();
        }

        private void SerialCharReceived(object sender, byte b)
        {
            Invoke(() => 
            { 
                SerialOutputTextbox.AppendText(new string((char)b, 1)); 
            });
        }

        void BreakPointHit(object sender, BreakpointInfo breakpoint)
        {
            SetState(false, string.Format("Breakpoint at line {0} ({1})", breakpoint.LineNumber, breakpoint.SourceFileName));

            //UpdateWatches();
        }

        private void ContinueButton_Click(object sender, EventArgs e)
        {
            mDebugger.TargetContinue();

            SetState(true, "");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mDebugger.Initialize();
        }

        private void SetState(bool running, string breakDesc)
        {
            Invoke(() => 
            {
                StatusLabel.Text = running ? "Running" : "Stopped";
                ContinueButton.Enabled = !running;
                BreakpointDetailsLabel.Text = breakDesc;
            });
        }



        // __ Symbols _________________________________________________________


        


        private void InitSymbols()
        {
            mSymbolTable = new SymbolTableParser(ElfFile);
            
            foreach (var sym in mSymbolTable.Symbols.Values)
            {
                var l = new ListViewItem(new string[] { 
                    sym.Name, 
                    "0x" + sym.MemoryAddress.ToString("X8"), 
                    sym.GetInterpretedValue().ToString()
                });

                l.Tag = sym;
                WatchesListView.Items.Add(l);
            }
        }


        // __ Source __________________________________________________________


        private void InitSource()
        {
            DisassemblyTextbox.Lines = ObjectDumper.GetDisassembly(ElfFile).ToArray();
        }


        private void InitDwarf()
        {
            mDwarfParser = new DwarfParser(ElfFile);
        }

        


        // __ Helpers _________________________________________________________


        private void Invoke(Action action)
        { 
            if (InvokeRequired)
            {
                Invoke((Delegate)action);
            }
            else
            {
                action.Invoke();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Editor.Core.ObjectExplorerControl.ShowDialog(mDwarfParser, "Dwarf tree");
        }
    }



    public delegate void BoolDelegate(bool b, string s);
}
