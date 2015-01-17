using System;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using HtmlAgilityPack;

namespace beastie {
	public class XowaWeb
	{
		string xowaPath = @"D:\ngrams\datasets-xowa\xowa_app_windows_64_v1.10.1.1";
		string xowaBuild = "xowa_windows_64";
		string xowaPort = "8080";
		string xowaJava = "java";
		//TODO: / or \ separator depending on OS
		string xowaArgs = @"-jar {0}\{1}.jar --app_mode http_server --http_server_port {2}";


		private Process cmd;

		public XowaWeb() {

		}

		public void EnWiktionaryPage(string name) {
			string template = @"http://localhost:{0}/en.wiktionary.org/wiki/{1}";

			string url = string.Format(template, xowaPort, Uri.EscapeDataString(name));

			//TODO: switch to more low level HttpWebRequest

			WebClient client = new WebClient ();
			client.Encoding = System.Text.Encoding.UTF8;
			//client.Headers.Add ("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
			//string s = client.DownloadString(url);

			Stream data = client.OpenRead (url);
			StreamReader reader = new StreamReader (data, Encoding.UTF8);
			//string s = reader.ReadToEnd ();
			//MemoryStream output	= new MemoryStream();

			HtmlDocument doc = new HtmlDocument();
			//Encoding.UTF8 ?

			doc.Load(reader);

			//Console.WriteLine(doc.DocumentNode.WriteContentTo());

			data.Close ();
			//reader.Close ();
			//[@lang='en']

			HtmlNodeCollection h2 = doc.DocumentNode.SelectNodes("//h2/span[@class='mw-headline']");
			if (h2 != null) {
				foreach (var langNode in h2) {
					Console.WriteLine(langNode.InnerText.Trim());
					Console.WriteLine(langNode.WriteContentTo());
				}
			}

			//Console.WriteLine(doc.DocumentNode.InnerText);

			// *[@id="mw-content-text"]
			// *[@id="English"]

		}


		public void StartWebService() {
			string args = string.Format(xowaArgs, xowaPath, xowaBuild, xowaPort);

			ProcessStartInfo cmdsi = new ProcessStartInfo ();
			cmdsi.FileName = xowaJava;
			cmdsi.Arguments = args;
			Console.WriteLine("Running: " + cmdsi.FileName + " " + cmdsi.Arguments);
			Process cmd = Process.Start(cmdsi);

			System.Threading.Thread.Sleep(3000); // sleep 3 seconds
		}

		public void KillWebService() {
			if (cmd != null) {
				cmd.Kill(); // TODO: be kinder
			}
			cmd = null;
		}

	}
}