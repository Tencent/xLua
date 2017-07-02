---
title: 热标识
type: guide
order: 1000
---

## 热标识

### 使用方式

1、添加HOTFIX_ENABLE宏打开该特性（在Unity3D的File->Build Setting->Scripting Define Symbols下添加）。编辑器、各手机平台这个宏要分别设置！如果是自动化打包，要注意在代码里头用API设置的宏是不生效的，需要在编辑器设置。

（建议平时开发业务代码不打开HOTFIX_ENABLE，只在build手机版本或者要在编译器下开发补丁时打开HOTFIX_ENABLE）

2、执行XLua/Generate Code菜单。

3、注入，构建手机包这个步骤会在构建时自动进行，编辑器下开发补丁需要手动执行"XLua/Hotfix Inject In Editor"菜单。注入成功会打印“hotfix inject finish!”或者“had injected!”。

### 内嵌模式

默认通过小工具执行代码注入，也可以采用内嵌到编辑器的方式，定义INJECT_WITHOUT_TOOL宏即可。

定义INJECT_WITHOUT_TOOL宏后，热补丁特性依赖Cecil，添加HOTFIX_ENABLE宏之后，可能会报找不到Cecil。这时你需要到Unity安装目录下找到Mono.Cecil.dll，Mono.Cecil.Pdb.dll，Mono.Cecil.Mdb.dll，拷贝到项目里头。

注意：如果你的Unity安装目录没有Mono.Cecil.Pdb.dll，Mono.Cecil.Mdb.dll（往往是一些老版本），那就只拷贝Mono.Cecil.dll（你从别的版本的Unity拷贝一套可能会导致编辑器不稳定），这时你需要定义HOTFIX_SYMBOLS_DISABLE，这会导致C#代码没法调试以及Log的栈源文件及行号错乱（所以赶紧升级Unity）。

参考命令（可能Unity版本不同会略有不同）：

```shell
OSX命令行 cp /Applications/Unity/Unity.app/Contents/Managed/Mono.Cecil.* Project/Assets/XLua/Src/Editor/
Win命令行 copy UnityPath\Editor\Data\Managed\Mono.Cecil.* Project\Assets\XLua\Src\Editor\
```

### 约束

不支持静态构造函数。

不支持在子类override函数通过base调用父类实现。

目前只支持Assets下代码的热补丁，不支持引擎，c#系统库的热补丁。

### API
xlua.hotfix(class, [method_name], fix)

* 描述         ： 注入lua补丁
* class        ： C#类，两种表示方法，CS.Namespace.TypeName或者字符串方式"Namespace.TypeName"，字符串格式和C#的Type.GetType要求一致，如果是内嵌类型（Nested Type）是非Public类型的话，只能用字符串方式表示"Namespace.TypeName+NestedTypeName"；
* method_name  ： 方法名，可选；
* fix          ： 如果传了method_name，fix将会是一个function，否则通过table提供一组函数。table的组织按key是method_name，value是function的方式。

xlua.private_accessible(class)

* 描述          ： 让一个类的私有字段，属性，方法等可用
* class         ： 同xlua.hotfix的class参数

### 标识要热更新的类型

和其它配置一样，有两种方式

方式一：直接在类里头打Hotfix标签；

方式二：在一个static类的static字段或者属性里头配置一个列表。属性可以用于实现的比较复杂的配置，比如根据Namespace做白名单。

```csharp
public static class HotfixCfg
{
    [Hotfix]
    public static List<Type> by_field = new List<Type>()
    {
        typeof(HotFixSubClass),
        typeof(GenericClass<>),
    };

    [Hotfix]
    public static List<Type> by_property
    {
        get
        {
            return (from type in Assembly.Load("Assembly-CSharp").GetTypes()
                    where type.Namespace == "XXXX"
                    select type).ToList();
        }
    }
}
```

### Hotfix Flag

Hotfix标签可以设置一些标志位对生成代码及插桩定制化

* Stateless

Stateless和Stateful的区别请看下下节。

* Stateful

同上。

* ValueTypeBoxing

值类型的适配delegate会收敛到object，好处是代码量更少，不好的是值类型会产生boxing及gc，适用于对text段敏感的业务。

* IgnoreProperty

不对属性注入及生成适配代码，一般而言，大多数属性的实现都很简单，出错几率比较小，建议不注入。

* IgnoreNotPublic

不对非public的方法注入及生成适配代码。除了像MonoBehaviour那种会被反射调用的私有方法必须得注入，其它仅被本类调用的非public方法可以不注入，只不过修复时会工作量稍大，所有引用到这个函数的public方法都要重写。

