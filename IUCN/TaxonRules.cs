using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {

    public class TaxonRules {

        public enum Field { None, commonName, commonPlural, adj, forcesplit, splitoff, below, belowRank, includes, comprises, means, wikilink }

        public Dictionary<Field, string> items = new Dictionary<Field, string>();

        public TaxonRules() {

        }

        public string this[Field key] {
            get {
                return GetField(key);
            }
            set {
                SetField(key, value);
            }
        }

        // also accessable with []
        public void SetField(Field field, string value) {
            items[field] = value;
        }

        public string GetField(Field field) {
            //return items[field];
            return items.GetOrDefault(field);
        }

        public string commonName {
            get { return items.GetOrDefault(Field.commonName); }
            set { items[Field.commonName] = value; }
        }

        public string commonPlural {
            get { return items.GetOrDefault(Field.commonPlural); }
            set { items[Field.commonPlural] = value; }
        }

        public string adj {
            get { return items.GetOrDefault(Field.adj); }
            set { items[Field.adj] = value; }
        }

        public string forceSplitString {
            get { return items.GetOrDefault(Field.forcesplit); }
            set { items[Field.forcesplit] = value; }
        }

        public bool forceSplit {
            get { return !string.IsNullOrWhiteSpace(items.GetOrDefault(Field.forcesplit));  }
            set { items[Field.forcesplit] = (value ? "true" : null); }
        }

        public string splitOff {
            get { return items.GetOrDefault(Field.splitoff); }
            set { items[Field.splitoff] = value; }
        }

        public string below {
            get { return items.GetOrDefault(Field.below); }
            set { items[Field.below] = value; }
        }

        public string belowRank {
            get { return items.GetOrDefault(Field.belowRank); }
            set { items[Field.belowRank] = value; }
        }

        public string includes {
            get { return items.GetOrDefault(Field.includes); }
            set { items[Field.includes] = value; }
        }
        public string comprises {
            get { return items.GetOrDefault(Field.comprises); }
            set { items[Field.comprises] = value; }
        }
        public string means {
            get { return items.GetOrDefault(Field.means); }
            set { items[Field.means] = value; }
        }
        public string wikilink {
            get { return items.GetOrDefault(Field.wikilink); }
            set { items[Field.wikilink] = value; }
        }
    }

    
}
