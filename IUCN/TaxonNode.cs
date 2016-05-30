using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace beastie {
    public class TaxonNode {

        //TODO list at end of file

        public TaxaRuleList ruleList;
        public TaxonRules rules {
            get {
                if (ruleList == null)
                    return null;

                return ruleList.GetDetails(name);
            }
        }

        public string rank;

        public TaxonName nodeName;
        public string name {
            get {
                if (nodeName == null) {
                    nodeName = new TaxonNameUnassigned(name);
                }
                return nodeName.taxon;
            }
            set {
                if (value == "ZZZZZ Not assigned" || value == "Not assigned" || value == null) {
                    nodeName = new TaxonNameUnassigned(value);
                } else {
                    //nodeName = new TaxonName(value); // for species only that aren't checked
                    nodeName = new TaxonPage(this, value);
                }
            }
        }

        public bool nameIsAssigned {
            get {
                return nodeName.isAssigned;
            }
        }


        /*
    public string name;
    public bool nameIsAssigned {
        get {
            return (!string.IsNullOrEmpty(name)) && name != "Not assigned" && name != "ZZZZZ Not assigned";
        }
    }
    */

        string enwikiArticle;

        public TaxonNode parent;
        public List<TaxonNode> children = new List<TaxonNode>();

        List<IUCNBitri> bitris = new List<IUCNBitri>(); // species and lower level

        public bool isMajorRank() {
            string[] majorRanks = new string[] { "kingdom", "phylum", "class", "order", "family", "genus", "species" };

            return (majorRanks.Contains(rank));
        }

        public TaxonNode() {
        }

        // case-insensitive search if this is taxon t, or any parent is (isa)
        public bool IsOrParentIs(string t) {
            //if (nodeName.taxon.ToLowerInvariant() == t.ToLowerInvariant()) //TODO: use regex?
            if (Regex.IsMatch(Regex.Escape(nodeName.taxon), Regex.Escape(t), RegexOptions.IgnoreCase)) 
                return true;

            if (parent == null)
                return false;

            return parent.IsOrParentIs(t);
        }

        Dictionary<RedStatus, TaxonStats> statsCache;
        public TaxonStats GetStats(RedStatus statusFilter = RedStatus.Null) {
            if (statsCache == null) {
                statsCache = new Dictionary<RedStatus, TaxonStats>();
            }
            if (!statsCache.ContainsKey(statusFilter)) {
                statsCache[statusFilter] = new TaxonStats(this, statusFilter);
            }

            return statsCache[statusFilter];
        }

        public string StatsSummary(RedStatus status = RedStatus.Null) {
            var all_stats = GetStats();
            var cr_stats = GetStats(status);

            if (status == RedStatus.Null) {
                return string.Format("{0} sp, {1} ssp, {2} sp subpop, {3} ssp subpops",
                    all_stats.species, all_stats.subspecies, all_stats.subpops_species, all_stats.subpops_subspecies);
            } else {
                return string.Format("{0} / {4} sp, {1} / {5} ssp, {2} / {6} sp subpop, {3} / {7} ssp subpops",
                    cr_stats.species, cr_stats.subspecies, cr_stats.subpops_species, cr_stats.subpops_subspecies,
                    all_stats.species, all_stats.subspecies, all_stats.subpops_species, all_stats.subpops_subspecies
                    );
            }

        }


        public void Add(IUCNTaxonLadder details) {
            //TODO: check if already exists. If so, warn that CSV export may include regional items (can't actually tell them apart in CSV file)
            if (rank == "top") {
                TaxonNode current = this;

                // check manual insertion of inbetween-taxa
                if (ruleList != null) {
                    foreach (var r in details.rankName) {
                        string rRank = r.Key;
                        string rTaxon = r.Value;

                        var rules = ruleList.GetDetails(rTaxon);

                        if (rules != null) {
                            string below = rules.below; // newTaxonName
                            string belowRank = rules.belowRank;

                            if (below == null) {
                                //Console.Error.WriteLine("Warning: maybe 'below' taxon didn't have a rank or something: " + r.Value);
                                continue;
                            }

                            if (belowRank == null) {
                                Console.Error.WriteLine("Warning: maybe 'below' taxon didn't have a rank or something: " + r.Value);
                                continue;
                            }

                            details.InsertBelow(rRank, belowRank, below);

                            //TODO: continue on (or restart process). 
                            break; // can't remove or will get error "System.InvalidOperationException: Collection was modified"

                        }
                    }
                }


                foreach (string drank in details.ranks) {
                    string dname = details.rankName[drank];

                    if (string.IsNullOrWhiteSpace(dname))
                        continue;

                    TaxonNode tn = current.FindChild(drank, dname);
                    if (tn == null) {
                        tn = new TaxonNode();
                        tn.ruleList = ruleList;
                        tn.rank = drank;
                        tn.name = dname;
                        tn.parent = current;
                        current.children.Add(tn);
                    }
                    current = tn;
                }

                if (current.rank != "top") {
                    current.AddSpeciesChild(details);
                }
            } else {
                // then what?
            }
        }

        /**
		 * IUCN Red List Index of species survival
		 * 
		 * @returns 1 if all speceies are LC, and 0 if all extinct. 
		 * Ignores subspecies and stocks/subpopulations
		 */
        public double RLI() {
            /*
			Put simply, the number of species in each Red List Category
			is multiplied by the Category weight (which ranges from 0 for
			Least Concern, 1 for Near Threatened, 2 for Vulnerable, 3 for
			Endangered, 4 for Critically Endangered and 5 for Extinct in the
			Wild and Extinct). These products are summed, divided by the
			maximum possible product (the number of species multiplied by
			the maximum weight), and subtracted from one. This produces
			an index that ranges from 0 to 1 (see below).
			-- https://portals.iucn.org/library/sites/library/files/documents/2009-001.pdf
			*/

            var valid = DeepBitris().Where(bt => bt.isSpecies && bt.Status.RliWeight() != null);
            int numerator = (int)valid.Sum(bt => bt.Status.RliWeight());
            int denominator = valid.Count() * 5;
            double rli = 1 - ((double)numerator / (double)denominator);

            return rli;
        }

        public string StocksOrSubpopsText(int count, bool newspaperNumbers = false, string status = null) {
            // note: assessing whether to use stock or subpopulation from this TaxonNode, but
            // count may be for one threat status of this TaxonNode's children, e.g. one species

            //status example: "endangered" (only used if newspaperNumbers is true)

            string plural = "";
            if (count > 1)
                plural = "s";

            string nbsp = "&nbsp;";
            string countText = count.ToString();
            if (newspaperNumbers) {
                if (!string.IsNullOrEmpty(status)) {
                    status = " " + status; // add space
                    nbsp = " "; // don't use nbsp
                } else {
                    status = string.Empty;
                }

                countText = count.NewspaperNumber() + status;
            }

            return countText + nbsp + "subpopulation" + plural;

            // used to check for stock vs subpopulation, but stocks are subpopulations and stocks sounds dumb

            /*
			if (DeepBitris().Any(b => b.isStockpop)) {
				if (DeepBitris().All(b => !b.isStockpop || b.stockpop.ToLowerInvariant().Contains("subpopulation"))) {
					return countText + nbsp + "subpopulation" + plural;

				} else if (DeepBitris().All(b => !b.isStockpop || b.stockpop.ToLowerInvariant().Contains("stock"))) {
					return countText  + nbsp + "stock" + plural;

				} else {
					if (count > 1) {
						if (newspaperNumbers) {

                            //return countText + " subpopulations or stocks";
                            return countText + " subpopulations";
                        } else {
							return countText + nbsp + "subpopulations/stocks";
						}
					} else {
						if (newspaperNumbers) {
							return countText + " subpopulation or stock";
						} else {
							return countText + nbsp + "subpopulation/stock";
						}
					}
				}

			} else {
				return null;
			}
            */
        }

        public string StocksOrSubpopsHeading() {
            int pops = DeepBitriCountWhere(b => b.isStockpop);
            if (pops == 0)
                return string.Empty;

            //TODO: use GetStats and default to "Subpopulations"

            if (DeepBitris().Any(b => b.isStockpop)) {
                if (DeepBitris().All(b => !b.isStockpop || b.stockpop.ToLowerInvariant().Contains("subpopulation"))) {
                    return "Subpopulations";
                } else if (DeepBitris().All(b => !b.isStockpop || b.stockpop.ToLowerInvariant().Contains("stock"))) {
                    return "Stocks";
                } else {
                    return "Subpopulations and stocks";
                }

            } else {
                return null;
            }
        }

        public void AddSpeciesChild(IUCNTaxonLadder details) {
            //bitris.Add(details.FullSpeciesName());

            IUCNBitri bitri = details.ExtractBitri();

            bitris.Add(bitri);

            if (!bitri.Status.isEvaluated()) {
                Console.Error.WriteLine("adding unevaluated bitri: " + bitri.FullName() + ", status: " + bitri.Status);
            }
        }


        public TaxonNode CreatePseduoNode(string newNodeName, TaxonNode[] include, TaxonNode[] exclude) {
            TaxonNode node = new TaxonNode();
            node.rank = "paraphyletic group";
            node.name = newNodeName; //TODO?
            node.parent = null; // don't attach to parent?
            if (exclude == null)
                exclude = new TaxonNode[] { };

            if (include.Length == 1) {
                // if only one inclusion, then copy its children as psuedo node's own
                // TODO / FIXME: only excludes immidate children
                node.children = new List<TaxonNode>(include[0].children.AsEnumerable().Where(ch => !exclude.Contains(ch)));
            } else {
                // TODO: broken: isn't going to exclude anything unless it's on both include and exclude lists
                node.children = include.Where(ch => !exclude.Contains(ch)).ToList();
            }
            node.statsCache = null;
            node.ruleList = ruleList;

            /*
            foreach (var excluded in exclude) {
                var parentOfExcluded = excluded.parent;
                //clone parent with missing child
                if (p == )
                TaxonNode clone = (TaxonNode) p.MemberwiseClone();
                clone.children = new List<TaxonNode>(p.children.AsEnumerable()); // clone.. TODO: there's probably a better way to do this.
                clone.children.RemoveAll(ch => ch == excluded);
                clone.statsCache = null;
                clone.ruleList = ruleList;

                
                node.children.RemoveAll(ch => ch == p);
                node.children.Add(clone);
            }
            */

            return node;
        }

        //delete me
        public bool hasDivisions(RedStatus status = RedStatus.Null, int depth = 0) {
            //TODO

            if (children == null || children.Count() <= 1)
                return false;

            return true;
        }

        // returns a list of child nodes, and possibly an "other" node which groups some children. 
        // or returns nothing if no child nodes or if doesn't need to be divided further
        public IEnumerable<TaxonNode> Divisions(RedStatus status = RedStatus.Null, int depth = 0) {
            bool nonDividableRank = (rank == "family" || rank == "genus" || rank == "species");

            if (nonDividableRank || children.Count() <= 1) {
                yield break;
            }

            //TODO: put "Not assigned" last
            // uh, not sorted by name anyway
            /*
            foreach (var ch in children) {
                if (ch.name == "Not assigned") {
                    ch.name = "ZZZZZ Not assigned"; // "ZZZZZ " for sorting. removed later
                                                    //TODO: better sorter
                }
            }
            */

            // sort by redlist indicator (extinction risk) // TODO: Put "Not assigned" last
            var sortedChildren = from child in children orderby child.RLI() select child;

            bool forceDivide = (rules != null && rules.forceSplit);

            if (forceDivide) {
                //Console.Error.WriteLine("found force split: " + this.nodeName); // debug
                foreach (var child in sortedChildren)
                    yield return child;

                yield break;
            }

            TaxonStats stats = new TaxonStats(this, status);

            //int divide = 27; // don't split if less than 27 bi/tris. 
            int oneDivide = 15; //  20; // allow one split if over 20 (originally designed to cause CR bats to split into micro and macrobats (>20), but not new world monkeys <27))

            int mergeMaxSize = 5; // merge into an "other" category if <= than this number of bitris... (originally 7)
            if (depth == 0) {
                mergeMaxSize = 2;
            }
            int mergeMinGroups = 4; // ...in at least this many groups (taxa)

            //if (children.Count == 1) {} // jump to child without displaying it

            var dividableChildren = children.Where(ch => (ch.GetStats(status).species >= oneDivide || ch.GetStats(status).subspecies >= oneDivide) && ch.nodeName.isAssigned);
            int dividableChildrenCount = dividableChildren.Count();

            if (dividableChildrenCount == 0) {
                yield break;
            }

            /*
            int childBitris = stats.bitris;
            
            // no point breaking up family into genera
            //TODO: don't check if "family", check below ranks are genera
            bool doDivide = forceDivide ||
                (childBitris > divide && children.Count > 2 && dividableRank) ||
                (childBitris > oneDivide && children.Count == 2 && dividableRank);
            */

            //check if there's a lot of solo items (or less than 7) and group those together in "otherNode"
            var viewableChildren = children.Where(ch => ch.GetStats(status).bitris > 0);
            var mergableChildren = viewableChildren.Where(ch => ch.GetStats(status).bitris <= mergeMaxSize || !ch.nodeName.isAssigned);
            bool mergable = mergableChildren.Count() >= mergeMinGroups;
            // very mergable: 3 groups of 2 or less. TODO: clean up code. bit hacked on.
            var veryMergableChildren = viewableChildren.Where(ch => ch.GetStats(status).bitris <= 2 || !ch.nodeName.isAssigned);
            bool veryMergable = mergableChildren.Count() >= 3;

            TaxonNode otherNode = null;
            if (mergable || veryMergable) {
                if (mergable) {
                    if (mergableChildren.Count() == children.Count()) {
                        yield break;
                    }
                } else {
                    if (veryMergableChildren.Count() == children.Count()) {
                        yield break;
                    }

                }

                otherNode = new TaxonNode();
                //TODO: TaxonName subclass for nodeName, e.g. "Other megabats" "Other mammalian species"
                otherNode.nodeName = new TaxonNameOther(this.nodeName);
                otherNode.ruleList = ruleList;
                otherNode.rank = "no rank";
                otherNode.parent = this;

                //otherNode.bitris = mergableChildren.SelectMany(ch => ch.AllBitrisDeepWhere(bt => bt.Status.MatchesFilter(status))).ToList();
                if (mergable) {
                    otherNode.bitris = mergableChildren.SelectMany(ch => ch.AllBitrisDeepWhere()).ToList();
                } else if (veryMergable) {
                    otherNode.bitris = veryMergableChildren.SelectMany(ch => ch.AllBitrisDeepWhere()).ToList();
                }
            }


            if (otherNode == null) {
                foreach (var child in sortedChildren) {
                    yield return child;
                }
                yield break;

            } else {
                foreach (var child in sortedChildren) {
                    if (!mergableChildren.Contains(child))
                        yield return child;
                }
                yield return otherNode;
                yield break;

            }

        }

        public void PrettyPrint(TextWriter output, RedStatus status = RedStatus.Null, int depth = 0) {
            if (output == null) {
                output = Console.Out;
            }

            //TaxonStats stats = new TaxonStats(this, status);
            TaxonStats stats = GetStats(status);

            //bool anything = (DeepBitriCount(status, 1) > 0);
            if (stats.noBitris)
                return;

            /*
            int divide = 27; // don't split if less than 27 bi/tris. 
			int oneDivide = 20; // allow one split if over 20 (to cause CR bats to split, but not new world monkeys.. very arbitrary)
			//TODO: check if the 2 children have anything that will be displayed

			//TODO: check if there's a lot of solo items and group those together, each with a (family) suffix

			int childBitris = DeepBitriCount(status, divide);

			//if (children.Count == 1) {} // jump to child without displaying it

			bool forceDivide = (rules != null && rules.forceSplit);
			bool dividableRank = (rank != "family" && rank != "genus" && rank != "species");
			// no point breaking up family into genera
			//TODO: don't check if "family", check below ranks are genera
			bool doDivide = forceDivide || 
				(childBitris > divide && children.Count > 2 && dividableRank) ||
				(childBitris > oneDivide && children.Count == 2 && dividableRank);

            //var header = new TaxonHeaderBlurb(this, name, depth, comprises, includes, means);
            */

            if (depth == 0) {
                output.WriteLine("<!-- This article was auto-generated by [[User:Beastie Bot]]. -->");

                output.WriteLine(IUCNChart.Text(this));

                string diagram = status.WikiImage();
                if (diagram != null) {
                    output.WriteLine(diagram);
                    output.WriteLine();
                }

                output.WriteLine(TaxonHeaderBlurb.ArticleBlurb(this, status));
            }


            string headerString = TaxonHeaderBlurb.HeadingString(this, depth, status);
            if (!string.IsNullOrWhiteSpace(headerString)) {
                output.WriteLine(headerString);
            }

            var subHeadings = Divisions(status, depth);

            if (subHeadings.Count() > 0) {

                string statsText = BlurbBeforeSplit.Text(this, status, depth); // //header.PrintStatsBeforeSplit(status);
                if (!string.IsNullOrWhiteSpace(statsText)) {
                    output.WriteLine(statsText);
                }

                //var sortedChildren = from child in children orderby child.RLI() select child;  // sort by redlist indicator (extinction risk)
                //foreach (var ch in sortedChildren) {
                foreach (var ch in subHeadings) {
                    ch.PrettyPrint(output, status, depth + 1);
                }

            } else {

                //TODO: format subsp. properly 

                //comma separated:
                //string binoms = AllBitrisDeep().Select(binom => "''[[" + Altname(binom) + "]]''").JoinStrings(", ");

                //list:
                // "{{columns-list|4;font-style:italic|" // https://en.wikipedia.org/wiki/IUCN_Red_List_Critically_Endangered_species_(Animalia)

                //TODO: order by: get stock/pops to the end 

                bool includeStatus = (status == RedStatus.Null); // show status for each species only if all statuses are being shown

                //TODO: use GetStats for these:


                List<IUCNBitri> deepBitriList;
                if (status.isNull()) {
                    deepBitriList = AllBitrisDeepWhere();
                } else {
                    deepBitriList = AllBitrisDeepWhere(bt => bt.Status.MatchesFilter(status));
                }

                bool anyBinoms = stats.species > 0; // //deepBitriList.Any(bt => bt.isSpecies);
                bool anySubspecies = stats.subspecies > 0; // deepBitriList.Any(bt => bt.isTrinomial && !bt.isStockpop);
                bool anyStockPops = stats.subpops_total > 0; // deepBitriList.Any(bt => bt.isStockpop);

                // Grey text (only if at least 3 species/subspecies etc)
                //if (deepBitriList.Count() >= 3) { 

                // only if 4+ species (todo: or subspecies?)
                if (GetStats(status).species >= 3) {
                    string grayText = TaxonHeaderBlurb.GrayText(this);
                    if (!string.IsNullOrWhiteSpace(grayText)) {
                        output.WriteLine(grayText);
                    }
                }

                if (anyBinoms) {
                    if (anySubspecies || anyStockPops) {
                        output.WriteLine("\n'''Species'''");
                    }
                    output.WriteLine(FormatBitriList(deepBitriList.Where(bt => bt.isSpecies), includeStatus));
                } else {
                    output.WriteLine(string.Empty);
                }

                if (anySubspecies) {
                    //TODO: plant varieties and shit
                    output.WriteLine("'''Subspecies'''");
                    output.WriteLine(FormatBitriList(deepBitriList.Where(bt => bt.isTrinomial && !bt.isStockpop), includeStatus));
                }

                if (anyStockPops) {
                    //output.WriteLine("'''Stocks and populations'''");
                    output.WriteLine("'''" + StocksOrSubpopsHeading() + "'''");
                    //output.WriteLine(FormatBitriList(deepBitriList.Where(bt => bt.isStockpop), includeStatus));
                    var groups = deepBitriList.Where(bt => bt.isStockpop).GroupBy(b => b.BasicName()).OrderBy(b => b.Key);
                    var grouped = groups.Select(g => g.First().CloneMultistockpop(StocksOrSubpopsText(g.Count())));
                    //foreach (var group in groups) {
                    //output.WriteLine("* " + FormatBitri(group.First(), false, StocksOrSubpopsText(group.Count()) ));
                    //}
                    output.WriteLine(FormatBitriList(grouped, false, 3));
                }
                output.WriteLine(string.Empty);

            }

            if (depth == 0) {
                output.WriteLine(BlurbFooter.Footer(this, status));
            }


        }


        public string FormatBitriList(IEnumerable<IUCNBitri> bitris, bool includeStatus = false, int columns = 3) {
            if (bitris.Count() == 0)
                return string.Empty;

            //string cols_start = "{{columns-list|" + columns + "|"; // \n
            string cols_start = "{{columns-list|colwidth=30em|";
            string cols_end = "}}";

            if (bitris.Count() < columns) {
                cols_start = string.Empty;
                cols_end = string.Empty;
            }

            return cols_start +
                bitris.OrderBy(bt => bt.FullName())
                .Select(binom => "*" + FormatBitri(binom, includeStatus))
                .JoinStrings("\n")
                + cols_end;
        }

        public string FormatBitri(IUCNBitri bitri, bool includeStatus = false) {
            //string commonName = null;
            //string wikiPage = null;
            //string basicName = bitri.BasicName();
            if (bitri == null)
                return null;

            TaxonName taxonName = bitri.TaxonName(); // BeastieBot.Instance().GetTaxonPage(bitri); // .CommonName() 

            //e.g. "[[Gorilla gorilla|Western gorilla]]" or "''[[Trachypithecus poliocephalus poliocephalus]]''"
            bool upperFirstChar = true;
            string bitriLinkText = taxonName.CommonNameLink(upperFirstChar);

            string pop = string.Empty;
            if (bitri.isStockpop) {
                pop = " (" + bitri.stockpop + ")";
            }

            string special = string.Empty;
            if (bitri.Status == RedStatus.PE) {
                special = " (possibly&nbsp;extinct)"; //TODO: switch a unicode nbsp?
            } else if (bitri.Status == RedStatus.PEW) {
                special = " (possibly extinct in the wild)";
            }

            string extinct = (bitri.Status == RedStatus.EX ? "{{Extinct}}" : "");
            //string wikipediaStatus = bitri.Status;
            string wikipediaStatus = " {{IUCN status|" + bitri.Status.Limited() + "}}"; // Turns PE into CR (which is good, as PE text is added) BUT: will also turn LR/cd into NT
            string status = (includeStatus && bitri.Status.Limited() != RedStatus.None) ? wikipediaStatus : "";

            return string.Format("{0}{1}{2}{3}{4}", extinct, bitriLinkText, pop, status, special);
        }

        public List<IUCNBitri> AllBitrisDeepWhere(Func<IUCNBitri, bool> whereFn = null, List<IUCNBitri> bitrisList = null) {
            if (bitrisList == null) {
                bitrisList = new List<IUCNBitri>();
            }
            if (whereFn == null) {
                bitrisList.AddRange(bitris);
            } else {
                bitrisList.AddRange(bitris.Where(whereFn));
            }

            foreach (var child in children) {
                child.AllBitrisDeepWhere(whereFn, bitrisList);
            }

            return bitrisList;
        }

        // note: doesn't serach BiTris (and can't return one anyway)
        public TaxonNode FindNode(string taxonName) {
            if (taxonName == null)
                return this;

            return FindChildTaxonDeep(taxonName);
        }

        public TaxonNode FindChildTaxonDeep(string taxonName) {
            if (taxonName == null)
                return null;

            string lowername = taxonName.ToLowerInvariant();

            return AllChildrenBreadthFirst().Where(t => t.name.ToLowerInvariant() == lowername).FirstOrDefault();
        }

        public IEnumerable<TaxonNode> AllChildrenBreadthFirst() {

            // Search breadth first. Note: does not search Bitris
            Queue<TaxonNode> queue = new Queue<TaxonNode>();
            queue.Enqueue(this);

            while (queue.Count > 0) {
                TaxonNode current = queue.Dequeue();
                if (current == null)
                    continue;

                yield return current;

                foreach (var c2 in current.children) {
                    queue.Enqueue(c2);
                }
            }
        }

        public TaxonNode FindChild(string qname) {
            return FindChild(null, qname);
        }

        public TaxonNode FindChild(string qrank, string qname) {
            //TODO: search within ranks if plausably there
            foreach (var child in children) {
                if (child.name == qname) {
                    if (qrank == null || child.rank == qrank) {
                        return child;
                    } else {
                        Console.Error.WriteLine("Weirdness finding {0}. Expected Rank: {1} Found Rank: {2}", name, rank, child.rank);
                        return null;
                        //return child; // return it anyway?
                    }
                }
            }
            return null;
        }

        public void PrintReportMissing(TaxonNode[] contents) {
            var missing = ReportMissing(contents);

            foreach (var t in missing) {
                Console.WriteLine("Missing: " + t.nodeName.TaxonWithRankDebug() + " -- " + t.GetStats().bitris);
            }

            if (missing.Count() == 0) {
                Console.WriteLine("OK: No missing taxa found.");
            }
        }

        public void PrintReportMissingSpeciesNames() {
            foreach (IUCNBitri bitri in bitris.Where(bt => !bt.isStockpop)) {
                if (string.IsNullOrEmpty(bitri.CommonNameEng)) {
                    continue;
                }

                var page = bitri.TaxonName();

                if (page.CommonName() == null)
                    continue;

                var names = bitri.CommonNamesEng(); // note: filters out names like "Species code: Ag"
                if (names.Count() == 1) {
                    Console.WriteLine(page.taxon + " = " + names[0].ToLowerInvariant());
                } else {
                    Console.WriteLine(page.taxon + " = " + names[0].ToLowerInvariant() + " // " + names[1].ToLowerInvariant());
                }

            }

            //TODO: comment headings for 3 depth or something

            foreach (var child in children) {
                child.PrintReportMissingSpeciesNames();
            }

        }


        TaxonNode[] ReportMissing(TaxonNode[] contents) {

            //return AllChildrenBreadthFirst().Where(t => t.name.ToLowerInvariant() == lowername).FirstOrDefault(null);


            if (contents.Contains(this)) {
                return null; // new TaxonNode[] {};  // none missing
            }

            List<TaxonNode> MissingList = new List<TaxonNode>();
            int childrenMatched = 0;

            foreach (var child in children) {
                var missing = child.ReportMissing(contents);
                if (missing != null) {
                    if (missing.Length == 1 && missing[0] == child) {
                        childrenMatched++;
                    }
                    MissingList.AddRange(missing);
                }
            }

            if (childrenMatched == children.Count()) {
                return new TaxonNode[] { this };
            } else {
                return MissingList.ToArray();
            }
        }


        public void PrintReportDuplicateCommonNames1() {
            // searches for duplicates but is kinda crap.
            // picks up a lot of useful stuff but too much noise
            // doesn't search within IUCN's name list
            // duplicate taxa names in a heirarchy often dont matter so much 

            Console.WriteLine("Searching for duplicate common names...");

            Dictionary<string, string> dupes = new Dictionary<string, string>(); // normalized name, an example of non-normalized name

            Dictionary<string, List<TaxonNode>> nodes = new Dictionary<string, List<TaxonNode>>();
            Dictionary<string, List<IUCNBitri>> bitriNames = new Dictionary<string, List<IUCNBitri>>();

            foreach (var node in AllChildrenBreadthFirst().Where(n => n.rank != "genus" && n.rank != "species" && n.rank != "subspecies" && n.rank != "infraspecific name")) {
                string exampleName = node.nodeName.CommonName();
                if (exampleName == null)
                    continue;

                string normalizedName = exampleName.NormalizeForComparison();

                List<TaxonNode> currentList = null;
                if (nodes.TryGetValue(normalizedName, out currentList)) {
                    currentList.Add(node);
                    dupes[normalizedName] = exampleName;
                    Console.WriteLine("Dupe found (node-node): {0} ({1}) {2} = {3}", exampleName, normalizedName,
                        node.nodeName.TaxonWithRankDebug(),
                        currentList[0].nodeName.TaxonWithRankDebug());

                } else {
                    currentList = new List<TaxonNode>();
                    currentList.Add(node);
                    nodes[normalizedName] = currentList;
                }
            }

            foreach (IUCNBitri bitri in DeepBitris().Where(bt => bt.isSpecies)) {  // .Where(bt => !bt.isStockpop)) { // TODO: trinomials
                string exampleName = bitri.TaxonName().CommonName();
                if (exampleName == null)
                    continue;

                string normalizedName = exampleName.NormalizeForComparison();

                List<IUCNBitri> currentList = null;
                if (bitriNames.TryGetValue(normalizedName, out currentList)) {

                    dupes[normalizedName] = exampleName;
                    currentList.Add(bitri);
                    Console.WriteLine("Dupe found (bitri-bitri): {0} ({1}) {2} = {3}", exampleName, normalizedName,
                        bitri.FullName(),
                        currentList[0].FullName());

                } else {
                    currentList = new List<IUCNBitri>();
                    currentList.Add(bitri);
                    bitriNames[normalizedName] = currentList;

                    if (nodes.ContainsKey(normalizedName)) {
                        dupes[normalizedName] = exampleName;
                        Console.WriteLine("Dupe found (bitri-node): {0} ({1}) {2} = {3} ", exampleName, normalizedName,
                            bitri.FullName(),
                            nodes[normalizedName].First().nodeName.TaxonWithRankDebug());
                    }
                }
            }

            foreach (var dupeEntry in dupes) {
                string dupeNomralized = dupeEntry.Key;
                string dupeReadableExample = dupeEntry.Value;

                //Console.WriteLine(dupe);
                List<TaxonNode> nodeList = null;
                List<IUCNBitri> bitriList = null;
                bool isNodes = nodes.TryGetValue(dupeNomralized, out nodeList);
                bool isBitris = bitriNames.TryGetValue(dupeNomralized, out bitriList);

                string listString = string.Format("Duplicate common name: {0} ({1}) : {2} - {3} ",
                    dupeReadableExample,
                    dupeNomralized,
                    (isNodes ? nodeList.Select(n => n.nodeName.TaxonWithRankDebug()).JoinStrings(", ") : ""),
                    (isBitris ? bitriList.Select(bt => bt.FullName()).JoinStrings(", ") : ""));

                Console.WriteLine(listString);

            }
        }

        public void CommonNameIssuesReport() {
            try {
                string commonNameIssuesFile = FileConfig.Instance().commonNameIssuesFile;
                Console.WriteLine("Saving IUCN Common Name issues report: " + commonNameIssuesFile);
                StreamWriter commonNameIssuesReportOutput = new StreamWriter(commonNameIssuesFile, false, Encoding.UTF8);
                var commonNameIssuesReport = new IUCNCommonNameIssuesReport(this, commonNameIssuesReportOutput);
                commonNameIssuesReport.MakeReport();
                commonNameIssuesReportOutput.Close();
            } catch (Exception e) {
                Console.WriteLine("Error writing IUCN Common Name issues report. " + e);
            }
        }

        public void PrintReportDuplicateCommonNamesAndPagesV2() {
            Console.WriteLine("Rading caps.txt rules...");
            RedListCapsReport.ReadCapsToRules();

            string filename = FileConfig.Instance().CommonNameDupesFile;
            string wikifilename = FileConfig.Instance().WikiAmbigDupesFile;
            string wikiDupeReportfilename = FileConfig.Instance().WikiDupesReportFile;
            string capsReportFilename = FileConfig.Instance().CapsReportFile + "_generated.txt";
            //string capsReportFilename = FileConfig.Instance().CapsReportFile + ".txt";

            //test opening output files using "append" just to check if can write to file before spending time generating report
            StreamWriter dupeOutputTest = new StreamWriter(filename, true, Encoding.UTF8);
            dupeOutputTest.Close();
            StreamWriter dupeOutputTest2 = new StreamWriter(wikifilename, true, Encoding.UTF8);
            dupeOutputTest2.Close();
            StreamWriter capsReportWriterTest = new StreamWriter(capsReportFilename, true, Encoding.UTF8);
            capsReportWriterTest.Close();

            bool showProgress = false;
            Console.WriteLine("Searching for duplicate common names...");

            Dupes binomNameDupes = Dupes.FindByCommonNames(DeepBitris().Where(bt => bt.isSpecies));
            Dupes trinoNameDupes = Dupes.FindByCommonNames(DeepBitris().Where(bt => bt.isTrinomial && !bt.isStockpop), binomNameDupes);

            Console.WriteLine("Searching for names which redirect to the same wiki page...");

            Dupes wikiBinNameDupes = Dupes.FindWikiAmbiguous(DeepBitris().Where(bt => bt.isSpecies));
            Dupes wikiTriNameDupes = Dupes.FindWikiAmbiguous(DeepBitris().Where(bt => bt.isTrinomial && !bt.isStockpop), wikiBinNameDupes);

            Console.WriteLine("Saving dupe ruleset file: " + filename);
            StreamWriter dupeOutput = new StreamWriter(filename, false, Encoding.UTF8);
            //TODO: wiki date
            dupeOutput.WriteLine("// Duplicate names. " + FileConfig.Instance().iucnRedListFileShortDate);
            dupeOutput.WriteLine();
            binomNameDupes.ExportWithBitris(dupeOutput, "name-ambiguous //", trinoNameDupes);
            dupeOutput.WriteLine();
            trinoNameDupes.ExportWithBitris(dupeOutput, "name-ambiguous-for-infraspecies //", binomNameDupes);


            Dupes WikiSpeciesDupes;
            Dupes WikiHigherDupes;
            wikiBinNameDupes.SplitSpeciesSspLevel(out WikiSpeciesDupes, out WikiHigherDupes);

            Dupes WikiTrispeciesDupes;
            Dupes WikiTriHigherDupes;
            wikiBinNameDupes.SplitSspLevel(out WikiTrispeciesDupes, out WikiTriHigherDupes);

            Console.WriteLine("Saving wikipage ambiguity report: " + wikiDupeReportfilename);
            StreamWriter wikiDupeReportOutput = new StreamWriter(wikiDupeReportfilename, false, Encoding.UTF8);
            wikiDupeReportOutput.WriteLine("https://en.wikipedia.org/wiki/User:Beastie_Bot/Redirects_to_same_title");
            wikiDupeReportOutput.WriteLine("These are scientific names which are listed as separate species (or subspecies) in the IUCN Red List but redirect to the same Wikipedia article. " + FileConfig.Instance().iucnRedListFileShortDate);
            wikiDupeReportOutput.WriteLine();
            wikiDupeReportOutput.WriteLine("==Link to same species==");
            WikiSpeciesDupes.ExportWithBitris(wikiDupeReportOutput, "<small>is linked from</small>", wikiTriNameDupes, true);
            wikiDupeReportOutput.WriteLine();
            wikiDupeReportOutput.WriteLine("==Link to higher taxa==");
            WikiHigherDupes.ExportWithBitris(wikiDupeReportOutput, "<small>is linked from</small>", wikiTriNameDupes, true);
            wikiDupeReportOutput.WriteLine();
            wikiDupeReportOutput.WriteLine("==Subspecies redirects linking to same trinomial==");
            wikiDupeReportOutput.WriteLine("(Subspecies or other infraspecies taxa)");
            WikiTrispeciesDupes.ExportWithBitris(wikiDupeReportOutput, "<small>is linked from</small>", wikiBinNameDupes, true);
            wikiDupeReportOutput.Close();

            ruleList.BinomAmbig = new HashSet<String>(binomNameDupes.dupes.Keys.AsEnumerable());
            ruleList.InfraAmbig = new HashSet<String>(trinoNameDupes.dupes.Keys.AsEnumerable());
            ruleList.WikiPageAmbig = new HashSet<String>(wikiBinNameDupes.dupes.Keys.AsEnumerable());
            ruleList.WikiSpeciesDupes = WikiSpeciesDupes;
            ruleList.WikiHigherDupes = WikiHigherDupes;
            
            //Note: caps report requires dupes to be already worked out so it doesn't cause dupeless names to get cached (e.g. subspecies like Mt. Kilimanjaro guereza)

            StreamWriter capsReportWriter = new StreamWriter(capsReportFilename, false, Encoding.UTF8);
            RedListCapsReport capsReport = new RedListCapsReport();
            capsReport.FindWords(this);
            capsReport.PrintWords(capsReportWriter);
            capsReportWriter.Close();

            // TODO: move this to end?
            CommonNameIssuesReport(); // note: may require RedListCapsReport to be read first


        }


        /**
		 * Count the number of bi/trinomials below (includes stocks/pops unless filtered out)
		 */
        public int DeepBitriCount(RedStatus statusFilter = RedStatus.Null, int max = int.MaxValue) {
			if (statusFilter == RedStatus.Null) {
				return DeepBitriCountWhere(null, max);
			} else {
				//total += bitris.Where(b => b.redlistStatus == statusFilter).Count();
				return DeepBitriCountWhere(b => b.Status.MatchesFilter(statusFilter), max);
			}
		}

		// count binomials only (not stocks/pops, not trinomials)
		public int DeepSpeciesCount(RedStatus statusFilter = RedStatus.Null, int max = int.MaxValue) {
            if (statusFilter == RedStatus.Null) {
                return DeepBitriCountWhere(b => b.isSpecies, max);
            } else {
                //total += bitris.Where(b => b.redlistStatus == statusFilter).Count();
                return DeepBitriCountWhere(b => b.isSpecies && b.Status.MatchesFilter(statusFilter), max);
            }
        }

		public IEnumerable<IUCNBitri> DeepBitris() {
			foreach (var bt in bitris) {
				yield return bt;
			}

			foreach (var child in children) {
				foreach (var bt in child.DeepBitris()) {
					yield return bt;
				}
			}
		}

		public int DeepBitriCountWhere(Func<IUCNBitri, bool> whereFn, int max = int.MaxValue) {
			int total = 0;
			if (whereFn == null) {
				total += bitris.Count();
			} else {
				total += bitris.Where(whereFn).Count();
			}

			foreach (var child in children) {
				total += child.DeepBitriCountWhere(whereFn, max);
				if (total > max)
					return total;
			}

			return total;
		}


        // zero all possible status counts first, so you don't have to check for value existing
        public Dictionary<RedStatus, int> DeepBitriStatusCountWhereWithZeroes(Func<IUCNBitri, bool> whereFn) {
            //TODO: use TaxonStats
            Dictionary<RedStatus, int> statuses = new Dictionary<RedStatus, int>();
            foreach (RedStatus rs in Enum.GetValues(typeof(RedStatus))) {
                statuses[rs] = 0;
            }
            return DeepBitriStatusCountWhere(whereFn, statuses);
        }

        public Dictionary<RedStatus, int> DeepBitriStatusCountWhere(Func<IUCNBitri, bool> whereFn, Dictionary<RedStatus, int> addTo = null) {
            Dictionary<RedStatus, int> statuses = null;

            if (addTo == null) {
                statuses = new Dictionary<RedStatus, int>();
            } else {
                statuses = addTo;
            }

			if (whereFn != null) {
				foreach (var bitri in bitris.Where(whereFn)) {
                    statuses.AddCount(bitri.Status, 1);
				}
			} else {
				foreach (var bitri in bitris) {
                    statuses.AddCount(bitri.Status, 1);
                }
			}

			foreach (var child in children) {
				child.DeepBitriStatusCountWhere(whereFn, statuses);
			}

			return statuses;
		}

        
        static string AppendIfNotZero(int number, string a, string b) {
            return number == 0 ? a : a + b;
        }



	}


}

