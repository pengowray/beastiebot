using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Linq;
using System.Data.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RestSharp;
using System.Runtime.Serialization;
using LumenWorks.Framework.IO.Csv;

namespace beastie {
	public class GNIDownloader
	{
		string urlTemplate = @"http://gni.globalnames.org/name_strings.json?search_term=ns%3A{0}%2A&per_page=1000&page={1}"; // page starts at 1. unencoded text is: ns:{0}*
		//string detailsUrl = @"http://gni.globalnames.org/name_strings/{0}.xml"; // 0 = id number.. e.g. http://gni.globalnames.org/name_strings/10108231.xml
		//string topIndex = @"http://gni.globalnames.org/name_strings?expand=R"; // find which pages exist starting with R

		public GNIDownloader() {
		}

		// From AAA to ZZZ
		static IEnumerable<string> GenerateAAAZZZ() {
			char start = 'p';
			//char start = 'a';

			for (char c = start; c <= 'z'; c++)
				for (char d = 'a'; d <= 'z'; d++)
					for (char e = 'a'; e <= 'z'; e++)
						yield return new string(new char[] { c, d, e });
		}

		// replaces "{0}" in uriTemplate with AAA to ZZZ
		public void ReadUrisAAAZZZ() {
			string outputFilepath = FileConfig.Instance().gniDownloadFile;
			var output = new StreamWriter(outputFilepath, false, Encoding.UTF8);
			using (output) {
				WriteHeader(output);
				foreach (string s in GenerateAAAZZZ()) {
					//string uri = string.Format(urlTemplate, s);
					ReadGNIStrings(s, output);
				}
			}
		}

		public void Test() {
			//string index = "THE"; // many entries
			//string index = "RZW"; // should have 0 entries
			string index = "abc"; // should have 0 entries

			ReadGNIStrings(index);
		}

		public void WriteHeader(TextWriter output = null) {
			output.WriteLine("id,name");
		}

		// extracts all name strings from the GNI database (may make multiple requests for paginated pages)
		public void ReadGNIStrings(string index, TextWriter output = null) {
			int pageNumber = 1;
			bool isNextPage = true;

			if (output == null) {
				output = Console.Out;
			}

			while (isNextPage) {
				string uriString = string.Format(urlTemplate, index, pageNumber);
				Console.Error.WriteLine(uriString);

				//TODO: handle errors and timeouts, e.g. https://stackoverflow.com/questions/2269607/how-to-programmatically-download-a-large-file-in-c-sharp

				//TODO: allow caching/saving resulting file

				WebRequest request = WebRequest.Create(uriString);
				WebResponse response = request.GetResponse();

				StreamReader input = null;
				using (Stream responseStream = response.GetResponseStream()) {

					if (uriString.EndsWith(".gz")) {
						GZipStream gzstream = new GZipStream(responseStream, CompressionMode.Decompress, true);
						input = new StreamReader(gzstream, Encoding.UTF8); //or Encoding.UNICODE  ?

					} else {
						input = new StreamReader(responseStream, Encoding.UTF8); // Encoding.Unicode?
					}

					string page = input.ReadToEnd();

					var json = JsonConvert.DeserializeObject<GNIPage>(page);

					// not true: "Search will return ‘next_page’ field if more data exist for the search"
					if (json.name_strings != null) {
						foreach (var item in json.name_strings) {
							bool quoteRegardless = true; // quote even if not needed, because unquoted text makes me nervous
							output.WriteLine("{0},{1}", item.id, item.name.CsvEscape(quoteRegardless));
						}	
					}

					int covered = json.per_page * json.page_number;
					isNextPage = (covered < json.name_strings_total);

					//Console.WriteLine("{0},{1},{2}", json.per_page, json.page_number, json.name_strings_total);
					//Console.WriteLine("Covered so far: {0}. More? {1}", covered, isNextPage);

					pageNumber++;
				}
			}

		}

	}

	[DataContract]
	public class GNIPage {
		// {"name_strings":
		//    [{...,"id":8663701,"name":"Thea assamica J.W. Mast."},{...},...],
		// "per_page":1000,"page_number":1,"name_strings_total":56438}

		[DataMember]
		public List<GNINameStrings> name_strings;

		[DataMember]
		public int per_page;

		[DataMember]
		public int page_number;

		[DataMember]
		public int name_strings_total;
	}

	[DataContract]
	public class GNINameStrings {
		[DataMember]
		public long id;

		[DataMember]
		public string name;
	}
}

