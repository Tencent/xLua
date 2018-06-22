# XLua configuration

All xLua configurations support three methods: tagging, static lists, and dynamic lists.

There are two requirements and two recommended items for configuration:

* List mode must use static fields/properties.
* List mode must be placed in a static type.
* Using tagging is not recommended.
* Placing the list mode configuration in the Editor directory is recommended.

**Tagging**

xLua uses a whitelist to indicate which code is to be generated, and the whitelist is configured via attributes. For example, if you want to call a C# type from Lua or you want to generate the adaptation code, you can add a LuaCallCSharp tag for this type:

~~~csharp
[LuaCallCSharp]
publicclassA
{

}
~~~

This mode is convenient, but it will increase the code on the il2cpp and therefore is not recommended.

**Static list**

Sometimes we cannot directly tag a type, such as a system API, a library without source code, or an instantiated generic type. In this case, you can declare a static field in a static type. This field can be any type except for BlackList and AdditionalProperties, as long as IEnumerable&lt;Type&gt; is implemented (these two exceptions will be specifically described later). Then add a tag to this field:

~~~csharp
[LuaCallCSharp]
public static List<Type> mymodule_lua_call_cs_list = new List<Type>()
{
    typeof(GameObject),
    typeof(Dictionary<string, int>),
};
~~~

This field needs to be placed in a **static type** and placing it in the **Editor directory** is recommended.

**Dynamic list**

Declare a static property and tag it accordingly.

~~~csharp
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
~~~

Getter is code. You can use it to implement a lot of results, such as configuration by namespace, configuration by assembly, and so on.

This property needs to be placed in a **static type** and placing it in the **Editor directory** is recommended.

### XLua.LuaCallCSharp

When adding this configuration for a C# type, xLua will generate the adapter code for this type (including constructing an instance for the type, and accessing its member properties & methods and static properties & methods). Otherwise, it will try to gain access using the reflection mode with lower performance.

Adding this configuration to the Extension Methods of a type will also generate the adaptation code and append it to the member methods of the extended type.

XLua will only generate the type loaded with this configuration. It will not automatically generate the adaptation code of its parent type. When accessing the parent type method of the child type object, if the parent type has the LuaCallCSharp configuration, the parent type's adaptation code will be executed. Otherwise it will try to gain access using the reflection mode.

The reflection mode access not only has poor performance, but also may cause failed access on the il2cpp due to code stripping. This problem can be avoided through the ReflectionUse tag, which is described below.

### XLua.ReflectionUse

When adding this configuration to a C# type, xLua generates a link.xml to block code stripping on the il2cpp.

For extension methods, you must add LuaCallCSharp or ReflectionUse to make them accessible.

It is recommended that all types to be accessed in Lua have the LuaCallCSharp or ReflectionUse tag, to insure their proper operation on all platforms.

### XLua.DoNotGen

This indicates that some of the functions, fields, and properties in a type do not generate code and are accessed through the reflection mode.

Only the fields or properties in the standard Dictionary<Type, List<string>> can be used. The key indicates the effective type. Value is a list. The name of the functions, fields, and properties with no code generated are configured.

The differences from ReflectionUse are: 1. ReflectionUse specifies the entire type; 2. Upon the first access to a function (field, property), ReflectionUse will wrap the entire type, while DoNotGen will only wrap the function (field, property). In other words, DoNotGen is lazier.

The differences from BlackList are: 1. BlackList cannot be used when it is configured. 2. BlackList can specify an overloaded function, while DoNotGen cannot.

### XLua.CSharpCallLua

This allows you to adapt a Lua function to a C# delegate (one scenario is various callbacks at the C# side: UI events, delegate parameters, such as List&lt;T&gt;:ForEach; another scenario is to use the Get function of LuaTable to indicate that a Lua function is bound to a delegate), or to adapt a Lua table to a C# interface. The delegate or interface needs this configuration.

### XLua.GCOptimize

A C# pure value type (Note: It refers to a struct that contains only the value type, and it can nest other structs that contain only the value type) or a C# enumerated value has this configuration. xLua generates gc-optimized code for this type. The result is that the value type is passed between Lua and C# with no (C#)gc alloc generated, and that no gc is generated during array access to this type. For various GC-free scenarios, refer to the 05\_NoGc example.

Any type except enumeration (including the complex types that contain parameterless constructors) will generate Lua tables for that type, as well as the conversion code of a one-dimensional array with modified type. This will optimize the performance of this conversion, including fewer gc allocs.

### XLua.AdditionalProperties

This is GCOptimize's extended configuration. Sometimes, some structs want to make the field private and access the field through the property. In this case, you need to use this configuration (by default, GCOptimize only packetizes/depacketizes the public field).

The tagging mode is relatively simple and the configuration mode is complicated. The requirements are that Dictionary&lt;Type, List&lt;string&gt;&gt; type, and the Key of the Dictionary are effective types; and value is the list of property names. See xLua's configuration of several UnityEngine value types and the SysGCOptimize type.

### XLua.BlackList

If you do not want to generate an adaption code for a member of a type, you can implement it with this configuration.

The tagging method is relatively simple, and the corresponding member can be added.

Considering that it may be necessary to add one of the overloaded functions to the blacklist, the configuration is more complicated. The type is List&lt;List&lt;string&gt;&gt;. For each member, the first-level list has only one entry and the second-level list is a string list. The first string is the full path name of the type, the second string is the member name. If the member is a method, you also need to list the full path of the type of its parameters starting from the third string.

For example, the following adds a property of GameObject and a method of FileInfo to the blacklist:

~~~csharp
[BlackList]
public static List<List<string>> BlackList = new List<List<string>>()  {
    new List<string>(){"UnityEngine.GameObject", "networkView"},
    new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
};
~~~

### The following is the generator configuration, which must be placed in the Editor directory.

### CSObjectWrapEditor.GenPath

Configures the path of the generated code, with the type being a string. By default, it is plated in &quot;Assets/XLua/Gen/&quot;.

### CSObjectWrapEditor.GenCodeMenu

This configuration is used for secondary development of the build engine. When adding this tag to a parameterless function, it will trigger calling the function when executing the &quot;XLua/Generate Code&quot; menu.

