## xLua Tutorial

### Load Lua files

1. Execute strings

   The most basic way to execute a string is to use LuaEnv.DoString. The string must be compliant with Lua syntax.
For example:

       luaenv.DoString("print('hello world')")

   See the full code in the XLua\Tutorial\LoadLuaScript\ByString directory.

   > However, this mode is not recommended. We recommend the following mode:

2. Load Lua files.

   Use Lua's require function.

       DoString("require 'byfile'")

   See the full code in the XLua\Tutorial\LoadLuaScript\ByFile directory.

   The require actually calls the loaders one by one to load the file. The loading process stops if one loader succeeds. If all loaders fail, no file found will be reported. 
In addition to native loaders, xLua also adds loaders loaded from the Resource. Note that because the Resource supports only  limited numbers of extensions, the Lua file under Resources must be added the txt extension (see the attached example).

   The recommended way to load Lua scripts is as follows: Ensure the entire program is a DoString ("require 'main'"). Then, load other scripts in main.lua (like executing the Lua script's command line: lua main.lua).

   Someone may ask: What should I do if my Lua file is downloaded, is extracted from a file in a custom format, or needs to be decrypted? Good question. XLua's custom loader can meet your needs.

3. Customized loaders

   It’s simple to customize loaders on xLua. Only one interface is involved:

       public delegate byte[] CustomLoader(ref string filepath);
       public void LuaEnv.AddLoader(CustomLoader loader)

   With the AddLoader, you can register a callback. Its parameter is a string. When calling require in the Lua code, the parameter will be transparently transferred to the callback, which can load the specified files based on this parameter. If debugging support is required, you need to modify the filepath to a real path and transfer it. The returned value of this callback is a byte array. Null means the loader is not found. Otherwise, the content of the Lua file is returned. 
Can IIPS IFS be loaded? Yes. Write a loader to call IIPS interface to read the content of the file. Can encrypted files be loaded? Yes, write the loader to read the file and return it after decrypting it. 
See the complete example in the XLua\Tutorial\LoadLuaScript\Loader directory.

### C# accesses Lua.

This means that C# initiates access to the Lua data structure. 
The examples mentioned in this chapter can be found in the XLua\Tutorial\CSharpCallLua directory.

1. Get a global basic data type
Access LuaEnv.Global, which provides a template Get method. You can specify the type returned.

       luaenv.Global.Get<int>("a")
       luaenv.Global.Get<string>("b")
       luaenv.Global.Get<bool>("c")

2. Access a global table

   What class should I specify if the above Get method also been used?

   1. Map it to an ordinary class or struct.

      Define a class, which has a public property of the field corresponding to table. Doing this either with or without a parameter constructor is OK. For example, for {f1 = 100, f2 = 100}, you can define a class that contains public int f1;public int f2;. 
In this way, xLua will help you to create a new instance and set the corresponding fields to it.

      Table properties can be more or less than class properties. You can nest other complex classes. 
It should be noted that this process is to copy values. Complex classes will have higher overhead. And, modified field values of the class will not be synchronized to table, and vice versa.

      This feature can reduce the generation overhead by adding the class to GCOptimize. For details, please see the configuration documentation. 
Is mapping in the reference mode available? Yes, this is one method:

   2. Map to an interface

      This mode relies on the generated code. (If no code is generated, the InvalidCastException error will be reported.) The code generator will generate an instance of this interface. When getting a property, the generated code will get the corresponding table field; when setting a property, the generated code will also set the corresponding table field. You can even access Lua functions via the interface method.

   3. Lightweight by value mode: map to Dictionary<>, List<>

      If you do not want to define the type or interface, you can using this. The premise is that table key and value are of the same type.

   4. Another methods are woking on ref mode: map to LuaTable class.

      The advantage of this mode lies in that there is no need to generate code, but there are also some problems with this method. For example, it is an order of magnitude slower than mode 2, and there is no class checking.

3. Access a global function.

   This still uses the Get method, but maps to a different class.

   1. Map to a delegate

      This is the recommended approach, with much better performance and higher class safety. The disadvantage of this method is the generated code. (If no code is generated, the InvalidCastException error will be reported.)

      How do I declare a delegate? 
For each parameter of the function, declare an input type parameter. 
How do I deal with multiple returned values? Map to the C# output parameters from left to right. The output parameters include returned value, out parameter, and ref parameter.

      What parameters and returned value types are supported? All are supported. A variety of complex types can be returned, including out, ref modified, and even another delegate.

      Using a delegate is even simpler. It can be used just like a function.

   2. Map to LuaFunction

      The advantages and disadvantages of this approach are exactly the opposite of the first method. 
Using this is also simple. LuaFunction has a Call function with variable parameters, and you can transfer any type and any number of parameters. The returned values are an array of objects, corresponding to Lua multiple returned values.

4. Usage suggestions

   1. The overhead is high to access Lua global data, especially tables and functions. We recommended doing this as little as possible. For example, during initialization, get the Lua function to be called later (map to the delegate) and save it. Then, you can directly call the delegate. (Tables are similar)

   2. If all implementations of Lua are in the delegate and an interface mode, their use can be completely decoupled from xLua. A dedicated module can be responsible for the initialization of xLua and the mapping of delegates and interfaces. Then, you can set these delegates and interfaces to where they will be used.

### Lua calls C#

> The examples covered in this section are all under XLua\Tutorial\LuaCallCSharp

#### Create a new C# object

You can create a new C# object this way:

    var newGameObj = new UnityEngine.GameObject();

Make it correspond to Lua this way:

    local newGameObj = CS.UnityEngine.GameObject()

These are basically the same, except in the following ways:

    1. No new keyword is provided in Lua; 
    2. All C# related content is placed in the CS, including constructors, static member properties, and methods.

How do I deal with multiple constructors? No problem, xLua supports overloads. For example, if you want to call the constructor for GameObject with a string parameter, you can write it this way:

    local newGameObj2 = CS.UnityEngine.GameObject('helloworld')

#### Access C# static properties and methods.

##### Read static properties

    CS.UnityEngine.Time.deltaTime

##### Write static properties

    CS.UnityEngine.Time.timeScale = 0.5

##### Call static methods

    CS.UnityEngine.GameObject.Find('helloworld')

Tip: For the types you need to frequently access, you can reference them with local variables before calling them. This can reduce programing time and improve performance.

    local GameObject = CS.UnityEngine.GameObject
    GameObject.Find('helloworld')

#### Access C# member properties, methods

##### Read member properties

    testobj.DMF

##### Write member properties

    testobj.DMF = 1024

##### Call member methods

Note: When calling the member method, the first parameter needs to transfer this object. We recommended using the colon syntactic sugar as shown below:

    testobj:DMFunc()

##### Parent properties and methods

XLua supports (via derived types) access to static properties and static methods of a base type, (via derived type instances) access to member properties, and member methods of base type.

##### Parameter input and output properties (out and ref)

Processing rules for parameters called by Lua: The ordinary parameter of C# is an input formal parameter. Ref modified is an input formal parameter, but out is not. The rest correspond from left to right to the actual parameter list called by Lua.

The processing rule for returned values called by Lua: The returned value of a C# function (if any) is a returned value, out is a returned value, and ref is a returned value. The rest correspond to multiple returned values of Lua from left to right.

##### Overload method:

This method allows you to access overloaded functions directly through different parameter types, for example:

    testobj:TestFunc(100)
    testobj:TestFunc('hello')

The integer parameter TestFunc and the string parameter TestFunc will be accessed separately.

Note: xLua only supports overloaded function calls to a certain extent. Because Lua supports far fewer types than C# does, there will be one-to-many situations. For example, C#'s int, float, and double types all correspond to Lua's number type. In the above example, if TestFunc has these overload parameters, the first line will not be able to distinguish between them and only one of them can be called (the first in the generated code).

##### Operators

These operators are supported: +, -, *, /, ==, unary-, <, <=, %[]

##### Methods whose parameters have default values:

This is the same as when C# calls a function with a default value. If the given actual parameters are less than the formal parameters, the default values will be added.

##### Variable parameter methods

For the following C# parameter:

    void VariableParamsFunc(int a, params string[] strs)

You can call it in Lua this way:

    testobj:VariableParamsFunc(5, 'hello', 'john')

##### Use extension methods

Lua can use these directly after you define them in C#.

##### Generic (template) methods

These are not directly supported, but you can call them after packaging them through the Extension methods feature.

##### The Enumerated Type

Enumerated values are just like static properties of the enumerated type.

    testobj:EnumTestFunc(CS.Tutorial.TestEnum.E1)

The EnumTestFunc function parameter shown above is the Tutorial.TestEnum type.

Enum has a __CastFrom method. This implements conversion from an integer or string to an enumerated value. For example:

    CS.Tutorial.TestEnum.__CastFrom(1)
    CS.Tutorial.TestEnum.__CastFrom('E1')

##### Delegate use (call, +, -)

Call C# delegate: This is the same as calling the ordinary Lua function.

+ operator: This corresponds to the C# + operator. It combines two calls into a call chain, and the right operand can be of the same type as the C# delegate or Lua function.

- operator: In contrast to +, this removes a delegate from the call chain.

> PS: The delegate property can be set with a Lua function.

##### Events:

For example, testobj has the following event definition: public event Action TestEvent;

Add event callbacks

    testobj:TestEvent('+', lua_event_callback)

Remove event callbacks

    testobj:TestEvent('-', lua_event_callback)

##### Support for 64-bit integers

    In Lua version 5.3, 64-bit integers (long, ulong) are mapped to native 64-bit integers. Since the LuaJIT version (equivalent to the standard Lua 5.1 version ) does not support 64-bit integers, xLua provides a 64-bit support extension library. Both C# long and ulong integers will be mapped to userdata. 
    
     64-bit operations, comparisons, print
    
     support, and Lua number operations are supported in Lua. It should be noted for comparison
    
     that in the 64 extension library, only int64 and ulong integers will be strongly converted to long integers first and then transferred to Lua. In some of Ulong's operations, for the sake of comparison, we use the same support mode as Java: providing a set of APIs. For details, see the API documentation.

##### Automatic conversion between C# complex types and tables

For a C# complex type with no parameter constructor, a table can be used directly as a substitute on Lua. The table corresponds to a public field of a complex type. It supports function parameter transfer, property assignment, etc. For example: 
The definition of B structure (type also supported) on C# is as follows:

    public struct A
    {
    public int a;
    }
    
    public struct B
    {
    public A b;
    public double c;
    }

A type has this member function:

    void Foo(B b)

Lua can call it in this way:

    obj:Foo({b = {a = 100}, c = 200})

##### Get type (equivalent to C#'s typeof)

For example, you can get the type information of the UnityEngine.ParticleSystem type this way:

    typeof(CS.UnityEngine.ParticleSystem)

##### "Strong" conversion

Lua is not a typed language, so it has no "strong" conversion with strongly typed languages. However, there's something similar: tell xLua to call an object with the specified generated code. Under what circumstances will it be used? The answer is that, sometimes third-party libraries expose an interface or an abstract type. The implementation type is hidden, so we cannot generate code for the implementation type. This implementation type will be identified by xLua as the ungenerated code and accessed via reflection. Frequently calling it will affect performance significantly. We can add this interface or abstract type to the generated code, and then specify using the generated code to access objects:

    cast(calc, typeof(CS.Tutorial.Calc))

The above example specifies using the generated code of CS.Tutorial.Calc to access the calc object.

