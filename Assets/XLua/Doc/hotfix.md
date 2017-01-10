## 约束

为了不影响开发，这个特性默认是不打开的，需要添加HOTFIX_ENABLE宏（在Unity3D的File->Build Setting->Scripting Define Symbols下添加）。

打开HOTFIX_ENABLE后由于il和源代码已经对应不上，所以（双击Unity3D日志）代码会定位到错误的地方，调试功能工作也不正常。

打开HOTFIX_DEBUG_SYMBOLS可以解决il和源代码对应不上的问题，但如果在编辑器下运行过后在修改代码，会报Assembly-CSharp.dll.mdb的写冲突错误。

建议的做法是HOTFIX_ENABLE，HOTFIX_DEBUG_SYMBOLS在build手机版本的时候打开，平时编辑器下需要开发补丁的时候打开HOTFIX_ENABLE，开发补丁时需要调试的时候打开HOTFIX_DEBUG_SYMBOLS。

热补丁特性依赖Cecil，添加HOTFIX_ENABLE宏之后，可能会报找不到Cecil。这时你需要到Unity安装目录下找到Mono.Cecil.dll，拷贝到项目里头。而HOTFIX_DEBUG_SYMBOLS则依赖Mono.Cecil.Pdb.dll，Mono.Cecil.Mdb.dll。

热补丁需要执行XLua/Generate Code才能正常运行。

参考命令（可能Unity版本不同会略有不同，把别的Unity版本带的拷贝过来试了也能用，比如有的老版本Unity是不带Mono.Cecil.Pdb.dll，Mono.Cecil.Mdb.dll的，这时可以把新版本带的整套拷贝过来）：

```shell
OSX命令行 cp /Applications/Unity/Unity.app/Contents/Managed/Mono.Cecil.* Project/Assets/XLua/Src/Editor/
Win命令行 copy UnityPath\Editor\Data\Managed\Mono.Cecil.* Project\Assets\XLua\Src\Editor\
```

不支持静态构造函数。

目前只支持Assets下代码的热补丁，不支持引擎，c#系统库的热补丁。

注意：要等打印了hotfix inject finish!后才运行例子，否则会类似xlua.access, no field __Hitfix0_Update的错误

## API
xlua.hotfix(class, [method_name], fix)

* 描述         ： 注入lua补丁
* class        ： C#类，两种表示方法，CS.Namespace.TypeName或者字符串方式"Namespace.TypeName"，字符串格式和C#的Type.GetType要求一致，如果是内嵌类型（Nested Type）是非Public类型的话，只能用字符串方式表示"Namespace.TypeName+NestedTypeName"；
* method_name  ： 方法名，可选；
* fix          ： 如果传了method_name，fix将会是一个function，否则通过table提供一组函数。table的组织按key是method_name，value是function的方式。

xlua.private_accessible(class)

* 描述          ： 让一个类的私有字段，属性，方法等可用
* class         ： 同xlua.hotfix的class参数

## 标识要热更新的类型

打上Hotfix标签即可。

## Stateless和Stateful

打Hotfix标签时，默认是Stateless方式，你也可以选Stateful方式，我们先说区别，再说使用场景。

Stateless方式是指用Lua对成员函数修复时，C#对象直接透传给作为Lua函数的第一个参数。

Stateful方式下你可以在Lua的构造函数返回一个table，然后后续成员函数调用会把这个table给传递过去。

Stateless比较适合无状态的类，有状态的话，你得通过反射去操作私有成员，也没法新增状态（field）。Stateless有个好处，可以运行的任意时刻执行替换。

Stateful的代价是会在类增加一个LuaTable类型的字段（中间层面增加，不会改源代码）。但这种方式是适用性更广，比如你不想要lua状态，可以在构造函数拦截那返回空。而且操作状态性能比反射操作C#私有变量要好，也可以随意新增任意的状态信息。缺点是，执行成员函数之前就new好的对象，接收到的状态会是空，所以需要重启，在一开始就执行替换。

## 打补丁

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

构造函数对应的method_name是".ctor"。

如果是Stateful方式，你可以返回一个table作为这个对象的状态。

和普通函数不一样的是，构造函数的热补丁并不是替换，而是开头调用lua函数后继续原有逻辑。

* 属性

对于名为“AProp”的属性，会对应一个getter，method_name等于get_AProp，setter的method_name等于set_AProp。

* []操作符

赋值对应set_Item，取值对应get_Item。第一个参数是self，赋值后面跟key，value，取值只有key参数，返回值是取出的值。

* 其它操作符

C#的操作符都有一套内部表示，比如+号的操作符函数名是op_Addition（其它操作符的内部表示可以去请参照相关资料），覆盖这函数就覆盖了C#的+号操作符。

* 事件

比如对于事件“AEvent”，+=操作符是add_AEvent，-=对应的是remove_AEvent。这两个函数均是第一个参数是self，第二个参数是操作符后面跟的delegate。

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

你只能对GenericClass\<double\>，GenericClass\<int\>这些类，而不是对GenericClass打补丁。

另外值得一提的是，要注意泛化类型的命名方式，比如GenericClass\<double\>的命名是GenericClass`1[System.Double]，具体可以看[MSDN](https://msdn.microsoft.com/en-us/library/w3f99sx1.aspx)。

对GenericClass<double>打补丁的实例如下：

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

