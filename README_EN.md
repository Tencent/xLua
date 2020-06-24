![](Assets/XLua/Doc/xLua.png)

[![license](http://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Tencent/xLua/blob/master/LICENSE.TXT)
[![release](https://img.shields.io/badge/release-v2.1.15-blue.svg)](https://github.com/Tencent/xLua/releases)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/Tencent/xLua/pulls)
[![Build status](https://travis-ci.org/Tencent/xLua.svg?branch=master)](https://travis-ci.org/Tencent/xLua)

## Lua programming solution for C#

xLua adds Lua scripting capability to Unity, .Net, Mono, and other C# environments. With xLua, Lua code and C# code can easily call each other.

## xLua's superior features

xLua has many breakthroughs in function, performance, and ease of use. The most significant features are:

* You can inplace C# implementations (methods, operators, properties, events, etc...) by Lua's during runtime.
* Outstanding GC optimization, customized struct, no C# gc alloc when passing the enumerated objects between C# and lua;
* Lightweight development with no needs to generate code in editor mode;

## Installation

Unpack the zip package and you will see an Assets directory, which corresponds to the Unity project's Assets directory. Keep the directory structure in your Unity project.

If you want to install it to another directory, please see the [FAQs](Assets/XLua/Doc/Faq_EN.md).

## Documents

* [FAQs](Assets/XLua/Doc/Faq_EN.md): Frequently asked questions are summarized here. You can find answers to questions for beginners.
* (Must-read) [XLua Tutorial](Assets/XLua/Doc/XLua_Tutorial_EN.md): This is a tutorial. The supporting code can be found [here](Assets/XLua/Tutorial/).
* (Must-read) [XLua Configuration](Assets/XLua/Doc/Configure_EN.md): Descriptions on how to configure xLua.
* [Hotfix Operation Guide](Assets/XLua/Doc/Hotfix_EN.md): Description on how to use the hotfix feature.
* [Add/remove third-party Lua Libraries on xLua](Assets/XLua/Doc/Add_Remove_Lua_Lib.md): Descriptions on how to add or remove third-party Lua extension libraries.
* [xLua APIs](Assets/XLua/Doc/XLua_API_EN.md): API documentation
* [Secondary Development of the Build Engine](Assets/XLua/Doc/Custom_Generate_EN.md): Descriptions on how to do secondary development of the build engine.

## Quick Start

A complete example requires only 3 lines of code:

Install xLua, create a MonoBehaviour drag scenario, add the following code to Start:

```csharp
XLua.LuaEnv luaenv = new XLua.LuaEnv();
luaenv.DoString("CS.UnityEngine.Debug.Log('hello world')");
luaenv.Dispose();
```

1. The DoString parameter is a string, and you can enter any allowable Lua code. In this example, Lua calls C#’s UnityEngine.Debug.Log to print a log.

2. A LuaEnv instance corresponds to a Lua virtual machine. Due to overhead, it is recommended that the Lua virtual machine be globally unique.

It is simple that C# actively calls Lua. For example, the recommended method to call Lua's system function is:

* Declare

```csharp
[XLua.CSharpCallLua]
public delegate double LuaMax(double a, double b);
```

* Bind

```csharp
var max = luaenv.Global.GetInPath<LuaMax>("math.max");
```

* Call

```csharp
Debug.Log("max:" + max(32, 12));
```

It is recommended that you bind once and reuse it. If code is generated, no gc alloc is generated when calling max.

## Hotfix

* This has lower intrusiveness, and it can be used without any modification of the original code of the old project.
* This has little impact on the runtime, which is almost the same as the original program which hotfix is not used.
* If you have problems, you can also use Lua to patch. Then the Lua code logic is involved.

[Here](Assets/XLua/Doc/Hotfix_EN.md) is the usage guide:

## More Examples

* [01_Helloworld](Assets/XLua/Examples/01_Helloworld/): Quick Start Examples
* [02_U3DScripting](Assets/XLua/Examples/02_U3DScripting/): This example shows how to use Mono to write MonoBehaviour.
* [03_UIEvent](Assets/XLua/Examples/03_UIEvent/): This example shows how to use Lua to write UI logic.
* [04_LuaObjectOrented](Assets/XLua/Examples/04_LuaObjectOrented/): This example shows the cooperation between Lua's object-oriented programming and C#.
* [05_NoGc](Assets/XLua/Examples/05_NoGc/): This example shows how to avoid the value type GC.
* [06_Coroutine](Assets/XLua/Examples/06_Coroutine/): This example shows how Lua coroutines work with Unity coroutines.
* [07_AsyncTest](Assets/XLua/Examples/07_AsyncTest/): This example shows how to use Lua coroutines to synchronize asynchronous logic.
* [08_Hotfix](Assets/XLua/Examples/08_Hotfix/): These are Hotfix examples (Please enable hotfix feature. See the [Guide](Assets/XLua/Doc/Hotfix_EN.md) for details).
* [09_GenericMethod](Assets/XLua/Examples/09_GenericMethod/): This is a generic function support demo.
* [10_SignatureLoader](Assets/XLua/Examples/10_SignatureLoader/): This example shows how to read the Lua script with a digital signature. See the [Digital Signature](Assets/XLua/Doc/signature.md) document for details.
* [11_RawObject](Assets/XLua/Examples/11_RawObject/): This example shows how to specify transferring a Lua number in the int after boxing when the C# parameter is an object.
* [12_ReImplementInLua](Assets/XLua/Examples/12_ReImplementInLua/): This shows how to change complex value types to Lua implementations.

## Technical support

QQ Group 1: 612705778 (may be full)

QQ Group 2: 703073338

Check answers: If you encounter a problem, please read the FAQs first.

