## Usage

1. Add the HOTFIX_ENABLE macro to enable this feature (to File->Build Setting->Scripting Define Symbols on Unity3D). This macro should be set separately on the editor and each mobile platform! For automatic packaging, it should be noted that the macro set in the code in the API are not valid and it needs to be set in the editor.

(It is recommended that you use HOTFIX_ENABLE only for mobile phone build versions or for developing a patch in the editor, but not for usual development codes)

2. Execute the XLua/Generate Code menu.

3. This step will be automatically carried out in injection and construction of mobile phone packages. You need to manually execute the "XLua/Hotfix Inject In Editor" menu when developing a hotfix in the editor. After successful injection, "hotfix inject finish!" or "had injected!" will be printed.

## Constraints

Static constructors are not supported.

At present, only the code hotfix for Assets is supported, but not the code hotfix for engine or for the C# system library.

## API

xlua.hotfix(class, [method_name], fix)

* Description: Inject Lua patch
* class: C# class, two representation methods, CS.Namespace.Typename or string method "Namespace.Typename"; The string format is consistent with the Type.GetType of C#; If the nested type is non-Public type, you can only use the string method to represent "Namespace.TypeName+NestedTypeName".
* method_name: method name, optional;
* fix: If you transfer method_name, fix will be a function; otherwise it will provide a set of functions through the table. The table organization is based on the principle that key is method_name and value is the function.

base(csobj)

* Description: The child type override function is implemented by calling the parent type via base.
* csobj: object
* Returned value: new object, which can be used to call the method via base

Example (in HotfixTest2.cs):

```lua
xlua.hotfix(CS.BaseTest, 'Foo', function(self, p)
    print('BaseTest', p)
    base(self):Foo(p)
end)
```

util.hotfix_ex(class, method_name, fix)

* Description: This is the enhanced version of xlua.hotfix, which can perform the original function in the fix function. Its disadvantage is that the implementation of fix will be slightly slower.
* method_name: method name;
* fix: This is the Lua function used to replace the C# method.

## Tag the type of hotfix

Like other configurations, there are two methods:

Configure a list in the static field or property of a static type. Properties can be used to implement more complex configurations, such as whitelisting based on Namespace. 

~~~csharp
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
~~~

## Hotfix Flag

The Hotfix flag can set some flags to customize the generated code and instrumentation.

* Stateless and Stateful

This is is a legacy setting. The Stateful method has been removed in the new version because similar effects can be achieved with the xlua.util.state interface. For how to use this interface, see the sample code in HotfixTest2.cs.

Without Stateful, the default is Stateless, so there is no need to set this flag.

* ValueTypeBoxing

Value type adaptation. The delegate will converge to the object. Its advantage lies in having less code. Its disadvantage is that boxing and gc are generated. It is suitable for text segment sensitive services.

* IgnoreProperty

No property injection and generation of adaptation code. In general, most properties have simple implementation and lower error probability. No injection is recommended.

* IgnoreNotPublic

No injection or generation of adaptation code for non-public methods. Only private methods (such as MonoBehaviour) which are called by reflection will be injected. Other non-public methods that are only called by this type may not be injected. but the workload for repair will be slightly heavier. All public methods that reference this function must be rewritten.

* Inline

No generation of the adaptation delegate. The process code is directly injected into the function body.

* IntKey

Instead of generating static fields, all injection points are managed centrally in an array.

Advantages: Little effect on the text segment.

Disadvantages: This is not as convenient as the default method, and needs to use the id to indicate which function the hotfix is used for. This id is assigned when the code is injected into the tool. The function-id mapping will be saved in Gen/Resources/hotfix_id_map.lua.txt. Automatic timestamping is backed up to the same level directory as hotfix_id_map.lua.txt. After releasing the mobile version, properly save the file.

The format of this file is like this (Note: This file is only used in IntKey mode. If you do not specify IntKey mode injection, only an empty table is returned in the file):

~~~lua
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
~~~

To replace the Update function of HotfixTest, you have to

~~~lua
CS.XLua.HotfixDelegateBridge.Set(7, func)
~~~

If it is an overloaded function, a function name will correspond to multiple ids, such as the Add function above.

Can it be automated? Yes. xlua.util provides the auto_id_map function. When it is executed once, you can use the type and method name to indicate the repaired function.

~~~lua
(require 'xlua.util').auto_id_map()
xlua.hotfix(CS.HotfixTest, 'Update', function(self)
        self.tick = self.tick + 1
        if (self.tick % 50) == 0 then
            print('<<<<<<<<Update in lua, tick = ' .. self.tick)
        end
    end)
