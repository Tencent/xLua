一、目录说明

XLua\Doc\XLua介绍.ppt
XLua的介绍

XLua\Doc\XLua API.doc
XLua的API说明

XLua\Doc\关注性能的猛击这里.doc
如何利用代码生成来优化性能

XLua\Examples\01_Configure
示例，演示如何使用Lua作为配表数据加载。

XLua\Examples\02_U3DScripting
示例，演示完全使用Lua进行Unity程序开发的可能性，在程序中可以看到C#和Lua是如何互相访问的。

XLua\Examples\03_UIEvent
示例，演示Lua如何和uGUI配合使用，在程序中可以看到一个Lua函数直接注册到UI事件中。

XLua\Examples\04_GetADelegate
示例，演示Lua函数转换到一个含ref，out参数的delegate。

XLua\Gen
XLua生成的Wrapper代码的放置目录。

XLua\Src
XLua的C#代码。

二、注意事项！！
1、假如你发现XLua\Gen下的代码编译不过，请删除后，执行菜单Lua->Gen Lua Warp Files
2、代码生成相关请看位于XLua\Doc目录下的《关注性能的猛击这里.doc》一文。