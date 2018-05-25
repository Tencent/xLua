## C# API
### LuaEnv类
#### object[] DoString(string chunk, string chunkName = "chuck", LuaTable env = null)
描述：

    执行一个代码块。

参数：

    chunk: Lua代码的字符串；
    chunkName： 发生error时的debug显示信息中使用，指明某某代码块的某行错误；
    env ：这个代码块的环境变量；
返回值：

    代码块里return语句的返回值;
    比如：return 1, “hello”，DoString返回将包含两个object的数组， 一个是double类型的1， 一个是string类型的“hello”
    
例如：

    LuaEnv luaenv = new LuaEnv();
    object[] ret = luaenv.DoString("print(‘hello’)\r\nreturn 1")
    UnityEngine.Debug.Log("ret="+ret[0]);
    luaenv.Dispose()

#### T LoadString<T>(string chunk, string chunkName = "chunk", LuaTable env = null)

描述：

    加载一个代码块，但不执行，只返回类型可以指定为一个delegate或者一个LuaFunction

参数：

    chunk: Lua代码的字符串；
    chunkName： 发生error时的debug显示信息中使用，指明某某代码块的某行错误；
    env ：这个代码块的环境变量；

返回值：

    代表该代码块的delegate或者LuaFunction类；

#### LuaTable Global;

描述：

    代表lua全局环境的LuaTable

### void Tick()

描述：

    清除Lua的未手动释放的LuaBase对象（比如：LuaTable， LuaFunction），以及其它一些事情。
    需要定期调用，比如在MonoBehaviour的Update中调用。

### void AddLoader(CustomLoader loader)

描述：

    增加一个自定义loader

参数：

    loader：一个包括了加载函数的委托，其类型为delegate byte[] CustomLoader(ref string filepath)，当一个文件被require时，这个loader会被回调，其参数是调用require所使用的参数，如果该loader找到文件，可以将其读进内存，返回一个byte数组。如果需要支持调试的话，而filepath要设置成IDE能找到的路径（相对或者绝对都可以）

#### void Dispose()

描述：
    
    Dispose该LuaEnv。

> LuaEnv的使用建议：全局就一个实例，并在Update中调用GC方法，完全不需要时调用Dispose

### LuaTable类

#### T Get<T>(string key)

描述：

    获取在key下，类型为T的value，如果不存在或者类型不匹配，返回null；


#### T GetInPath<T>(string path)

描述：

    和Get的区别是，这个函数会识别path里头的“.”，比如var i = tbl.GetInPath<int>(“a.b.c”)相当于在lua里头执行i = tbl.a.b.c，避免仅为了获取中间变量而多次调用Get，执行效率更高。

#### void SetInPath<T>(string path, T val)

描述：

    和GetInPaht<T>对应的setter；

#### void Get<TKey, TValue>(TKey key, out TValue value)

描述：

     上面的API的Key都只能是string，而这个API无此限制；

#### void Set<TKey, TValue>(TKey key, TValue value)

描述：

     对应Get<TKey, TValue>的setter；

#### T Cast<T>()

描述：

    把该table转成一个T指明的类型，可以是一个加了CSharpCallLua声明的interface，一个有默认构造函数的class或者struct，一个Dictionary，List等等。

#### void SetMetaTable(LuaTable metaTable)

描述：
    
    设置metaTable为table的metatable

### LuaFunction类

> 注意：用该类访问Lua函数会有boxing，unboxing的开销，为了性能考虑，需要频繁调用的地方不要用该类。建议通过table.Get<ABCDelegate>获取一个delegate再调用（假设ABCDelegate是C#的一个delegate）。在使用使用table.Get<ABCDelegate>之前，请先把ABCDelegate加到代码生成列表。

#### object[] Call(params object[] args)

描述：

    以可变参数调用Lua函数，并返回该调用的返回值。

#### object[] Call(object[] args, Type[] returnTypes)

描述：

    调用Lua函数，并指明返回参数的类型，系统会自动按指定类型进行转换。

