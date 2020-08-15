## xLua教程

### Lua文件加载

1. 执行字符串

    最基本是直接用LuaEnv.DoString执行一个字符串，当然，字符串得符合Lua语法
    比如：

        luaenv.DoString("print('hello world')")

    完整代码见XLua\Tutorial\LoadLuaScript\ByString目录
    > 但这种方式并不建议，更建议下面介绍这种方法。

2. 加载Lua文件

    用lua的require函数即可
    比如：

        DoString("require 'byfile'")

    完整代码见XLua\Tutorial\LoadLuaScript\ByFile目录

    require实际上是调一个个的loader去加载，有一个成功就不再往下尝试，全失败则报文件找不到。
    目前xLua除了原生的loader外，还添加了从Resource加载的loader，需要注意的是因为Resource只支持有限的后缀，放Resources下的lua文件得加上txt后缀（见附带的例子）。

    建议的加载Lua脚本方式是：整个程序就一个DoString("require 'main'")，然后在main.lua加载其它脚本（类似lua脚本的命令行执行：lua main.lua）。

    有童鞋会问：要是我的Lua文件是下载回来的，或者某个自定义的文件格式里头解压出来，或者需要解密等等，怎么办？问得好，xLua的自定义Loader可以满足这些需求。

3. 自定义Loader

    在xLua加自定义loader是很简单的，只涉及到一个接口：

        public delegate byte[] CustomLoader(ref string filepath);
        public void LuaEnv.AddLoader(CustomLoader loader)

    通过AddLoader可以注册个回调，该回调参数是字符串，lua代码里头调用require时，参数将会透传给回调，回调中就可以根据这个参数去加载指定文件，如果需要支持调试，需要把filepath修改为真实路径传出。该回调返回值是一个byte数组，如果为空表示该loader找不到，否则则为lua文件的内容。
    有了这个就简单了，用IIPS的IFS？没问题。写个loader调用IIPS的接口读文件内容即可。文件已经加密？没问题，自己写loader读取文件解密后返回即可。。。
    完整示例见XLua\Tutorial\LoadLuaScript\Loader

### C#访问Lua

这里指的是C#主动发起对Lua数据结构的访问。
本章涉及到的例子都可以在XLua\Tutorial\CSharpCallLua下找到。

1. 获取一个全局基本数据类型
    访问LuaEnv.Global就可以了，上面有个模版Get方法，可指定返回的类型。
    
        luaenv.Global.Get<int>("a")
        luaenv.Global.Get<string>("b")
        luaenv.Global.Get<bool>("c")

2. 访问一个全局的table

    也是用上面的Get方法，那类型要指定成啥呢？
    1. 映射到普通class或struct

        定义一个class，有对应于table的字段的public属性，而且有无参数构造函数即可，比如对于{f1 = 100, f2 = 100}可以定义一个包含public int f1;public int f2;的class。
        这种方式下xLua会帮你new一个实例，并把对应的字段赋值过去。

        table的属性可以多于或者少于class的属性。可以嵌套其它复杂类型。
        要注意的是，这个过程是值拷贝，如果class比较复杂代价会比较大。而且修改class的字段值不会同步到table，反过来也不会。

        这个功能可以通过把类型加到GCOptimize生成降低开销，详细可参见配置介绍文档。
        那有没有引用方式的映射呢？有，下面这个就是：

    2. 映射到一个interface
    
        这种方式依赖于生成代码（如果没生成代码会抛InvalidCastException异常），代码生成器会生成这个interface的实例，如果get一个属性，生成代码会get对应的table字段，如果set属性也会设置对应的字段。甚至可以通过interface的方法访问lua的函数。

    3. 更轻量级的by value方式：映射到Dictionary<>，List<>

        不想定义class或者interface的话，可以考虑用这个，前提table下key和value的类型都是一致的。

    4. 另外一种by ref方式：映射到LuaTable类
    
        这种方式好处是不需要生成代码，但也有一些问题，比如慢，比方式2要慢一个数量级，比如没有类型检查。

