using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
namespace PCHincluder
{
	// Script to include a particular line as first line of code after the header comment in all CPP files.
	class Program
	{
		static string Include = "#include \"gamePCH.h\"";

		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Required directory path as an argument");
				return;
			}
			List<string> files = GetFilesRecursive(args[0]);
			int found = 0, added = 0;
			Regex commentStart = new Regex(@"^\s*/\*");
			Regex comment = new Regex(@"^\s*(\*|//)");
			Regex empty = new Regex(@"^\s*$");
			foreach (string s in files)
			{
				string ext = Path.GetExtension(s);
				if (ext != ".cpp" /*&& ext != ".h"*/ || Path.GetFileNameWithoutExtension(s) == "gamePCH")
				{
					continue;
				}
				List<string> lines = new List<string>(File.ReadAllLines(s));
				bool changed = false;
				bool headerFound = false;
				for(int i = 0;i < lines.Count; i++)
				{
					string line = lines[i];

					if (commentStart.IsMatch(line) && !headerFound)
					{
						headerFound = true;
						// skipping header comment
						continue;
					}
					if (comment.IsMatch(line))
					{
						continue;
					}
					if (empty.IsMatch(line))
					{
						continue;
					}
					if (line.StartsWith(Include))
					{
						// already present
						found++;
						break;
					}

					lines.Insert(i, Include);
					added++;
					changed = true;
					break;
				}
				if (changed)
				{
					// recreate the file
					StreamWriter sw = File.CreateText(s);
					foreach(string line in lines)
					{
						sw.WriteLine(line);
					}
					sw.Close();
				}
			}
		}

		public static List<string> GetFilesRecursive(string b)
		{
			// 1.
			// Store results in the file results list.
			List<string> result = new List<string>();

			// 2.
			// Store a stack of our directories.
			Stack<string> stack = new Stack<string>();

			// 3.
			// Add initial directory.
			stack.Push(b);

			// 4.
			// Continue while there are directories to process
			while (stack.Count > 0)
			{
				// A.
				// Get top directory
				string dir = stack.Pop();

				try
				{
					// B
					// Add all files at this directory to the result List.
					result.AddRange(Directory.GetFiles(dir, "*.*"));

					// C
					// Add all directories at this directory.
					foreach (string dn in Directory.GetDirectories(dir))
					{
						stack.Push(dn);
					}
				}
				catch
				{
					// D
					// Could not open the directory
				}
			}
			return result;
		}
	}
}
