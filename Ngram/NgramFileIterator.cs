using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;

namespace beastie {
    public class NgramFileIterator {

        //TODO: move more stuff over from NgramReader

        // From AA to ZZ
        static IEnumerable<string> GenerateAAZZ() {
            for (char c = 'a'; c <= 'z'; c++)
                for (char d = 'a'; d <= 'z'; d++)
                    yield return new string(new char[] { c, d });
        }

        static IEnumerable<char> GenerateAZ() {
            for (char c = 'a'; c <= 'z'; c++)
                yield return c;
        }

        // replaces "{0}" in uriTemplate with aa to zz
        public static IEnumerable<string> AAZZ(string template) {
            foreach (string s in GenerateAAZZ()) {
                string uri = string.Format(template, s);
                yield return uri;
            }
        }

        // replaces "{0}" in uriTemplate with a to z
        public static IEnumerable<string> AZ(string template) {
            foreach (char s in GenerateAZ()) {
                string uri = string.Format(template, s);
                yield return uri;
            }
        }

        public static StreamReader OpenGzip(string path) {
            //if (!path.ToLowerInvariant().EndsWith(".gz"))
            //    return null; //TODO: throw error
            //string nogzPath = path.Substring(0, path.Length - ".gz".Length);
            FileInfo fileToDecompress = new FileInfo(path);
            FileStream originalFileStream = fileToDecompress.OpenRead();
            GZipStream gzstream = new GZipStream(originalFileStream, CompressionMode.Decompress);
            return new StreamReader(gzstream, Encoding.UTF8); //Encoding.UNICODE previously because utf8 caused duplicate entry errors?
        }

        public static StreamReader OpenFile(string filename) {
            if (filename.ToLowerInvariant().EndsWith(".gz")) {
                return OpenGzip(filename);

            } else {
                FileInfo file = new FileInfo(filename);
                FileStream fileStream = file.OpenRead();
                return new StreamReader(fileStream, Encoding.UTF8); //Encoding.UNICODE previously because utf8 caused duplicate entry errors?
            }
        }


        public static IEnumerable<StreamReader> OpenFiles(IEnumerable<string> filenames) {
            foreach (var filename in filenames)
                yield return OpenFile(filename); // ? auto close files when done?
        }
    }
}