using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;

namespace TrainerRipper
{
	class Program
	{
		// Generates an sql update with spells taught by an npc with specific ID by parsing wowhead.
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("requires npc id as an argument");
				Console.ReadKey();
			}
			string npcId = args[0];
			string url = "http://www.wowhead.com/npc=" + npcId;
			List<string> content;
			try
			{
				content = ReadPage(url);
			}
			catch (Exception e)
			{
				Console.WriteLine("Couldn't read the page: " + e.Message);
				return;
			}
			Regex r = new Regex(@"new Listview\(\{template: 'spell'.*data: \[(.+)\]\}\);");
			StreamWriter outp = File.CreateText("npc_trainer_"+npcId+".sql");
			foreach (string line in content)
			{
				Match m = r.Match(line);
				if (!m.Success)
				{
					continue;
				}
				// found our line
				string[] spells = m.Groups[1].Captures[0].Value.Split(new string[] { "}," }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string spell in spells)
				{
					string str = Regex.Replace(spell, "[\\{\\}\"'\\[\\]]", "");
					string[] fields = str.Split(',');
					Dictionary<string, string> map = new Dictionary<string, string>();
					foreach (string field in fields)
					{
						try
						{
							string[] keyval = field.Split(':');
							map[keyval[0]] = string.IsNullOrEmpty(keyval[1]) ? "0" : keyval[1];
						}
						catch
						{
						}
					}
					if (!map.ContainsKey("id") || !map.ContainsKey("level"))
					{
						continue;
					}
					if (!map.ContainsKey("trainingcost"))
					{
						// let it be free :P
						map["trainingcost"] = "0";
					}
					if (!map.ContainsKey("skill"))
					{
						map["skill"] = "0";
					}
					if (map.ContainsKey("name"))
					{
						outp.WriteLine("# " + map["name"].Replace("@", ""));
					}
					outp.WriteLine("replace into npc_trainer (entry, spell, spellcost, reqskill, reqlevel) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}');",
						npcId, map["id"], map["trainingcost"], map["skill"], map["level"]);
				}
			}
			outp.Close();
		}

		static List<string> ReadPage(string url)
		{
			WebRequest wrGETURL = WebRequest.Create(url);
			Stream objStream = wrGETURL.GetResponse().GetResponseStream();
			StreamReader objReader = new StreamReader(objStream);

			string sLine = "";
			int i = 0;
			List<string> content = new List<string>();
			while (sLine != null)
			{
				i++;
				sLine = objReader.ReadLine();
				if (sLine != null)
					content.Add(sLine);
			}
			return content;
			
		}
	}
}
