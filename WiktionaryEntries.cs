using System;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using System.Xml.Linq;

using MySql.Data.MySqlClient;
using ICSharpCode.SharpZipLib.BZip2;

namespace beastie
{
	public class WiktionaryEntries : IEnumerable // <WiktionaryEntry>
	{
		string path;

		public WiktionaryEntries(string path) {
			this.path = path;

		}

		public StreamReader Stream() {
			//TODO: separate helper class to create this stream (and re-use for WiktionaryDatabase too)
			StreamReader stream;
			if (path.EndsWith(".gz")) {
				GZipStream gzstream = new GZipStream(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), CompressionMode.Decompress);
				stream = new StreamReader(gzstream, Encoding.Unicode); //utf8 causes duplicate entry errors in the code this was copied from (WiktionaryDatabase.ImportDatabaseFile)
			} else if (path.EndsWith(".bz2") || path.EndsWith(".bzip2")) {
				Console.WriteLine("bzip stream");
				BZip2InputStream bzstream = new BZip2InputStream(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
				stream = new StreamReader(bzstream, Encoding.UTF8);
			} else {
				stream = new StreamReader(path, Encoding.Unicode);
			}

			// test stream (comment out or it wont work)
			/*
			Console.Write("a line: {0}", stream.ReadLine());
			Console.Write("first 100 chars: ");
			for (int i=0; i<100; i++) {
				int c = stream.Read();
				//Console.Write("{0} ", c);
				Console.Write("{0}", (char)c);
			}
			Console.WriteLine();
			*/

			return stream;
		}


		public void Process() {
			foreach (WiktionaryEntry page in this) {
				Console.WriteLine("**** '{0}'", page.title);
			
				Dictionary<string,string> sections = page.Sections();
				foreach(string heading in sections.Keys) {
					Console.WriteLine("Heading '{0}':\n{1}", heading, sections[heading]);
				}
				//Console.WriteLine("{0}", page.text);
			}
							
		}

		//TODO: move to anotherr class?
		public void TemplateUsageStats(string lang="English") {
			foreach (WiktionaryEntry page in this) {
				Console.WriteLine("**** '{0}'", page.title);
				if (page.Sections().ContainsKey("English")) {
					string englishEntry = page.Sections()["English"];
					WiktionaryEntry.TemplateList( englishEntry );
				}
			}
		}

		public IEnumerator GetEnumerator() { // IEnumerator<WiktionaryEntry>
			StreamReader stream = Stream();
			XmlTextReader reader = new XmlTextReader(stream);
			WiktionaryEntry page = null; // current page
			string currentElement = null;
			
			while (reader.Read())  {
				switch (reader.NodeType) {
				case XmlNodeType.Element: // The node is an Element.
					if (reader.Name == "page") {
						page = new WiktionaryEntry();
						//} else if (currentElement == null && page != null) { // only top level elements inside <page>
					} else if (page != null) { // only top level elements inside <page>
						currentElement = reader.Name;
					}
					//Console.Write("<" + reader.Name);
					
					while (reader.MoveToNextAttribute()) { // Read attributes.
						//Console.Write(" " + reader.Name + "='" + reader.Value + "'");
					}
					//Console.WriteLine(">");
					break;
				case XmlNodeType.Text: //Display the text in each element.
					if (currentElement == "title") {
						page.title = reader.Value;
					} else if (currentElement == "text") {
						page.text = reader.Value;
					} else if (currentElement == "sha1") {
						page.sha1 = reader.Value;
					} else if (currentElement == "id") {
						page.id = long.Parse(reader.Value);
					} else if (currentElement == "parentid") {
						page.parentid = long.Parse(reader.Value);
					} else if (currentElement == "ns") {
						page.ns = int.Parse(reader.Value);
					}
					
					
					//Console.WriteLine (reader.Value);
					break;
				case XmlNodeType. EndElement: //Display end of element.
					//Console.Write("</" + reader.Name);
					//Console.WriteLine(">");
					if (reader.Name == "page") {
						// page done. return it (if it's in the main namespace)

						if (page.ns == 0) {

							yield return page;

						}
						
						page = null;

					} else if (currentElement == reader.Name) {
						currentElement = null;
					}
					break;
				}
			}



		
		}

		
		public void PrintXml() {
			StreamReader stream = Stream();
			XmlTextReader reader = new XmlTextReader(stream);
			
			while (reader.Read())  {
				switch (reader.NodeType) {
				case XmlNodeType.Element: // The node is an Element.
					Console.Write("<" + reader.Name);
					
					while (reader.MoveToNextAttribute()) // Read attributes.
						Console.Write(" " + reader.Name + "='" + reader.Value + "'");
					//Console.Write(">");
					Console.WriteLine(">");
					break;
				case XmlNodeType.Text: //Display the text in each element.
					Console.WriteLine (reader.Value);
					break;
				case XmlNodeType. EndElement: //Display end of element.
					Console.Write("</" + reader.Name);
					Console.WriteLine(">");
					break;
				}
			}
			
		}
		

	}

}

