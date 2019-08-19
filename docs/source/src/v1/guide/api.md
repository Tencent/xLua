---
title: C# API
type: guide
order: 101
---

## C# API

### LuaEnv类

#### DoString

`object[] DoString(string chunk, string chunkName = "chuck", LuaTable env = null)`

执行一个代码块。

**参数**

* chunk - Lua代码的字符串
* chunkName - 发生error时的debug显示信息中使用，指明某某代码块的某行错误
* env - 这个代码块的环境变量

**返回值**

代码块里return语句的返回值。

比如：`return 1, "hello"`中，DoString将返回包含两个object的数组， 一个是double类型的1， 一个是string类型的hello。

**示例**

```csharp
LuaEnv luaenv = new LuaEnv();
object[] ret = luaenv.DoString("print('hello')\r\nreturn 1");
UnityEngine.Debug.Log("ret=" + ret[0]);
luaenv.Dispose();
```

#### LoadString

`T LoadString<T>(string chunk, string chunkName = "chunk", LuaTable env = null)`

加载一个代码块，但不执行，只返回类型。

可以指定为一个delegate或者一个LuaFunction。

**参数**

* chunk - Lua代码的字符串
* chunkName - 发生error时的debug显示信息中使用，指明某某代码块的某行错误
* env - 这个代码块的环境变量

**返回值**

代表该代码块的delegate或LuaFunction。

#### Global

`LuaTable Global`

代表lua全局环境的LuaTable。

#### Tick

`void Tick()`

清除Lua的未手动释放的LuaBase对象（如：LuaTable、LuaFunction），以及其它一些事情。

需要定期调用，比如每秒在MonoBehaviour的Update中调用。

