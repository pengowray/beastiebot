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
	public class WiktionaryEntries
	{
		string path;

		public WiktionaryEntries(string path) {
			this.path = path;
		}

		public void process() {
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

