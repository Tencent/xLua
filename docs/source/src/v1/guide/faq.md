---
title: FAQ
type: guide
order: 3
---

## FAQ

### xLua发布包怎么用？

xLua目前以zip包形式发布，在工程目录下解压即可。

### xLua可以放别的目录吗？

可以，但生成代码目录需要配置一下（默认放`Assets\XLua\Gen`目录），具体可以看《XLua的配置.doc》的GenPath配置介绍。

### lua源码只能以txt后缀？

什么后缀都可以。

如果你想以`TextAsset`打包到安装包（比如放到`Resources`目录），Unity不认`lua`后缀，这是Unity的规则。

如果你不打包到安装包，就没有后缀的限制：比如自行下载到某个目录（这也是热更的正确姿势），然后通过CustomLoader或者设置package.path去读这个目录。

那为啥xLua本身带的lua源码（包括示例）为什么都是txt结尾呢？因为xLua本身就一个库，不含下载功能，也不方便运行时去某个地方下载代码，通过TextAsset是较简单的方式。

### Plugins源码在哪里可以找到，怎么使用？

Plugins源码位于`xLua_Project_Root/build`下。

源码编译依赖cmake，安装cmake后执行make_xxxx_yyyy.zz即可，xxxx代表平台，比如ios，android等，yyyy是要集成的虚拟机，有lua53和luajit两者，zz是后缀，windows下是bat，其它平台是sh。

windows编译依赖Visual Studio 2015。

android编译在linux下执行，依赖NDK，并且需要把脚本中ANDROID_NDK指向NDK的安装目录。

ios和osx需要在mac下编译。

### 报类似“xlua.access, no field __Hitfix0_Update”的错误怎么解决？

按[Hotfix操作指南](hotfix.html)一步步操作。

### 报“please install the Tools”

没有把Tools安装到Assets平级目录，安装包，或者master下都能找到这个目录。

### 报“This delegate/interface must add to CSharpCallLua : XXX”异常怎么解决？

在编辑器下xLua不生成代码都可以运行，出现这种提示，要么是该类型没加CSharpCallLua，要么是加之前生成过代码，没重新执行生成。

解决办法，确认XXX（类型名）加上CSharpCallLua后，清除代码后运行。

### hotfix下怎么触发一个event

首先通过xlua.private_accessible开启私有成员访问。

跟着通过对象的"&事件名"字段调用delegate，例如self\['&MyEvent'\]()，其中MyEvent是事件名。

### 怎么对Unity Coroutine的实现函数打补丁？

见[Hotfix操作指南](hotfix.html)相应章节。

### 支持NGUI（或者UGUI/DOTween等等）么？

支持，xLua最主要的特性是让你原来用C#写的地方可以换成用lua写，你C#能用的插件，基本都能用。

### 如果需要调试，CustomLoader的filepath参数该如何处理？

lua里头调用require 'a.b'时，CustomLoader会被调用，并传入字符串"a.b"，你需要理解这字符串，（从文件/内存/网络等）加载好lua文件，返回两个东西，第一个是调试器可以理解的路径，比如：a/b.lua，这个通过设置ref类型的filepath参数返回，第二个是UTF8格式的源码的字节流（byte[]），通过返回值返回。

### 什么是生成代码？

xLua支持的lua和C#间交互技术之一，这种技术通过生成两者间的适配代码来实现交互，性能较好，是推荐的方式。

另一种交互技术是反射，这种方式对安装包的影响更少，可以在性能要求不高或者对安装包大小很敏感的场景下使用。

### 改了接口后，之前生成的代码出现错误怎么办？

清除掉生成代码（执行“Clear Generated Code”菜单，如果你重启过，会找不到这个菜单，这时你可以手动删除整个生成代码目录），等编译完成后重新生成。

### 应该什么时候生成代码？

开发期不建议生成代码，可以避免很多由于不一致导致的编译失败，以及生成代码本身的编译等待。

build手机版本前必须执行生成代码，建议做成自动化的。

做性能调优，性能测试前必须执行生成代码，因为生成和不生成性能的区别还是很大的。

### CS名字空间下有所有C# API是不是很占内存？

由于用了lazyload，这个“有”只是个虚拟的概念，比如UnityEngine.GameObject，是访问第一次CS.UnityEngine.GameObject或者第一个实例往lua传送才加载该类型方法，属性等。

### LuaCallSharp以及CSharpCallLua两种生成各在什么场景下用？

