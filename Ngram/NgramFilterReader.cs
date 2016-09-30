using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace beastie {
    public class NgramFilterReader {

        public static IEnumerable<IEnumerable<Ngram>> ReadGroupedFiltered(StreamReader stream, int minYear) {
            foreach (var ngrams in ReadGrouped(stream)) {
                var filtered = ngrams.Where(n => n.year > minYear);

                yield return filtered;
            }
        }

        /// <summary>
        /// Returns lists of ngrams with the same lemma, i.e. all years for each ngram lemma
        /// </summary>
        /// public static IEnumerable<IEnumerable<Ngram>> ReadGrouped(StreamReader stream) {
        public static IEnumerable<List<Ngram>> ReadGrouped(StreamReader stream) {
            string currentLemma = null;
            List<Ngram> ngrams = new List<Ngram>();
            bool loop = true;
            using (stream) {
                while (loop) {
                    string line = stream.ReadLine();

                    if (line == null) {
                        if (ngrams.Count > 0) {
                            //yield return ngrams.AsEnumerable();
                            yield return ngrams;
                        }

                        loop = false;

                    } else {

                        Ngram ngram = new Ngram(line);

                        if (ngram.lemmaText != currentLemma) {
                            if (ngrams.Count > 0) {
                                //yield return ngrams.AsEnumerable();
                                yield return ngrams;
                                ngrams.Clear();
                            }
                            currentLemma = ngram.lemmaText;
                        }

                        ngrams.Add(ngram);
                    }
                    
                }
            }
        }


    }
}