3. 访问一个全局的function

    仍然是用Get方法，不同的是类型映射。
    1. 映射到delegate

        这种是建议的方式，性能好很多，而且类型安全。缺点是要生成代码（如果没生成代码会抛InvalidCastException异常）。

        delegate要怎样声明呢？
        对于function的每个参数就声明一个输入类型的参数。
        多返回值要怎么处理？从左往右映射到c#的输出参数，输出参数包括返回值，out参数，ref参数。

        参数、返回值类型支持哪些呢？都支持，各种复杂类型，out，ref修饰的，甚至可以返回另外一个delegate。

        delegate的使用就更简单了，直接像个函数那样用就可以了。

    2. 映射到LuaFunction
        
        这种方式的优缺点刚好和第一种相反。
        使用也简单，LuaFunction上有个变参的Call函数，可以传任意类型，任意个数的参数，返回值是object的数组，对应于lua的多返回值。

4. 使用建议

    1. 访问lua全局数据，特别是table以及function，代价比较大，建议尽量少做，比如在初始化时把要调用的lua function获取一次（映射到delegate）后，保存下来，后续直接调用该delegate即可。table也类似。

    2. 如果lua侧的实现的部分都以delegate和interface的方式提供，使用方可以完全和xLua解耦：由一个专门的模块负责xlua的初始化以及delegate、interface的映射，然后把这些delegate和interface设置到要用到它们的地方。

### Lua调用C#

> 本章节涉及到的实例均在XLua\Tutorial\LuaCallCSharp下

#### new C#对象

你在C#这样new一个对象：

    var newGameObj = new UnityEngine.GameObject();

对应到Lua是这样：

    local newGameObj = CS.UnityEngine.GameObject()

基本类似，除了：

    1. lua里头没有new关键字；
    2. 所有C#相关的都放到CS下，包括构造函数，静态成员属性、方法；

如果有多个构造函数呢？放心，xlua支持重载，比如你要调用GameObject的带一个string参数的构造函数，这么写：

    local newGameObj2 = CS.UnityEngine.GameObject('helloworld')

#### 访问C#静态属性，方法

##### 读静态属性

    CS.UnityEngine.Time.deltaTime

##### 写静态属性

    CS.UnityEngine.Time.timeScale = 0.5

##### 调用静态方法

    CS.UnityEngine.GameObject.Find('helloworld')

小技巧：如果需要经常访问的类，可以先用局部变量引用后访问，除了减少敲代码的时间，还能提高性能：

    local GameObject = CS.UnityEngine.GameObject
    GameObject.Find('helloworld')

#### 访问C#成员属性，方法

##### 读成员属性

    testobj.DMF

##### 写成员属性

    testobj.DMF = 1024

##### 调用成员方法

注意：调用成员方法，第一个参数需要传该对象，建议用冒号语法糖，如下

    testobj:DMFunc()

##### 父类属性，方法

xlua支持（通过派生类）访问基类的静态属性，静态方法，（通过派生类实例）访问基类的成员属性，成员方法

##### 参数的输入输出属性（out，ref）

Lua调用侧的参数处理规则：C#的普通参数算一个输入形参，ref修饰的算一个输入形参，out不算，然后从左往右对应lua 调用侧的实参列表；

Lua调用侧的返回值处理规则：C#函数的返回值（如果有的话）算一个返回值，out算一个返回值，ref算一个返回值，然后从左往右对应lua的多返回值。

##### 重载方法
直接通过不同的参数类型进行重载函数的访问，例如：

    testobj:TestFunc(100)
    testobj:TestFunc('hello')

将分别访问整数参数的TestFunc和字符串参数的TestFunc。

