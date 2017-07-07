using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word2vec.Tools;

namespace beastie.WordVector
{
    public class TestAnnoy
    {
        public void AnnoyTests()
        {
            var annoyVocab = NamedVocabulary.LoadNamed(VocabName.glove_twitter_27B_200d_annoy500, false);
            FastVocabulary vocabulary = (FastVocabulary)annoyVocab.vocab;

            int count = 15;
            string boy = "boy";
            //string girl = "girl";
            //string woman = "woman";

            Console.WriteLine("top " + count + " closest to \"" + boy + "\" words:");
            var closest = vocabulary.Nearest(boy, count);

            /* Is simmilar to:
            * var closest = vocabulary[boy].GetClosestFrom(vocabulary.Words.Where(w => w != vocabulary[boy]), count);
            */
            foreach (var neightboor in closest)
                Console.WriteLine(neightboor.Representation.Word + "\t\t" + neightboor.Distance);

            string[] someWords = "teenager kitten cause declaration potato ladder grow rope fish stick cake hope murder".Split(' ');

            //log.Log("Distance: teenager");
            vocabulary.limitTrees = 50;
            foreach (var word in someWords)
            {
                var rep = vocabulary[word];
                if (rep != null)
                {
                    closest = vocabulary.Nearest(rep, count);
                    Console.WriteLine("top " + count + " closest to \"" + word + "\" words "); // (of first 100k words):");
                    foreach (var neightboor in closest)
                        Console.WriteLine(neightboor.Representation.Word + "\t\t" + neightboor.Distance);

                }
                else
                {
                    Console.WriteLine("failed to find: " + word);
                }
            }

            vocabulary.limitTrees = 500;
            foreach (var word in someWords)
            {
                var rep = vocabulary[word];
                if (rep != null)
                {
                    closest = vocabulary.Nearest(rep, count);
                    Console.WriteLine("top " + count + " closest to \"" + word + "\" words "); // (of first 100k words):");
                    foreach (var neightboor in closest)
                        Console.WriteLine(neightboor.Representation.Word + "\t\t" + neightboor.Distance);

                }
                else
                {
                    Console.WriteLine("failed to find: " + word);
                }
            }

        }
    }
}
