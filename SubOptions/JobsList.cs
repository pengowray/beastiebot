using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace beastie {
    class JobsListOptions {
        [Option('i', "id", HelpText = "List child jobs of this job id (DataImport). Leave blank to list from top.")]
        public long? id { get; set; }

    }
}