注意：xlua只一定程度上支持重载函数的调用，因为lua的类型远远不如C#丰富，存在一对多的情况，比如C#的int，float，double都对应于lua的number，上面的例子中TestFunc如果有这些重载参数，第一行将无法区分开来，只能调用到其中一个（生成代码中排前面的那个）

##### 操作符

支持的操作符有：+，-，*，/，==，一元-，<，<=， %，[]

##### 参数带默认值的方法

和C#调用有默认值参数的函数一样，如果所给的实参少于形参，则会用默认值补上。

##### 可变参数方法
对于C#的如下方法：

    void VariableParamsFunc(int a, params string[] strs)

可以在lua里头这样调用：

    testobj:VariableParamsFunc(5, 'hello', 'john')

##### 使用Extension methods

在C#里定义了，lua里就能直接使用。

##### 泛化（模版）方法

不直接支持，可以通过Extension methods功能进行封装后调用。

##### 枚举类型

枚举值就像枚举类型下的静态属性一样。

    testobj:EnumTestFunc(CS.Tutorial.TestEnum.E1)

上面的EnumTestFunc函数参数是Tutorial.TestEnum类型的。

枚举类支持__CastFrom方法，可以实现从一个整数或者字符串到枚举值的转换，例如：

    CS.Tutorial.TestEnum.__CastFrom(1)
    CS.Tutorial.TestEnum.__CastFrom('E1')

##### delegate使用（调用，+，-）

C#的delegate调用：和调用普通lua函数一样

+操作符：对应C#的+操作符，把两个调用串成一个调用链，右操作数可以是同类型的C# delegate或者是lua函数。

-操作符：和+相反，把一个delegate从调用链中移除。

> Ps：delegate属性可以用一个luafunction来赋值。

##### event

比如testobj里头有个事件定义是这样：public event Action TestEvent;

增加事件回调

    testobj:TestEvent('+', lua_event_callback)

移除事件回调

    testobj:TestEvent('-', lua_event_callback)

##### 64位整数支持

    Lua53版本64位整数（long，ulong）映射到原生的64位整数，而luajit版本，相当于lua5.1的标准，本身不支持64位，xlua做了个64位支持的扩展库，C#的long和ulong都将映射到userdata：
    
    支持在lua里头进行64位的运算，比较，打印
    
    支持和lua number的运算，比较
    
    要注意的是，在64扩展库中，实际上只有int64，ulong也会先强转成long再传递到lua，而对ulong的一些运算，比较，我们采取和java一样的支持方式，提供一组API，详情请看API文档。

##### C#复杂类型和table的自动转换

对于一个有无参构造函数的C#复杂类型，在lua侧可以直接用一个table来代替，该table对应复杂类型的public字段有相应字段即可，支持函数参数传递，属性赋值等，例如：
C#下B结构体（class也支持）定义如下：

    public struct A
    {
        public int a;
    }

    public struct B
    {
        public A b;
        public double c;
    }

某个类有成员函数如下：

    void Foo(B b)

在lua可以这么调用

    obj:Foo({b = {a = 100}, c = 200})

##### 获取类型（相当于C#的typeof）

比如要获取UnityEngine.ParticleSystem类的Type信息，可以这样

    typeof(CS.UnityEngine.ParticleSystem)

##### “强”转

lua没类型，所以不会有强类型语言的“强转”，但有个有点像的东西：告诉xlua要用指定的生成代码去调用一个对象，这在什么情况下能用到呢？有的时候第三方库对外暴露的是一个interface或者抽象类，实现类是隐藏的，这样我们无法对实现类进行代码生成。该实现类将会被xlua识别为未生成代码而用反射来访问，如果这个调用是很频繁的话还是很影响性能的，这时我们就可以把这个interface或者抽象类加到生成代码，然后指定用该生成代码来访问：

    cast(calc, typeof(CS.Tutorial.Calc))

上面就是指定用CS.Tutorial.Calc的生成代码来访问calc对象。
