using System;
using CommandLine;
using CommandLine.Text;

namespace beastie {
    class RedlistNodesOptions : CommonSubOptions {

        //TODO
        [Option('o', "output", HelpText = "(NYI) Output file")]
        public string outputFile { get; set; }

        [Option('t', "taxon", HelpText = "The taxon to get the descendents of (requred)")]
        public string taxon { get; set; }

        //TODO
        [Option('d', "depth", HelpText = "(NYI) The depth to display. 1 = child nodes (default), 2 = grandchild nodes, etc")]
        public int depth { get; set; }

        [Option('r', "useRules", HelpText = "true:Use the internal rules list (as used in wikipedia-redlist), false:show raw IUCN data")]
        public bool useRules { get; set; }
    }
}

