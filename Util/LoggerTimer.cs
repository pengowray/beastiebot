using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {
    public class LoggerTimer {
        DateTime startTime;
        DateTime lastLog;
        string lastMessage = null;

        public LoggerTimer() {
            startTime = DateTime.Now;
            lastLog = startTime;
        }

        public void Log(string message) {
            DateTime time = DateTime.Now;
            var totalElapsed = time.Subtract(startTime);
            var elapsedSinceLast = time.Subtract(lastLog);

            if (lastMessage != null) {
                string doneMessage = string.Format("[{0}] (+{1}) Done: {2}\n",
                    DateTime.UtcNow.ToString("s"),
                    elapsedSinceLast.ToString(),
                    lastMessage);

                Console.WriteLine(doneMessage);
            }

            totalElapsed.ToString();
            string fullMessage = string.Format("[{0}] ={1} Start: {2}\n",
                DateTime.UtcNow.ToString("s"),
                totalElapsed.ToString(),
                message);

            //Console.Error.WriteLine(fullMessage);
            Console.WriteLine(fullMessage);

            lastLog = time;
            lastMessage = message;
        }

    }
}
