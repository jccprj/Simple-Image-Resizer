using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple_Image_Resizer
{
    public class WorkerArguments
    {
        public String Path { get; set; }
        public String FileTypes { get; set; }
        public int Rate { get; set; }
    }
}
