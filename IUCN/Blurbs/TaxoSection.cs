using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie  {
    public class TaxoSection {
        public string title; // e.g. "Species", "Subspecies" or "Possibly extinct varieties"
        public IEnumerable<IUCNBitri> list;
        public bool isDefault = false; // default sections don't need a title if they're the only section (e.g. "Species")

        public TaxoSection(string title, IEnumerable<IUCNBitri> list, bool isDefault = false) {
            this.title = title;
            this.list = list;
            this.isDefault = isDefault;
        }


    }
}
