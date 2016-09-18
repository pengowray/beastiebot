using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {
    public class Blurb {

        public static string Percent(int count, int total) {
            double percent = (double)count / (double)total;
            return Percent(percent);
        }

        public static string Percent(double percent) {
            if (percent > .1f) { // e.g. 11%
                return percent.ToString("P0");
            } else if (percent > .01f) {  // e.g. 1.1%
                return percent.ToString("P1");
            } else {  // e.g. 0.11%
                return percent.ToString("P2");
            }
        }

    }
}
