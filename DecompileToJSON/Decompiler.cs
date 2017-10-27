using DecompileToJSON.Encapsulation;
using DecompileToJSON.ILSpy;
//using dnEditor.Misc;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecompileToJSON
{
    public class Decompiler
    {
        private string _PathAndAssemblyFile;
        private string _JsonOutputDir;

        private ModuleDefMD _manifestModule;
        private AssemblyDefinition _assemblyDef;
        private string[] _FilesToDecompile;

        public Decompiler(string PathAndAssemblyFile, string JsonOutputDir)
        {
            _PathAndAssemblyFile = PathAndAssemblyFile;

            _JsonOutputDir = JsonOutputDir;

            string DirectoryToSearch= Path.GetDirectoryName(PathAndAssemblyFile);
            string fileNameToSearchFor= Path.GetFileName(PathAndAssemblyFile);

            _FilesToDecompile =  Directory.GetFiles(DirectoryToSearch, fileNameToSearchFor);

            if (_FilesToDecompile.Length == 0)
                throw new Exception("Could not file any DLLs that match the pattern");
                
        }

        public void Decompile()
        {
            foreach (var filename in _FilesToDecompile)
            {
                Decompile(filename);
            }
        }

        private void Decompile(string PathAndAssemblyFile)
        {
            var decompilerObjects = new List<DecompilerObject>();

            //string version  = FileVersionInfo.GetVersionInfo(PathAndAssemblyFile).FileVersion;

            _manifestModule = ModuleDefMD.Load(File.ReadAllBytes(PathAndAssemblyFile));
            AssemblyDefinition assembly = Translate_From_ModuleDefMD_To_AssemblyDefinition(_manifestModule);


            if (_manifestModule.Types.Any())
            {
                foreach (TypeDef type in _manifestModule.Types.OrderBy(t => t.Name.ToLower()))
                {
                    var decompilerObject = new DecompilerObject();
                    decompilerObject.Namespace = type.Namespace;
                    decompilerObject.Name = type.Name;

                    PopulateEvents(decompilerObject, type);
                    PopulateFields(decompilerObject, type);
                    PopulateMethods(decompilerObject, type,assembly);
                    
                    decompilerObjects.Add(decompilerObject);
                }
            }
            
            var contentsToWriteToFile = Newtonsoft.Json.JsonConvert.SerializeObject(decompilerObjects,Newtonsoft.Json.Formatting.Indented);
            

            var fullpath = Path.GetDirectoryName(PathAndAssemblyFile);
            var fileName = Path.GetFileNameWithoutExtension(PathAndAssemblyFile);

            string JsonOutputDir = Path.Combine(new string[] { fullpath, "JSON" });
            if (!Directory.Exists(JsonOutputDir))
                Directory.CreateDirectory(JsonOutputDir);

            string JsonOuputFile = Path.Combine(new string[] { fullpath,"JSON", fileName + ".JSON" });

            contentsToWriteToFile = contentsToWriteToFile.Replace("\\r\\r\\n", Environment.NewLine);
            contentsToWriteToFile = contentsToWriteToFile.Replace("\\t", "\t");

            File.WriteAllText(JsonOuputFile, contentsToWriteToFile);

            Console.WriteLine("");
            Console.WriteLine(JsonOuputFile);
        }

        private void PopulateEvents(DecompilerObject decompilerObject, TypeDef type)
        {
            foreach (var EventDef in type.Events)
            {
                var decompilerEvent = new DecompilerEvent();
                decompilerEvent.Name = EventDef.Name;
                decompilerObject.Events.Add(decompilerEvent);
            }
        }

        private void PopulateFields(DecompilerObject decompilerObject, TypeDef type)
        {
            foreach (var fieldDef in type.Fields)
            {
                var decompilerField = new DecompilerField();
                decompilerField.Name = fieldDef.Name;
                decompilerObject.Fields.Add(decompilerField);
            }
        }

        private void PopulateMethods(DecompilerObject decompilerObject, TypeDef type, AssemblyDefinition assembly)
        {
            foreach (var methodDef in type.Methods)
            {
                Console.Write("...");
                var decompilerMethod = new DecompilerMethod();
                decompilerMethod.Name = methodDef.Name;

                object MonoMethod = Translate_From_DNLib_MethodDef_To_Mono_MethodDefinition(methodDef, assembly);

                if (MonoMethod != null)
                {
                    var mtp = (IMetadataTokenProvider)MonoMethod;
                    MonoMethod = assembly.MainModule.LookupToken(mtp.MetadataToken);
                    DefaultAssemblyResolver bar = GlobalAssemblyResolver.Instance;
                    bool savedRaiseResolveException = true;
                    try
                    {
                        if (bar != null)
                        {
                            savedRaiseResolveException = bar.RaiseResolveException;
                            bar.RaiseResolveException = false;
                        }
                        var il = new ILSpyTextDecompiler();

                        decompilerMethod.Sourcecode = il.Decompile(MonoMethod);
                        string srcCode = decompilerMethod.Sourcecode.Replace("\n\r", Environment.NewLine);
                        srcCode = srcCode.Replace("\n", Environment.NewLine);
                        decompilerMethod.Sourcecode = srcCode;
                    }
                    catch
                    {
                       
                    }
                }
                decompilerObject.Methods.Add(decompilerMethod);
            }

        }

        private MethodDefinition Translate_From_DNLib_MethodDef_To_Mono_MethodDefinition(MethodDef methodDef, AssemblyDefinition assembly)
        {
            try
            {
                string methodPath = methodDef.FullName;
                string typePath = methodDef.DeclaringType.FullName;

                TypeDefinition type = assembly.MainModule.GetType(typePath);
                MethodDefinition methodDefinition = type.Methods.First(m => m.FullName == methodPath);

                return methodDefinition;
            }
            catch
            {
                return null;
            }
        }

        private AssemblyDefinition Translate_From_ModuleDefMD_To_AssemblyDefinition(ModuleDefMD manifestModule)
        {
            using (var assemblyStream = new MemoryStream())
            {
                try
                {
                    if (manifestModule.IsILOnly)
                    {
                        var writerOptions = new ModuleWriterOptions(manifestModule);
                        writerOptions.Logger = DummyLogger.NoThrowInstance;
                        MetaDataOptions metaDataOptions = new MetaDataOptions();
                        metaDataOptions.Flags = MetaDataFlags.PreserveAll;
                        manifestModule.Write(assemblyStream, writerOptions);
                    }
                    else
                    {
                        var writerOptions = new NativeModuleWriterOptions(manifestModule);
                        writerOptions.Logger = DummyLogger.NoThrowInstance;
                        MetaDataOptions metaDataOptions = new MetaDataOptions();
                        metaDataOptions.Flags = MetaDataFlags.PreserveAll;
                        manifestModule.NativeWrite(assemblyStream, writerOptions);
                    }
                }
                catch (Exception)
                {
                    if (assemblyStream.Length == 0)
                        return null;
                }

                assemblyStream.Position = 0;
                AssemblyDefinition newAssembly = AssemblyDefinition.ReadAssembly(assemblyStream);

                return newAssembly;
            }
        }

        private void  WriteDecompiledObjectsToFile(string AssemblyFile,string OutputFolder, List<DecompilerObject> ObjectsToOutput )
        {

        }
    }
}
