using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace XLua
{
    public class XLuaHotfixInject
    {
        public static void Useage()
        {
            Console.WriteLine("XLuaHotfixInject assmbly_path [search_path1, search_path2 ...]");
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Useage();
                return;
            }

            try
            {
                var assmbly_path = Path.GetFullPath(args[0]);
                AppDomain currentDomain = AppDomain.CurrentDomain;
                List<string> search_paths = args.Skip(1).ToList();
                currentDomain.AssemblyResolve += new ResolveEventHandler((object sender, ResolveEventArgs rea) =>
                {
                    foreach (var search_path in search_paths)
                    {
                        string assemblyPath = Path.Combine(search_path, new AssemblyName(rea.Name).Name + ".dll");
                        if (File.Exists(assemblyPath))
                        {
                            return Assembly.Load(File.ReadAllBytes(assemblyPath));
                        }
                    }
                    return null;
                });
                var assembly = Assembly.Load(File.ReadAllBytes(assmbly_path));
                Hotfix.Config(assembly.GetTypes());
                Hotfix.HotfixInject(assmbly_path, args.Skip(1));
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception in hotfix inject: " + e);
            }
        }
    }
}
