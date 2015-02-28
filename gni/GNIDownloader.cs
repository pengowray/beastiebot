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
			//char start = 'p';
			char start = 'a';

			for (char c = start; c <= 'z'; c++)
				for (char d = 'a'; d <= 'z'; d++)
					for (char e = 'a'; e <= 'z'; e++)
						yield return new string(new char[] { c, d, e });
		}

		// replaces "{0}" in uriTemplate with AAA to ZZZ
		public void ReadUrisAAAZZZ() {
			int maxRetries = 10;
			string outputFilepath = FileConfig.Instance().gniDownloadFile;
			var output = new StreamWriter(outputFilepath, false, Encoding.UTF8);
			using (output) {
				WriteHeader(output);
				foreach (string s in GenerateAAAZZZ()) {
					int retries = 0;
					bool success = false;
					int currentRetryTime = 2; // start at 2 second but double each fail
					while (!success) {
						var buffer = new StringWriter();
						//success = ReadGNIStrings(s, output);
						success = ReadGNIStrings(s, buffer);
						if (success) {
							output.Write(buffer.ToString());
							output.Flush();

						} else {
							retries++;
							if (retries < maxRetries) {
								Console.Error.WriteLine("Retrying in {0} seconds... {1} of {2}", currentRetryTime, retries, maxRetries);
								System.Threading.Thread.Sleep(currentRetryTime * 1000);
								currentRetryTime *= 2;
							} else {
								Console.Error.WriteLine("Failed.");
								return;
							}
						}
					}
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
		public bool ReadGNIStrings(string index, TextWriter output = null) {
			// http://gni.globalnames.org/name_strings.json?search_term=ns%3Azxz%2A&per_page=1000
			// zero-entry page example: {"name_strings":null,"per_page":1000,"page_number":1,"name_strings_total":0}
			int pageNumber = 1;
			int strings_total = -1;
			bool isNextPage = true;
			int totalPages = -1;

			if (output == null) {
				output = Console.Out;
			}

			while (isNextPage) {
				string uriString = string.Format(urlTemplate, index, pageNumber);
				if (totalPages == -1) {
					Console.Error.WriteLine("{0}: page:{1}", index, pageNumber);
				} else {
					Console.Error.WriteLine("{0}: page:{1} of {2}", index, pageNumber, totalPages);
				}
				Console.Error.WriteLine(uriString);

				//TODO: handle errors and timeouts, e.g. https://stackoverflow.com/questions/2269607/how-to-programmatically-download-a-large-file-in-c-sharp

				//TODO: allow caching/saving resulting file

				try {
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

						if (json == null) {
							// fail, retry.
							return false;
						}

						if (pageNumber == 1) {
							strings_total = json.name_strings_total;
							if (json.name_strings_total == 0) {
								// ok, nothing to capture
								return true;
							}

						} else {
							if (strings_total != json.name_strings_total) {
								// fail! count has changed
								return false;
							}
						}

						if (json.name_strings != null) {
							foreach (var item in json.name_strings) {
								output.WriteLine("{0},{1}", item.id, item.name.CsvEscapeSafe()); // escape C# style, and then quote for CSV.  was: CsvEscape(true)
							}	
						}

						// gni docs say, but it isn't true: "Search will return ‘next_page’ field if more data exist for the search"
						int covered = json.per_page * json.page_number;
						isNextPage = (covered < json.name_strings_total);
						totalPages = (int)Math.Ceiling((double)(json.name_strings_total) / json.per_page);

						//Console.WriteLine("{0},{1},{2}", json.per_page, json.page_number, json.name_strings_total);
						//Console.WriteLine("Covered so far: {0}. More? {1}", covered, isNextPage);

						pageNumber++;
					}

				} catch (Exception e) {
					Console.Error.WriteLine(e.Message);
					return false;
				}
			}
			return true;
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
		public long id; // actually, will always fit in an in, but whatever

		[DataMember]
		public string name;
	}
}