* Inline

不生成适配delegate，直接在函数体注入处理代码。

* IntKey

不生成静态字段，而是把所有注入点放到一个数组集中管理。

好处：对text段影响小。

坏处：使用不像默认方式那么方便，需要通过id来指明hotfix哪个函数，而这个id是代码注入工具时分配的，函数到id的映射会保存在Gen/Resources/hotfix_id_map.lua.txt，并且自动加时间戳备份到hotfix_id_map.lua.txt同级目录，发布手机版本后请妥善保存该文件。

该文件的格式大概如下：

```lua
return {
    ["HotfixTest"] = {
        [".ctor"] = {
            5
        },
        ["Start"] = {
            6
        },
        ["Update"] = {
            7
        },
        ["FixedUpdate"] = {
            8
        },
        ["Add"] = {
            9,10
        },
        ["OnGUI"] = {
            11
        },
    },
}
```

想要替换HotfixTest的Update函数，你得

```lua
CS.XLua.HotfixDelegateBridge.Set(7, func)
```

如果是重载函数，将会一个函数名对应多个id，比如上面的Add函数。

能不能自动化一些呢？可以，xlua.util提供了auto_id_map函数，执行一次后你就可以像以前那样直接用类，方法名去指明修补的函数。

```lua
(require 'xlua.util').auto_id_map()
xlua.hotfix(CS.HotfixTest, 'Update', function(self)
		self.tick = self.tick + 1
		if (self.tick % 50) == 0 then
			print('<<<<<<<<Update in lua, tick = ' .. self.tick)
		end
	end)
```

前提是hotfix_id_map.lua.txt放到可以通过require 'hotfix_id_map'引用到的地方。

ps：虽然xlua执行代码注入时会把hotfix_id_map.lua.txt放到Resources下，但那时似乎Unity已经不再处理新增的文件。貌似可以通过提前执行“Hotfix inject in Editor”来提前生成，但id不一定一样，比如有的类型里头有平台/编辑器专用的api，编辑器下和真机下的id将不一样。

### 使用建议

* 对所有较大可能变动的类型加上Hotfix标识；
* 建议用反射找出所有函数参数、字段、属性、事件涉及的delegate类型，标注CSharpCallLua；
* 业务代码、引擎API、系统API，需要在Lua补丁里头高性能访问的类型，加上LuaCallCSharp；
* 引擎API、系统API可能被代码剪裁调（C#无引用的地方都会被剪裁），如果觉得可能会新增C#代码之外的API调用，这些API所在的类型要么加LuaCallCSharp，要么加ReflectionUse；

### Stateless和Stateful

打Hotfix标签时，默认是Stateless方式，你也可以选Stateful方式，我们先说区别，再说使用场景。

Stateless方式是指用Lua对成员函数修复时，C#对象直接透传给作为Lua函数的第一个参数。

Stateful方式下你可以在Lua的构造函数返回一个table，然后后续成员函数调用会把这个table给传递过去。

Stateless比较适合无状态的类，有状态的话，你得通过反射去操作私有成员，也没法新增状态（field）。Stateless有个好处，可以运行的任意时刻执行替换。

Stateful的代价是会在类增加一个LuaTable类型的字段（中间层面增加，不会改源代码）。但这种方式是适用性更广，比如你不想要lua状态，可以在构造函数拦截那返回空。而且操作状态性能比反射操作C#私有变量要好，也可以随意新增任意的状态信息。缺点是，执行成员函数之前就new好的对象，接收到的状态会是空，所以需要重启，在一开始就执行替换。

### 打补丁

xlua可以用lua函数替换C#的构造函数，函数，属性，事件的替换。lua实现都是函数，比如属性对于一个getter函数和一个setter函数，事件对应一个add函数和一个remove函数。

* 函数

method_name传函数名，支持重载，不同重载都是转发到同一个lua函数。

比如：

```csharp

// 要fix的C#类
[Hotfix]
public class HotfixCalc
{
    public int Add(int a, int b)
    {
        return a - b;
    }

    public Vector3 Add(Vector3 a, Vector3 b)
    {
        return a - b;
    }
｝

```

```lua

xlua.hotfix(CS.HotfixCalc, 'Add', function(self, a, b)
    return a + b
end)

```

静态函数和成员函数的区别是，成员函数会加一个self参数，这个self在Stateless方式下是C#对象本身（对应C#的this），Stateful方式下传lua构造函数实现的返回值（一个table或者nil）

普通参数对于lua的参数，ref参数对应lua的一个参数和一个返回值，out参数对于lua的一个返回值。

