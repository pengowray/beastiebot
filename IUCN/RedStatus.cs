using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {

    public enum RedStatus { None, Null, Unknown, EX, EW, CR, PE, PEW, EN, VU, CD, NT, LC, DD, NE };

    public static class RedStatusFunctions {

        // IUCN redlist status (e.g. EN)
        public static RedStatus New(string value) { // was: StatusString.set {}
            if (string.IsNullOrWhiteSpace(value)) {
                return RedStatus.None;
            }

            string v = value.Trim().ToUpperInvariant();
            switch (v) {
                case "EX": case "EXINCT": return RedStatus.EX;
                case "EW": case "EXTINCT IN THE WILD": return RedStatus.EW;
                case "CR": case "CRITICALLY ENDANGERED":  return RedStatus.CR;
                case "PE": case "CR(PE)": case "POSSIBLY EXTINCT": return RedStatus.PE;
                case "CRITICALLY ENDANGERED (POSSIBLY EXTINCT)": return RedStatus.PE;
                case "PEW": case "CR(PEW)": case "PW": case "CR(PW)": case "POSSIBLY EXTINCT IN THE WILD": return RedStatus.PEW;
                case "CRITICALLY ENDANGERED (POSSIBLY EXTINCT IN THE WILD)": return RedStatus.PEW;
                case "EN": case "ENDANGERED": return RedStatus.EN;
                case "VU": case "VULNERABLE": return RedStatus.VU;
                case "CD": case "LR/CD": case "LC/CD": case "CONSERVATION DEPENDENT": return RedStatus.CD;
                case "NT": case "LR/NT": case "LC/NT": case "NEAR THREATENED": return RedStatus.NT;
                case "LC": case "LR/LC": case "LC/LC": case "LEAST CONCERN":  return RedStatus.LC;
                case "DD": case "DATA DEFICIENT":  return RedStatus.DD;
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

            return status;
        }

        public static bool isThreatenedOrExtinct(this RedStatus status) {
            //string[] vulnerable = new string[] { "CR", "EN", "VU", "PE", "PW", "PEW", "EX" };
            RedStatus[] vulnerable = new RedStatus[] { RedStatus.CR, RedStatus.EN, RedStatus.VU,
                RedStatus.EW, RedStatus.EX, RedStatus.PE, RedStatus.PEW };

            return vulnerable.Contains(status);
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
                case RedStatus.DD: return null;
            }

            return null;
        }

        // I don't actually use this anywhere
        public static string Text(this RedStatus status) {
            switch (status) {
                case RedStatus.LC: return "least concern";
                case RedStatus.NT: return "near threatened";
                case RedStatus.CD: return "conservation-dependent";
                case RedStatus.VU: return "vulnerable";
                case RedStatus.EN: return "endangered";
                case RedStatus.CR: return "critically endangered";
                case RedStatus.PE: return "critically endangered (possibly extinct)";
                case RedStatus.PEW: return "critically endangered (possibly extinct in the wild)";
                case RedStatus.EX: return "extinct";
                case RedStatus.EW: return "extinct in the wild";
                case RedStatus.NE: return "not evaluated";
                case RedStatus.DD: return "data deficient";
                case RedStatus.Unknown: return "unknown";
                case RedStatus.None: return null;
                case RedStatus.Null: return null;
            }

            return null; // throw error. should never happen.
        }

        public static string HexColor(this RedStatus status) {
            switch (status) {
                case RedStatus.LC: return "#006666"; 
                case RedStatus.NT: case RedStatus.CD: return "#99cc99";
                case RedStatus.VU: return "#cc9900";
                case RedStatus.EN: return "#cc6633";
                case RedStatus.CR: case RedStatus.PE: case RedStatus.PEW: return "#cc3333";
                case RedStatus.EX: return "#000";
                case RedStatus.EW: return "#fff";
                case RedStatus.DD: return "#aaa";
                case RedStatus.NE: case RedStatus.Unknown: case RedStatus.None: return "#999"; // should be unused
                case RedStatus.Null: return "#999";
            }

            return null; // should never happen
        }

    }

}
