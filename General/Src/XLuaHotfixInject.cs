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
            Console.WriteLine("XLuaHotfixInject assmbly_path id_map_file_path [cfg_assmbly2_path] [search_path1, search_path2 ...]");
        }

        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Useage();
                return;
            }

            try
            {
                var assmbly_path = Path.GetFullPath(args[0]);
                string cfg_append = null;
                if (args.Length > 2)
                {
                    cfg_append = Path.GetFullPath(args[2]);
                    if (!cfg_append.EndsWith(".data"))
                    {
                        cfg_append = null;
                    }
                }
                AppDomain currentDomain = AppDomain.CurrentDomain;
                List<string> search_paths = args.Skip(cfg_append == null ? 2 : 3).ToList();
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
                if (cfg_append != null)
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(cfg_append, FileMode.Open)))
                    {
                        int count = reader.ReadInt32();
                        for(int i = 0; i < count; i++)
                        {
                            string k = reader.ReadString();
                            int v = reader.ReadInt32();
                            if (!Hotfix.hotfixCfg.ContainsKey(k))
                            {
                                Hotfix.hotfixCfg.Add(k, v);
                            }
                        }
                    }
                }
                Hotfix.HotfixInject(assmbly_path, args.Skip(cfg_append == null ? 2 : 3), args[1]);
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception in hotfix inject: " + e);
            }
        }
    }
}
