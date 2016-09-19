using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace beastie.beastieDB {
    // also: DeleteOld, DeleteAll, ContinueOnly, DeleteOldAndStart, StartThenReplace ...
    public enum JobAction { None, ContinueOrStartNewIfNone, StartNewIfNone, ForceStartNew }
    
    class ImportJob {
    }
}
