using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Humanizer.Inflections;
using Humanizer;
using System.Globalization;

namespace beastie {
    public static class HumanizeUtil {

        private static bool init = false;
        private static CultureInfo english;

        private static void Init() {
            Vocabularies.Default.AddIrregular("species", "species"); // fixes "subspecies" plural
            english = new CultureInfo("en-US");

            init = true;
        }

        //TODO: option to use &nbsp;
        // "newspaper" refers to the style guide used by most newspapers, which use words for numbers one to ten, and then numerals for larger numbers
        // I don't know if there's a better name for this style, but I'm calling it "newspaper" here.
        public static string ToNewspaperQuantity(this string thing, int quantity) {
            if (!init) Init();

            if (quantity >= 0 && quantity <= 10) {
                //TODO: set language (English?)
                return thing.ToQuantity(quantity, ShowQuantityAs.Words);

            } else if (quantity >= 10000) {
                // e.g. 14,462
                //return quantity.ToString("N0");
                return thing.ToQuantity(quantity, "N0", english);

            } else {
                //return quantity.ToString();
                return thing.ToQuantity(quantity);
            }

        }

        // make a list
        // converts 
        // from { { 1, "apple"}, {2, "banana"}, {30000, "biscuit"} } 
        // to "one apple, two bananas, and 30,000 biscuits"
        // prependIsAre: "is 1 apple" or "are 2 bananas"
        public static string ToNewspaperQualtities(this IEnumerable<Tuple<int, string>> countsOfThings, bool excludeZeros = true, bool prependIsAre = false) {
            if (!init) Init();

            //without excludeZeros or prependIsAre: return countsOfThings.Where(t => t.Item1 > 0).Select(t => t.Item2.ToNewspaperQuantity(t.Item1)).Humanize();

            var filteredList = excludeZeros ? countsOfThings.Where(t => t.Item1 > 0) : countsOfThings;
            if (filteredList.Count() == 0)
                return string.Empty;

            string isAre = string.Empty;
            if (prependIsAre) {
                isAre = filteredList.First().Item1 == 1 ? "is " : "are ";
            }

            return isAre + filteredList.Select(t => t.Item2.ToNewspaperQuantity(t.Item1)).Humanize();

            //test:
            //Console.WriteLine(new string[] { "species".ToQuantity(1), "variety".ToQuantity(2), "subspecies".ToQuantity(1) }.Humanize());
            //Console.WriteLine(new string[] { "species".ToQuantity(1, ShowQuantityAs.Words), "variety".ToQuantity(2, ShowQuantityAs.Words), "subspecies".ToQuantity(1, ShowQuantityAs.Words) }.Humanize());
        }

        public static string ToNewspaperQualtities(this IEnumerable<TaxoSection> sections, bool alwaysIncludeDefault = false, bool prependIsAre = true) {
            var filteredList = sections.Where(t => (alwaysIncludeDefault && t.isDefault) || t.list.Count() > 0);

            string isAre = string.Empty;
            if (prependIsAre) {
                isAre = filteredList.First().list.Count() == 1 ? "is " : "are ";
            }

            return isAre + filteredList
                .Select(t => t.title.ToLowerInvariant().ToNewspaperQuantity(t.list.Count())) //TODO: maybe make titles non-title case?
                .Humanize();
        }

        public static string ToNewspaperSentence(this TaxonNode node, RedStatus status = RedStatus.None, bool alwaysIncludeDefault = false) {
            var sections = node.GetSections(status);
            var filteredList = sections.Where(t => (alwaysIncludeDefault && t.isDefault) || t.list.Count() > 0);

            string assessedAs = string.Empty;
            if (status == RedStatus.DD) {
                assessedAs = " evaluated as " + status.Text();
            }  else if (!status.isNull() && status != RedStatus.EXplus) {
                assessedAs = " assessed as " + status.Text();
            } else {
                assessedAs = " evaluated by the IUCN";
            }

            if (filteredList.Count() == 1) {
                // && if (node.nodeName.AdjectiveFormAvailable()) {
                string title = filteredList.First().title.ToLowerInvariant();
                int quantity = filteredList.First().list.Count();
                bool singular = (quantity == 1);
                title = (singular ? title.Singularize(false) : title.Pluralize(false));

                string nounPhrase = node.nodeName.Adjectivize(false, false, title, "in"); // "mammalian species" or "subspecies in the class Mammalia"

                string sentence = string.Format("There {0} {1} {2}{3}. ",
                    (singular ? "is" : "are"),
                    quantity.NewspaperNumber(),
                    nounPhrase,
                    assessedAs
                    );

                return sentence;

            } else {

                string commonName = node.nodeName.CommonNameLower();
                string taxonWithRank = node.nodeName.TaxonWithRank();
                string nounPhrase = (commonName != null ? "of " + commonName : "in " + taxonWithRank);

                string sentence = string.Format("There {0} {1}{2}. ",
                    filteredList.ToNewspaperQualtities(alwaysIncludeDefault, true),
                    nounPhrase,
                    assessedAs);

                return sentence;
            }

            
        }

        // dont think this works
        public static string ToNewspaperQualtities(this string list) {
            if (!init) Init();

            if (string.IsNullOrWhiteSpace(list))
                return string.Empty;

            //List<int> quantities = new List<int>();
            //List<string> things = new List<string>();
            List<Tuple<int, string>> countsOfThings = new List<Tuple<int, string>>();

            var items = list.Split(',');
            foreach (var item in items) {
                if (!item.Contains(' '))
                    continue;

                var halves = item.Split(new char[] { ' ' }, 2);
                int val = 0;
                if (int.TryParse(halves[0], out val)) {
                    //quantities.Add(val);
                    //things.Add(halves[1]);
                    countsOfThings.Add(new Tuple<int, string>(val, halves[1]));
                }
            }

            //return things.ToNewspaperQualtities(quantities);
            return countsOfThings.ToNewspaperQualtities();
        }
    }
}