# xLua的配置

xLua所有的配置都支持三种方式：打标签；静态列表；动态列表。

配置有两必须两建议：

* 列表方式均必须是static的字段/属性
* 列表方式均必须放到一个static类
* 建议不用标签方式
* 建议列表方式配置放Editor目录（如果是Hotfix配置，而且类位于Assembly-CSharp.dll之外的其它dll，必须放Editor目录）

**打标签**

xLua用白名单来指明生成哪些代码，而白名单通过attribute来配置，比如你想从lua调用c#的某个类，希望生成适配代码，你可以为这个类型打一个LuaCallCSharp标签：

```csharp

[LuaCallCSharp]
publicclassA
{

}

```

该方式方便，但在il2cpp下会增加不少的代码量，不建议使用。

**静态列表**

有时我们无法直接给一个类型打标签，比如系统api，没源码的库，或者实例化的泛化类型，这时你可以在一个静态类里声明一个静态字段，该字段的类型除BlackList和AdditionalProperties之外只要实现了IEnumerable&lt;Type&gt;就可以了（这两个例外后面具体会说），然后为这字段加上标签：

```csharp

[LuaCallCSharp]
public static List<Type> mymodule_lua_call_cs_list = new List<Type>()
{
    typeof(GameObject),
    typeof(Dictionary<string, int>),
};

```

这个字段需要放到一个 **静态类** 里头，建议放到 **Editor目录** 。

**动态列表**

声明一个静态属性，打上相应的标签即可。

```csharp

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

```

Getter是代码，你可以实现很多效果，比如按名字空间配置，按程序集配置等等。

这个属性需要放到一个 **静态类** 里头，建议放到 **Editor目录** 。

### XLua.LuaCallCSharp

一个C#类型加了这个配置，xLua会生成这个类型的适配代码（包括构造该类型实例，访问其成员属性、方法，静态属性、方法），否则将会尝试用性能较低的反射方式来访问。

一个类型的扩展方法（Extension Methods）加了这配置，也会生成适配代码并追加到被扩展类型的成员方法上。

xLua只会生成加了该配置的类型，不会自动生成其父类的适配代码，当访问子类对象的父类方法，如果该父类加了LuaCallCSharp配置，则执行父类的适配代码，否则会尝试用反射来访问。

反射访问除了性能不佳之外，在il2cpp下还有可能因为代码剪裁而导致无法访问，后者可以通过下面介绍的ReflectionUse标签来避免。

### XLua.ReflectionUse

一个C#类型类型加了这个配置，xLua会生成link.xml阻止il2cpp的代码剪裁。

对于扩展方法，必须加上 `LuaCallCSharp` 或者 `ReflectionUse` 才可以被访问到。

建议所有要在Lua访问的类型，要么加LuaCallCSharp，要么加上ReflectionUse，这才能够保证在各平台都能正常运行。

### XLua.DoNotGen

指明一个类里头的部分函数、字段、属性不生成代码，通过反射访问。

只能标准 `Dictionary<Type, List<string>>` 的field或者property。key指明的是生效的类，value是一个列表，配置的是不生成代码的函数、字段、属性的名字。

和ReflectionUse的区别是：1、ReflectionUse指明的是整个类；2、当第一次访问一个函数（字段、属性）时，ReflectionUse会把整个类都wrap，而DoNotGen只wrap该函数（字段、属性），换句话DoNotGen更lazy一些；

和BlackList的区别是：1、BlackList配了就不能用；2、BlackList能指明某重载函数，DoNotGen不能；

### XLua.CSharpCallLua

如果希望把一个lua函数适配到一个C# delegate（一类是C#侧各种回调：UI事件，delegate参数，比如List&lt;T&gt;:ForEach；另外一类场景是通过LuaTable的Get函数指明一个lua函数绑定到一个delegate）。或者把一个lua table适配到一个C# interface，该delegate或者interface需要加上该配置。

### XLua.GCOptimize

一个C#纯值类型（注：指的是一个只包含值类型的struct，可以嵌套其它只包含值类型的struct）或者C#枚举值加上了这个配置。xLua会为该类型生成gc优化代码，效果是该值类型在lua和c#间传递不产生（C#）gc alloc，该类型的数组访问也不产生gc。各种无GC的场景，可以参考 `05_Gc` 例子。

除枚举之外，包含无参构造函数的复杂类型，都会生成lua table到该类型，以及改类型的一维数组的转换代码，这将会优化这个转换的性能，包括更少的gc alloc。

### XLua.AdditionalProperties

这个是GCOptimize的扩展配置，有的时候，一些struct喜欢把field做成是私有的，通过property来访问field，这时就需要用到该配置（默认情况下GCOptimize只对public的field打解包）。

标签方式比较简单，配置方式复杂一点，要求是 `Dictionary<Type, List<string>>` 类型，Dictionary 的 Key 是要生效的类型，Value 是属性名列表。可以参考XLua对几个UnityEngine下值类型的配置，SysGCOptimize类。

### XLua.BlackList

如果你不要生成一个类型的一些成员的适配代码，你可以通过这个配置来实现。

标签方式比较简单，对应的成员上加就可以了。

由于考虑到有可能需要把重载函数的其中一个重载列入黑名单，配置方式比较复杂，类型是 `List<List<string>>`，对于每个成员，在第一层List有一个条目，第二层List是个string的列表，第一个string是类型的全路径名，第二个string是成员名，如果成员是一个方法，还需要从第三个string开始，把其参数的类型全路径全列出来。

例如下面是对 GameObject 的一个属性以及FileInfo的一个方法列入黑名单：

```csharp

[BlackList]
public static List<List<string>> BlackList = new List<List<string>>()  {
    new List<string>(){"UnityEngine.GameObject", "networkView"},
    //new List<string>(){ typeof(UnityEngine.GameObject).FullName, "networkView"},
    new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
    //new List<string>(){ typeof(System.IO.FileInfo).FullName, "GetAccessControl",typeof(System.Security.AccessControl.AccessControlSections).FullName },
};

```

### 下面是生成期配置，必须放到Editor目录下

### CSObjectWrapEditor.GenPath

配置生成代码的放置路径，类型是string。默认放在&quot;Assets/XLua/Gen/&quot;下。

### CSObjectWrapEditor.GenCodeMenu

该配置用于生成引擎的二次开发，一个无参数函数加了这个标签，在执行&quot;XLua/Generate Code&quot;菜单时会触发这个函数的调用。