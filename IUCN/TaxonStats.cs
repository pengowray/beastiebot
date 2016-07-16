using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {
    public class TaxonStats {
        public TaxonNode node { get; private set; }
        public RedStatus status { get; private set; } // status filter

        public TaxonStats(TaxonNode node, RedStatus statusFilter) {
            this.node = node;
            this.status = statusFilter;
            CalcBasics();
        }

        // ////////
        // Calculated Stats
        // ////////

        // Calculated Stats: bitri stats

        // no bitris in this or any child nodes? was: "!anything"
        public bool noBitris { get; private set; }
        public int bitris { get; private set; }
        //public int childrenWithBitris { get; private set; } // number of direct child w bitris
        private float _averageDivisonSize;
        public float averageDivisonSize {
            get {
                if (_averageDivisonSize == 0) {
                    node.children.Average(n => n.GetStats(status).bitris);
                }
                return _averageDivisonSize;
            }
        }

        // Calculated Stats: species stats

        public int species { get; private set; }

        public int subspecies { get; private set; } // aka infraspecies. excludes subpopulations. includes varieties (for historical reasons)
        public int subspecies_varieties { get; private set; } // number of "subspecies" which are actually varieties
        public int subspecies_actual_subsp {
            get {
                return subspecies - subspecies_varieties;
            }
        }

        //note: there are no subpopulations of plants (and therefore of varieties) listed (as of 2016-1)

        public int subpops_species { get; private set; }
        public int subpops_subspecies { get; private set; }
        public int subpops_total { get; private set; } // total of species + subspecies subpopulations

        // functions

        private void CalcBasics() {
            noBitris = (node.DeepBitriCount(status, 1) == 0);
            if (noBitris)
                return; // maybe?

            bitris = node.DeepBitriCount(status);
            species = node.DeepSpeciesCount(status);
            subspecies = node.DeepBitriCountWhere(b => !b.isStockpop && b.isTrinomial && b.Status.MatchesFilter(status));
            subspecies_varieties = node.DeepBitriCountWhere(b => !b.isStockpop && b.isVariety && b.Status.MatchesFilter(status));

            //subpopulations  = node.DeepBitriCountWhere(b => b.isStockpop && b.Status.MatchesFilter(status));
            subpops_species    = node.DeepBitriCountWhere(b => b.isStockpop && !b.isTrinomial && b.Status.MatchesFilter(status));
            subpops_subspecies = node.DeepBitriCountWhere(b => b.isStockpop &&  b.isTrinomial && b.Status.MatchesFilter(status));
            subpops_total = subpops_species + subpops_subspecies;

            
        }

        

    }
}


//TODO: only include some division: ?
//enum Inclusions { all, species, subspecies, stockpops, stockpopSp, stockpopSsp }
//bool includeBinomials;
//bool includeTrinomials;
//bool includeNormals;
//bool includeStockpops;