看调用者和被调用者，比如要在lua调用C#的GameObject.Find函数，或者调用gameobject的实例方法，属性等，GameObject类要加LuaCallSharp，而想把一个lua函数挂到UI回调，这是调用者是C#，被调用的是一个lua函数，所以回调声明的delegate要加CSharpCallLua。

有时会比较迷惑人，比如`List<int>.Find(Predicate<int> match)`的调用，`List<int>`当然是加LuaCallSharp，而`Predicate<int>`却要加CSharpCallLua，因为match的调用者在C#，被调用的是一个lua函数。

更无脑一点的方式是看到“This delegate/interface must add to CSharpCallLua : XXX”，就把XXX加到CSharpCallLua即可。

### 值类型传递会有gc alloc么？

如果你使用的是delegate调用lua函数，或者用LuaTable、LuaFunction的无gc接口，或者数组的话，以下值类型都是没gc的：

1、所有的基本值类型（所有整数，所有浮点数，decimal）；

2、所有的枚举类型；

3、字段只包含值类型的struct，可嵌套其它只包含值类型struct；

其中2、3需要把该类型加到GCOptimize。

### 反射在ios下可用吗？

ios下的限制有两个：1、没有jit；2、代码剪裁（stripping）；

对于C#通过delegate或者interface调用lua，如果不生成代码是用反射的emit，这依赖jit，所以这目前只在编辑器可用。

对于lua调用C#，主要会被代码剪裁影响，这时你可以配置ReflectionUse（不要配LuaCallSharp），执行“Generate Code”，这时不会对该类生成封装代码，而是生成link.xml把该类配置为不剪裁。

简而言之，除了CSharpCallLua是必须的（这类生成代码往往不多），LuaCallSharp生成都可以改为用反射。

### 支持泛化方法的调用么？

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

### 支持lua调用C#重载函数吗？

支持，但没有C#端支持的那么完善，比如重载方法void Foo(int a)和void Foo(short a)，由于int和short都对应lua的number，是没法根据参数判断调用的是哪个重载。这时你可以借助扩展方法来为其中一个起一个别名。

### 编辑器下运行正常，打包的时候生成代码报“没有某方法/属性/字段定义”怎么办？

往往是由于该方法/属性/字段是扩在条件编译里头，只在UNITY_EDITOR下有效，这是可以通过把这方法/属性/字段加到黑名单来解决，加了之后要等编译完成后重新执行代码生成。

### this[string field]或者this[object field]操作符重载为什么在lua无法访问？（比如`Dictionary<string, xxx>`, `Dictionary<object, xxx>`在lua中无法通过dic['abc']或者dic.abc检索值）

在2.1.5~2.1.6版本把这个特性去掉，因为：1、这个特性会导致基类定义的方法、属性、字段等无法访问（比如Animation无法访问到GetComponent方法）；2、key为当前类某方法、属性、字段的名字的数据无法检索，比如Dictionary类型，dic['TryGetValue']返回的是一个函数，指向Dictionary的TryGetValue方法。

建议直接方法该操作符的等效方法，比如Dictionary的TryGetValue，如果该方法没有提供，可以在C#那通过Extension method封装一个使用。

### 有的Unity对象，在C#为null，在lua为啥不为nil呢？比如一个已经Destroy的GameObject

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

### 泛型实例怎么构造

涉及的类型都在mscorlib，Assembly-CSharp程序集的话，泛型实例的构造和普通类型是一样的，都是CS.namespace.typename()，可能比较特殊的是typename的表达，泛型实例的typename的表达包含了标识符非法符号，最后一部分要换成["typename"]，以`List<string>`为例

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

### 调用LuaEnv.Dispose时，报“try to dispose a LuaEnv with C# callback!”错是什么原因？

这是由于C#还存在指向lua虚拟机里头某个函数的delegate，为了防止业务在虚拟机释放后调用这些无效（因为其引用的lua函数所在虚拟机都释放了）delegate导致的异常甚至崩溃，做了这个检查。

怎么解决？释放这些delegate即可，所谓释放，在C#中，就是没有引用：

你是在C#通过LuaTable.Get获取并保存到对象成员，赋值该成员为null；

你是在lua那把lua函数注册到一些事件事件回调，反注册这些回调；

如果你是通过xlua.hotfix(class, method, func)注入到C#，则通过xlua.hotfix(class, method, nil)删除；

要注意以上操作在Dispose之前完成。

### C#参数（或字段）类型是object时，传递整数默认是以long类型传递，如何指明其它类型？比如int

看[例子11](https://github.com/Tencent/xLua/blob/master/Assets/XLua/Examples/11_RawObject/RawObjectTest.cs)


