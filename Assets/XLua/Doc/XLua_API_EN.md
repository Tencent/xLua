## C# APIs

### LuaEnv type

#### object[] DoString(string chunk, string chunkName = "chuck", LuaTable env = null)

Description:

    Executes a code block.

Parameter:

    chunk: Lua code string; 
    chunkName: This is used in the debug message when an error occurs, indicating a certain line in a certain code block has an error. 
    env: This is the environment variable of this code block.

Returned value:

    The returned value of the return statement in the code block. 
    For example: return 1, "hello". DoString returns an array that will contain two objects. One is 1 of the double type, and the other is the string "hello".

For example:

    LuaEnv luaenv = new LuaEnv();
    object[] ret = luaenv.DoString("print(‘hello’)\r\nreturn 1")
    UnityEngine.Debug.Log("ret="+ret[0]);
    luaenv.Dispose()

#### T LoadString<T>(string chunk, string chunkName = "chunk", LuaTable env = null)

Description:

    This loads a code block, but does not execute it. It only returns the type can be specified as a delegate or a LuaFunction.

Parameter:

    chunk: Lua code string; 
    chunkName: This is used in the debug message when an error occurs, indicating a certain line in a certain code block has an error. 
    env: This is the environment variable of this code block.

Returned Value:

    This is the delegate or LuaFunction type that represents the code block.

#### LuaTable Global;

Description:

    This is the LuaTable representing the Lua global environment.

### void Tick()

Description:

    This clears Lua's LuaBase objects that have not been manually released (for example LuaTable, LuaFunction), and other things. 
    This needs to be called periodically, for example in the Update of MonoBehaviour.

### void AddLoader(CustomLoader loader)

Description:

    Adds a custom loader

Parameter:

    loader: A delegate that includes the loaded function. The type is delegate byte[] CustomLoader(ref string filepath). When a file is required, the loader will be called back. Its parameters are the parameters used to call require. If the loader finds the file, it reads it into memory and returns a byte array. If debug support is required, the filepath should be set to one the IDE can find (relative or absolute).

#### void Dispose()

Description:

    This disposes the LuaEnv.

> LuaEnv usage suggestion: use only one instance globally. Call the GC method in Update, and call Dispose when it is not required.

### LuaTable type

#### T Get<T>(string key)

Description:

    Gets the value of type T on key. Null is returned if it does not exist or the type does not match.

#### T GetInPath<T>(string path)

Description:

    The difference from Get is that this function will identify the "." in the path. For example, var i = tbl.GetInPath<int>(“a.b.c”) is equivalent to executing i = tbl.abc in Lua. This avoids calling Get multiple times and obtaining intermediate variables. It has higher execution efficiency.

#### void SetInPath<T>(string path, T val)

Description:

    Setter corresponding to SetInPath<T>;

#### void Get<TKey, TValue>(TKey key, out TValue value)

Description:

    The key of the APIs described above can only be a string, but this API has no such restriction.

#### void Set<TKey, TValue>(TKey key, TValue value)

Description:

    This is the setter corresponding to Get<TKey, TValue>.

#### T Cast<T>()

Description:

    Converts the table to a type specified by T. It can be an interface with a CSharpCallLua declaration, a type or struct with a default constructor, a Dictionary, a List, and so on.

#### void SetMetaTable(LuaTable metaTable)

Description:

    Sets metatable to a table metatable

### LuaFunction type

