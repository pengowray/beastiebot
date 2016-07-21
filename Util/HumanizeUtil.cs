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
        public static string ToNewspaperQualtities(this IEnumerable<Tuple<int, string>> countsOfThings, bool excludeZeros = true) {
            if (!init) Init();

            //return countsOfThings.Select(t => t.Item2.ToQuantity(t.Item1)).Humanize();
            return countsOfThings.Where(t => t.Item1 > 0).Select(t => t.Item2.ToNewspaperQuantity(t.Item1)).Humanize();

            //test:
            //Console.WriteLine(new string[] { "species".ToQuantity(1), "variety".ToQuantity(2), "subspecies".ToQuantity(1) }.Humanize());
            //Console.WriteLine(new string[] { "species".ToQuantity(1, ShowQuantityAs.Words), "variety".ToQuantity(2, ShowQuantityAs.Words), "subspecies".ToQuantity(1, ShowQuantityAs.Words) }.Humanize());
        }

        public static string ToNewspaperQualtities(this IEnumerable<TaxoSection> sections) {
            return sections
                .Where(t => t.isDefault || t.list.Count() > 0)
                .Select(t => t.title.ToLowerInvariant().ToNewspaperQuantity(t.list.Count())) //TODO: maybe make titles non-title case?
                .Humanize();
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