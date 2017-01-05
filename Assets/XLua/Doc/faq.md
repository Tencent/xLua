# FAQ

## xLua发布包怎么用？

xLua目前以zip包形式发布，在Assets目录下解压即可。

## xLua可以放别的目录吗？

可以，但生成代码目录需要配置一下（默认放Assets\XLua\Gen目录），具体可以看《XLua的配置.doc》的GenPath配置介绍。

## Plugins源码在哪里可以找到，怎么使用？

Plugins源码位于xLua_Project_Root/build下。

源码编译依赖cmake，安装cmake后执行make_xxxx_yyyy.zz即可，xxxx代表平台，比如ios，android等，yyyy是要集成的虚拟机，有lua53和luajit两者，zz是后缀，windows下是bat，其它平台是sh。

windows编译依赖Visual Studio 2015。

android编译在linux下执行，依赖NDK，并且需要把脚本中ANDROID_NDK指向NDK的安装目录。

ios和osx需要在mac下编译。

## 报类似“xlua.access, no field __Hitfix0_Update”的错误怎么解决？

要等打印了hotfix inject finish!才点击运行

## 打开热补丁后，调用栈和源码行号对应不上怎么办？

添加HOTFIX_DEBUG_SYMBOLS可以解决这问题。但是，打开以后要注意下这点：如果你在编辑器里头点击了运行，你得重启下Unity再修改代码，因为运行过Unity编辑器会持有Assembly-CSharp.dll.mdb的句柄，而cecil写debug信息要覆盖这个文件，会报覆盖失败的错误。

## 报“This delegate/interface must add to CSharpCallLua : XXX”异常怎么解决？

在编辑器下xLua不生成代码都可以运行，出现这种提示，要么是该类型没加CSharpCallLua，要么是加之前生成过代码，没重新执行生成。

解决办法，确认XXX（类型名）加上CSharpCallLua后，清除代码后运行。

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