> Note: Accessing Lua functions with this type will result in overhead from boxing and unboxing. For the sake of performance, do not use this type if frequent calls are required. It is recommended that you use table.Get<ABCDelegate> to get a delegate and then call it (assuming ABCDelegate is a delegate of C#). Before using table.Get<ABCDelegate>, add ABCDelegate to the list of generated code.

#### object[] Call(params object[] args)

Description:

    This calls the Lua function with the variable parameters and returns the returned value of the call.

#### object[] Call(object[] args, Type[] returnTypes)

Description:

    This calls the Lua function and specifies the type of the returned parameter. The system will automatically convert the specified type.

#### void SetEnv(LuaTable env)

Description:

    Equivalent to Lua's setfenv function.

## Lua API

### CS objects

#### CS.namespace.class(...)

Description:

    This calls a C# type constructor and returns a type instance.

For Example:

    local v1=CS.UnityEngine.Vector3(1,1,1)

#### CS.namespace.class.field

Description:

    This accesses a C# static member.

For Example:

    Print(CS.UnityEngine.Vector3.one)

#### CS.namespace.enum.field

Description:

    This accesses an enumerated value.

#### Typeof Functions

Description:

    This is similar to the typeof keyword in C#: a Type object is returned. For example, in GameObject.AddComponent, one overload requires a Type parameter.

For Example:

    newGameObj:AddComponent(typeof(CS.UnityEngine.ParticleSystem))

#### Unsigned 64-bit is supported

##### uint64.tostring

Description:

    Unsigned number to string.

##### uint64.divide

Description:

    Unsigned number division.

##### uint64.compare

Description:

    Unsigned comparison: 0 is returned for equal, positive for greater than, and negative for less than.

##### uint64.remainder

Description:

    Unsigned modulus.

##### uint64.parse

Description: 
String to unsigned number.

#### xlua.structclone

Description:

    This clones a c# structure.

#### xlua.private_accessible(class)

Description:

    This makes the private fields, properties, methods of a type available.

#### Cast Function

Description:

    This indicates that the object is accessed with a specific interface, which is useful when the implementation type is inaccessible (such as internal modification). Use it in the following way (assuming that the following calc object implements C#'s PerformentTest.ICalc interface):

For Example:

    cast(calc, typeof(CS.PerformentTest.ICalc))

Then, no other APIs are available.
Accessing a csharp object is like accessing a table. Calling a function is like calling the Lua function. Operators can also be used to access the C# operator. Here is an example:

    local v1=CS.UnityEngine.Vector3(1,1,1)
    local v2=CS.UnityEngine.Vector3(1,1,1)
    v1.x = 100
    v2.y = 100
    print(v1, v2)
    local v3 = v1 + v2
    print(v1.x, v2.x)
    print(CS.UnityEngine.Vector3.one)
    print(CS.UnityEngine.Vector3.Distance(v1, v2))

## Type mapping

### Basic data type

|C# type|Lua type|
|-|-|
|sbyte, byte, short, ushort, int ,uint ,double ,char ,float|number|
|decimal|userdata|
|long ,ulong|userdata/lua_Integer(lua53)|
|bytes[]|string|
|bool|boolean|
|string|string|

### Complex data type

|C# type|Lua type|
|-|-|
|LuaTable|table|
|LuaFunction|function|
|class or struct instance|userdata, table|
|method, delegate|function|

#### LuaTable:

If C# specifies inputting the LuaTable type (including the input parameters of the C# method or the returned value of the Lua method) from Lua, then it must be a table in Lua. Or, if C# does not specify the type, the table in Lua should be converted to LuaTable .

#### LuaFunction:

If C# specifies inputting the LuaFunction type (including the input parameters of the C# method or the returned value of the Lua method) from Lua, then it must be a function in Lua. Or, if C# does not specify the type, the function on Lua should be converted to LuaFunction.

#### LuaUserData:

This is Lua userdata corresponding to the non-C# Managered object.

#### Class or struct instance:

This transfers a class or struct instance from C#, maps it to Lua userdata, and accesses the member of the userdata via __index.
If C# specifies inputting objects of the specified type from Lua, then the userdata of the type instance is used directly in Lua. If the specified type has a default constructor, the table in Lua is automatically converted. The conversion rule is: call the constructor to construct an instance, and convert the field corresponding to table to each setter member in C#.

#### Method and delegate:

Both member methods and delegates correspond to the Lua functions. 
The ordinary parameters and reference parameters on C# correspond to the Lua function parameters. The returned value on C# corresponds to the first returned value on Lua. The reference parameters and out parameters correspond to the 2nd to Nth parameters on Lua in sequence.

## Macros

#### HOTFIX_ENABLE

This enables the hotfix function.

#### NOT_GEN_WARNING

This prints warning when there is reflection.

#### GEN_CODE_MINIMIZE

Generates code in a way that minimizes the code segments.

