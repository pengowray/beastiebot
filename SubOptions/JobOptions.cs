using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace beastie {
    class JobOptions {
        [Option('f', "force", HelpText = "If true, run the job even if it has previously completed successfully.")]
        public bool force { get; set; }

    }
}
