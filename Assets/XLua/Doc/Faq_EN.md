# FAQs

## How to use xLua distribution package?

xLua is currently released as a zip package and can be extracted to the project directory.

## Can xLua be placed in another directory?

Yes, but the generated code directory needs to be configured (by default, it is in the Assets\XLua\Gen directory). For details, see the GenPath configuration in XLua Configuration.doc.

The important thing to note about changing directories is that the generated code and xLua core code must be in the same assembly. If you want to use the hotfix function, the xLua core code must be in the Assembly-CSharp assembly.

## Does Lua source code only use the txt extension?

It can use any extension.

If you want to add TextAsset to an installation package (for example, to the Resources directory), Unity does not identify the Lua extension. This is Unity's rule.

If you do not add it to the installation package, there is no limit to the extension. For example, in case that you download it to a directory (this is also practicable in hotfix mode), and then read this directory with CustomLoader or by setting package.path.

Why does the Lua source code (including examples) of xLua use the txt extension? Because xLua itself is a library, it doesn't provide download functionality, and it's inconvenience to download code from somewhere else during runtime. TextAsset is a simpler solution.

## The editor (or non-il2cpp for Android) runs normally, but when iOS calls a function, "attempt to call a nil value" is reported.

By default, il2cpp will strip code, such as engine code, C# system APIs, and third-party dlls. In simple terms, functions in these places will not be compiled into your final release package if your C# code does not access them.

