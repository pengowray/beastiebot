using beastie.beastieDB;
using beastie.WordNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie.pagebuilder {
    class PageBuilder {
        string templateFile = @"C:\ngrams\templates-qtionary\simpler.html";
        string outputPath = @"C:\ngrams\output-qtionary\";
        string blockTemplate = @"
    <section class=""qa_block"">
      <p class=""q_column"">{0}</p>
      <p class=""a_column"">{1}</p>
    </section>";
        string template = null;

        public void BuildPages() {
            // rare words in Alice: Hjckrrh, Seaography, shan't, needn't, Uglification, ..., uglify, rosetree, ... Eaglet, jurymen, Pennyworth, driest, untwist, dormouse, draggled, buttercup
            string[] interestingWords =
                @"cat, rosetree, buttercup, untwist, dormouse, draggled, driest, bedraggled, acclaim, ugly, heat, horn, hyper, inflict, accent, abandon, dog, tiger, key, keys, Angelina Jolie, Buckingham Palace, letting, world, weapon, artwork, chicken, happy, the, unleash, variant, waiter, uplifting".Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

            // interesting relations: keys -> stereo faceplate
            // ugly -> embaraembarrassing
            // renewable energy -> smart grid, reducing carbon emissions

            foreach (var word in interestingWords) {
                BuildPage(word);
            }

            // all words with word distance data
            foreach (var word in BeastieDatabase.Database().WordDistancesData.Select(d => d.word)) {
                BuildPage(word);
            }



        }

        public void BuildPage(string lemma) {
            if (string.IsNullOrWhiteSpace(lemma))
                return;

            if (lemma.Contains('/') || lemma.Contains('\\') || lemma.Contains(':') || lemma.Contains('*') || lemma.Contains('>') || lemma.Contains('<') || lemma.Contains('.'))
                return; // don't wanna deal with that for now (messes with file names)

            string lemmaLower = lemma.ToLowerInvariant();

            if (lemmaLower.StartsWith("con"))
                return; // Avoid use of "\\.\" in the path. (??)

            if (lemmaLower != lemma && !lemma.Contains(' '))
                return; // single words must be lowercase

            Console.WriteLine("lemma: " + lemma);
            StringBuilder blockText = new StringBuilder();
            bool foundSomewhere = false;

            if (template == null) {
                // load template (TODO: move to other method?)
                FileInfo file = new FileInfo(templateFile);
                FileStream fileStream = file.OpenRead();
                var reader = new StreamReader(fileStream, Encoding.UTF8); //Encoding.UNICODE previously because utf8 caused duplicate entry errors?

                string line = null;
                StringBuilder buffer = new StringBuilder();
                while ((line = reader.ReadLine()) != null) {
                    buffer.AppendLine(line);
                }
                template = buffer.ToString();
            }

            var db = BeastieDatabase.Database();
            string normLemma = BeastieDatabase.NormalizeForWordsData(lemma);
            var distData = db.WordDistancesData.Where(w => w.word == lemma).FirstOrDefault(); //TODO: not just first once
            var similarWords = distData.DeserializeData();
            var similar = similarWords?.distances.Select(d => d.word.Replace("_"," ")).JoinStrings(", ");


            // what to do with morphs? 
            // "aardwolf"	"n"	"aardwolves"
            // "mother superior"	"n"	"mothers superior"
            // "mover and shaker"	"n"	"2631"	"movers and shakers"
            // "overflow"	"v"	"overflown"
            // "sink"	"v"	"sank"
            // "whacky" "a"	"whackier"
            // "whacky" "a" "whackiest"
            // "abide"	"v"	"abode"

            // odd: mink stole (fashion), Mink Stole (actress)

            if (string.IsNullOrEmpty(similar)) {
                //return;

            } else {
                blockText.AppendFormat(blockTemplate, "Which words are found in a similar context?", similar);
                foundSomewhere = true;
            }


            var unetdb = SqlunetDatabase.Database(); // WordNet+
            var wordid = unetdb.words.Where(w => (string)w.lemma == normLemma).Select(w => w.wordid).FirstOrDefault();
            if (wordid != 0) {
                //blockText.AppendFormat(blockTemplate, "Is it in WordNet / Unet?", "Yes."); // TODO

                //var senses = unetdb.senses.Where(w => w.wordid == wordid);
                // "FROM words INNER JOIN senses USING (wordid) INNER JOIN synsets USING (synsetid)"
                var senseQuery = from w in unetdb.words
                                 join sen in unetdb.senses on w.wordid equals sen.wordid
                                 join syn in unetdb.synsets on sen.synsetid equals syn.synsetid
                                 where w.wordid == wordid
                                 select syn;

                int sense = 1;
                foreach (var syn in senseQuery) {
                    blockText.AppendFormat(blockTemplate, "sense " + sense, syn.definition);
                    foundSomewhere = true;

                    //blockText.AppendFormat(blockTemplate, "synonyms" + sense, syn.);

                    sense++;

                    // what types of links are most common?
                    // select linkid, linktypes.link, count(linkid) as count from semlinks join linktypes using (linkid) group by linkid order by count desc;
                    // select linkid, linktypes.link, count(linkid) as count from lexlinks join linktypes using (linkid) group by linkid order by count desc;
                    // examples of a sense:
                    // 
                    // list lexlinks: (word-to-word)
                    /*
                        select linkid, word1.lemma, linktypes.link, word2.lemma
                        from lexlinks 
                        join linktypes using (linkid)
                        join words as word1 on (lexlinks.word1id = word1.wordid) 
                        join words as word2 on (lexlinks.word2id = word2.wordid)
                        order by linkid, word1.lemma, word2.lemma


                        rarer types:
                        where linkid != 81 and linkid != 80 and linkid != 30
                    */
                    // related views:
                    // CREATE VIEW sensesXsemlinksXsenses AS SELECT linkid,s.synsetid AS ssynsetid,s.wordid AS swordid,s.senseid AS ssenseid,s.casedwordid AS scasedwordid,s.sensenum AS ssensenum,s.lexid AS slexid,s.tagcount AS stagcount,s.sensekey AS ssensekey,s.pos AS spos,s.lexdomainid AS slexdomainid,s.definition AS sdefinition,d.synsetid AS dsynsetid,d.wordid AS dwordid,d.senseid AS dsenseid,d.casedwordid AS dcasedwordid,d.sensenum AS dsensenum,d.lexid AS dlexid,d.tagcount AS dtagcount,d.sensekey AS dsensekey,d.pos AS dpos,d.lexdomainid AS dlexdomainid,d.definition AS ddefinition FROM sensesXsynsets AS s INNER JOIN semlinks AS l ON s.synsetid = l.synset1id INNER JOIN sensesXsynsets AS d ON l.synset2id = d.synsetid
                    // CREATE VIEW sensesXlexlinksXsenses AS SELECT linkid,s.synsetid AS ssynsetid,s.wordid AS swordid,s.senseid AS ssenseid,s.casedwordid AS scasedwordid,s.sensenum AS ssensenum,s.lexid AS slexid,s.tagcount AS stagcount,s.sensekey AS ssensekey,s.pos AS spos,s.lexdomainid AS slexdomainid,s.definition AS sdefinition,d.synsetid AS dsynsetid,d.wordid AS dwordid,d.senseid AS dsenseid,d.casedwordid AS dcasedwordid,d.sensenum AS dsensenum,d.lexid AS dlexid,d.tagcount AS dtagcount,d.sensekey AS dsensekey,d.pos AS dpos,d.lexdomainid AS dlexdomainid,d.definition AS ddefinition FROM sensesXsynsets AS s INNER JOIN lexlinks AS l ON s.synsetid = l.synset1id AND s.wordid = l.word1id INNER JOIN sensesXsynsets AS d ON l.synset2id = d.synsetid AND l.word2id = d.wordid
                    // CREATE VIEW sensesXsynsets AS SELECT * FROM senses INNER JOIN synsets USING (synsetid)
                    /*
most common:
"1"	"hypernym"	"89172"
"2"	"hyponym"	"89151"
"40"	"similar"	"21434"
...
least common:
"15"	"substance holonym"	"797"
"16"	"substance meronym"	"797"
"21"	"entail"	"408" // e.g. burglarise -> break in. bury -> cover. canoe (verb) -> paddle,  "cannibalize" ("use parts of something to repair something else") -> dismantle. catch up -> follow. 
"23"	"cause"	"221" // e.g. anger(201789790:"make angry") -> see red,anger(201790925:"become angry"); enrage(201799899:"put into a rage; make violently angry") -> rage(201800044:"feel intense anger")

lexlinks: "slang"	"domain member usage" ____ only 4 slang words senses tagged:
"baddie", "bennie", "cat", "stiff"
*/

                }
            }

            //TODO: sanatize output for html, especially: < > & ;

            //blockText.AppendFormat(blockTemplate, "Which words does it modify??", dependents); // TODO
            //blockText.AppendFormat(blockTemplate, "Which words modify it??", dependencies); // TODO

            if (foundSomewhere == false) {
                Console.WriteLine("Not found: " + lemma);
                return;
            }

            string output = template
                .Replace("{{{word}}}", lemma)
                .Replace("{{{tagline}}}", lemma.ToUpperInvariant())
                .Replace("{{{block}}}", blockText.ToString());
                //.Replace("{{{dependents}}}", "—")
                //.Replace("{{{dependencies}}}", "—")
                //.Replace("{{{similar}}}", similar);

            string outFile = outputPath + lemma + ".html";
            var outWriter = new StreamWriter(outFile, false, Encoding.UTF8);
            outWriter.Write(output);
            outWriter.Close();
        }
    }
}
