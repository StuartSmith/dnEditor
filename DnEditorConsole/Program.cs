using DecompileToJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecompileToJsonConsole
{
    class Program
    {

        private const string ERROR_MSG = "Invalid command line for Reverse engineering code to JSON ...\n" +
                                         "Example command line :\n\n" +
                                        "/a:Path and name of Assembly file to decompile \n" +
                                        "/o:Path to where to Export JSON File will be exported to \n" +
                                        "/a:C:\\temp\\myAssembly.dll /o:C:\\temp\\"
                                        ;

        /// <summary>
        /// Arguments to Run 
        /// /a:Path to assembly file to decompile
        /// /o:Path to JSON file to create
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            ArgsHelper oArgsHelp = new ArgsHelper(args);

            System.ApplicationException ex = new System.ApplicationException(ERROR_MSG);
            if (args.Count() == 0)
                throw ex;

            string AssemblyFileAndPath = oArgsHelp["a"];
            string OutPutPath = oArgsHelp["o"];

            Decompiler decompiler = new Decompiler(AssemblyFileAndPath, OutPutPath);
            decompiler.Decompile();

        }
    }
}
