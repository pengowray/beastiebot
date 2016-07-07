using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {
    class IUCNChart : Blurb {

        //TODO: maybe make this an extension method?

        static string SliceText(TaxonNode node, RedStatus limitedStatus, Dictionary<RedStatus, int> statuses, int total, bool noLink = false) {
            string sliceTemplateNoLink = "({0} : {1} : {2}) ";
            string sliceTemplateWithLink = "({0} : {1} : {2} : {3}) ";

            // defaults:
            string shortCaption = limitedStatus.TextWithRecently().UpperCaseFirstChar() + " (" + limitedStatus.ToString() + ")";
            int count = statuses[limitedStatus];
            string color = limitedStatus.HexColor();
            string listName = node.nodeName.ListOf(limitedStatus);

            //override defaults
            if (limitedStatus == RedStatus.EX) {
                shortCaption = "Extinct, since 1500 (EX)";
            } else if (limitedStatus == RedStatus.CR) {
                count = statuses[RedStatus.CR] + statuses[RedStatus.PE] + statuses[RedStatus.PEW];
            } else if (limitedStatus == RedStatus.NT) {
                if (statuses[RedStatus.CD] > 0) {
                    count = statuses[RedStatus.NT] + statuses[RedStatus.CD];
                    shortCaption = "Near threatened (NT, LR/cd)";
                }
            }

            string captionName = node.nodeName.Adjectivize(false, false, "species", "in");
            string percent = Percent(count, total);
            string longCaption = string.Format("{0} {1} {2} ({3})", count, limitedStatus.Text(), captionName, percent);
            string link = "[[" + listName + "|" + longCaption + "]]";

            string slice = string.Format(noLink ? sliceTemplateNoLink : sliceTemplateWithLink, 
                count,
                shortCaption,
                color,
                link);

            return slice;
        }

        public static string Text(TaxonNode node) { // was: DeepBitriStatusGraph()
            var statuses = node.DeepBitriStatusCountWhereWithZeroes(bt => bt.isSpecies);
            string captionName = node.nodeName.Adjectivize(false, true, "species", "in").UpperCaseFirstChar();

            string slices = "";

            int fullyAssessed = statuses[RedStatus.EX] + statuses[RedStatus.EW] +
    statuses[RedStatus.PE] + statuses[RedStatus.PEW] +
    statuses[RedStatus.CR] + statuses[RedStatus.EN] + statuses[RedStatus.VU] +
    statuses[RedStatus.LC] + +statuses[RedStatus.CD] + statuses[RedStatus.NT];

            int evaluated = fullyAssessed + statuses[RedStatus.DD];

            int extant = evaluated - statuses[RedStatus.EX]; // for percentages
            int extantEvaluated = extant;
            int extantFullyAssessed = fullyAssessed - statuses[RedStatus.EX];

            int threatened = statuses[RedStatus.CR] + statuses[RedStatus.EN] + statuses[RedStatus.VU]
                + statuses[RedStatus.PE] + statuses[RedStatus.PEW];

            // http://www.iucnredlist.org/about/summary-statistics#How_many_threatened
            int threatenedUpperEstimate = threatened + statuses[RedStatus.DD];

            int notthreatened = statuses[RedStatus.LC] + statuses[RedStatus.CD] + statuses[RedStatus.NT];
            string notthreatenedText = (statuses[RedStatus.CD] > 0) ? "NT, LR/cd, LC." : "NT and LC.";

            int EXOrEW_lowerbound = statuses[RedStatus.EX] + statuses[RedStatus.EW];
            int EXOrEW_upperbound = EXOrEW_lowerbound + statuses[RedStatus.PEW] + statuses[RedStatus.PE];
            string EXOrEW = string.Empty;
            string ExMsg = string.Empty;
            if (statuses[RedStatus.EX] > 0) {
                //ExMsg = "The chart omits extinct (EX) species. There " + (statuses[RedStatus.EX] > 1 ? "are " : "is ") + FormatNum(statuses[RedStatus.EX]) + " in this category. ";
                ExMsg = "{{efn|Chart omits extinct (EX) species|group=ic}}";
            }
            //string ExEwCounts = "EX: " + statuses[RedStatus.EX] + ExMsg + "; EW: " + statuses[RedStatus.EW] + ". ";

            if (EXOrEW_lowerbound == EXOrEW_upperbound) {
                EXOrEW = FormatNum(EXOrEW_lowerbound) + @" are extinct or extinct in the wild";
            } else {
                EXOrEW = FormatNum(EXOrEW_lowerbound) + @" to " + FormatNum(EXOrEW_upperbound) + @" are extinct or extinct in the wild";
                //+ "{{efn|" + ExEwCounts + @"Upper estimate includes critically endangered species tagged as ''possibly extinct'' (" + statuses[RedStatus.PE] + " species) and ''possibly extinct in the wild'' (" + statuses[RedStatus.PEW] + " species).|group=ic}}";
            }
            if (EXOrEW_upperbound > 0) {
                EXOrEW += @":
** " + statuses[RedStatus.EX] + @" extinct <small>(EX)</small> species" + ExMsg + @"
** " + statuses[RedStatus.EW] + @" extinct in the wild <small>(EW)</small>
** " + statuses[RedStatus.PE] + @" possibly extinct <small>[CR(PE)]</small>
** " + statuses[RedStatus.PEW] + @" possibly extinct in the wild <small>[CR(PEW)]</small>";
            } else { 
                EXOrEW += ".";
            }

            // {{efn|Footnote 3}}
            // ==Notes==
            // {{notelist}}

            int total = extant; //  statuses.Values.Sum() - statuses[RedStatus.EX];

            //(77 : Extinct(since 1500) : #000 : [[link|link text / caption]]) 
            //TODO: make neater
            // slices += SliceText(node, RedStatus.EX, statuses); // exclude EX: only do "proportion of extant" ala IUCN's summary stats, http://www.iucnredlist.org/about/summary-statistics#How_many_threatened
            slices += SliceText(node, RedStatus.EW, statuses, total);
            slices += SliceText(node, RedStatus.CR, statuses, total);
            slices += SliceText(node, RedStatus.EN, statuses, total);
            slices += SliceText(node, RedStatus.VU, statuses, total);
            slices += SliceText(node, RedStatus.NT, statuses, total);
            slices += SliceText(node, RedStatus.LC, statuses, total);
            slices += SliceText(node, RedStatus.DD, statuses, total);

            string chart_top = @"{{Image frame
|width = 230
|align=right
|pos=bottom
|content=<div style=""background-color: #F9F9F9; font-size: 75%; text-align: left;"">
{{ #invoke:Chart | pie chart
| title = Extant " + node.name + @" (IUCN, " + FileConfig.Instance().iucnRedListFileShortDate + @")
| radius = 110
| units suffix = _species
| slices = "; // (77 : Extinct(since 1500) : #000) ( 2 : Extinct in the wild : #FFF ) ( 213 : Critically endangered (CR): #cc3333 ) ( 477 : Endangered (EN): #cc6633 ) ( 509 : Vulnerable (VU): #cc9900 ) ( 319 : Near threatened : #99cc99 ) ( 3117 : Least concern  : #006666 ) ( 799 : Data deficient : #aaa ) }}

            string chart_bot = @"
}}</div>
|caption='''" + captionName + @"''' (IUCN, " + FileConfig.Instance().iucnRedListFileShortDate + @")
* " + FormatNum(extantEvaluated) + @" extant species have been evaluated
* " + FormatNum(extantFullyAssessed) + @" of those are fully assessed{{efn|excludes [[data deficient]] evaluations.|group=ic}}
* " + FormatNum(notthreatened) + @" are not threatened at present{{efn|" + notthreatenedText + @"|group=ic}}
* " + FormatNum(threatened) + @" to " + FormatNum(threatenedUpperEstimate) + @" are threatened{{efn|Threatened comprises CR, EN and VU. Upper estimate additionally includes DD.|group=ic}}
* " + EXOrEW + @"
----
<small>{{notelist|group=ic}}</small>
}}";

/*
* 80 to 110 are extinct or extinct in the wild:
** 78 extinct <small>(EX)</small> species{{efn|omitted from chart|group=ic}}
** 2 extinct in the wild <small>(EW)</small>
** 30 possibly extinct <small>[CR(PE)]</small>
** 0 possibly extinct in the wild <small>[CR(PEW)]</small>
*/

// todo: use a group for notes? {{notelist|group=beastieChart}}

            // "fully assessed" is called "adequate data" in 2001-categories-criteria (http://www.iucnredlist.org/technical-documents/categories-and-criteria/2001-categories-criteria)

            //* " + threatened + @" are threatened (CR, EN, VU) — x%
            //*" + statuses[RedStatus.PE] + @" are considered possibly extinct — x % of CR, y % of total
            //* " + statuses[RedStatus.PEW] + @" are considered possibly extinct in the wild — x % of CR, y % of total

            string chart = chart_top + slices + chart_bot;

            //statuses.Keys.JoinStrings(" : "),
            //statuses.Values.Select(i => i.ToString()).JoinStrings(" : ");

            return chart;
        }

        static string FormatNum(int number) {
            if (number < 10000) {
                // e.g. 5502
                return number.ToString();
            } else {
                // e.g. 14,462
                return number.ToString("N0");
            }
        }

    }
}
