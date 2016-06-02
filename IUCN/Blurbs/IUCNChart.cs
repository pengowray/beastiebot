using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {
    class IUCNChart : Blurb {

        //TODO: maybe make this an extension method?

        static string SliceText(TaxonNode node, RedStatus limitedStatus, Dictionary<RedStatus, int> statuses, bool noLink = false) {
            string sliceTemplateNoLink = "({0} : {1} : {2}) ";
            string sliceTemplateWithLink = "({0} : {1} : {2} : {3}) ";

            // defaults:
            string shortCaption = limitedStatus.TextWithRecently().UpperCaseFirstChar() + " (" + limitedStatus.ToString() + ")";
            int count = statuses[limitedStatus];
            string color = limitedStatus.HexColor();
            string listName = node.nodeName.ListOf(limitedStatus);

            //override defaults
            if (limitedStatus == RedStatus.EX) {
                shortCaption = "Extinct, since 1500";
            } else if (limitedStatus == RedStatus.CR) {
                count = statuses[RedStatus.CR] + statuses[RedStatus.PE] + statuses[RedStatus.PEW];
            } else if (limitedStatus == RedStatus.NT) {
                if (statuses[RedStatus.CD] > 0) {
                    count = statuses[RedStatus.NT] + statuses[RedStatus.CD];
                    shortCaption = "Near threatened (NT, LR/cd)";
                }
            }

            string captionName = node.nodeName.Adjectivize(false, false, "species", "in");
            string percent = Percent(count, statuses.Values.Sum()); // TODO: optimize: don't re-sum this every time. TODO: also ignore any in NE (shouldn't be any)
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

            //(77 : Extinct(since 1500) : #000 : [[link|link text / caption]]) 

            //TODO: make neater
            slices += SliceText(node, RedStatus.EX, statuses);
            slices += SliceText(node, RedStatus.EW, statuses);
            slices += SliceText(node, RedStatus.CR, statuses);
            slices += SliceText(node, RedStatus.EN, statuses);
            slices += SliceText(node, RedStatus.VU, statuses);
            slices += SliceText(node, RedStatus.NT, statuses);
            slices += SliceText(node, RedStatus.LC, statuses);
            slices += SliceText(node, RedStatus.DD, statuses);

            string chart_top = @"{{Image frame
|width = 220
|align=right
|pos=bottom
|content=<div style=""background-color: #F9F9F9; font-size: 75%; text-align: left;"">
{{ #invoke:Chart | pie chart
| title = " + node.name + @" (IUCN, " + FileConfig.Instance().iucnRedListFileShortDate + @")
| radius = 110
| units suffix = _species
| slices = "; // (77 : Extinct(since 1500) : #000) ( 2 : Extinct in the wild : #FFF ) ( 213 : Critically endangered (CR): #cc3333 ) ( 477 : Endangered (EN): #cc6633 ) ( 509 : Vulnerable (VU): #cc9900 ) ( 319 : Near threatened : #99cc99 ) ( 3117 : Least concern  : #006666 ) ( 799 : Data deficient : #aaa ) }}

            int fullyAssessed = statuses[RedStatus.EX] + statuses[RedStatus.EW] +
                statuses[RedStatus.PE] + statuses[RedStatus.PEW] +
                statuses[RedStatus.CR] + statuses[RedStatus.EN] + statuses[RedStatus.VU] +
                statuses[RedStatus.LC] + +statuses[RedStatus.CD] + statuses[RedStatus.NT];

            int evaluated = fullyAssessed + statuses[RedStatus.DD];

            int threatened = statuses[RedStatus.CR] + statuses[RedStatus.EN] + statuses[RedStatus.VU]
                + statuses[RedStatus.PE] + statuses[RedStatus.PEW];

            int notthreatened = statuses[RedStatus.LC] + statuses[RedStatus.CD] + statuses[RedStatus.NT];
            string notthreatenedText = (statuses[RedStatus.CD] > 0) ? "(LC, NT, LR/cd)" : "(LC, NT)";

            int EXOrEW_lowerbound = statuses[RedStatus.EX] + statuses[RedStatus.EW];
            int EXOrEW_upperbound = EXOrEW_lowerbound + statuses[RedStatus.PEW] + statuses[RedStatus.PE];
            string EXOrEW = string.Empty;
            if (EXOrEW_lowerbound == EXOrEW_upperbound) {
                EXOrEW = string.Format("{0} are extinct or extinct in the wild <small>(EX, EW)</small>", FormatNum(EXOrEW_lowerbound));
            } else {
                EXOrEW = string.Format("{0} to {1} are extinct or extinct in the wild <small>(EX, EW, CR(PE), CR(PEW))</small>", FormatNum(EXOrEW_lowerbound), FormatNum(EXOrEW_upperbound));
            }
            
            string chart_bot = @"
}}</div>
|caption='''" + captionName + @"''' (IUCN, " + FileConfig.Instance().iucnRedListFileShortDate + @")
* " + FormatNum(evaluated) + @" species have been evaluated
* " + FormatNum(fullyAssessed) + @" are fully assessed <small>(excludes [[Data deficient|DD]])</small>
* " + FormatNum(notthreatened) + @" are not threatened at present <small>" + notthreatenedText + @"</small>
* " + FormatNum(threatened) + @" are threatened <small>(CR, EN, VU)</small>
* " + EXOrEW + @"}}";


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
