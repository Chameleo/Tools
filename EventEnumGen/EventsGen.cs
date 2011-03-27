using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class EventsGen : Form
    {
        public EventsGen()
        {
            InitializeComponent();
        }

        // This method generates eEvents_FrameXML enum from hex rays output of the function 
        // where the strings from that enum are used to initialize a huge array.
        private void Input_TextChanged(object sender, EventArgs e)
        {
            StreamWriter outp = File.CreateText("eEvents_FrameXML.h");
            outp.WriteLine("enum eEvents_FrameXML");
            outp.WriteLine("{");
            Regex r = new Regex(".*dword_([0-9A-Fa-f]+)(?=/[.+/])? =.*\"(.+)\"");
            string[] lines = Input.Lines;
            Array.Sort(lines);
            string lastEvent = "";
            Dictionary<string, int> occurences = new Dictionary<string, int>();
            int firstOffset = 0;
            foreach (string line in lines)
            {
                Match m = r.Match(line);
				if (!m.Success)
				{
					continue;
				}
                int offset = Convert.ToInt32(m.Groups[1].Captures[0].Value, 16);
                string eventName = m.Groups[2].Captures[0].Value;
                if(firstOffset == 0)
                {
                    firstOffset = offset;
                }
                offset -= firstOffset;
                offset /= 4;
                if (occurences.ContainsKey(eventName))
                {
                    occurences[eventName] += 1;
                }
                else
                {
                    occurences[eventName] = 1;
                }
                lastEvent = eventName;
                outp.WriteLine("    EVENT_" + eventName + (occurences[eventName] > 1 ? ("_" + occurences[eventName]) : "") +
                    " = 0x{0:X},", offset);
            }
            outp.WriteLine("}");
            outp.Close();
        }
    }
}
