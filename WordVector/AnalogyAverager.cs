using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word2vec.Tools;
using SF.Snowball.Ext;

namespace beastie.WordVector {
    class AnalogyAverager {
        NamedVocabulary namedVocab; // todo: set on init
        int numberOfPairs = 0; // todo: public readable
        Representation total; // todo: public readable
        Representation _average; // access with Average()
        HashSet<string> sourceWordStems = new HashSet<string>();
        EnglishStemmer stemmer;

        public void AddPair(string neatWord1, string neatWord2) {
            string unneat1 = namedVocab.Unneaten(neatWord1);
            string unneat2 = namedVocab.Unneaten(neatWord2);
            var rep1 = namedVocab.vocab.GetRepresentationOrNullFor(unneat1);
            var rep2 = namedVocab.vocab.GetRepresentationOrNullFor(unneat2);
            if (rep1 != null && rep2 != null) {
                if (total == null) {
                    //total = new Representation(rep.NumericVector.ToArray());
                    total = rep2.Substract(rep1);
                    numberOfPairs = 1;
                } else {
                    total = total.Add(rep2.Substract(rep1));
                    numberOfPairs++;
                }
                AddExclusion(neatWord1);
                AddExclusion(neatWord2);
                
                _average = null;
            }
        }

        void AddExclusion(string neatWord) {
            sourceWordStems.Add(Stem(neatWord));
        }

        string Stem(string neatWord) {
            if (stemmer == null) {
                stemmer = new SF.Snowball.Ext.EnglishStemmer(); // Porter2
            }

            string word = neatWord.ToLowerInvariant();
            stemmer.SetCurrent(word);
            bool success = stemmer.Stem();
            if (success == true) {
                return stemmer.GetCurrent();
            } else {
                return word;
            }

        }
        public Representation Average() {
            if (_average == null)
                _average = total.Normalize();

            return _average;
        }

        public WordRepresentation[] ApplyReverseAnalogy(string neatWord2) {
            return null;
        }

        public WordDistance[] ApplyAnalogy(string neatWord1, int count=50) {
            string unneat1 = namedVocab.Unneaten(neatWord1);
            var rep = namedVocab.vocab.GetRepresentationOrNullFor(unneat1);
            if (rep == null)
                return null;

            var words = namedVocab.vocab.Nearest(rep.Add(Average()), count);
            return words.Where(w => !sourceWordStems.Contains(Stem(w.Representation.Word))).ToArray();

        }

    }
}
