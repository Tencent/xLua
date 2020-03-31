# FAQ

## xLua发布包怎么用？

xLua目前以zip包形式发布，在工程目录下解压即可。

## xLua可以放别的目录吗？

可以，但生成代码目录需要配置一下（默认放Assets\XLua\Gen目录），具体可以看《XLua的配置.doc》的GenPath配置介绍。

更改目录要注意的是：生成代码和xLua核心代码必须在同一程序集。如果你要用热补丁特性，xLua核心代码必须在Assembly-CSharp程序集。

## xLua的配置

如果你用的是lua编程，你可能需要把一些频繁访问的类配置到LuaCallCSharp，这些类的成员如果有条件编译（大多数情况下是UNITY_EDITOR）的话，你需要通过BlackList配置排除；如果你需要通过delegate回调到lua的地方，得把这些delegate配置到CSharpCallLua。

如果你用的是热补丁，你需要把要注入的代码加到Hotfix列表；如果你需要通过delegate回调到lua的地方，也得把这些delegate配置到CSharpCallLua。

xLua提供了强大的动态配置，让你可以结合反射实现任意的自动化配置，动态配置介绍看[这里](configure.md)。xLua希望你能根据自身项目的需求自行配置，同时为了方便部分对反射api了解不够的童鞋，xLua也针对上面两者方式分别写了参考配置：[ExampleConfig.cs](../Editor/ExampleConfig.cs)，直接打开相应部分的注释即可使用。

## lua源码只能以txt后缀？

什么后缀都可以。

如果你想以TextAsset打包到安装包（比如放到Resources目录），Unity不认lua后缀，这是Unity的规则。

如果你不打包到安装包，就没有后缀的限制：比如自行下载到某个目录（这也是热更的正确姿势），然后通过CustomLoader或者设置package.path去读这个目录。

那为啥xLua本身带的lua源码（包括示例）为什么都是txt结尾呢？因为xLua本身就一个库，不含下载功能，也不方便运行时去某个地方下载代码，通过TextAsset是较简单的方式。

## 编辑器(或非il2cpp的android)下运行正常，ios下运行调用某函数报“attempt to call a nil value”

il2cpp默认会对诸如引擎、c#系统api，第三方dll等等进行代码剪裁。简单来说就是这些地方的函数如果你C#代码没访问到的就不编译到你最终发布包。

解决办法：增加引用（比如配置到LuaCallCSharp，或者你自己C#代码增加那函数的访问），或者通过link.xml配置（当配置了ReflectionUse后，xlua会自动帮你配置到link.xml）告诉il2cpp别剪裁某类型。

## Unity 2018及以上版本兼容性问题解决

2.1.14前的版本都建议先升级到2.1.14，升级后，还有如下两个使用注意事项：

1、默认配置不生成代码运行会报错

这是因为Api Compatibility Level设置为.NET Standard 2.0，而.NET Standard 2.0不支持emit导致的。

解决方案：平时开发Api Compatibility Level设置为.NET 4.x，就能支持编辑器不生成代码开发。发布手机版本时，按Unity官方的建议，可配置为.NET Standard 2.0，包会更小些。

2、生成代码后，一些系统类型的生成代码会报一些方法不存在。

据研究表明，Unity 2018设置.NET 4.X Equivalent的话，其运行和编译用的库不一致，前者比后者多一些API。

运行用的是：unity安装目录\Editor\Data\MonoBleedingEdge\lib\mono\unityjit\mscorlib.dll

编译链接的是：unity安装目录\Editor\Data\MonoBleedingEdge\lib\mono\4.7.1-api\mscorlib.dll