/*
 *
 *
 *
TODO: 

"Myomorpha contains 39 critically endangered species. " => "Myomorpha (meaning "mouse-like") contains 39 critically endangered species." ?

separate stock/pop headings for sp and ssp 

Narwhal speciesbox
Monodon monoceros
| genus = Monodon
| species = monoceros

"All zero species in Odobenidae which have been assessed are critically endangered. "
(done) Gecarcinucidae (many redirects to genus, not monotypic). ALso: Parastacidae, and Isopoda. e.g. Ceylonthelphusa sanguinea, Thermosphaeroma cavicauda
How to handle?: 
(done) Acinonyx jubatus ssp. hecki => Acinonyx jubatus hecki (animals only)
(avoided) Dexteria floridana => Dexteria (monotypic) 
(ok) Haplochromis sp. 'parvidens-like'
(ok) Lipochromis sp. nov. 'small obesoid'
Epiplatys olbrechtsi ssp. azureus
(done, hidden) Oncorhynchus nerka (FRASER RIVER, MIDDLE: Quesnel (summer))
Gastonia mauritiana => Polyscias maraisiana
Leucocharis pancheri => Leucocharis pancheri

** Dremomys rufigenis => Red-cheeked squirrel
** Dremomys pyrrhomerus => Red-cheeked squirrel

Walrus => type species in taxobox, not binomial

?? Okapia johnstoni=>Okapi - Redirect is to another binoimal

(ok?) Dipodomys margaritae=>Margarita Island kangaroo rat - Redirect is not to a bionomial (bi -> tri is caught)

(ok) Epinephelus cifuentesi (Gal�pagos vs Galápagos) // RedList csv is ANSI / Windows 1252, not Unicode

Subpops:
Centrophorus acus (Western Central Atlantic subpopulation)

Cycloramphidae wikilink Cycloramphinae // different spelling redirect (not common name)
Anura wikilink Anura (frog)  // disambig link

// (done) Holoaden bradei out of place (unsorted) (maybe an illusion)

// (done) "Not assigned"
// (done) "Cardinal (bird) species" => Cardinal species" 
// (done) too much space after Morogoro pretty grasshopper (Thericleidae), before Phasmatodea

TODO:
// (done) Huso huso => Beluga (sturgeon)
// (done) Huso dauricus => Kaluga (fish)


Blurbs under headings:
Of the 100 blah species which have been assessed by the IUCN, 55 are threatened with extinction. The 12 critically endangered of these are listed. 2 blah are listed as "data deficient". 
All the [assessed/threatened/crit] are endemic to Antarctica [and/or] China.
44 species of the 230 which have been assessed are critically endangered. An additional 12 species are classified as "Data Deficient".
(important) There are 12 critically endangered [category] ([commoon name]) species, and 2 critically endangered subspecies: blah and blah.
(important) 3 stocks/populations have been assessed as critically endangered:

Panthera pardus nimr = Arabian leopard
Pennatomys nivalis => Pennatomys // only species in the genus


Pygmy sunfish species
Some researchers believe they are related to sticklebacks and pipefishes (order Syngnathiformes) rather than Perciformes.

static string[] animaliaBreakdown = new string[] { 
    "Mollusca", // phylum
    "fish: Actinopterygii + Chondrichthyes + Myxini + Cephalaspidomorphi + Sarcopterygii" // classes within Animalia (kingdom),Chordata (phylum)
    "Insecta", // class within Arthropoda (phylum)
    "Arachnida", // // class within Arthropoda,
    "Arthropoda -Insecta -Arachnida", // link to them // maxillopoda, malacostraca, branchiopoda, diplopoda, chilopoda, ostracoda
    "invertibrate: (+Insecta) (+Arachnida) +Cnidaria +Annelida +Onychophora +Nemertina " // worms and jellies, link to others
    "crustaceans: +Branchiopoda +Ostracoda +Malacostraca" // classes not found in iucn CR: +Remipedia +Cephalocarida

    // - Mammalia (class) - Reptilia (class) - Aves (class) - Amphibia (class)
}

{"Actinopterygii": "ray-finned fishes", // pl
    "Chondrichthyes": "cartilaginous fish", "includes sharks, rays, chimaeras", // note: chimaeras may be moved
    // "Myxini": "Hagfish" // name of wikipedia page already
    "Cephalaspidomorphi", "lamprey" // lampreys moved to Petromyzontiformes (order. the rest of the class is long extinct fossils) // //"Hyperoartia"
    "Sarcopterygii", "lobe-finned fish", "includes lungfish, coelacanths. Tetrapods are excluded here."
    "Cnidaria",, "includes sea anemones, corals, and sea jellies"
    "Annelida", "annelid (segmented worm)", "includes ragworms, earthworms and leeches."
    "Onychophora", "velvet worm"
    "Nemertina", "ribbon worm" // = Nemertea
    "Arachnida", "arachnid", "including spiders, scorpions, harvestmen, ticks, mites, and solifuges"

"Caprimulgiformes" "including the potoos, nightjars, and eared-nightjars"
}
*/

//tracheophyta - Vascular plants, "also known as tracheophytes or higher plants"
