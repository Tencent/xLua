## 生成引擎二次开发

xLua的生成引擎支持二次开发，你可以利用它来生成一些文本类型的文件（比如代码，配置等）。xLua本身的link.xml文件的生成就是一个生成引擎插件做的。其它应用场景，比如生成Lua IDE的自动完成配置文件，都可以用这特性来完成。

## 总体介绍

插件需要提供两个东西：1、生成文件的模版；2、一个回调函数，该回调函数接受用户的配置，返回需要注入到模版的数据以及文件的输出流。

## 模版语法

模版语法很简单，只有三种元素：

* eval：语法是<%=exp%>，exp是任意表达式，将计算并以字符串形式输出exp的值；
* code：语法是<% if true then end%>，蓝色部分是任意lua代码，这些代码会执行；
* literal：除eval和code之外其它部分，literal原样输出。

示例：

```xml
<%
require "TemplateCommon"
%>

<linker>
<%ForEachCsList(assembly_infos, function(assembly_info)%>
	<assembly fullname="<%=assembly_info.FullName%>">
	    <%ForEachCsList(assembly_info.Types, function(type)
		%><type fullname="<%=type:ToString()%>" preserve="all"/>
		<%end)%>
	</assembly>
<%end)%>
</linker>
```

TemplateCommon有一些预定义的函数可以使用，比如ForEachCsList，可以搜索下工程的TemplateCommon.lua.txt看下有那些函数可以用，就普通的lua而已，你自己写一套也可以。

## API

```csharp
public static void CSObjectWrapEditor.Generator.CustomGen(string template_src, GetTasks get_tasks)
```

* template_src ： 模版的源码；
* get_tasks    ： 回调函数，类型是GetTasks，用来接受用户的配置，返回需要注入到模版的数据以及文件的输出流；

```csharp
public delegate IEnumerable<CustomGenTask> GetTasks(LuaEnv lua_env, UserConfig user_cfg);
```

* lua_env      ： LuaEnv对象，因为返回的模版数据需要放到LuaTable，需要用到LuaEnv.NewTable；
* user_cfg     ： 用户的配置；
* return       ： 返回值中，CustomGenTask代表的是一个生成文件，而IEnumerable类型表示同一个模版可以生成多个文件；

```csharp
public struct UserConfig
{
    public IEnumerable<Type> LuaCallCSharp;
    public IEnumerable<Type> CSharpCallLua;
    public IEnumerable<Type> ReflectionUse;
}
```

```csharp
public struct CustomGenTask
{
    public LuaTable Data;
    public TextWriter Output;
}
```

示例：

```csharp
public static IEnumerable<CustomGenTask> GetTasks(LuaEnv lua_env, UserConfig user_cfg)
{
    LuaTable data = lua_env.NewTable();
    var assembly_infos = (from type in user_cfg.ReflectionUse
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
```

* 这里只生成一个文件，故只返回一个CustomGenTask；
* data就是模版要使用的数据，这里塞了一个assembly_infos字段，这个字段如何使用可以回头看看模版部分；

## 标签

一般来说你可以通过MenuItem开一个菜单来执行触发自定义生成操作，但有时你希望生成操作直接由xLua的“Generate Code”菜单触发，你就需要用到CSObjectWrapEditor.GenCodeMenu

示例：

```csharp
[GenCodeMenu]//加到Generate Code菜单里头
public static void GenLinkXml()
{
    Generator.CustomGen(ScriptableObject.CreateInstance<LinkXmlGen>().Template.text, GetTasks);
}
```


ps：以上所有相关代码都在XLua\Src\Editor\LinkXmlGen目录下，也正是文章开头说的link.xml的生成功能的实现。
