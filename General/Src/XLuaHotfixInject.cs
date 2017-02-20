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

            var assmbly_path = Path.GetFullPath(args[0]);
            var assembly = Assembly.Load(File.ReadAllBytes(assmbly_path));
            Hotfix.Config(assembly.GetTypes());
            Hotfix.HotfixInject(assmbly_path, args.Skip(1));
        }
    }
}
