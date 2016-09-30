using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace beastie {
    class JobsCleanOptions {
        [Option('m', "markonly", HelpText = "If set, only mark old jobs as complete (with a completion date), but don't delete the data.")]
        public bool markonly { get; set; }

        [Option('k', "keep-main-entry", HelpText = "If set, keep the main job records (DataImport) even after deleting the data, e.g. to check the logs. Note: child records still deleted. Also option isn't saved, and job will be deleted if job-clean is run again without this option.")]
        public bool keepMainEntry { get; set; }

        [Option('i', "id", HelpText = "id of job (DataImport) to delete (along with its children). [currently ignored with job-clean]")]
        public long? id { get; set; }
    }
}
