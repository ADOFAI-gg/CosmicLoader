using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CosmicLoader.Starter;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MethodAttributes = dnlib.DotNet.MethodAttributes;
using MethodImplAttributes = dnlib.DotNet.MethodImplAttributes;

namespace CosmicLoader.Installer
{
    internal class Program
    {
        public static string AdofaiPath = Utils.FindAdofaiPath();

        static Program() { }

        static void Main(string[] args)
        {
            var staticctor = typeof(Program).GetConstructor(Type.EmptyTypes);
            var input = Console.ReadLine();
            switch (input)
            {
                case "0":
                    SetPath();
                    break;

                case "1":
                    Install();
                    break;

                case "2":
                    Uninstall();
                    break;

                case "3":
                    Update();
                    break;
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void Install()
        {
            string targetDll = Utils.TargetDLL(AdofaiPath);
            var orig = targetDll + "_original";
            if (File.Exists(orig))
            {
                File.Copy(orig, targetDll, true);
            }
            else
            {
                File.Copy(targetDll, orig, false);
            }

            var modCtx = ModuleDef.CreateModuleContext();
            ModuleDefMD module;
            using (var stream = File.Open(targetDll, FileMode.Open, FileAccess.ReadWrite))
            {
                var bytes = new byte[stream.Length];
                int read = stream.Read(bytes, 0, bytes.Length);
                if (read != bytes.Length)
                {
                    throw new Exception("Could not read all bytes from file");
                }
                module = ModuleDefMD.Load(bytes, modCtx);
            }

            var asm = module.Assembly;
            Console.WriteLine("Assembly: {0}", asm);

            var type = asm.Find("UnityEngine.Canvas", true);
            Console.WriteLine("Type: {0}", type);

            var cctor = type.Methods.FirstOrDefault(m => m.Name == ".cctor");
            if (cctor != null)
            {
                type.Methods.Remove(cctor);
            }

            var method = new MethodDefUser(".cctor", MethodSig.CreateStatic(module.CorLibTypes.Void));
            method.Attributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig;
            method.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            type.Methods.Add(method);

            var loadType = new TypeRefUser(module, "System.Reflection", "Assembly", module.CorLibTypes.AssemblyRef);
            var loadMethod = new MemberRefUser(module, "LoadFrom", MethodSig.CreateStatic(module.CorLibTypes.Void), loadType);

            var mod = ModuleDefMD.Load(typeof(Injection).Module);
            var injtype = new TypeRefUser(module, "CosmicLoader.Starter", "Injection", mod.Assembly.ToAssemblyRef());
            var entry = new MemberRefUser(module, "Start", MethodSig.CreateStatic(module.CorLibTypes.Void), injtype);
            Console.WriteLine("Entry: {0}", entry);
            var il = new CilBody();
            method.Body = il;

            il.Instructions.Add(OpCodes.Call.ToInstruction(entry));
            il.Instructions.Add(OpCodes.Ret.ToInstruction());

            module.Write(targetDll);
            File.Copy("CosmicLoader.dll", Path.Combine(AdofaiPath, "A Dance of Fire and Ice_Data", "Managed", "CosmicLoader.dll"), true);
        }

        static void Uninstall()
        {
            string targetDll = Utils.TargetDLL(AdofaiPath);
            var orig = targetDll + "_original";
            if (File.Exists(orig))
            {
                File.Copy(orig, targetDll, true);
                File.Delete(orig);
                return;
            }

            File.Copy(targetDll, orig, false);

            var modCtx = ModuleDef.CreateModuleContext();
            ModuleDefMD module;
            using (var stream = File.Open(targetDll, FileMode.Open, FileAccess.ReadWrite))
            {
                var bytes = new byte[stream.Length];
                int read = stream.Read(bytes, 0, bytes.Length);
                if (read != bytes.Length)
                {
                    throw new Exception("Could not read all bytes from file");
                }
                module = ModuleDefMD.Load(bytes, modCtx);
            }

            var asm = module.Assembly;
            Console.WriteLine("Assembly: {0}", asm);

            var type = asm.Find("UnityEngine.Canvas", true);
            Console.WriteLine("Type: {0}", type);

            var cctor = type.Methods.FirstOrDefault(m => m.Name == ".cctor");
            if (cctor != null)
            {
                type.Methods.Remove(cctor);
            }

            module.Write(targetDll);
        }

        static void SetPath()
        {

        }

        static void Update()
        {

        }

        static void Check()
        {

        }
    }

    internal class Utils
    {
        private static string[] _adofaiPaths = {
            @"C:\Program Files (x86)\Steam\steamapps\common\A Dance of Fire and Ice",
            @"C:\Program Files\Steam\steamapps\common\A Dance of Fire and Ice",
        };

        public static string FindAdofaiPath() => _adofaiPaths.FirstOrDefault(Directory.Exists);

        public static string TargetDLL(string adofaiPath)
        {
            return Path.Combine(adofaiPath, "A Dance of Fire and Ice_Data", "Managed", "UnityEngine.UIModule.dll");
        }
    }
}