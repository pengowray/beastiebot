using System;
using CommandLine;
using CommandLine.Text;

namespace beastie {
    class RedlistNodesOptions : CommonSubOptions {
        //TODO
        [Option('o', "output", HelpText = "Output file")]
        public string outputFile { get; set; }

        [Option('t', "taxon", HelpText = "The taxon to get the descendents of (requred)")]
        public string taxon { get; set; }

        [Option('d', "depth", HelpText = "The depth to display. 1 = child (default), 2 = grandchild nodes, etc")]
        public int depth { get; set; }


    }
}

