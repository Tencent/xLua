using UnityEngine;
using System.Collections.Generic;
using XLua;
using System.IO;
using System.Text;
using System.Linq;
using CSObjectWrapEditor;

public class LinkXmlGen : ScriptableObject
{
    public TextAsset Template;

    public static IEnumerable<CustomGenTask> GetTasks(LuaEnv lua_env, UserConfig user_cfg)
    {
        LuaTable data = lua_env.NewTable();
        var assembly_infos = (from type in (user_cfg.ReflectionUse.Concat(user_cfg.LuaCallCSharp))
                              group type by type.Assembly.GetName().Name into assembly_info
                              select new { FullName = assembly_info.Key, Types = assembly_info.ToList()}).ToList();
        data.Set("assembly_infos", assembly_infos);

        yield return new CustomGenTask
        {
            Data = data,
            Output = new StreamWriter(GeneratorConfig.common_path + "/link.xml",
            false, Encoding.UTF8)
        };
    }

    [GenCodeMenu]//加到Generate Code菜单里头
    public static void GenLinkXml()
    {
        Generator.CustomGen(ScriptableObject.CreateInstance<LinkXmlGen>().Template.text, GetTasks);
    }
}
