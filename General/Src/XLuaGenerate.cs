using System;
using System.Linq;
using System.Reflection;
using CSObjectWrapEditor;
using System.IO;

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

            var assembly = Assembly.LoadFile(Path.GetFullPath(args[0]));
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
            }, assembly.GetTypes());
        }
    }
}