~~~

The premise is that hotfix_id_map.lua.txt is in a directory that can be referenced by require 'hotfix_id_map'.

## Usage suggestions

* Add the Hotfix flag to all types that are most likely to be modified.
* It is recommended that you use reflection to find all delegate types involved in the function parameters, fields, properties and events, and then add CSharpCallLua.
* Add LuaCallCSharp to business code, engine APIs, system APIs, and the types to which the Lua hotfix requires high-performance access.
* Engine APIs and system APIs may be stripped by code (those not referenced by C# will be stripped). If you think you may add API calls other than C# codes, these APIs are added to either LuaCallCSharp or ReflectionUse.

## Patching

Xlua uses Lua functions to replace C#'s constructors, functions, properties and events. Lua implementations are based on functions. For example, the property corresponds to a getter function and a setter function, and the event corresponds to an add function and a remove function.

* Functions

method_name transfers the function name and supports overload. Different overloads are forwarded to the same Lua function.

For example:

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

The difference between a static function and a member function is that a member function will have a self parameter added. This self is the C# object itself in Stateless mode (corresponding to C#'s this).

Ordinary parameters correspond to Lua parameters. The ref parameter corresponds to a Lua parameter and a returned value, and the out parameter corresponds to a returned value on Lua.

Generic functions have the same hotfix rules as ordinary functions.

* Constructors

The method_name corresponding to the constructor is ".ctor".

Unlike the ordinary function, the hotfix of the constructor is not a replacement, but calls Lua after executing the original logic.

* Properties

The property named "AProp" will correspond to a getter, its method_name equals to get_AProp, and the setter's method_name is equal to set_AProp.

* []Operators

The setter corresponds to set_Item, and the getter corresponds to get_Item. The first parameter is self, which is followed by key and value for the setter. The getter has only the key parameter, and the returned value is the value that it has gotten. 

* Other operators

The operators of C# have a set of internal representations. For example, the + operator function name is op_Addition (for the internal representation of other operators, see the relevant document). Overriding this function will override the + operator of C#.

* Events

For example, for the event "AEvent", the += operator is add_AEvent, and the -= operator is remove_AEvent. The first parameter of the two functions is self, and the second parameter is the delegate immediately after the operator.

Then, directly access the private delegate corresponding to the event via xlua.private_accessible. (For versions newer than 2.1.11, calling xlua.private_accessible is not required.) The event can be triggered directly through the object's "&event name" field, such as self\['&MyEvent'\](), where MyEvent is the event name.

* Destructors

The method_name is "Finalize” and transfers a self parameter.

Unlike the ordinary function, the hotfix of the destructor is not a replacement, but continues the original logic after calling the Lua function.

* Generic types

Other rules are the same. It should be noted that each instantiated generic type is an independent type. You can only patch each instantiated type. For example:

```csharp
public class GenericClass<T>
{
｝
```

You can only patch GenericClass\<double\> and GenericClass\<int\>, instead of GenericClass.

The following is an example of patching GenericClass<double>:

```csharp
luaenv.DoString(@"
    xlua.hotfix(CS.GenericClass(CS.System.Double), {
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

* Unity coroutine

Using util.cs_generator, you can simulate an IEnumerator with a function. The coroutine.yield here is similar to the yield return in C#. For example, the following C# code is equivalent to the corresponding hotfix code.

~~~csharp
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
~~~

~~~csharp
luaenv.DoString(@"
    local util = require 'xlua.util'
    xlua.hotfix(CS.HotFixSubClass,{
        Start = function(self)
            return util.cs_generator(function()
                while true do
                    coroutine.yield(CS.UnityEngine.WaitForSeconds(3))
                    print('Wait for 3 seconds')
                end
            end)
        end;
    })
");
~~~

* Entire type

If you want to replace the entire type, you don't need to call xlua.hotfix again and again, you can do it all at once. Just give a table and press method_name = function.

```lua
xlua.hotfix(CS.StatefullTest, {
    ['.ctor'] = function(csobj)
        return util.state(csobj, {evt = {}, start = 0, prop = 0})
    end;
    set_AProp = function(self, v)
        print('set_AProp', v)
        self.prop = v
    end;
    get_AProp = function(self)
        return self.prop
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
    GenericTest = function(self, a)
       print(self, a)
    end;
    Finalize = function(self)
       print('Finalize', self)
    end
})
```

