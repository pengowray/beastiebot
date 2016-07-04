using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {

    public enum RedStatus { None, Null, Unknown, EX, EW, CR, PE, PEW, EN, VU, CD, NT, LC, DD, NE, EXplus };
    
    public static class RedStatusFunctions {

        // IUCN redlist status (e.g. EN)
        public static RedStatus New(string value) { // was: StatusString.set {}
            if (string.IsNullOrWhiteSpace(value)) {
                return RedStatus.None;
            }

            string v = value.Trim().ToUpperInvariant();
            switch (v) {
                case "EX": case "EXTINCT": return RedStatus.EX;
                case "EW": case "EXTINCT IN THE WILD": return RedStatus.EW;
                case "CR": case "CRITICALLY ENDANGERED": return RedStatus.CR;
                case "PE": case "CR(PE)": case "POSSIBLY EXTINCT": return RedStatus.PE;
                case "CRITICALLY ENDANGERED (POSSIBLY EXTINCT)": return RedStatus.PE;
                case "PEW": case "CR(PEW)": case "PW": case "CR(PW)": case "POSSIBLY EXTINCT IN THE WILD": return RedStatus.PEW;
                case "CRITICALLY ENDANGERED (POSSIBLY EXTINCT IN THE WILD)": return RedStatus.PEW;
                case "EN": case "ENDANGERED": return RedStatus.EN;
                case "VU": case "VULNERABLE": return RedStatus.VU;
                case "CD": case "LR/CD": case "LC/CD": return RedStatus.CD;
                case "CONSERVATION DEPENDENT": case "CONSERVATION-DEPENDENT": return RedStatus.CD;
                case "NT": case "LR/NT": case "LC/NT": case "NEAR THREATENED": return RedStatus.NT;
                case "LC": case "LR/LC": case "LC/LC": case "LEAST CONCERN": return RedStatus.LC;
                case "DD": case "DATA DEFICIENT": return RedStatus.DD;
                case "NE": case "NOT EVALUATED": return RedStatus.NE;
                    // case "NULL": return RedStatus.Null;
            }

            //                case RedStatus.PE: return "critically endangered (possibly extinct)";
            //                case RedStatus.PEW: return "critically endangered (possibly extinct in the wild)";

            return RedStatus.Unknown; //TODO: throw exception?
        }

        public static RedStatus Limited(this RedStatus status) {
            if (status == RedStatus.PE || status == RedStatus.PEW)
                return RedStatus.CR;

            if (status == RedStatus.CD)
                return RedStatus.NT;

            if (status == RedStatus.NE || status == RedStatus.Unknown || status == RedStatus.Null)
                return RedStatus.None;

            if (status == RedStatus.EXplus)
                return RedStatus.EX;

            return status;
        }


        public static bool MatchesFilter(this RedStatus status, RedStatus filterIn) {
            if (filterIn == RedStatus.Null) // no filter
                return true;

            if (filterIn == status) // exact match
                return true;

            if (filterIn == RedStatus.EXplus) 
                return status.isExtinctOrPossiblyExtinct();

            if (filterIn.Limited() == filterIn) {
                if (filterIn == status.Limited()) // e.g. match PE for CR filter (but not other way around)
                    return true;
            }

            return false;
        }

        public static bool isThreatenedOrExtinct(this RedStatus status) {
            RedStatus[] threatenedOrEx = new RedStatus[] { RedStatus.CR, RedStatus.EN, RedStatus.VU,
                RedStatus.EW, RedStatus.EX,
                RedStatus.PE, RedStatus.PEW };

            return threatenedOrEx.Contains(status);
        }

        public static bool isThreatened(this RedStatus status) {
            RedStatus[] threatened = new RedStatus[] { RedStatus.CR, RedStatus.EN, RedStatus.VU,
                // RedStatus.EW, RedStatus.EX,
                RedStatus.PE, RedStatus.PEW };

            return threatened.Contains(status);
        }


        public static bool isExtinctOrPossiblyExtinct(this RedStatus status) {
            RedStatus[] extinctPlus = new RedStatus[] { 
                RedStatus.EW, RedStatus.EX,
                RedStatus.PE, RedStatus.PEW,
                RedStatus.EXplus};

            return extinctPlus.Contains(status);
        }

        public static bool isNull(this RedStatus status) {
            return (status == RedStatus.Null);
        }

        // returns true for DD (DD means not fully evaluated)
        public static bool isEvaluated(this RedStatus status) {
            return !(status == RedStatus.None || status == RedStatus.NE || status == RedStatus.Unknown || status == RedStatus.Null);
        }

        //static private Dictionary<RedStatus, int?> RliValues;

        public static int? RliWeight(this RedStatus status) { // public int? CategoryWeight() {
            switch (status.Limited()) {
                case RedStatus.LC: return 0;
                case RedStatus.NT: return 1; // also LR/cd, LR/nt
                case RedStatus.VU: return 2;
                case RedStatus.EN: return 3;
                case RedStatus.CR: return 4; // also PE, PEW
                case RedStatus.EX: return 5;
                case RedStatus.EW: return 5;
                case RedStatus.None: return null; // also NE, Unknown, Null
                case RedStatus.EXplus: return null; // shouldn't happen
                case RedStatus.DD: return null;
            }

            return null;
        }

        public static string Text(this RedStatus status) {
            switch (status) {
                case RedStatus.LC: return "least concern";
                case RedStatus.NT: return "near threatened";
                case RedStatus.CD: return "conservation dependent";
                case RedStatus.VU: return "vulnerable";
                case RedStatus.EN: return "endangered";
                case RedStatus.CR: return "critically endangered";
                case RedStatus.PE: return "critically endangered (possibly extinct)";
                case RedStatus.PEW: return "critically endangered (possibly extinct in the wild)";
                case RedStatus.EX: return "extinct";
                case RedStatus.EXplus: return "extinct"; // filter which includes PE/PEW/EW
                case RedStatus.EW: return "extinct in the wild";
                case RedStatus.NE: return "not evaluated";
                case RedStatus.DD: return "data deficient";
                case RedStatus.Unknown: return "unknown";
                case RedStatus.None: return null;
                case RedStatus.Null: return null;
            }

            return null; // throw error. should never happen.
        }

        public static string TextWithRecently(this RedStatus status) {
            if (status == RedStatus.EX || status == RedStatus.EXplus)
                return "recently extinct";

            return status.Text();
        }

        /// <returns>Best matching English Wikipedia page title for conservation status</returns>
        public static string WikiPage(this RedStatus status) {
            switch (status) {
                case RedStatus.LC: return "Least-concern species";
                case RedStatus.NT: return "Near-threatened species";
                case RedStatus.CD: return "Conservation-dependent species";
                case RedStatus.VU: return "Vulnerable species";
                case RedStatus.EN: return "Endangered species";
                case RedStatus.CR: return "Critically endangered";
                case RedStatus.PE: return "Critically endangered"; // TODO: create "Possibly extinct" wikipedia page
                case RedStatus.PEW: return "Critically endangered"; // TODO: linkto "Possibly exinct#Possibly extinct in the wild" when it's created
                case RedStatus.EX: return "Extinction";
                case RedStatus.EXplus: return "Extinction";
                case RedStatus.EW: return "Extinct in the wild";
                case RedStatus.NE: return "Not evaluated";
                case RedStatus.DD: return "Data deficient";
                case RedStatus.Unknown: return null;
                case RedStatus.None: return null;
                case RedStatus.Null: return null;
            }

            return null; // throw error. should never happen.

        }

        public static string WikiLink(this RedStatus status) {
            string wikipage = status.WikiPage();
            if (wikipage == null) {
                return status.Text();
            }

            string statusText = status.Text();

            if (statusText.UpperCaseFirstChar() == wikipage) {
                return "[[" + status.Text() + "]]";
            }

            return "[[" + wikipage + "|" + status.Text() + "]]";
        }

        public static string HexColor(this RedStatus status) {
            switch (status) {
                case RedStatus.LC: return "#006666";
                case RedStatus.NT: case RedStatus.CD: return "#99cc99";
                case RedStatus.VU: return "#cc9900";
                case RedStatus.EN: return "#cc6633";
                case RedStatus.CR: case RedStatus.PE: case RedStatus.PEW: return "#cc3333";
                case RedStatus.EX: return "#000";
                case RedStatus.EXplus: return "#000";
                case RedStatus.EW: return "#542344"; //return "#fff";
                case RedStatus.DD: return "#aaa";
                case RedStatus.NE: case RedStatus.Unknown: case RedStatus.None: return "#999"; // should be unused
                case RedStatus.Null: return "#999";
            }

            return null; // should never happen... 
        }

        public static string WikiImage(this RedStatus status) {
            switch (status) {
                case RedStatus.LC: return null; // TODO
                case RedStatus.CD: return "[[File:Status_iucn2.3_CD.svg|thumb|A visualization of the categories in the no-longer used \"IUCN 1994 Categories & Criteria(version 2.3)\", with ''conservation dependent'' (LR/cd) highlighted. The category was folded into the Near Threatened (NT) category in the 2001 revision, but some species which have not been re-evaluated retain the assessment.]]";
                case RedStatus.NT: return "[[File:Status iucn3.1 NT.svg|thumb|Near Threatened (NT) species do not currently qualify for Critically Endangered (CR), Endangered (EN) or Vulnerable (VU), but are likely to qualify for a threatened category in the near future, or are already close to qualifying.]]";
                case RedStatus.VU: return "[[File:Status iucn3.1 VU.svg|thumb|Vulnerable (VU) species are considered to be facing a high risk of extinction in the wild.]]";
                case RedStatus.EN: return "[[File:Status iucn3.1 EN.svg|thumb|Endangered (EN) species are considered to be facing a very high risk of extinction in the wild.]]";
                case RedStatus.PE: //TODO
                case RedStatus.PEW: //TODO
                case RedStatus.CR: return "[[File:Status iucn3.1 CR.svg|thumb|Critically Endangered (CR) species face an extremely high risk of extinction in the wild.]]";
                case RedStatus.EX: return null; //TODO
                case RedStatus.EXplus: return null; //TODO
                case RedStatus.EW: return null; //TODO
                case RedStatus.DD: return null; //TODO
                case RedStatus.NE: case RedStatus.Unknown: case RedStatus.None: return null;
                case RedStatus.Null: return null;
            }

            return null; // should never happen... 
        }

    }

}
