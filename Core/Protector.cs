using System;
using System.Linq;
using System.Reflection;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using IL_Emulator_Dynamic;
using Core.Properties;
using VMExample.Instructions;

namespace Core
{
    public class Protector
    {
        public static string path2;

        public static ModuleDefMD moduleDefMD { get; private set; }
        public static string name { get; private set; }

        public static byte[] Protect(byte[] assemblyData)
        {
            Console.WriteLine("Hi");

            name = "Eddy^CZ"; //Key

            moduleDefMD = ModuleDefMD.Load(assemblyData); //load the unprotected binary in dnlib
            asmRefAdder(); //this will resolve references (dlls) such as mscorlib and any dlls the unprotected binary may use. this will be to make sure resolving methods/types/fields in another assembly can be correctly identified
            Console.WriteLine("processing");
            Protection.MethodProccesor.ModuleProcessor(); //this will process the module
            Console.WriteLine("Passed processing");
            var nativePath = Resources.NativeEncoderx86;
            EmbeddedResource emv = new EmbeddedResource("X86", (nativePath));
            moduleDefMD.Resources.Add(emv);
            EmbeddedResource emv64 = new EmbeddedResource("X64", (Resources.NativeEncoderx64));
            moduleDefMD.Resources.Add(emv64);

            byte[] cleanConversion = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Runtime.dll"));

            EmbeddedResource embc = new EmbeddedResource("RT", cleanConversion); //Full
            moduleDefMD.Resources.Add(embc);

            EmbeddedResource emb = new EmbeddedResource("Eddy^CZ", Resources.XorMethod); //XorMethod
            moduleDefMD.Resources.Add(emb);


            /* Writing */
            ModuleWriterOptions modOpts = new ModuleWriterOptions(moduleDefMD);
            modOpts.MetaDataOptions.Flags =
             MetaDataFlags
              .PreserveAll; //we need to preserve all metadata tokens, otherwise resolving tokens to methods will not match the originals
            modOpts.MetaDataLogger =
             DummyLogger
              .NoThrowInstance; //since we make an unverifiable module dnlib will throw an exception. the reason we do this is because when using publically available tools this may crash them when trying to save the module.
            MemoryStream mem = new MemoryStream();
            moduleDefMD.Write(mem, modOpts); //save the module.
            return mem.ToArray();
        }


        private static void asmRefAdder()
        {
            var asmResolver = new AssemblyResolver { EnableTypeDefCache = true };
            var modCtx = new ModuleContext(asmResolver);
            asmResolver.DefaultModuleContext = modCtx;
            var asmRefs = moduleDefMD.GetAssemblyRefs().ToList();
            moduleDefMD.Context = modCtx;
            foreach (var asmRef in asmRefs)
            {
                try
                {
                    if (asmRef == null)
                        continue;
                    var asm = asmResolver.Resolve(asmRef.FullName, moduleDefMD);
                    if (asm == null)
                        continue;
                    moduleDefMD.Context.AssemblyResolver.AddToCache(asm);

                }
                catch { }
            }
        }
    }
}
