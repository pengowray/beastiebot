using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word2vec.Tools;

namespace beastie.WordVector {
    public class SimilarWords {
        public string word;
        public SimpleWordDistance[] distances;

        public SimilarWords() {
        }

        public SimilarWords(string word, WordDistance[] similar) {
            this.word = word;
            if (similar == null)
                return;

            distances = similar.Select(s => new SimpleWordDistance(s)).ToArray();
        }
    }

    public class SimpleWordDistance {
        public string word;
        public double distance;

        public SimpleWordDistance() {
        }

        public SimpleWordDistance(string word, double distance) {
            this.word = word;
            this.distance = distance;
        }

        public SimpleWordDistance(WordDistance wd) {
            this.word = wd.Representation.Word;
            this.distance = wd.Distance;
        }
    }
}
