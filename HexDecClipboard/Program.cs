using System;
using System.Text.RegularExpressions;

namespace HexDecClipboard
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                bool toDec = false;
                string text = System.Windows.Forms.Clipboard.GetText();
                if(String.IsNullOrEmpty(text))
                {
                    return;
                }
                if(args.Length > 0 && args[0] == "h")
                {
                    toDec = true;
                    text = text.Replace("0x", "");
                }
                else if (text.StartsWith("0x"))
                {
                    toDec = true;
                    text = text.Substring(2);
                }
                else if (Regex.IsMatch(text, @"[^\-0-9]"))
                {
                    toDec = true;   // assume contains A-F
                }
                text = text.ToLower();
                if (!Regex.IsMatch(text, "-?[0-9a-fA-F]+"))
                {
                    return; // invalid format
                }
                long value = toDec ? Convert.ToInt64(text, 16) : Int64.Parse(text);
                System.Windows.Forms.Clipboard.SetText(string.Format(toDec ? "{0}" : "{0:X}", value));
            }
            catch (Exception)
            {
            }
        }
    }
}
