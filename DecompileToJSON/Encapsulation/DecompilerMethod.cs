using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecompileToJSON.Encapsulation
{
    [DebuggerDisplay("{Name}")]
    public class DecompilerMethod : IDecompilerType
    {
        

        public string Name { get; set ; }
        public string Sourcecode { get ; set; }
    }
}
