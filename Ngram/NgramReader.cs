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

//TODO: read book (via Cliff, Democracy guy): the advertised mind, seducing the subconscious
//TODO: also: seductive interaction design
//TODO: create vocabprimer.com

namespace beastie
{

	public class NgramReader {

		protected long _lineCount = 0;

		public NgramReader () {
		}



        // replaces "{0}" in uriTemplate with AA to ZZ
        public void ReadUrisAAZZ(string uriTemplate) {
			foreach (string uri in NgramFileIterator.AAZZ(uriTemplate)) {
				ReadUri(uri);
				Console.WriteLine(uri);
			}
		}

		public void ReadUri(string uriString) {
			// http://storage.googleapis.com/books/ngrams/books/googlebooks-eng-all-2gram-20120701-aa.gz

			//TODO: handle errors and timeouts, e.g. https://stackoverflow.com/questions/2269607/how-to-programmatically-download-a-large-file-in-c-sharp

			//TODO: allow caching/saving resulting file

			WebRequest request = WebRequest.Create(uriString);
			WebResponse response = request.GetResponse();
			using (Stream responseStream = response.GetResponseStream()) {

				if (uriString.EndsWith(".gz")) {
					ReadCompressedStream(responseStream);

				} else {
					ReadStream(responseStream);

				}
			}
		}

		// will handle compressed files ending in .gz
		public void ReadFile(string filename = null) { // ReadFile(string filename)
			if (filename == null) {
				filename = FileConfig.Instance().speciesNgramFile;
			}
			//Console.WriteLine("Reading ngrams from {0}", filename);  

			if (filename.EndsWith(".gz")) {
				//FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
				FileStream stream = new FileInfo(filename).OpenRead();
				ReadCompressedStream(stream);

			} else {
				var input = new StreamReader(filename, Encoding.UTF8);
				Read(input);
			}
		}

		public virtual void ReadStream(Stream stream) {
			var input = new StreamReader(stream, Encoding.UTF8); // Encoding.Unicode?
			Read(input);
		}

        public StreamReader GetCompressedStream(Stream stream) {
            GZipStream gzstream = new GZipStream(stream, CompressionMode.Decompress, true);

            //DeflateStream gzstream = new DeflateStream(stream, CompressionMode.Decompress); // , true?
            return new StreamReader(gzstream, Encoding.UTF8); //Encoding.UNICODE previously because utf8 caused duplicate entry errors?
        }

        public void ReadCompressedStream(Stream stream) {
            Console.Out.WriteLine("reading compressed stream: " + stream);
            var input = GetCompressedStream(stream);
            Read(input);
		}

		public virtual void Read(StreamReader input) {
			Start();

			using (input)  {
				while (true) {
					string line = null;
					try {
						line = input.ReadLine();
					} catch (Exception e) {
						Console.Error.WriteLine("error: " + e);
						break;
					}
					if (line == null) break;
					_lineCount++;

					if (_lineCount % 1000000 == 0)
						Console.WriteLine("processing line {0}: {1}", _lineCount, line);

					ProcessLine(line);
				}
			
				Console.WriteLine("Finished reading. Line count: {0}", _lineCount); // 86,618,505 for 'a' (googlebooks-eng-all-1gram-20120701-a)
				End();
			}
		}
		
		protected virtual void Start() {
		}

		protected virtual void End() {
		}

		public virtual void Close() {

		}

        
		protected virtual void ProcessLine(string line) {

			//Ngram ngram = new Ngram(line);

			// override me

			//example line:
			// avgas_NOUN	1947	20	9
			// lemma TAB year TAB match_count TAB volume_count

		}


    }
}