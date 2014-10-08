﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using dnEditor.Forms;
using dnEditor.Misc.ILSpy;
using dnlib.DotNet;
using Mono.Cecil;

namespace dnEditor.Misc
{
    public static class MonoTranslator
    {
        public static AssemblyDefinition Translate(AssemblyDef assembly)
        {
            using (var assemblyStream = new MemoryStream())
            {
                assembly.Write(assemblyStream);

                assemblyStream.Position = 0;

                AssemblyDefinition newAssembly = AssemblyDefinition.ReadAssembly(assemblyStream);

                return newAssembly;
            }
        }

        public class MonoMethod
        {
            private readonly string _methodPath;
            private readonly string _typePath;

            public MonoMethod(MethodDef method)
            {
                _methodPath = method.FullName;
                _typePath = method.DeclaringType.FullName;
            }

            public MethodDefinition Method(AssemblyDefinition assembly)
            {
                TypeDefinition type = assembly.MainModule.GetType(_typePath);
                MethodDefinition method = type.Methods.First(m => m.FullName == _methodPath);

                return method;
            }
        }

        public class Decompiler
        {
            private readonly BackgroundWorker _worker = new BackgroundWorker();

            public void Start()
            {
                _worker.DoWork += worker_DoWork;
                _worker.RunWorkerAsync();
            }

            private void worker_DoWork(object sender, DoWorkEventArgs e)
            {
                MainForm.RtbILSpy.BeginInvoke(new MethodInvoker(() =>
                {
                    MainForm.RtbILSpy.Clear();
                    MainForm.RtbILSpy.Text = Environment.NewLine + " Decompiling...";
                }));

                try
                {
                    var assembly = Translate(MainForm.CurrentAssembly.Assembly);

                    var dnMethod = new MonoMethod(MainForm.CurrentAssembly.Method.NewMethod);
                    object method = dnMethod.Method(assembly);

                    if (method == null)
                        return;

                    var mtp = (IMetadataTokenProvider)method;
                    method = assembly.MainModule.LookupToken(mtp.MetadataToken);

                    if (method == null || string.IsNullOrEmpty(method.ToString()))
                    {
                        MainForm.RtbILSpy.BeginInvoke(new MethodInvoker(
                            () =>
                            {
                                MainForm.RtbILSpy.Clear();
                                MainForm.RtbILSpy.Text = "Could not find member by Metadata Token!";
                            }));

                        return;
                    }

                    var bar = GlobalAssemblyResolver.Instance;
                    bool savedRaiseResolveException = true;
                    try
                    {
                        if (bar != null)
                        {
                            savedRaiseResolveException = bar.RaiseResolveException;
                            bar.RaiseResolveException = false;
                        }

                        var il = new ILSpyDecompiler();
                        string source = il.Decompile(method);

                        MainForm.RtbILSpy.BeginInvoke(new MethodInvoker(() =>
                        {
                            MainForm.RtbILSpy.Clear(); 
                            MainForm.RtbILSpy.Rtf = source;
                        }));
                    }
                    finally
                    {
                        if (bar != null)
                            bar.RaiseResolveException = savedRaiseResolveException;
                    }
                }
                catch (Exception o)
                {
                    MainForm.RtbILSpy.BeginInvoke(new MethodInvoker(
                        () =>
                        {
                            MainForm.RtbILSpy.Clear();
                            MainForm.RtbILSpy.Text = "Decompilation unsuccessful!" + Environment.NewLine +
                                                    Environment.NewLine + o.Message;
                        }));
                }
            }
        }
    }
}
