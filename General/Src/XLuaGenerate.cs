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
            Generator.GenAll("../Assets/XLua/Src/Editor/Template/", assembly.GetTypes());
        }
    }
}
