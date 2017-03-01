## xLua通用版本

xLua通用版本致力于在C#环境提供lua脚本支持。相比Unity版本，仅去掉了诸如print重定向到Console窗口，Unity专用的脚本加载器。其它的一切功能都保留。特性列表请看[这里](../Assets/XLua/Doc/features.md)。

## 如何使用

将XLua.Mini.dll放入工程，对应版本的xlua本地动态库放到能通过pinvoke加载的路径下（比如程序执行目录）。

## 生成代码[可选]

XLua.Mini.dll是通过反射来实现lua与C#间的交互，需要更高性能，可以通过生成代码获得。

1、按教程[XLua的配置.doc](../Assets/XLua/Doc/XLua的配置.doc)配置好要生成的类型；

2、重新编译后，用配套的工具XLuaGenerate对工程的编译结果（exe或者dll）执行代码生成：XLuaGenerate xxx.exe/xxx.dll，生成代码会放在当前目录下的Gen目录。

3、新建一个和原来一样的工程，添加XLUA_GENERAL宏

4、删除XLua.Mini.dll，加入XLua的配套源码包(发布包的Src目录)，加入步骤2的生成代码；

5、这工程生成exe或者dll已经通过生成代码适配。

## Hotfix

对已经生成了代码的exe或者dll，用工具XLuaHotfixInject执行注入即可，Hotfix特性的详细使用请看[Hotfix操作指南](../Assets/XLua/Doc/hotfix.md)


## 快速入门

~~~csharp

using XLua;

public class XLuaTest
{
    public static void Main()
    {
        LuaEnv luaenv = new LuaEnv();
        luaenv.DoString("CS.System.Console.WriteLine('hello world')");
        luaenv.Dispose();
    }
}

~~~


