![](Assets/XLua/Doc/xLua.png)

[![license](http://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Tencent/xLua/blob/master/LICENSE.TXT)
[![release](https://img.shields.io/badge/release-v2.1.12-blue.svg)](https://github.com/Tencent/xLua/releases)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/Tencent/xLua/pulls)
[![Build status](https://travis-ci.org/Tencent/xLua.svg?branch=master)](https://travis-ci.org/Tencent/xLua)

[(English Documents Available)](README_EN.md)

## C#下Lua编程支持

xLua为Unity、 .Net、 Mono等C#环境增加Lua脚本编程的能力，借助xLua，这些Lua代码可以方便的和C#相互调用。

## xLua的突破

xLua在功能、性能、易用性都有不少突破，这几方面分别最具代表性的是：

* 可以运行时把C#实现（方法，操作符，属性，事件等等）替换成lua实现；
* 出色的GC优化，自定义struct，枚举在Lua和C#间传递无C# gc alloc；
* 编辑器下无需生成代码，开发更轻量；

更详细的特性、平台支持介绍请看[这里](Assets/XLua/Doc/features.md)。

## 安装

打开zip包，你会看到一个Assets目录，这目录就对应Unity工程的Assets目录，保持这目录结构放到你的Unity工程。

如果希望安装到其它目录，请看[FAQ](Assets/XLua/Doc/faq.md)相关介绍。

## 文档

* [常见问题解答](Assets/XLua/Doc/faq.md)：常见问题都总结在这里，初使用大多数问题都可以在这里找到答案。
* (必看)[XLua教程](Assets/XLua/Doc/XLua教程.md)：教程，其配套代码[这里](Assets/XLua/Tutorial/)。
* (必看)[XLua的配置](Assets/XLua/Doc/configure.md)：介绍如何配置xLua。
* [热补丁操作指南](Assets/XLua/Doc/hotfix.md)：介绍如何使用热补丁特性。
* [XLua增加删除第三方lua库](Assets/XLua/Doc/XLua增加删除第三方lua库.md)：如何增删第三方lua扩展库。
* [XLua API](Assets/XLua/Doc/XLua_API.md)：API文档。
* [生成引擎二次开发指南](Assets/XLua/Doc/custom_generate.md)：介绍如何做生成引擎的二次开发。

## 快速入门

一个完整的例子仅需3行代码：

安装好xLua，建一个MonoBehaviour拖到场景，在Start加入如下代码：

```csharp
XLua.LuaEnv luaenv = new XLua.LuaEnv();
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

* 侵入性小，老项目原有代码不做任何调整就可使用。
* 运行时影响小，不打补丁基本和原有程序一样。
* 出问题了可以用Lua来打补丁，这时才会走到lua代码逻辑；

[这里](Assets/XLua/Doc/hotfix.md)是使用指南。

## lua5.3 vs luajit

xLua有两个版本，分别集成了lua5.3和luajit，一个项目只能选择其一。这两个版本C#代码是一样的，不同的是Plugins部分。

lua5.3的特性更丰富些，比如支持原生64位整数，支持苹果bitcode，支持utf8等。出现问题因为是纯c代码，也好定位。比起luajit，lua对安装包的影响也更小。

而luajit胜在性能，如果其jit不出问题的话，可以比lua高一个数量级。目前luajit作者不打算维护luajit，在找人接替其维护，后续发展不太明朗。

项目可以根据自己情况判断哪个更适合。因为目前lua53版本使用较多，所以xLua工程Plugins目录下默认配套是lua53版本。

## 更多示例

* [01_Helloworld](Assets/XLua/Examples/01_Helloworld/): 快速入门的例子。
* [02_U3DScripting](Assets/XLua/Examples/02_U3DScripting/): 展示怎么用lua来写MonoBehaviour。
* [03_UIEvent](Assets/XLua/Examples/03_UIEvent/): 展示怎么用lua来写UI逻辑。
* [04_LuaObjectOrented](Assets/XLua/Examples/04_LuaObjectOrented/): 展示lua面向对象和C#的配合。
* [05_NoGc](Assets/XLua/Examples/05_NoGc/): 展示怎么去避免值类型的GC。
* [06_Coroutine](Assets/XLua/Examples/06_Coroutine/): 展示lua协程怎么和Unity协程相配合。
* [07_AsyncTest](Assets/XLua/Examples/07_AsyncTest/): 展示怎么用lua协程来把异步逻辑同步化。
* [08_Hotfix](Assets/XLua/Examples/08_Hotfix/): 热补丁的示例（需要开启热补丁特性，如何开启请看[指南](Assets/XLua/Doc/hotfix.md)）。
* [09_GenericMethod](Assets/XLua/Examples/09_GenericMethod/): 泛化函数支持的演示。
* [10_SignatureLoader](Assets/XLua/Examples/10_SignatureLoader/): 展示如何读取经数字签名的lua脚本，参见[数字签名](Assets/XLua/Doc/signature.md)的文档介绍。
* [11_RawObject](Assets/XLua/Examples/11_RawObject/): 当C#参数是object时，如何把一个lua number指定以boxing后的int传递过去。
* [12_ReImplementInLua](Assets/XLua/Examples/12_ReImplementInLua/): 展示如何将复杂值类型改为lua实现。

## 技术支持

一群：612705778(可能已满)

二群：703073338 

验证答案：有问题先找FAQ


