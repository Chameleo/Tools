using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;

using dict = System.Collections.Generic.Dictionary<string, object>;
namespace Stats
{
	// Generates SQL update from stats contained in JSON data in file stats.txt
	class Program
	{
		static void Main(string[] args)
		{

			string input = String.Join("\r\n", File.ReadAllLines("stats.txt"));
			var json = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
			dict stats = (dict)json.DeserializeObject(input);
			dict races = (dict)stats["race"];
			dict combo = (dict)stats["combo"];
			StreamWriter outp = File.CreateText("player_stats.sql");
			StreamWriter outp2 = File.CreateText("player_classlevelstats.sql");
			for (int r = 1; r <= 11; r++)
			{
				if(r == 9)
					continue;
				Object[] raceStats = (Object[])races[r.ToString()];
				for (int c = 1; c <= 11; c++)
				{
					if (c == 10)
						continue;
					dict classStats = (dict)combo[c.ToString()];
					int l = 10;
					if (c == 6)
					{
						l = 55; // DK start at 55
					}
					for(; l <=85; l++)
					{
						Object[] lvlStats = (object[])classStats[l.ToString()];
						// classes with no mana
						if ((int)lvlStats[6] == 100)
						{
							lvlStats[6] = 0;
						}
						outp.WriteLine("replace into player_levelstats VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}');",
							r, c, l, Sum(lvlStats[0], raceStats[0]), Sum(lvlStats[1], raceStats[1]),
							Sum(lvlStats[2], raceStats[2]), Sum(lvlStats[3], raceStats[3]), Sum(lvlStats[4], raceStats[4]));
						if (r == 1)	// these are race independent
						{
							outp2.WriteLine("replace into player_classlevelstats VALUES ('{0}', '{1}', '{2}', '{3}');",
							c, l, lvlStats[5], lvlStats[6]);
						}
					}
				}
			}
			outp.Close();
			outp2.Close();
		}

		static int Sum(object a, object b)
		{
			return (int)a + (int)b;
		}
	}
}