泛化函数的打补丁规则和普通函数一样。

* 构造函数

构造函数对应的method_name是`.ctor`。

如果是Stateful方式，你可以返回一个table作为这个对象的状态。

和普通函数不一样的是，构造函数的热补丁并不是替换，而是执行原有逻辑后调用lua。

* 属性

对于名为“AProp”的属性，会对应一个getter，method_name等于get_AProp，setter的method_name等于set_AProp。

* []操作符

赋值对应set_Item，取值对应get_Item。第一个参数是self，赋值后面跟key，value，取值只有key参数，返回值是取出的值。

* 其它操作符

C#的操作符都有一套内部表示，比如+号的操作符函数名是op_Addition（其它操作符的内部表示可以去请参照相关资料），覆盖这函数就覆盖了C#的+号操作符。

* 事件

比如对于事件“AEvent”，+=操作符是add_AEvent，-=对应的是remove_AEvent。这两个函数均是第一个参数是self，第二个参数是操作符后面跟的delegate。

通过xlua.private_accessible来直接访问事件对应的私有delegate的直接访问后，可以通过对象的"&事件名"字段直接触发事件，例如self\['&MyEvent'\]()，其中MyEvent是事件名。

* 析构函数

method_name是"Finalize"，传一个self参数。

和普通函数不一样的是，析构函数的热补丁并不是替换，而是开头调用lua函数后继续原有逻辑。

* 泛化类型

其它规则一致，需要说明的是，每个泛化类型实例化后都是一个独立的类型，只能针对实例化后的类型分别打补丁。比如：

```csharp
public class GenericClass<T>
{
｝
```

你只能对GenericClass`<double>`，GenericClass`<int>`这些类，而不是对GenericClass打补丁。

另外值得一提的是，要注意泛化类型的命名方式，比如GenericClass`<double>`的命名是GenericClass`1[System.Double]，具体可以看[MSDN](https://msdn.microsoft.com/en-us/library/w3f99sx1.aspx)。

对GenericClass`<double>`打补丁的实例如下：

```csharp
luaenv.DoString(@"
    xlua.hotfix(CS['GenericClass`1[System.Double]'], {
        ['.ctor'] = function(obj, a)
            print('GenericClass<double>', obj, a)
        end;
        Func1 = function(obj)
            print('GenericClass<double>.Func1', obj)
        end;
        Func2 = function(obj)
            print('GenericClass<double>.Func2', obj)
            return 1314
        end
    })
");
```

* Unity协程

通过util.cs_generator可以用一个function模拟一个IEnumerator，在里头用coroutine.yield，就类似C#里头的yield return。比如下面的C#代码和对应的hotfix代码是等同效果的

```csharp
[XLua.Hotfix]
public class HotFixSubClass : MonoBehaviour {
    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);
            Debug.Log("Wait for 3 seconds");
        }
    }
}
```

```csharp
luaenv.DoString(@"
    local util = require 'xlua.util'
	xlua.hotfix(CS.HotFixSubClass,{
		Start = function(self)
			return util.cs_generator(function()
			    while true do
				    coroutine.yield(CS.UnityEngine.WaitForSeconds(3))
                    print('Wait for 3 seconds')
                end				
			end
		end;
	})
");
```

* 整个类

如果要替换整个类，不需要一次次的调用xlua.hotfix去替换，可以整个一次完成。只要给一个table，按method_name = function组织即可

```lua

xlua.hotfix(CS.StatefullTest, {
    ['.ctor'] = function(csobj)
        return {evt = {}, start = 0}
    end;
    set_AProp = function(self, v)
        print('set_AProp', v)
        self.AProp = v
    end;
    get_AProp = function(self)
        return self.AProp
    end;
    get_Item = function(self, k)
        print('get_Item', k)
        return 1024
    end;
    set_Item = function(self, k, v)
        print('set_Item', k, v)
    end;
    add_AEvent = function(self, cb)
        print('add_AEvent', cb)
        table.insert(self.evt, cb)
    end;
    remove_AEvent = function(self, cb)
       print('remove_AEvent', cb)
       for i, v in ipairs(self.evt) do
           if v == cb then
               table.remove(self.evt, i)
               break
           end
       end
    end;
    Start = function(self)
        print('Start')
        for _, cb in ipairs(self.evt) do
            cb(self.start, 2)
        end
        self.start = self.start + 1
    end;
    StaticFunc = function(a, b, c)
       print(a, b, c)
    end;
    Finalize = function(self)
       print('Finalize', self)
    end
})

```

