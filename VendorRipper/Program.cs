using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

using dict = System.Collections.Generic.Dictionary<string, object>;
namespace VendorRipper
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("requires npcId as a parameter");
				Console.ReadKey();
                return;
			}
			string npcId = args[0];
            string url;
            if (args.Length > 1)
                url = args[1];
            else
                url = "http://www.wowhead.com/npc=" + npcId;

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
			//Regex r = new Regex(@"new Listview\(\{template: 'item', id: 'sells'.*data: (\[.+\])\}\);");
            Regex r = new Regex(@"new Listview\(\{template: 'item'.*data: (\[.+\])\}\);");
			StreamWriter outp = File.CreateText("npc_vendor_" + npcId + ".sql");
			foreach (string line in content)
			{
				Match m = r.Match(line);
				if (!m.Success)
				{
					continue;
				}
				// found our line
				var json = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
				string data = m.Groups[1].Captures[0].Value;
				data = data.Replace("[,", "[0,");	// otherwise deserializer complains
				object[] items = (object[])json.DeserializeObject(data);
				foreach(dict itemInfo in items)
				{
					try
					{
						int id = (int)itemInfo["id"];
                        int maxcount = 0;
                        if(itemInfo.ContainsKey("avail"))
						    maxcount = (int)itemInfo["avail"];
						if (maxcount < 0)
							maxcount = 0;
                        string name = "";
                        if (itemInfo.ContainsKey("name"))
                            name = (string)itemInfo["name"];
						// todo, figure out extended cost from honor cost
						outp.WriteLine("replace into `npc_vendor`(`entry`,`slot`,`item`,`maxcount`,`incrtime`,`ExtendedCost`) values ( '{0}', '{1}', '{2}', '{3}', '{4}', '{5}'); -- {6}",
								npcId, 0, id, maxcount, 0, 0, name);
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
					}
				}
				
				// should have only one data line
				break;
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
