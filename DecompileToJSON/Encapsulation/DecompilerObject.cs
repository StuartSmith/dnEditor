using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecompileToJSON.Encapsulation
{
    [DebuggerDisplay ("{Namespace} {Name}")]
    public class DecompilerObject
    {        
        public List<DecompilerField> Fields { get; set; } = new List<DecompilerField>();
        public List<DecompilerEvent> Events { get; set; } = new List<DecompilerEvent>();
        public List<DecompilerMethod> Methods { get; set; } = new List<DecompilerMethod>();
        //public List<DecompilerProperty> Properties { get; set; } = new List<DecompilerProperty>();


        public string Namespace { get; set; }
        public string Name { get; set; }
        
    }
}