参考示例：[XLua Example - U3D Scripting - LuaBehaviour.cs L82](https://github.com/Tencent/xLua/blob/master/Assets/XLua/Examples/02_U3DScripting/LuaBehaviour.cs#L82)

#### AddLoader

`void AddLoader(CustomLoader loader)`

添加一个自定义loader。

**参数**

* loader - 一个包括了加载函数的委托，其类型为delegate byte\[\] CustomLoader\(ref string filepath\)。

当一个文件被require时，这个loader会被回调，其参数是调用require所使用的参数。如果该loader找到文件，可以将其读进内存，返回一个byte数组。如果需要支持调试的话，filepath要设置成IDE能找到的路径（相对或者绝对都可以）。

#### Dispose

`void Dispose()`

Dispose该LuaEnv。

**使用建议**

全局只创建一个LuaEnv实例，并在Update中调用GC方法，完全不需要时调用Dispose。

### LuaTable类

#### Get

`T Get<T>(string key)`

获取在key下，类型为T的value，如果不存在或者类型不匹配，返回null。

#### GetInPath

`T GetInPath<T>(string path)`

和Get的区别是，这个函数会识别path里的`.`。

比如`var i = tbl.GetInPath<int>("a.b.c")`相当于在lua里执行`i = tbl.a.b.c`，避免仅为了获取中间变量而多次调用Get，执行效率更高。

#### SetInPath

`void SetInPath<T>(string path, T val)`

和`GetInPath`对应的setter。

#### Get

`void Get<TKey, TValue>(TKey key, out TValue value)`

和上面API的区别是，上面的API的Key都只能是string，而这个API无此限制。

#### Set

`void Set<TKey, TValue>(TKey key, TValue value)`

对应Get的setter。

#### Cast

`T Cast<T>()`

把该table转成一个T指明的类型。

可以是一个加了CSharpCallLua声明的interface，一个有默认构造函数的class或者struct，一个Dictionary，List等等。

#### SetMetaTable

`void SetMetaTable(LuaTable metaTable)`

设置metaTable为table的metatable。

### LuaFunction类

用该类访问Lua函数会有boxing，unboxing的开销。

为了性能考虑，需要频繁调用的地方不要用该类。

建议通过`table.Get<FooDelegate>`获取一个delegate调用。

在使用`table.Get<FooDelegate>`之前，注意先把`FooDelegate`添加到代码生成列表。

#### Call

`object[] Call(params object[] args)`

以可变参数调用Lua函数，并返回该调用的返回值。

#### Call

`object[] Call(object[] args, Type[] returnTypes)`

调用Lua函数，并指明返回参数的类型，系统会自动按指定类型进行转换。

#### SetEnv

`void SetEnv(LuaTable env)`

相当于lua的setfenv函数。

## Lua API

### CS对象

#### CS.namespace.class\(...\)

调用一个C\#类型的构造函数,并返回类型实例

**示例**

```lua
local v1 = CS.UnityEngine.Vector3(1,1,1)
```

#### CS.namespace.class.field

访问一个C\#静态成员

**示例**

```lua
Print(CS.UnityEngine.Vector3.one)
```

#### CS.namespace.enum.field

访问一个枚举值

#### typeof函数

类似C\#的typeof关键字，返回一个Type对象。

**示例**

比如GameObject.AddComponent其中一个重载需要一个Type参数。

```lua
newGameObj:AddComponent(typeof(CS.UnityEngine.ParticleSystem))
```

#### 无符号64位支持

**uint64.tostring**

无符号数转字符串。

**uint64.divide**

无符号数除法。

**uint64.compare**

无符号比较，相对返回0，大于返回正数，小于返回负数。

**uint64.remainder**

无符号数取模。

**uint64.parse**

字符串转无符号数。

#### xlua.structclone

克隆一个c\#结构体

#### xlua.private\_accessible\(class\)

让一个类的私有字段，属性，方法等可用

#### cast函数

指明以特定的接口访问对象，这在实现类无法访问的时候（比如internal修饰）很有用，这时可以这么来（假设下面的calc对象实现了C\#的PerformentTest.ICalc接口）

**示例**

```lua
cast(calc, typeof(CS.PerformentTest.ICalc))
```

#### 其它

访问csharp对象和访问一个table一样，调用函数跟调用lua函数一样，也可以通过操作符访问c\#的操作符，示例：

```lua
local v1=CS.UnityEngine.Vector3(1,1,1)
local v2=CS.UnityEngine.Vector3(1,1,1)
v1.x = 100
v2.y = 100
print(v1, v2)
local v3 = v1 + v2
print(v1.x, v2.x)
print(CS.UnityEngine.Vector3.one)
print(CS.UnityEngine.Vector3.Distance(v1, v2))
```

## 类型映射

### 基本数据类型

| C\#类型 | Lua类型 |
| :--- | :--- |
| sbyte，byte，short，ushort，int，uint，double，char，float | number |
| decimal | userdata |
| long，ulong | userdata/lua\_Integer\(lua53\) |
| bytes\[\] | string |
| bool | boolean |
| string | string |

### 复杂数据类型

| C\#类型 | Lua类型 |
| :--- | :--- |
| LuaTable | table |
| LuaFunction | function |
| class或struct的实例 | userdata，table |
| method，delegate | function |

#### LuaTable

C\#侧指明从Lua侧输入（包括C\#方法的输入参数或者Lua方法的返回值）LuaTable类型，则要求Lua侧为table。或者Lua侧的table，在C\#侧未指明类型的情况下转换成LuaTable。

#### LuaFunction

C\#侧指明从Lua侧输入（包括C\#方法的输入参数或者Lua方法的返回值）LuaFunction类型，则要求Lua侧为function。或者Lua侧的function，在C\#侧未指明类型的情况下转换成LuaFunction。

#### LuaUserData

对应非C\# Managered对象的lua userdata。

#### class或struct的实例

从C\#传一个class或者struct的实例，将映射到Lua的userdata，并通过\_\_index访问该userdata的成员 C\#侧指明从Lua侧输入指定类型对象，Lua侧为该类型实例的userdata可以直接使用；如果该指明类型有默认构造函数，Lua侧是table则会自动转换，转换规则是：调用构造函数构造实例，并用table对应字段转换到c\#对应值后赋值各成员。

#### method， delegate

成员方法以及delegate都是对应lua侧的函数。 C\#侧的普通参数以及引用参数，对应lua侧函数参数；C\#侧的返回值对应于Lua的第一个返回值；引用参数和out参数则按序对应于Lua的第2到第N个参数。

## 宏

### HOTFIX\_ENABLE

打开hotfix功能。

### NOT\_GEN\_WARNING

反射时打印warning。

### GEN\_CODE\_MINIMIZE

以偏向减少代码段的方式生成代码。

