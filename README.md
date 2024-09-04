![](Assets/XLua/Doc/xLua.png)

[![license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Tencent/xLua/blob/master/LICENSE.TXT)
[![release](https://img.shields.io/badge/release-v2.1.15-blue.svg)](https://github.com/Tencent/xLua/releases)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/Tencent/xLua/pulls)
[![Build status](https://github.com/Tencent/xLua/actions/workflows/build.yml/badge.svg)](https://github.com/Tencent/xLua/actions/workflows/build.yml)

[(English Documents Available)](README_EN.md)

## C# 下 Lua 编程支持

xLua 为 Unity、.Net、Mono 等 C# 环境增加 Lua 脚本编程的能力，借助 xLua，这些 Lua 代码可以方便的和 C# 相互调用。

<br/>

## xLua 的突破

xLua 在功能、性能、易用性都有不少突破，这几方面分别最具代表性的是：

* 可以运行时把 C# 实现（方法，操作符，属性，事件等等）替换成 Lua 实现；
* 出色的 GC 优化，自定义 struct，枚举在 Lua 和 C# 间传递无 C# GC Alloc；
* 编辑器下无需生成代码，开发更轻量；

更详细的特性、平台支持介绍请参考 [xLua 文档: 功能特性](Assets/XLua/Doc/features.md)。

<br/>

## 安装

xLua 可以直接简单的安装在 Unity 项目中.

1. 从 [Releases](https://github.com/Tencent/xLua/releases) 中下载发行版, 或直接下载本仓库代码.
2. 打开下载下来的源码压缩包, 你会看到一个 Assets 目录, 这目录就对应 Unity 工程的 Assets 目录，保持这目录结构, 将其内容置入 Unity 项目即可.

> 注意, Assets/Examples 目录下为示例代码, 你应该在生产环境下删去他们.

如果希望安装到其它目录，请看 [FAQ](Assets/XLua/Doc/faq.md) 相关介绍。

<br/>

## 文档

* (必看) [XLua 教程](Assets/XLua/Doc/XLua教程.md)：教程，其配套代码[这里](Assets/XLua/Tutorial/)。
* (必看) [XLua 的配置](Assets/XLua/Doc/configure.md)：介绍如何配置xLua。
* [常见问题解答](Assets/XLua/Doc/faq.md)：常见问题都总结在这里，初使用大多数问题都可以在这里找到答案。
* [热补丁操作指南](Assets/XLua/Doc/hotfix.md)：介绍如何使用热补丁特性。
* [XLua增加删除第三方lua库](Assets/XLua/Doc/XLua增加删除第三方lua库.md)：如何增删第三方lua扩展库。
* [XLua API](Assets/XLua/Doc/XLua_API.md)：API文档。
* [生成引擎二次开发指南](Assets/XLua/Doc/custom_generate.md)：介绍如何做生成引擎的二次开发。

<br/>

## 快速入门

一个完整的例子仅需3行代码：

安装好xLua，建一个MonoBehaviour拖到场景，在Start加入如下代码：

```csharp
XLua.LuaEnv luaEnv = new XLua.LuaEnv();
luaEnv.DoString("CS.UnityEngine.Debug.Log('hello world')");
luaEnv.Dispose();
```

1. DoString 参数为 string，可输入任意合法的 Lua 代码，本示例在 Lua 里调用 C# 的 UnityEngine.Debug.Log 打印了个日志。
2. 一个 LuaEnv 实例对应 Lua 虚拟机，出于开销的考虑，建议全局唯一。

C#主动调用 Lua 也很简单，比如要调用 Lua 的系统函数，推荐方式是：

* 声明

  ```csharp
  [XLua.CSharpCallLua]
  public delegate double LuaMax(double a, double b);
  ```

* 绑定

  ```csharp
  var max = luaEnv.Global.GetInPath<LuaMax>("math.max");
  ```

* 调用

  ```csharp
  Debug.Log("max:" + max(32, 12));
  ```

注意, 请不要重复调用 `luaEnv.Global.GetInPath<LuaMax>`, 这没有任何必要.

<br/>

## 热补丁

除了使用 Lua 在 Unity 进行脚本编写, 你也可以使用 Lua 实现 "热补丁". xLua 提供了使用 Lua 逻辑替换 C# 方法逻辑的方案.

* 侵入性小，老项目原有代码不做任何调整就可使用。
* 运行时影响小，不打补丁基本和原有程序一样。
* 出问题了可以用 Lua 来打补丁，这时才会走到 Lua 代码逻辑；

参考使用指南: [xLua 文档: 热补丁](Assets/XLua/Doc/hotfix.md)

<br/>

## 更多示例

* [01_Helloworld](Assets/XLua/Examples/01_Helloworld/): 快速入门的例子。
* [02_U3DScripting](Assets/XLua/Examples/02_U3DScripting/): 展示怎么用 Lua 来写 MonoBehaviour。
* [03_UIEvent](Assets/XLua/Examples/03_UIEvent/): 展示怎么用 Lua 来写 UI 逻辑。
* [04_LuaObjectOrented](Assets/XLua/Examples/04_LuaObjectOrented/): 展示 Lua 面向对象和 C# 的配合。
* [05_NoGc](Assets/XLua/Examples/05_NoGc/): 展示怎么去避免值类型的GC。
* [06_Coroutine](Assets/XLua/Examples/06_Coroutine/): 展示 Lua 协程怎么和 Unity 协程相配合。
* [07_AsyncTest](Assets/XLua/Examples/07_AsyncTest/): 展示怎么用 Lua 协程来把异步逻辑同步化。
* [08_Hotfix](Assets/XLua/Examples/08_Hotfix/): 热补丁的示例（需要开启热补丁特性，如何开启请参考 [xLua 文档: 热补丁](Assets/XLua/Doc/hotfix.md)）。
* [09_GenericMethod](Assets/XLua/Examples/09_GenericMethod/): 泛化函数支持的演示。
* [10_SignatureLoader](Assets/XLua/Examples/10_SignatureLoader/): 展示如何读取经数字签名的lua脚本，参见[数字签名](Assets/XLua/Doc/signature.md)的文档介绍。
* [11_RawObject](Assets/XLua/Examples/11_RawObject/): 当 C# 参数是object时，如何把一个lua number指定以boxing后的int传递过去。
* [12_ReImplementInLua](Assets/XLua/Examples/12_ReImplementInLua/): 展示如何将复杂值类型改为 Lua 实现。
* [14_HotfixAsyncAwait](Assets/XLua/Examples/14_HotfixAsyncAwait/): 展示如何将异步函数和await关键字改为 Lua 实现。

<br/>

## 技术支持

一群：612705778 (已满)

二群：703073338 (已满)

三群：811246782

入群的问题：有问题该先从哪找答案

回答：FAQ

平时也要谨记这答案，90%以上问题都可以在[FAQ](Assets/XLua/Doc/faq.md)里头找到答案。这些问题就别在群刷屏了。

