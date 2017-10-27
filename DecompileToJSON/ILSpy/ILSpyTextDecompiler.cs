using ICSharpCode.Decompiler;

using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecompileToJSON.ILSpy
{
    public class ILSpyTextDecompiler
    {
        public string Decompile(object @object)
        {
            if (@object == null) return String.Empty;
            Language l = new CSharpLanguage();

            //ITextOutput output = new RtfTextOutput();
            ITextOutput output = new PlainTextOutput();
            var options = new DecompilationOptions();

            if (@object is AssemblyDefinition)
                l.DecompileAssembly((AssemblyDefinition)@object, output, options);
            else if (@object is TypeDefinition)
                l.DecompileType((TypeDefinition)@object, output, options);
            else if (@object is MethodDefinition)
                l.DecompileMethod((MethodDefinition)@object, output, options);
            else if (@object is FieldDefinition)
                l.DecompileField((FieldDefinition)@object, output, options);
            else if (@object is PropertyDefinition)
                l.DecompileProperty((PropertyDefinition)@object, output, options);
            else if (@object is EventDefinition)
                l.DecompileEvent((EventDefinition)@object, output, options);
            else if (@object is AssemblyNameReference)
            {
                output.Write("// Assembly Reference ");
                output.WriteDefinition(@object.ToString(), null);
                output.WriteLine();
            }
            else if (@object is ModuleReference)
            {
                output.Write("// Module Reference ");
                output.WriteDefinition(@object.ToString(), null);
                output.WriteLine();
            }
            else
            {
                output.Write(String.Format("// {0} ", @object.GetType().Name));
                output.WriteDefinition(@object.ToString(), null);
                output.WriteLine();
            }

            return output.ToString();
        }
    }
}
