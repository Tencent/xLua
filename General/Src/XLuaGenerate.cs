using System;
using System.Linq;
using System.Reflection;
using CSObjectWrapEditor;
using System.IO;
using System.Collections.Generic;

namespace XLua
{
    public class XLuaGenerate
    {
        public static void Useage()
        {
            Console.WriteLine("XLuaGenerate assmbly_path");
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Useage();
                return;
            }

            List<string> assemblyPathList = args.TakeWhile(path => 
                path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)).ToList();

            if (args.Length > assemblyPathList.Count)
            {
                GeneratorConfig.common_path = args[assemblyPathList.Count];
            }

            if (args.Length > assemblyPathList.Count + 1)
            {
                List<string> search_paths = args.Skip(assemblyPathList.Count + 1).ToList();
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler((object sender, ResolveEventArgs rea) =>
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
            }

            var allTypes = assemblyPathList.Select(path => Assembly.Load(File.ReadAllBytes(Path.GetFullPath(path))))
                .SelectMany(assembly => assembly.GetTypes());
            Generator.GenAll(new XLuaTemplates()
            {
                LuaClassWrap = new XLuaTemplate()
                {
                    name = "LuaClassWrap",
                    text = global::XLuaGenerate.Src.XLuaTemplates.LuaClassWrap_tpl,
                }, 
                LuaDelegateBridge = new XLuaTemplate()
                {
                    name = "LuaDelegateBridge",
                    text = global::XLuaGenerate.Src.XLuaTemplates.LuaDelegateBridge_tpl,
                },
                LuaDelegateWrap = new XLuaTemplate()
                {
                    name = "LuaDelegateWrap",
                    text = global::XLuaGenerate.Src.XLuaTemplates.LuaDelegateWrap_tpl,
                },
                LuaEnumWrap = new XLuaTemplate()
                {
                    name = "LuaEnumWrap",
                    text = global::XLuaGenerate.Src.XLuaTemplates.LuaEnumWrap_tpl,
                },
                LuaInterfaceBridge = new XLuaTemplate()
                {
                    name = "LuaInterfaceBridge",
                    text = global::XLuaGenerate.Src.XLuaTemplates.LuaInterfaceBridge_tpl,
                },
                LuaRegister = new XLuaTemplate()
                {
                    name = "LuaRegister",
                    text = global::XLuaGenerate.Src.XLuaTemplates.LuaRegister_tpl,
                },
                LuaWrapPusher = new XLuaTemplate()
                {
                    name = "LuaWrapPusher",
                    text = global::XLuaGenerate.Src.XLuaTemplates.LuaWrapPusher_tpl,
                },
                PackUnpack = new XLuaTemplate()
                {
                    name = "PackUnpack",
                    text = global::XLuaGenerate.Src.XLuaTemplates.PackUnpack_tpl,
                },
                TemplateCommon = new XLuaTemplate()
                {
                    name = "TemplateCommon",
                    text = global::XLuaGenerate.Src.XLuaTemplates.TemplateCommon_lua,
                },
            }, allTypes);
        }
    }
}