#### void SetEnv(LuaTable env)

描述：

    相当于lua的setfenv函数。

## Lua API

### CS对象

#### CS.namespace.class(...)

描述：

    调用一个C#类型的构造函数,并返回类型实例

例如：

    local v1=CS.UnityEngine.Vector3(1,1,1) 

#### CS.namespace.class.field

描述：

    访问一个C#静态成员
    
例如：

    Print(CS.UnityEngine.Vector3.one)


#### CS.namespace.enum.field

描述：
    
    访问一个枚举值

#### typeof函数

描述：
    
    类似C#里头的typeof关键字，返回一个Type对象，比如GameObject.AddComponent其中一个重载需要一个Type参数

例如：

    newGameObj:AddComponent(typeof(CS.UnityEngine.ParticleSystem))


#### 无符号64位支持

##### uint64.tostring

描述：

    无符号数转字符串。

##### uint64.divide

描述：

    无符号数除法。

##### uint64.compare

描述：

    无符号比较，相对返回0，大于返回正数，小于返回负数。

##### uint64.remainder

描述：
    
    无符号数取模。
    
##### uint64.parse

描述：
    字符串转无符号数。

#### xlua.structclone

描述：
    
    克隆一个c#结构体

#### cast函数

描述：
    
    指明以特定的接口访问对象，这在实现类无法访问的时候（比如internal修饰）很有用，这时可以这么来（假设下面的calc对象实现了C#的PerformentTest.ICalc接口）

例如：
    
    cast(calc, typeof(CS.PerformentTest.ICalc))

然后就木有其它API了
访问csharp对象和访问一个table一样，调用函数跟调用lua函数一样，也可以通过操作符访问c#的操作符，下面是一个例如：

    local v1=CS.UnityEngine.Vector3(1,1,1) 
    local v2=CS.UnityEngine.Vector3(1,1,1) 
    v1.x = 100 
    v2.y = 100 
    print(v1, v2)
    local v3 = v1 + v2
    print(v1.x, v2.x) 
    print(CS.UnityEngine.Vector3.one)
    print(CS.UnityEngine.Vector3.Distance(v1, v2))

## 类型映射

### 基本数据类型


|C#类型|Lua类型|
|-|-|
|sbyte，byte，short，ushort，int，uint，double，char，float|number|
|decimal|userdata|
|long，ulong|userdata/lua_Integer(lua53)|
|bytes[]|string|
|bool|boolean|
|string|string|

### 复杂数据类型

|C#类型|Lua类型|
|-|-|
|LuaTable|table|
|LuaFunction|function|
|class或者 struct的实例|userdata，table|
|method，delegate|function|

#### LuaTable：

C#侧指明从Lua侧输入（包括C#方法的输入参数或者Lua方法的返回值）LuaTable类型，则要求Lua侧为table。或者Lua侧的table，在C#侧未指明类型的情况下转换成LuaTable。

#### LuaFunction：

C#侧指明从Lua侧输入（包括C#方法的输入参数或者Lua方法的返回值）LuaFunction类型，则要求Lua侧为function。或者Lua侧的function，在C#侧未指明类型的情况下转换成LuaFunction。

#### LuaUserData：

对应非C# Managered对象的lua userdata。

#### class或者 struct的实例:

从C#传一个class或者struct的实例，将映射到Lua的userdata，并通过__index访问该userdata的成员
C#侧指明从Lua侧输入指定类型对象，Lua侧为该类型实例的userdata可以直接使用；如果该指明类型有默认构造函数，Lua侧是table则会自动转换，转换规则是：调用构造函数构造实例，并用table对应字段转换到c#对应值后赋值各成员。

#### method， delegate：

成员方法以及delegate都是对应lua侧的函数。
C#侧的普通参数以及引用参数，对应lua侧函数参数；C#侧的返回值对应于Lua的第一个返回值；引用参数和out参数则按序对应于Lua的第2到第N个参数。