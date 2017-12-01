# FAQ

## xLua发布包怎么用？

xLua目前以zip包形式发布，在工程目录下解压即可。

## xLua可以放别的目录吗？

可以，但生成代码目录需要配置一下（默认放Assets\XLua\Gen目录），具体可以看《XLua的配置.doc》的GenPath配置介绍。

更改目录要注意的是：生成代码和xLua核心代码必须在同一程序集。如果你要用热补丁特性，xLua核心代码必须在Assembly-CSharp程序集。

## lua源码只能以txt后缀？

什么后缀都可以。

如果你想以TextAsset打包到安装包（比如放到Resources目录），Unity不认lua后缀，这是Unity的规则。

如果你不打包到安装包，就没有后缀的限制：比如自行下载到某个目录（这也是热更的正确姿势），然后通过CustomLoader或者设置package.path去读这个目录。

那为啥xLua本身带的lua源码（包括示例）为什么都是txt结尾呢？因为xLua本身就一个库，不含下载功能，也不方便运行时去某个地方下载代码，通过TextAsset是较简单的方式。

## Plugins源码在哪里可以找到，怎么使用？

Plugins源码位于xLua_Project_Root/build下。

源码编译依赖cmake，安装cmake后执行make_xxxx_yyyy.zz即可，xxxx代表平台，比如ios，android等，yyyy是要集成的虚拟机，有lua53和luajit两者，zz是后缀，windows下是bat，其它平台是sh。

windows编译依赖Visual Studio 2015。

android编译在linux下执行，依赖NDK，并且需要把脚本中ANDROID_NDK指向NDK的安装目录。

ios和osx需要在mac下编译。

## 报类似“xlua.access, no field __Hitfix0_Update”的错误怎么解决？

按[Hotfix操作指南](hotfix.md)一步步操作。

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

## 支持泛化方法的调用么？

不直接支持，但能调用到。如果是静态方法，可以自己写个封装来实例化泛化方法。

如果是成员方法，xLua支持扩展方法，你可以添加一个扩展方法来实例化泛化方法。该扩展方法使用起来就和普通成员方法一样。

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

## 支持lua调用C#重载函数吗？

支持，但没有C#端支持的那么完善，比如重载方法void Foo(int a)和void Foo(short a)，由于int和short都对应lua的number，是没法根据参数判断调用的是哪个重载。这时你可以借助扩展方法来为其中一个起一个别名。

## 编辑器下运行正常，打包的时候生成代码报“没有某方法/属性/字段定义”怎么办？

往往是由于该方法/属性/字段是扩在条件编译里头，只在UNITY_EDITOR下有效，这是可以通过把这方法/属性/字段加到黑名单来解决，加了之后要等编译完成后重新执行代码生成。

## this[string field]或者this[object field]操作符重载为什么在lua无法访问？（比如Dictionary\<string, xxx\>, Dictionary\<object, xxx\>在lua中无法通过dic['abc']或者dic.abc检索值）

在2.1.5~2.1.6版本把这个特性去掉，因为：1、这个特性会导致基类定义的方法、属性、字段等无法访问（比如Animation无法访问到GetComponent方法）；2、key为当前类某方法、属性、字段的名字的数据无法检索，比如Dictionary类型，dic['TryGetValue']返回的是一个函数，指向Dictionary的TryGetValue方法。

建议直接方法该操作符的等效方法，比如Dictionary的TryGetValue，如果该方法没有提供，可以在C#那通过Extension method封装一个使用。

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

## 调用LuaEnv.Dispose时，报“try to dispose a LuaEnv with C# callback!”错是什么原因？

这是由于C#还存在指向lua虚拟机里头某个函数的delegate，为了防止业务在虚拟机释放后调用这些无效（因为其引用的lua函数所在虚拟机都释放了）delegate导致的异常甚至崩溃，做了这个检查。

怎么解决？释放这些delegate即可，所谓释放，在C#中，就是没有引用：

你是在C#通过LuaTable.Get获取并保存到对象成员，赋值该成员为null；

你是在lua那把lua函数注册到一些事件事件回调，反注册这些回调；

如果你是通过xlua.hotfix(class, method, func)注入到C#，则通过xlua.hotfix(class, method, nil)删除；

要注意以上操作在Dispose之前完成。

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

