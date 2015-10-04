using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockViewApp.WP81.Contracts
{
    public class LockViewRequestMetadata
    {
        public enum TaskPhase
        {
            /// <summary>
            /// The device is capable of finishing the task in one run
            /// </summary>
            Complete,
            /// <summary>
            /// The task is in tick phase
            /// </summary>
            Tick,
            /// <summary>
            /// The task is in tock phase.
            /// </summary>
            Tack,
            Toe,
        }
        public string RequestLanguage = "En-Us";
        public int ImageBytesPerRequest = 1024;
        public double ScaleFactor;
        public string PersistFileName { get; set; }
        public TaskPhase Phase { get; set; }
        public LockViewRequestMetadata()
        {
            PersistFileName = "MyBg.jpeg";
            Phase = TaskPhase.Complete;
        }
    }
}
