![](Assets/XLua/Doc/xLua.png)

![](https://img.shields.io/badge/release-v2.1.5-blue.png)

## Unity3D下Lua编程支持

xLua为Unity3D增加Lua脚本编程的能力，进而提供代码逻辑增量更新的可能。当然不仅仅如此，在coco2dx上的实践告诉我们，以Lua为主打语言的游戏客户端编程是可行的。

## xLua的突破

xLua在功能、性能、易用性都有不少突破，这几方面分别最具代表性的是：

* Unity3D全平台热补丁技术，可以运行时把C#实现（方法，操作符，属性，事件，构造函数，析构函数，支持泛化）替换成lua实现；
* 自定义struct，枚举在Lua和C#间传递无C# gc alloc；
* 编辑器下无需生成代码，开发更轻量；

更详细的特性、平台支持介绍请看[这里](Assets/XLua/Doc/features.md)。

## 安装

直接解压到Assets下可用。第一次使用建议把例子包也安装，运行看看效果。

如果希望安装到其它目录，请看[FAQ](Assets/XLua/Doc/faq.md)相关介绍。

## lua5.3 vs luajit

xLua有两个版本，分别集成了lua5.3和luajit，一个项目只能选择其一。这两个版本C#代码是一样的，不同的是Plugins部分。

lua5.3的特性更丰富些，比如支持原生64位整数，支持苹果bitcode，支持utf8等。出现问题因为是纯c代码，也好定位。比起luajit，lua对安装包的影响也更小。

而luajit胜在性能，如果其jit不出问题的话，可以比lua高一个数量级。目前luajit作者不打算维护luajit，已经在找人接替其维护，所以其前途堪然。

项目可以根据自己情况判断哪个更适合。因为目前lua53版本使用较多，所以xLua工程Plugins目录下默认配套是lua53版本。

## 快速入门

一个完整的例子仅需3行代码：

下载xLua后解压到Unity工程Assets目录下，建一个MonoBehaviour拖到场景，在Start加入如下代码：

```csharp
Lua.LuaEnv luaenv = new XLua.LuaEnv();
luaenv.DoString("CS.UnityEngine.Debug.Log('hello world')");
luaenv.Dispose();
```

1、DoString参数为string，可输入任意合法的Lua代码，本示例在lua里调用C#的UnityEngine.Debug.Log打印了个日志。

2、一个LuaEnv实例对应Lua虚拟机，出于开销的考虑，建议全局唯一。

C#主动调用lua也很简单，比如要调用lua的系统函数，推荐方式是：

* 声明

```csharp
[XLua.CSharpCallLua]
public delegate double LuaMax(double a, double b);
```

* 绑定

```csharp
var max = luaenv.Global.GetInPath<LuaMax>("math.max");
```

* 调用

```csharp
Debug.Log("max:" + max(32, 12));
```

建议绑定一次，重复使用。生成了代码的话，调用max是不产生gc alloc的。

## 热补丁

xLua支持热补丁，这意味着你可以：

* 1、开发只用C#；
* 2、运行也是C#，性能可以秒杀lua；
* 3、出问题了才用Lua来改掉C#出问题的部位，下次整体更新时换回正确的C#；能做到用户不重启程序fix bug；

如果你仅仅希望用热更新来fix bug，这是强烈建议的做法。[这里](Assets/XLua/Doc/hotfix.md)是使用指南。

## 更多示例

* [01_Helloworld](Assets/XLua/Examples/01_Helloworld/): 快速入门的例子。
* [02_U3DScripting](Assets/XLua/Examples/02_U3DScripting/): 展示怎么用lua来写MonoBehaviour。
* [03_UIEvent](Assets/XLua/Examples/03_UIEvent/): 展示怎么用lua来写UI逻辑。
* [04_LuaObjectOrented](Assets/XLua/Examples/04_LuaObjectOrented/): 展示lua面向对象和C#的配合。
* [05_NoGc](Assets/XLua/Examples/05_NoGc/): 展示怎么去避免值类型的GC。
* [06_Coroutine](Assets/XLua/Examples/06_Coroutine/): 展示lua协程怎么和Unity协程相配合。
* [07_AsyncTest](Assets/XLua/Examples/07_AsyncTest/): 展示怎么用lua协程来把异步逻辑同步化。
* [08_Hotfix](Assets/XLua/Examples/08_Hotfix/): 热补丁的示例（需要开启热补丁特性，如何开启请看[指南](Assets/XLua/Doc/hotfix.md)）。
 
## 文档

* [XLua介绍.ppt](Assets/XLua/Doc/XLua介绍.ppt)：总体介绍文档。
* [XLua教程.doc](Assets/XLua/Doc/XLua教程.doc)：教程，其配套代码[这里](Assets/XLua/Tutorial/)。
* [XLua的配置.doc](Assets/XLua/Doc/XLua的配置.doc)：介绍如何配置xLua。
* [XLua增加删除第三方lua库.doc](Assets/XLua/Doc/XLua增加删除第三方lua库.doc)：如何增删第三方lua扩展库。
* [XLua API.doc](Assets/XLua/Doc/XLua_API.doc)：API文档。