解决办法：用黑名单排除报错方法即可。不过2019年8月6号以前的版本的黑名单配置对泛型不友好，要一个个泛型实例的配置（比如，Dictionary<int, int>和Dictionary<float, int>要分别配置），而目前发现该问题主要出在泛型Dictionary上。可以更新到2019年8月6号之后的版本，该版本支持配置一个过滤器对泛型方法过滤。这里有对unity 2018的Dictionary的[针对性配置](https://github.com/Tencent/xLua/blob/master/Assets/XLua/Editor/ExampleConfig.cs#L277)，直接拷贝使用，如果碰到其它泛型也有多出来的方法，参考Dictionary进行配置。

## Plugins源码在哪里可以找到，怎么使用？

Plugins源码位于xLua_Project_Root/build下。

源码编译依赖cmake，安装cmake后执行make_xxxx_yyyy.zz即可，xxxx代表平台，比如ios，android等，yyyy是要集成的虚拟机，有lua53和luajit两者，zz是后缀，windows下是bat，其它平台是sh。

windows编译依赖Visual Studio 2015。

android编译在linux下执行，依赖NDK，并且需要把脚本中ANDROID_NDK指向NDK的安装目录。

ios和osx需要在mac下编译。

## 报类似“xlua.access, no field __Hitfix0_Update”的错误怎么解决？

按[Hotfix操作指南](hotfix.md)一步步操作。

## visual studio 2017下编译UWP原生库

visual studio 2017需要安装：1、“工作负载”下的“通用Window平台开发”；2、“单个组件”下的“用于ARM的Visual C++编译器和库”、“用于ARM64的Visual C++编译器和库”、“是用于ARM64的C++通用Windows平台工具”

## visual studio 2015下编译原生库

把build\vs2015下的bat文件拷贝到build目录，覆盖同名文件

## 报“please install the Tools”

没有把Tools安装到Assets平级目录，安装包，或者master下都能找到这个目录。

## 报“This delegate/interface must add to CSharpCallLua : XXX”异常怎么解决？

在编辑器下xLua不生成代码都可以运行，出现这种提示，要么是该类型没加CSharpCallLua，要么是加之前生成过代码，没重新执行生成。

解决办法，确认XXX（类型名）加上CSharpCallLua后，清除代码后运行。

如果编辑器下没问题，发布到手机报这错，表示你发布前没生成代码（执行“XLua/Generate Code”）。

## unity5.5以上执行"XLua/Hotfix Inject In Editor"菜单会提示"WARNING: The runtime version supported by this application is unavailable."

这是因为注入工具是用.net3.5编译，而unity5.5意思MonoBleedingEdge的mono环境并没3.5支持导致的，不过一般而言都向下兼容，目前为止也没发现该warning带来什么问题。

可能有人发现定义INJECT_WITHOUT_TOOL用内嵌模式会没有该warning，但问题是这模式是调试问题用的，不建议使用，因为可能会有一些库冲突问题。

## hotfix下怎么触发一个event

首先通过xlua.private_accessible开启私有成员访问。

跟着通过对象的"&事件名"字段调用delegate，例如self\['&MyEvent'\]()，其中MyEvent是事件名。

## 怎么对Unity Coroutine的实现函数打补丁？

见[Hotfix操作指南](hotfix.md)相应章节。

## 支持NGUI（或者UGUI/DOTween等等）么？

支持，xLua最主要的特性是让你原来用C#写的地方可以换成用lua写，你C#能用的插件，基本都能用。

## 如果需要调试，CustomLoader的filepath参数该如何处理？

lua里头调用require 'a.b'时，CustomLoader会被调用，并传入字符串"a.b"，你需要理解这字符串，（从文件/内存/网络等）加载好lua文件，返回两个东西，第一个是调试器可以理解的路径，比如：a/b.lua，这个通过设置ref类型的filepath参数返回，第二个是UTF8格式的源码的字节流（byte[]），通过返回值返回。

## 什么是生成代码？

xLua支持的lua和C#间交互技术之一，这种技术通过生成两者间的适配代码来实现交互，性能较好，是推荐的方式。

另一种交互技术是反射，这种方式对安装包的影响更少，可以在性能要求不高或者对安装包大小很敏感的场景下使用。

## 改了接口后，之前生成的代码出现错误怎么办？

清除掉生成代码（执行“Clear Generated Code”菜单，如果你重启过，会找不到这个菜单，这时你可以手动删除整个生成代码目录），等编译完成后重新生成。

## 应该什么时候生成代码？

开发期不建议生成代码，可以避免很多由于不一致导致的编译失败，以及生成代码本身的编译等待。

build手机版本前必须执行生成代码，建议做成自动化的。

做性能调优，性能测试前必须执行生成代码，因为生成和不生成性能的区别还是很大的。

## CS名字空间下有所有C# API是不是很占内存？

由于用了lazyload，这个“有”只是个虚拟的概念，比如UnityEngine.GameObject，是访问第一次CS.UnityEngine.GameObject或者第一个实例往lua传送才加载该类型方法，属性等。

## LuaCallSharp以及CSharpCallLua两种生成各在什么场景下用？

看调用者和被调用者，比如要在lua调用C#的GameObject.Find函数，或者调用gameobject的实例方法，属性等，GameObject类要加LuaCallSharp，而想把一个lua函数挂到UI回调，这是调用者是C#，被调用的是一个lua函数，所以回调声明的delegate要加CSharpCallLua。

有时会比较迷惑人，比如List<int>.Find(Predicate<int> match)的调用，List<int>当然是加LuaCallSharp，而Predicate<int>却要加CSharpCallLua，因为match的调用者在C#，被调用的是一个lua函数。

更无脑一点的方式是看到“This delegate/interface must add to CSharpCallLua : XXX”，就把XXX加到CSharpCallLua即可。

## 值类型传递会有gc alloc么？

如果你使用的是delegate调用lua函数，或者用LuaTable、LuaFunction的无gc接口，或者数组的话，以下值类型都是没gc的：

1、所有的基本值类型（所有整数，所有浮点数，decimal）；

2、所有的枚举类型；

3、字段只包含值类型的struct，可嵌套其它只包含值类型struct；

其中2、3需要把该类型加到GCOptimize。

## 反射在ios下可用吗？

ios下的限制有两个：1、没有jit；2、代码剪裁（stripping）；

对于C#通过delegate或者interface调用lua，如果不生成代码是用反射的emit，这依赖jit，所以这目前只在编辑器可用。

对于lua调用C#，主要会被代码剪裁影响，这时你可以配置ReflectionUse（不要配LuaCallSharp），执行“Generate Code”，这时不会对该类生成封装代码，而是生成link.xml把该类配置为不剪裁。

简而言之，除了CSharpCallLua是必须的（这类生成代码往往不多），LuaCallSharp生成都可以改为用反射。

## 支持泛型方法的调用么？

1、泛型约束到某个基类的支持，看例子：(../Examples/09_GenericMethod/)

2、没有泛型约束的建议封装为非泛型使用
如果是静态方法，可以自己写个封装来实例化泛型方法。

如果是成员方法，xLua支持扩展方法，你可以添加一个扩展方法来实例化泛型方法。该扩展方法使用起来就和普通成员方法一样。

```csharp
// C#
public static Button GetButton(this GameObject go)
{
    return go.GetComponent<Button>();
}
```

```lua
-- lua
local go = CS.UnityEngine.GameObject.Find("button")
go:GetButton().onClick:AddListener(function()
    print('onClick')
end)
```

3、如果xlua版本大于2.1.12的话，新增反射调用泛型方法的支持（有一定的限制，看后面的说明），比如对于这么个C#类型：
```csharp
public class GetGenericMethodTest
{
    int a = 100;
    public int Foo<T1, T2>(T1 p1, T2 p2)
    {
        Debug.Log(typeof(T1));
        Debug.Log(typeof(T2));
        Debug.Log(p1);
        Debug.Log(p2);
        return a;
    }

    public static void Bar<T1, T2>(T1 p1, T2 p2)
    {
        Debug.Log(typeof(T1));
        Debug.Log(typeof(T2));
        Debug.Log(p1);
        Debug.Log(p2);
    }
}
```
在lua那这么调用：
```lua
local foo_generic = xlua.get_generic_method(CS.GetGenericMethodTest, 'Foo')
local bar_generic = xlua.get_generic_method(CS.GetGenericMethodTest, 'Bar')

local foo = foo_generic(CS.System.Int32, CS.System.Double)
local bar = bar_generic(CS.System.Double, CS.UnityEngine.GameObject)

-- call instance method
local o = CS.GetGenericMethodTest()
local ret = foo(o, 1, 2)
print(ret)

-- call static method
bar(2, nil)
```

使用限制，只有下面几种情况可以：

* mono下可以
* il2cpp下，如果泛型参数是引用类型可以
* il2cpp下，如果泛型参数是值类型，C#那有用同样的泛型参数调用过（如果是hotfix场景下，一般在C#那会用同样的泛型参数调用过，所以在hotfix功能下一般都可用）


## 支持lua调用C#重载函数吗？

支持，但没有C#端支持的那么完善，比如重载方法void Foo(int a)和void Foo(short a)，由于int和short都对应lua的number，是没法根据参数判断调用的是哪个重载。这时你可以借助扩展方法来为其中一个起一个别名。

## 编辑器下运行正常，打包的时候生成代码报“没有某方法/属性/字段定义”怎么办？

往往是由于该方法/属性/字段是扩在条件编译里头，只在UNITY_EDITOR下有效，这是可以通过把这方法/属性/字段加到黑名单来解决，加了之后要等编译完成后重新执行代码生成。

## this[string field]或者this[object field]操作符重载为什么在lua无法访问？（比如Dictionary\<string, xxx\>, Dictionary\<object, xxx\>在lua中无法通过dic['abc']或者dic.abc检索值）

因为：1、这个特性会导致基类定义的方法、属性、字段等无法访问（比如Animation无法访问到GetComponent方法）；2、key为当前类某方法、属性、字段的名字的数据无法检索，比如Dictionary类型，dic['TryGetValue']返回的是一个函数，指向Dictionary的TryGetValue方法。

如果你的版本大于2.1.11，可以用get_Item来获取值，用set_Item来设置值。要注意只有this[string field]或者this[object field]才有这两个替代api，其它类型的key是没有的。

~~~lua
dic:set_Item('a', 1)
dic:set_Item('b', 2)
print(dic:get_Item('a'))
print(dic:get_Item('b'))
~~~

如果你的版本小于或等于2.1.11，建议直接方法该操作符的等效方法，比如Dictionary的TryGetValue，如果该方法没有提供，可以在C#那通过Extension method封装一个使用。

## 有的Unity对象，在C#为null，在lua为啥不为nil呢？比如一个已经Destroy的GameObject

其实那C#对象并不为null，是UnityEngine.Object重载的==操作符，当一个对象被Destroy，未初始化等情况，obj == null返回true，但这C#对象并不为null，可以通过System.Object.ReferenceEquals(null, obj)来验证下。

对应这种情况，可以为UnityEngine.Object写一个扩展方法：

~~~csharp
[LuaCallCSharp]
[ReflectionUse]
public static class UnityEngineObjectExtention
{
    public static bool IsNull(this UnityEngine.Object o) // 或者名字叫IsDestroyed等等
    {
        return o == null;
    }
}
~~~

然后在lua那你对所有UnityEngine.Object实例都使用IsNull判断

~~~lua
print(go:GetComponent('Animator'):IsNull())
~~~

## 泛型实例怎么构造

涉及的类型都在mscorlib，Assembly-CSharp程序集的话，泛型实例的构造和普通类型是一样的，都是CS.namespace.typename()，可能比较特殊的是typename的表达，泛型实例的typename的表达包含了标识符非法符号，最后一部分要换成["typename"]，以List<string>为例

~~~lua
local lst = CS.System.Collections.Generic["List`1[System.String]"]()
~~~

如果某个泛型实例的typename不确定，可以在C#测打印下typeof(不确定的类型).ToString()

如果涉及mscorlib，Assembly-CSharp程序集之外的类型的话，可以用C#的反射来做：

~~~lua
local dic = CS.System.Activator.CreateInstance(CS.System.Type.GetType('System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[UnityEngine.Vector3, UnityEngine]],mscorlib'))
dic:Add('a', CS.UnityEngine.Vector3(1, 2, 3))
print(dic:TryGetValue('a'))
~~~

如果你的xLua版本大于v2.1.12，将会有更漂亮的表达方式

~~~lua
-- local List_String = CS.System.Collections.Generic['List<>'](CS.System.String) -- another way
local List_String = CS.System.Collections.Generic.List(CS.System.String)
local lst = List_String()

local Dictionary_String_Vector3 = CS.System.Collections.Generic.Dictionary(CS.System.String, CS.UnityEngine.Vector3)
local dic = Dictionary_String_Vector3()
dic:Add('a', CS.UnityEngine.Vector3(1, 2, 3))
print(dic:TryGetValue('a'))
~~~


## 调用LuaEnv.Dispose时，报“try to dispose a LuaEnv with C# callback!”错是什么原因？

这是由于C#还存在指向lua虚拟机里头某个函数的delegate，为了防止业务在虚拟机释放后调用这些无效（因为其引用的lua函数所在虚拟机都释放了）delegate导致的异常甚至崩溃，做了这个检查。

怎么解决？释放这些delegate即可，所谓释放，在C#中，就是没有引用：

你是在C#通过LuaTable.Get获取并保存到对象成员，赋值该成员为null；

你是在lua那把lua函数注册到一些事件事件回调，反注册这些回调；

如果你是通过xlua.hotfix(class, method, func)注入到C#，则通过xlua.hotfix(class, method, nil)删除；

要注意以上操作在Dispose之前完成。

xlua提供了一个工具函数来帮助你找到被C#引用着的lua函数，util.print_func_ref_by_csharp，使用很简单，执行如下lua代码：

~~~lua
local util = require 'xlua.util'
util.print_func_ref_by_csharp()
~~~

可以看到控制台有类似这样的输出，下面第一行表示有一个在main.lua的第2行定义的函数被C#引用着

~~~bash
LUA: main.lua:2
LUA: main.lua:13
~~~

## 调用LuaEnv.Dispose崩溃

很可能是这个Dispose操作是由lua那驱动执行，相当于在lua执行的过程中把lua虚拟机给释放了，改为只由C#执行即可。

## C#参数（或字段）类型是object时，传递整数默认是以long类型传递，如何指明其它类型？比如int

看[例子11](../Examples/11_RawObject/RawObjectTest.cs)


## 如何做到先执行原来的C#逻辑，然后再执行补丁

用util.hotfix_ex，可以调用原先的C#逻辑

~~~lua
local util = require 'xlua.util'
util.hotfix_ex(CS.HotfixTest, 'Add', function(self, a, b)
   local org_sum = self:Add(a, b)
   print('org_sum', org_sum)
   return a + b
end)
~~~

## 怎么把C#的函数赋值给一个委托字段

2.1.8及之前版本，你把C#函数当成一个lua函数即可，性能会略低，因为委托调用时先通过Birdage适配代码调用lua，然后lua再调用回C#。

2.1.9 xlua.util新增createdelegate函数

比如如下C#代码

~~~csharp
public class TestClass
{
    public void Foo(int a)
    { 
    }
	
    public static void SFoo(int a)
    {
    }
｝
public delegate void TestDelegate(int a);
~~~

你可以指明用Foo函数创建一个TestDelegate实例
~~~lua
local util = require 'xlua.util'

local d1 = util.createdelegate(CS.TestDelegate, obj, CS.TestClass, 'Foo', {typeof(CS.System.Int32)}) --由于Foo是实例方法，所以参数2需要传TestClass实例
local d2 = util.createdelegate(CS.TestDelegate, nil, CS.TestClass, 'SFoo', {typeof(CS.System.Int32)})

obj_has_TestDelegate.field = d1 + d2 --到时调用field的时候将会触发Foo和SFoo，这不会经过Lua适配

~~~

## 为什么有时Lua错误直接中断了而没错误信息？

一般两种情况：

1、你的错误代码用协程跑，而标准的lua，协程出错是通过resume返回值来表示，可以查阅相关的lua官方文档。如果你希望协程出错直接抛异常，可以在你的resume调用那加个assert。

把类似下面的代码：

~~~lua
coroutine.resume(co, ...)
~~~

改为：

~~~lua
assert(coroutine.resume(co, ...))
~~~

2、上层catch后，不打印

比如某些sdk，在回调业务时，try-catch后把异常吃了。

## 重载含糊如何处理

比如由于忽略out参数导致的Physics.Raycast其中一个重载调用不了，比如short，int无法区分的问题。

首先out参数导致重载含糊比较少见，目前只反馈（截至2017-9-22）过Physics.Raycast一个，建议通过自行封装来解决（short，int这种情况也适用）：静态函数的直接封装个另外名字的，如果是成员方法则通过Extension method来封装。

如果是hotfix场景，我们之前并没有提前封装，又希望调用指定重载怎么办？

可以通过xlua.tofunction结合反射来处理，xlua.tofunction输入一个MethodBase对象，返回一个lua函数。比如下面的C#代码：

~~~csharp
class TestOverload
{
    public int Add(int a, int b)
    {
        Debug.Log("int version");
        return a + b;
    }

    public short Add(short a, short b)
    {
        Debug.Log("short version");
        return (short)(a + b);
    }
}
~~~

我们可以这么调用指定重载：

~~~lua
local m1 = typeof(CS.TestOverload):GetMethod('Add', {typeof(CS.System.Int16), typeof(CS.System.Int16)})
local m2 = typeof(CS.TestOverload):GetMethod('Add', {typeof(CS.System.Int32), typeof(CS.System.Int32)})
local f1 = xlua.tofunction(m1) --切记对于同一个MethodBase，只tofunction一次，然后重复使用
local f2 = xlua.tofunction(m2)

local obj = CS.TestOverload()

f1(obj, 1, 2) --调用short版本，成员方法，所以要传对象，静态方法则不需要
f2(obj, 1, 2) --调用int版本
~~~

注意：xlua.tofunction由于使用不太方便，以及使用了反射，所以建议做作为临时方案，尽量用封装的方法来解决。

## 支持interface扩展方法么？

考虑到生成代码量，不支持通过obj:ExtentionMethod()的方式去调用，支持通过静态方法的方式去调用CS.ExtentionClass.ExtentionMethod(obj)

## 如何把xLua的Wrap生成操作集成到我项目的自动打包流程中？

可以参考[例子13](../Examples/13_BuildFromCLI/)，通过命令行调用Unity自定义类方法出包。

## 使用热补丁特性，打手机版本时报DelegatesGensBridge.cs引用了不存在的类（比如仅编辑器使用的类型），应该如何处理？

这是因为Hotfix列表里头配置的类型（假设是类型A），对这些不存在的类型（假设是类型B）引用。找到这种类型，从Hotfix配置列表中排除（注意，排除的类型A，而不是类型B）。

如何找？VS中选中报不存在的类型，然后“Find All References”找到引用了这个类型的所有方法、属性。。这些方法、属性等等所在的类型就是你要排除的类型。

## 如何加载字节码

用luac编译后直接加载即可。

要注意默认lua字节码是区分32位和64位的，32位luac生成的字节码只能在32位虚拟机里头跑，可以按《[通用字节码](compatible_bytecode.md)》一文处理下。

## lua持有的c#对象怎么释放

达成下面两点即可释放：

* 1、lua所有对该C#对象释放
* 2、lua完成一次gc周期

貌似所有gc都是上述条件，但对于lua要特别说明下第二点，lua不像C#那样会后台启动一个gc线程在做垃圾回收，而是把gc拆分成小步骤插入到有内存分配。

所以注意一点：你没在运行lua代码，或者lua代码运行并未分配内存，你怎么等也不会有内存回收。

默认gc配置，lua要到达上次内存回收完成时内存占用的两倍才开启一轮新的gc周期，举例上次回收完毕20M内存，那么下次要等到40M才开始一轮gc周期。

按xLua的设计，一个C#对象引用传递到lua，仅让lua增加4字节内存（然而这可能是在C#侧内存占用很大的对象，比如贴图），可以看到通过持有C#引用要达成默认配置开启gc周期条件是比较困难的。

这时可以这么干：

* 1、设置GcPause，让gc更快开启，默认200表示2倍上次回收内存时开启，coco2dx设置为100，表示完成一趟gc后马上开启下一趟，另外也可以设置GcStepmul来加快gc回收速度，默认是200表示回收比内存分配快两倍，GcStepmul在coco2dx设置为5000
* 2、可以在场景切换之类对于性能要求不高的地方加入全量gc调用（通过LuaEnv.FullGc或者在lua里头调用collectgarbage('collect')都可以）。

## 多线程下莫名crash怎么解决

多线程使用需要在（Player Setting/Scripting Define Symbols）下添加THREAD_SAFE宏。

常见的不明显的多线程的场景，比如c#异步socket，对象析构函数等。