Solution: Add a reference (for example, configuring it to LuaCallCSharp, or adding access to that function to your C# code), or use the link.xml configuration (When ReflectionUse is configured, xLua will automatically configure it for you in link.xml) to tell il2cpp not to strip a certain type of code.

## Where can I find Plugins source code and how can I use it?

Plugins source code is in the xLua_Project_Root/build.

The source code compilation relies on CMake. After installing CMake, execute make_xxxx_yyyy.zz. xxxx stands for the platform, such as iOS or Android; yyyy is the virtual machine to be integrated, including Lua 5.3 and LuaJIT. The file extension is zz. The extension is bat for Windows and sh for other platforms.

Windows compilation relies on Visual Studio 2015.

Android compilation uses Linux, relies on NDK, and needs to point ANDROID_NDK in the script to the installation directory of NDK.

iOS and OS X need to be compiled on a Mac.

## How do I solve the "xlua.access, no field __Hitfix0_Update" error?

Follow the [Hotfix Operation Guide](hotfix.md).

## How do I solve the "please install the Tools" error?

Do not install Tools to the same level directory as Assets. You can find Tools in the installation package, or in the master directory.

## How do I solve the "This delegate/interface must add to CSharpCallLua: XXX" error?

In the editor, xLua can run even without generating code. This prompt appears either because CSharpCallLua was not to the type, or because the code was generated before adding, but no generation was executed again.

Solution: After confirming that CSharpCallLua has been added to XXX (type name), clear the code and run it again.

If there is no problem with the editor, the error will be reported to the mobile phone. This means that you did not generate the code (execute “XLua/Generate Code”) before release.

## What do I do if executing "XLua/Hotfix Inject In Editor" menu on Unity 5.5 or later versions produces the following prompt: "WARNING: The runtime version supported by this application is unavailable."

This is because the injection tool was compiled with .NET 3.5. The Unity 5.5 warning means that MonoBleedingEdge's mono environment does not support .NET 3.5. However, due to backward compatibility, no real problems related to this warning have been found so far.

You may find that defining INJECT_WITHOUT_TOOL in nested mode will not produce this warning. However, the problem is that this mode is used for debugging and is not recommended because it may cause some library conflicts.

## How do I trigger an event in hotfix?

Firstly, enable private member access using xlua.private_accessible.

Then, call delegates using the "&event name" field of the object, for example self\['&MyEvent'\](), where MyEvent is the event name.

## How do I patch Unity Coroutine's implementation function?

See the corresponding section of the [Hotfix Operation Guide](hotfix.md).

## Is NGUI (or UGUI/DOTween, etc...) supported?

Yes. The most important feature of xLua is that what you write with C# can be originally replaced with Lua, and the plugins available on C# will remain available.

## If debugging is needed, how do I deal with the filepath parameter of CustomLoader?

When Lua calls require 'a.b', CustomLoader will be called and the string "a.b" will be injected. You need to understand this string, load the Lua file (from file/memory/network, etc...) and return two things. The first thing is a path that the debugger can understand, for example a/b.lua, which is returned by setting the filepath parameter of the ref type. The second thing is the bytes[] of the source code in UTF8 format, which is returned with the returned value.

## What is generated code?

XLua supports one kind of Lua-C# interaction technique, which implements interaction by generating adaptation code between the two. It has better performance and is therefore recommended.

Another interaction technique is reflection, which has less impact on the installation package and can be used in scenarios which have lower performance requirements and has installation package size limit.

## How do I solve errors with the code generated before and after changing the interface?

Clear the generated code (execute the "Clear Generated Code" menu, which may disappear after restart. Then you can manually delete the entire generated code directory), and then regenerate the code when the compilation is completed.

## When should the code be generated?

During the development period, it is not recommended that code be generated, to avoid many compilation failures due to inconsistency and waiting time during compilation of the generated code.

The generated code must be executed before the build version for the mobile phone. Automatic execution is recommended.

Optimize performance. The generated code must be executed before the performance test because there are significant differences between the generated code and the code not generated.

## Do all C# APIs in CS namespaces occupy high memory?

Due to the use of LazyLoad, their existences are just a virtual concepts. For example, for UnityEngine.GameObject, its methods, and properties are loaded only when accessing the first CS.UnityEngine.GameObject or transferring the first instance to Lua.

## In what scenarios are LuaCallSharp and CSharpCallLua used?

It depends on the caller and the callee. For example, if you want to call C#'s GameObject, find a function in Lua, or call GameObject's instance methods or properties, the GameObject type needs to be added to LuaCallSharp. If you want to add a Lua function to the UI callback (in this case, C# is the caller and the Lua function is the callee), the delegate declared by the callback needs to be added to CSharpCallLua.

Sometimes, it is confusing, like when calling List<int>, for example. Find(Predicate<int> match) and List<int> will of course be added to LuaCallSharp. However, Predicate<int> needs to be added to CSharpCallLua, because the caller of match is C#, and a Lua function is called.

A more unthinkable way: When you see, "This delegate/interface must add to CSharpCallLua: XXX", just add XXX to CSharpCallLua.

## Will gc alloc appear in value type transfer?

If you are using the delegate to call a Lua function, if the LuaTable and LuaFunction that you use have no gc interface, or if there is an array, the following value types have no gc:

1. All the basic value types (all integers, all floating-point numbers, decimals)

2. All enumerated types

3. The field contains only the struct of value type, and it can nest other struct. 

For 2 and 3, pleases add those types to GCOptimize.

## Is reflection available on iOS?

There are two restrictions on iOS: 1. no JIT; 2. code stripping;

When C# calls Lua via delegates or interfaces, using reflection emit instead of the generated code relies on JIT, so this is currently only available in the editor mode.

If Lua calls C#, it will be mainly affected by code stripping. In this case, you can configure ReflectionUse (but not LuaCallSharp), and execute "Generate Code". No package code except link.xml will be generated for the type this time. Set this type to 'not to be stripped'.

In short, only CSharpCallLua is necessary (a little code of this type is generated), and reflection can be used in LuaCallSharp generation.

## Is calling generic methods supported?

This is partially supported. See [Example 9 for the degree of support.](../Examples/09_GenericMethod/)

There are other ways to call generic methods. If it is a static method, you can write a package to instantiate the generic method.

If it is a member method, xLua supports the extension method. You can add an extension method to instantiate a generic method. This extension method is just like an ordinary member method.

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

## Can Lua call C# overloaded functions?

Yes, but this is not as well supported as on C#. For example, in the overloaded methods void Foo(int a) and void Foo(short a), because int and short both correspond to the number on Lua, it is impossible to use the parameters to judge which overloaded method is called. In this situation, you can use the extension method to give an alias to one of them.

## What should I do if "A method/property/field is not defined" is reported when the code is generated during packaging, but it runs normally on the editor?

This is often because the method/property/field is extended in the conditional compilation, which is only available in UNITY_EDITOR. This can be solved by adding this method/property/field to the blacklist, and then re-executing code generation when the compilation is completed. 

## Why is this[string field] or this[object field] operator overload inaccessible in Lua? (For example, in Dictionary\<string, xxx\>, I cannot retrieve values in Dictionary\<object, xxx\> using dic['abc'] or dic.abc on Lua)

Because: 1. This feature will cause the base type-defined methods, properties, and fields to be inaccessible. (For example, the Animation cannot access the GetComponent method.); 2. The data cannot be retrieved if the key is the name of a method, property, or field of the current type. For example, in the Dictionary type, dic['TryGetValue'] returns a function that points to the TryGetValue method of the Dictionary.

If your version is newer than 2.1.11, you can use get_Item to get the value and set_Item to set the value. It should be noted that only this[string field] or this[object field] operator has these two alternative APIs. The keys of other types do not.

~~~lua
dic:set_Item('a', 1)
dic:set_Item('b', 2)
print(dic:get_Item('a'))
print(dic:get_Item('b'))
~~~

If your version is 2.1.11 or earlier, it is recommended that you directly use the equivalent method of the operator, such as the TryGetValue of the Dictionary. If this method is not available, you can package a method through the Extension method on C# and then use it.

## Why are some Unity objects null in C# but not nil in Lua, for example, a GameObject that has been destroyed?

In fact, the C# object is not null, but the UnityEngine.Object overloads the == operator. When an object is destroyed, or in the case of failed initialization, true is returned for obj == null, but this C# object is not null. You can check this using System.Object.ReferenceEquals(null, obj).

In this case, you can write an extension method for UnityEngine.Object.

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

Then, in Lua, use IsNull for all UnityEngine.Object instances.

~~~lua
print(go:GetComponent('Animator'):IsNull())
~~~

## How do I construct a generic instance?

If the types involved are all in the mscorlib and Assembly-CSharp assembly, the construction of the generic instance is the same as that of ordinary types. Both are CS.namespace.typename(). The typename expression is special, and the typename expression of the generic instance contains the identifier illegal symbol. The last part should be replaced with ["typename"]. Use List<string> as an example.

~~~lua
local lst = CS.System.Collections.Generic["List`1[System.String]"]()
~~~

If the typename of a generic instance is undefined, typeof(undefined type).ToString() can be printed on C#.

If the types involved are not in the mscorlib and Assembly-CSharp assembly, you can use the C# reflection:

~~~lua
local dic = CS.System.Activator.CreateInstance(CS.System.Type.GetType('System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[UnityEngine.Vector3, UnityEngine]],mscorlib'))
dic:Add('a', CS.UnityEngine.Vector3(1, 2, 3))
print(dic:TryGetValue('a'))
~~~

If your xLua version is larger than v2.1.12, you can

~~~lua
-- local List_String = CS.System.Collections.Generic['List<>'](CS.System.String) -- another way
local List_String = CS.System.Collections.Generic.List(CS.System.String)
local lst = List_String()

local Dictionary_String_Vector3 = CS.System.Collections.Generic.Dictionary(CS.System.String, CS.UnityEngine.Vector3)
local dic = Dictionary_String_Vector3()
dic:Add('a', CS.UnityEngine.Vector3(1, 2, 3))
print(dic:TryGetValue('a'))
~~~

## Why is the "try to dispose a LuaEnv with C# callback!" error reported when LuaEnv.Dispose is called?

This is because C# still has a delegate pointing to a function in the Lua virtual machine. Check this to prevent calling these invalid delegates after the virtual machine is released (They are invalid because the virtual machine on which the Lua function is referenced has been released.) from causing exceptions or even crashes.

How do I solve this? Release these delegates, that is the delegates nonexistent on C# that will not be referenced:

If you get and save the object member through LuaTable.Get on C#, then assign null to the member.

If you register the Lua function in Lua to some event callbacks, then deregister these callbacks.

If you perform an injection to C# via xlua.hotfix(class, method, func), then delete it via xlua.hotfix(type, method, nil).

Note that this should be done before Dispose.

## Crashes occur when calling LuaEnv.Dispose.

It is very likely that this Dispose operation is executed by Lua, which is equivalent to releasing the Lua virtual machine during the execution by Lua. Thus, this can only be executed by C#.

## When the C# parameter (or field) type is object, the integer is transferred as the long type by default. How do I specify other types like int, for example?

See [Example 11](../Examples/11_RawObject/RawObjectTest.cs)

## How do I execute the original C# logic before executing hotfix?

With util.hotfix_ex, you can call the original C# logic.

~~~lua
local util = require 'xlua.util'
util.hotfix_ex(CS.HotfixTest, 'Add', function(self, a, b)
   local org_sum = self:Add(a, b)
   print('org_sum', org_sum)
   return a + b
end)
~~~

## How do I assign a C# function to a delegate field?

In 2.1.8 or earlier versions, you can just use the C# function as a Lua function. The performance is lower, because the delegate calls Lua through the Birdage adaptation code first, and then Lua calls C#.

In 2.1.9, the createdelegate function is added in xlua.util. 

For example, see the following C# code:

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

You can specify using the Foo function to create a TestDelegate instance.

~~~lua
local util = require 'xlua.util'

local d1 = util.createdelegate(CS.TestDelegate, obj, CS.TestClass, 'Foo', {typeof(CS.System.Int32)}) --由于Foo是实例方法，所以参数2需要传TestClass实例
local d2 = util.createdelegate(CS.TestDelegate, nil, CS.TestClass, 'SFoo', {typeof(CS.System.Int32)})

obj_has_TestDelegate.field = d1 + d2 --到时调用field的时候将会触发Foo和SFoo，这不会经过Lua适配
~~~

## Why is Lua sometimes interrupted without an error message?

Generally, this happens in two situations:

1. You used a coroutine instead of the standard Lua to run the code that had an error. Coroutine errors are expressed via the returned resume value. Please refer to the relevant Lua official documents for more information. If you want to throw an exception directly for the coroutine error, you can add an assert in your resume call.

Change the following code:

~~~lua
coroutine.resume(co, ...)
~~~

to:

~~~lua
assert(coroutine.resume(co, ...))
~~~

2. Print doesn't work after upper layer catch.

For example, in some SDKs, the exception disappears after try-catch during callback.

## How do I deal with overload ambiguity?

For example, due to ignoring the out parameter, one of the overloads of Physics.Raycast cannot be called. (For example, short and int cannot be distinguished.)

First, it’s rare that the out parameter causes overload ambiguity. At present, we have only received such feedback about Physics.Raycast (as of Sept. 22, 2017). We recommended that you solve this via self-packaging (This is also applicable to short and int): Directly package static functions with other names. For a member method, package it with the Extension method.

In the hotfix scenario, we do not package it in advance. How do I call the specified overload?

Use xlua.tofunction and reflection. xlua.tofunction inputs a MethodBase object and returns a Lua function. For example, see the following C# code:

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

We can call the specified overload this way:

~~~lua
local m1 = typeof(CS.TestOverload):GetMethod('Add', {typeof(CS.System.Int16), typeof(CS.System.Int16)})
local m2 = typeof(CS.TestOverload):GetMethod('Add', {typeof(CS.System.Int32), typeof(CS.System.Int32)})
local f1 = xlua.tofunction(m1) --切记对于同一个MethodBase，只tofunction一次，然后重复使用
local f2 = xlua.tofunction(m2)

local obj = CS.TestOverload()

f1(obj, 1, 2) --调用short版本，成员方法，所以要传对象，静态方法则不需要
f2(obj, 1, 2) --调用int版本
~~~

Note: Since xlua.tofunction is not easy to use and reflection can also be used, doing this is recommended only as a temporary solution. Instead, try doing this with a package method.

## Is interface expansion method supported?

Due to the quantity of generated code, calling with obj:ExtentionMethod() is not supported. You can only call CS.ExtentionClass.ExtentionMethod(obj) through the static method. 

