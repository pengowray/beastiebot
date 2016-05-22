using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace beastie.Accord {
    public static class SequenceUtil {
        public static int[] StringToSeq(string s) {
            s = s.ToLowerInvariant();
            Regex rgx = new Regex("[^a-z ]");
            s = rgx.Replace(s, "");

            // 0: start
            // 1: end
            // 2: space
            // 3 to 28: A-Z

            int[] seq = new int[s.Length + 2];
            seq[0] = 0;
            seq[s.Length - 1] = 1;
            int a = (int)'a';
            for (int i=0; i<s.Length; i++) {
                if (s[i] == ' ') {
                    seq[i + 1] = 2;
                } else {
                    seq[i + 1] = (int)s[i] - a + 3;
                }
            }

            return seq;
        }
    }
}
