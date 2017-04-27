/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#if HOTFIX_ENABLE
#if XLUA_GENERAL || INJECT_WITHOUT_TOOL
#if !XLUA_GENERAL
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace XLua
{
    public static class Hotfix
    {
        static TypeReference objType = null;
        static TypeReference luaTableType = null;

        static TypeDefinition luaFunctionType = null;
        static MethodDefinition invokeSessionStart = null;
        static MethodDefinition functionInvoke = null;
        static MethodDefinition invokeSessionEnd = null;
        static MethodDefinition invokeSessionEndWithResult = null;
        static MethodDefinition inParam = null;
        static MethodDefinition inParams = null;
        static MethodDefinition outParam = null;

        static Dictionary<string, int> hotfixCfg;

        static void init(AssemblyDefinition assembly, IEnumerable<string> search_directorys)
        {
#if XLUA_GENERAL
            objType = assembly.MainModule.ImportReference(typeof(object));
#else
            objType = assembly.MainModule.Import(typeof(object));
#endif

            luaTableType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.LuaTable");

            luaFunctionType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.LuaFunction");
            invokeSessionStart = luaFunctionType.Methods.Single(m => m.Name == "InvokeSessionStart");
            functionInvoke = luaFunctionType.Methods.Single(m => m.Name == "Invoke");
            invokeSessionEnd = luaFunctionType.Methods.Single(m => m.Name == "InvokeSessionEnd");
            invokeSessionEndWithResult = luaFunctionType.Methods.Single(m => m.Name == "InvokeSessionEndWithResult");
            inParam = luaFunctionType.Methods.Single(m => m.Name == "InParam");
            inParams = luaFunctionType.Methods.Single(m => m.Name == "InParams");
            outParam = luaFunctionType.Methods.Single(m => m.Name == "OutParam");

            var resolver = assembly.MainModule.AssemblyResolver as BaseAssemblyResolver;
            foreach (var path in
                (from asm in AppDomain.CurrentDomain.GetAssemblies() select asm.ManifestModule.FullyQualifiedName)
                 .Distinct())
            {
                try
                {
                    resolver.AddSearchDirectory(System.IO.Path.GetDirectoryName(path));
                }
                catch(Exception)
                {

                }
            }

            if (search_directorys != null)
            {
                foreach(var directory in search_directorys)
                {
                    resolver.AddSearchDirectory(directory);
                }
            }
        }

        static List<TypeDefinition> hotfix_delegates = null;

        static bool isSameType(TypeReference left, TypeReference right)
        {
            return left.FullName == right.FullName
                && left.Module.Assembly.FullName == right.Module.Assembly.FullName
                && left.Module.FullyQualifiedName == right.Module.FullyQualifiedName;
        }

        static bool findHotfixDelegate(AssemblyDefinition assembly, MethodDefinition method, out TypeReference delegateType, out MethodReference invoke, int hotfixType)
        {
            for(int i = 0; i < hotfix_delegates.Count; i++)
            {
                MethodDefinition delegate_invoke = hotfix_delegates[i].Methods.Single(m => m.Name == "Invoke");
                var returnType = (hotfixType == 1 && method.IsConstructor && !method.IsStatic) ? luaTableType : method.ReturnType;
                if (isSameType(returnType, delegate_invoke.ReturnType))
                {
                    var parametersOfDelegate = delegate_invoke.Parameters;
                    int compareOffset = 0;
                    if (!method.IsStatic)
                    {
                        var typeOfSelf = (hotfixType == 1 && !method.IsConstructor) ? luaTableType :
                            (method.DeclaringType.IsValueType ? method.DeclaringType : objType);
                        if ((parametersOfDelegate.Count == 0) || parametersOfDelegate[0].ParameterType.IsByReference || !isSameType(typeOfSelf, parametersOfDelegate[0].ParameterType))
                        {
                            continue;
                        }
                        compareOffset++;
                    }
                    if (method.Parameters.Count != (parametersOfDelegate.Count - compareOffset))
                    {
                        continue;
                    }
                    bool paramMatch = true;
                    for (int j = 0; j < method.Parameters.Count; j++)
                    {
                        var param_left = method.Parameters[j];
                        var param_right = parametersOfDelegate[compareOffset++];
                        if (param_left.IsOut != param_right.IsOut
                            || param_left.ParameterType.IsByReference != param_right.ParameterType.IsByReference)
                        {
                            paramMatch = false;
                            break;
                        }
                        if (param_left.ParameterType.IsValueType != param_right.ParameterType.IsValueType)
                        {
                            paramMatch = false;
                            break;
                        }
						bool isparam = param_left.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.Name == "ParamArrayAttribute") != null;
						var type_left = (isparam || param_left.ParameterType.IsByReference || param_left.ParameterType.IsValueType) ? param_left.ParameterType : objType;
                        if (!isSameType(type_left, param_right.ParameterType))
                        {
                            paramMatch = false;
                            break;
                        }
                    }

                    if (!paramMatch)
                    {
                        continue;
                    }
                    delegateType = hotfix_delegates[i];
                    invoke = delegate_invoke;
                    return true;
                }
            }
            delegateType = null;
            invoke = null;
            return false;
        }

        static bool hasGenericParameter(TypeReference type)
        {
            if (type.HasGenericParameters)
            {
                return true;
            }
            if (type.IsByReference)
            {
                return hasGenericParameter(((ByReferenceType)type).ElementType);
            }
            if (type.IsArray)
            {
                return hasGenericParameter(((ArrayType)type).ElementType);
            }
            if (type.IsGenericInstance)
            {
                foreach (var typeArg in ((GenericInstanceType)type).GenericArguments)
                {
                    if (hasGenericParameter(typeArg))
                    {
                        return true;
                    }
                }
                return false;
            }
            return type.IsGenericParameter;
        }

        static bool isNoPublic(AssemblyDefinition assembly, TypeReference type)
        {
            if (type.IsByReference)
            {
                return isNoPublic(assembly, ((ByReferenceType)type).ElementType);
            }
            if (type.IsArray)
            {
                return isNoPublic(assembly, ((ArrayType)type).ElementType);
            }
            else
            {
                if (type.IsGenericInstance)
                {
                    foreach (var typeArg in ((GenericInstanceType)type).GenericArguments)
                    {
                        if (isNoPublic(assembly, typeArg))
                        {
                            return true;
                        }
                    }
                }

                var resolveType = type.Resolve();
                if ((!type.IsNested && !resolveType.IsPublic) || (type.IsNested && !resolveType.IsNestedPublic))
                {
                    return true;
                }
                if (type.IsNested)
                {
                    var parent = type.DeclaringType;
                    while (parent != null)
                    {
                        var resolveParent = parent.Resolve();
                        if ((!parent.IsNested && !resolveParent.IsPublic) || (parent.IsNested && !resolveParent.IsNestedPublic))
                        {
                            return true;
                        }
                        if (parent.IsNested)
                        {
                            parent = parent.DeclaringType;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                return false;
            }

        }

        static bool genericInOut(AssemblyDefinition assembly, MethodDefinition method, int hotfixType)
        {
            if (hasGenericParameter(method.ReturnType) || isNoPublic(assembly, method.ReturnType))
            {
                return true;
            }
            var parameters = method.Parameters;

            if (!method.IsStatic && (hotfixType == 0 || method.IsConstructor)
                && (hasGenericParameter(method.DeclaringType) || (method.DeclaringType.IsValueType && isNoPublic(assembly, method.DeclaringType))))
            {
                    return true;
            }
            for (int i = 0; i < parameters.Count; i++)
            {
                if ( hasGenericParameter(parameters[i].ParameterType) || ((parameters[i].ParameterType.IsValueType || parameters[i].ParameterType.IsByReference) && isNoPublic(assembly, parameters[i].ParameterType)))
                {
                    return true;
                }
            }
            return false;
        }

        static bool injectType(AssemblyDefinition assembly, TypeReference hotfixAttributeType, TypeDefinition type, bool noGen)
        {
            foreach(var nestedTypes in type.NestedTypes)
            {
                if (!injectType(assembly, hotfixAttributeType, nestedTypes, noGen))
                {
                    return false;
                }
            }
            if (type.Name.Contains("<")) // skip anonymous type
            {
                return true;
            }
            CustomAttribute hotfixAttr = type.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == hotfixAttributeType);
            int hotfixType;
            if (hotfixAttr != null)
            {
                hotfixType = (int)hotfixAttr.ConstructorArguments[0].Value;
            }
            else
            {
                if (!hotfixCfg.ContainsKey(type.FullName))
                {
                    return true;
                }
                hotfixType = hotfixCfg[type.FullName];
            }

            FieldReference stateTable = null;
            if (hotfixType == 1)
            {
                if (type.IsAbstract && type.IsSealed)
                {
                    throw new InvalidOperationException(type.FullName + " is static, can not be mark as Stateful!");
                }
                var stateTableDefinition = new FieldDefinition("__Hitfix_xluaStateTable", Mono.Cecil.FieldAttributes.Private, luaTableType);
                type.Fields.Add(stateTableDefinition);
                stateTable = stateTableDefinition.GetGeneric();
            }
            foreach (var method in type.Methods)
            {
                if (method.Name != ".cctor" && !method.IsAbstract && !method.IsPInvokeImpl && method.Body != null && !method.Name.Contains("<"))
                {
                    if ((noGen || method.HasGenericParameters || genericInOut(assembly, method, hotfixType)) ? !injectGenericMethod(assembly, method, hotfixType, stateTable) :
                        !injectMethod(assembly, method, hotfixType, stateTable))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

#if !XLUA_GENERAL
        [PostProcessScene]
        [UnityEditor.MenuItem("XLua/Hotfix Inject In Editor", false, 3)]
        public static void HotfixInject()
        {
            if (EditorApplication.isCompiling || Application.isPlaying)
            {
                return;
            }
#if NO_HOTFIX_GEN
            HotfixInject("./Library/ScriptAssemblies/Assembly-CSharp.dll", null, true, Utils.GetAllTypes());
#else
            HotfixInject("./Library/ScriptAssemblies/Assembly-CSharp.dll", null, false, Utils.GetAllTypes());
#endif
        }
#endif

            //返回-1表示没有标签
            static int getHotfixType(MemberInfo memberInfo)
        {
            foreach(var ca in memberInfo.GetCustomAttributes(false))
            {
                var ca_type = ca.GetType();
                if (ca_type.ToString() == "XLua.HotfixAttribute")
                {
                    return (int)(ca_type.GetProperty("Flag").GetValue(ca, null));
                }
            }
            return -1;
        }

        static void mergeConfig(Dictionary<string, int> hotfixCfg, MemberInfo test, Type cfg_type, Func<IEnumerable<Type>> get_cfg)
        {
            int hotfixType = getHotfixType(test);
            if (-1 == hotfixType || !typeof(IEnumerable<Type>).IsAssignableFrom(cfg_type))
            {
                return;
            }

            foreach (var type in get_cfg())
            {
                if (!type.IsDefined(typeof(ObsoleteAttribute), false)
                    && !type.IsEnum && !typeof(Delegate).IsAssignableFrom(type)
                    && (!type.IsGenericType || type.IsGenericTypeDefinition))
                {
                    string key = type.FullName.Replace('+', '/');
                    if (!hotfixCfg.ContainsKey(key) && (type.Namespace == null || (type.Namespace != "XLua" && !type.Namespace.StartsWith("XLua."))))
                    {
                        hotfixCfg.Add(key, hotfixType);
                    }
                }
            }
        }

        public static void Config(IEnumerable<Type> cfg_check_types)
        {
            if (cfg_check_types != null)
            {
                hotfixCfg = new Dictionary<string, int>();
                foreach (var type in cfg_check_types.Where(t => !t.IsGenericTypeDefinition && t.IsAbstract && t.IsSealed))
                {
                    var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                    foreach (var field in type.GetFields(flags))
                    {
                        mergeConfig(hotfixCfg, field, field.FieldType, () => field.GetValue(null) as IEnumerable<Type>);
                    }
                    foreach (var prop in type.GetProperties(flags))
                    {
                        mergeConfig(hotfixCfg, prop, prop.PropertyType, () => prop.GetValue(null, null) as IEnumerable<Type>);
                    }
                }
            }
        }

        public static void HotfixInject(string inject_assembly_path, IEnumerable<string> search_directorys, bool noGen, IEnumerable<Type> cfg_check_types = null)
        {
            AssemblyDefinition assembly = null;
            try
            {
#if HOTFIX_SYMBOLS_DISABLE
                assembly = AssemblyDefinition.ReadAssembly(inject_assembly_path);
#else
                var readerParameters = new ReaderParameters { ReadSymbols = true };
                assembly = AssemblyDefinition.ReadAssembly(inject_assembly_path, readerParameters);
#endif
                init(assembly, search_directorys);

                if (assembly.MainModule.Types.Any(t => t.Name == "__XLUA_GEN_FLAG"))
                {
                    Info("had injected!");
                    return;
                }

                assembly.MainModule.Types.Add(new TypeDefinition("__XLUA_GEN", "__XLUA_GEN_FLAG", Mono.Cecil.TypeAttributes.Class,
                    objType));

                Config(cfg_check_types);

                var hotfixDelegateAttributeType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.HotfixDelegateAttribute");
                hotfix_delegates = (from module in assembly.Modules
                                    from type in module.Types
                                    where type.CustomAttributes.Any(ca => ca.AttributeType == hotfixDelegateAttributeType)
                                    select type).ToList();

                var hotfixAttributeType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.HotfixAttribute");
                foreach (var type in (from module in assembly.Modules from type in module.Types select type))
                {
                    if (!injectType(assembly, hotfixAttributeType, type, noGen))
                    {
                        return;
                    }
                }
#if HOTFIX_SYMBOLS_DISABLE
                assembly.Write(inject_assembly_path);
                Info("hotfix inject finish!(no symbols)");
#else
                var writerParameters = new WriterParameters { WriteSymbols = true };
                assembly.Write(inject_assembly_path, writerParameters);
                Info("hotfix inject finish!");
#endif
            }
            catch(Exception e)
            {
                Error("Exception! " + e);
            }
            finally
            {
                if (assembly != null)
                {
                    Clean(assembly);
                }
            }
        }

        static void Info(string info)
        {
#if XLUA_GENERAL
            System.Console.WriteLine(info);
#else
            Debug.Log(info);
#endif
        }

        static void Error(string info)
        {
#if XLUA_GENERAL
            System.Console.WriteLine("Error:" + info);
#else
            Debug.LogError(info);
#endif
        }

        static void Clean(AssemblyDefinition assembly)
		{
			if (assembly.MainModule.SymbolReader != null)
			{
				assembly.MainModule.SymbolReader.Dispose();
			}
		}

        static OpCode[] ldargs = new OpCode[] { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };

        static readonly int MAX_OVERLOAD = 100;

        static string getDelegateName(MethodDefinition method)
        {
            string fieldName = method.Name;
            if (fieldName.StartsWith("."))
            {
                fieldName = fieldName.Substring(1);
            }
            string ccFlag = method.IsConstructor ? "_c" : "";
            string luaDelegateName = null;
            var type = method.DeclaringType;
            for (int i = 0; i < MAX_OVERLOAD; i++)
            {
                string tmp = ccFlag + "__Hitfix" + i + "_" + fieldName;
                if (!type.Fields.Any(f => f.Name == tmp)) // injected
                {
                    luaDelegateName = tmp;
                    break;
                }
            }
            return luaDelegateName;
        }

        static Instruction findNextRet(Mono.Collections.Generic.Collection<Instruction> instructions, Instruction pos)
        {
            bool posFound = false;
            for(int i = 0; i < instructions.Count; i++)
            {
                if (posFound && instructions[i].OpCode == OpCodes.Ret)
                {
                    return instructions[i];
                }
                else if (instructions[i] == pos)
                {
                    posFound = true;
                }
            }
            return null;
        }

        static bool injectMethod(AssemblyDefinition assembly, MethodDefinition method, int hotfixType, FieldReference stateTable)
        {
            var type = method.DeclaringType;
            var luaDelegateName = getDelegateName(method);
            if (luaDelegateName == null)
            {
                Error("too many overload!");
                return false;
            }

            bool isFinalize = (method.Name == "Finalize" && method.IsSpecialName);

            TypeReference delegateType = null;
            MethodReference invoke = null;

            int param_count = method.Parameters.Count + (method.IsStatic ? 0 : 1);

            if (!findHotfixDelegate(assembly, method, out delegateType, out invoke, hotfixType))
            {
                Error("can not find delegate for " + method.DeclaringType + "." + method.Name + "! try re-genertate code.");
                return false;
            }

            if (delegateType == null || invoke == null)
            {
                throw new Exception("unknow exception!");
            }
            FieldDefinition fieldDefinition = new FieldDefinition(luaDelegateName, Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.Private,
                delegateType);
            type.Fields.Add(fieldDefinition);
            FieldReference fieldReference = fieldDefinition.GetGeneric();

            bool statefulConstructor = (hotfixType == 1) && method.IsConstructor && !method.IsStatic;

            var insertPoint = method.Body.Instructions[0];
            var processor = method.Body.GetILProcessor();

            if (method.IsConstructor)
            {
                insertPoint = findNextRet(method.Body.Instructions, insertPoint);
            }

            while (insertPoint != null)
            {
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldsfld, fieldReference));
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Brfalse, insertPoint));

                if (statefulConstructor)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_0));
                }
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldsfld, fieldReference));
                for (int i = 0; i < param_count; i++)
                {
                    if (i < ldargs.Length)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(ldargs[i]));
                    }
                    else if (i < 256)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_S, (byte)i));
                    }
                    else
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg, (short)i));
                    }
                    if (i == 0 && hotfixType == 1 && !method.IsStatic && !method.IsConstructor)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldfld, stateTable));
                    }
                    else if (i == 0 && !method.IsStatic && type.IsValueType)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldobj, type));
                    }
                }

                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Call, invoke));
                if (statefulConstructor)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stfld, stateTable));
                }
                if (isFinalize && hotfixType == 1)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_0));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldnull));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stfld, stateTable));
                }
                if (!method.IsConstructor && !isFinalize)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ret));
                }

                if (!method.IsConstructor)
                {
                    break;
                }
                insertPoint = findNextRet(method.Body.Instructions, insertPoint);
            }

            if (isFinalize)
            {
                if (method.Body.ExceptionHandlers.Count == 0)
                {
                    throw new InvalidProgramException("Finalize has not try-catch? Type :" + method.DeclaringType);
                }
                method.Body.ExceptionHandlers[0].TryStart = method.Body.Instructions[0];
            }

            return true;
        }

        static MethodReference MakeGenericMethod(this MethodReference self, params TypeReference[] arguments)
        {
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceMethod(self);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        static FieldReference GetGeneric(this FieldDefinition definition)
        {
            if (definition.DeclaringType.HasGenericParameters)
            {
                var declaringType = new GenericInstanceType(definition.DeclaringType);
                foreach (var parameter in definition.DeclaringType.GenericParameters)
                {
                    declaringType.GenericArguments.Add(parameter);
                }
                return new FieldReference(definition.Name, definition.FieldType, declaringType);
            }

            return definition;
        }

        public static TypeReference GetGeneric(this TypeDefinition definition)
        {
            if (definition.HasGenericParameters)
            {
                var genericInstanceType = new GenericInstanceType(definition);
                foreach (var parameter in definition.GenericParameters)
                {
                    genericInstanceType.GenericArguments.Add(parameter);
                }
                return genericInstanceType;
            }

            return definition;
        }

        static bool injectGenericMethod(AssemblyDefinition assembly, MethodDefinition method, int hotfixType, FieldReference stateTable)
        {
            var type = method.DeclaringType;
            var luaDelegateName = getDelegateName(method);
            if (luaDelegateName == null)
            {
                Error("too many overload!");
                return false;
            }

            bool isFinalize = (method.Name == "Finalize" && method.IsSpecialName);

            FieldDefinition fieldDefinition = new FieldDefinition(luaDelegateName, Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.Private,
                luaFunctionType);
            type.Fields.Add(fieldDefinition);

            FieldReference fieldReference = fieldDefinition.GetGeneric();

            int param_start = method.IsStatic ? 0 : 1;
            int param_count = method.Parameters.Count + param_start;
            var insertPoint = method.Body.Instructions[0];
            var processor = method.Body.GetILProcessor();

            if (method.IsConstructor)
            {
                insertPoint = findNextRet(method.Body.Instructions, insertPoint);
            }

            while (insertPoint != null)
            {
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldsfld, fieldReference));
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Brfalse, insertPoint));

                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldsfld, fieldReference));
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, invokeSessionStart));

                bool statefulConstructor = (hotfixType == 1) && method.IsConstructor && !method.IsStatic;

                TypeReference returnType = statefulConstructor ? luaTableType : method.ReturnType;

                bool isVoid = returnType.FullName == "System.Void";

                int outCout = 0;

                for (int i = 0; i < param_count; i++)
                {
                    if (i == 0 && !method.IsStatic)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldsfld, fieldReference));
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_0));
                        if (hotfixType == 1 && !method.IsConstructor)
                        {
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldfld, stateTable));
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, MakeGenericMethod(inParam, luaTableType)));
                        }
                        else
                        {
                            if (type.IsValueType)
                            {
                                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldobj, method.DeclaringType.GetGeneric()));
                            }
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, MakeGenericMethod(inParam, method.DeclaringType.GetGeneric())));
                        }
                    }
                    else
                    {
                        var param = method.Parameters[i - param_start];
                        if (param.ParameterType.IsByReference)
                        {
                            outCout++;
                        }
                        if (!param.IsOut)
                        {
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldsfld, fieldReference));

                            if (i < ldargs.Length)
                            {
                                processor.InsertBefore(insertPoint, processor.Create(ldargs[i]));
                            }
                            else if (i < 256)
                            {
                                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_S, (byte)i));
                            }
                            else
                            {
                                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg, (short)i));
                            }

                            var paramType = param.ParameterType;

                            if (param.ParameterType.IsByReference)
                            {
                                paramType = ((ByReferenceType)paramType).ElementType;
                                if (paramType.IsValueType || paramType.IsGenericParameter)
                                {
                                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldobj, paramType));
                                }
                                else
                                {
                                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldind_Ref));
                                }
                            }
                            if (i == param_count - 1 && param.CustomAttributes.Any(ca => ca.AttributeType.FullName == "System.ParamArrayAttribute"))
                            {
                                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, MakeGenericMethod(inParams, ((ArrayType)paramType).ElementType)));
                            }
                            else
                            {
                                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, MakeGenericMethod(inParam, paramType)));
                            }
                        }
                    }
                }

                int outStart = (isVoid ? 0 : 1);

                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldsfld, fieldReference));
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldc_I4, outCout + outStart));
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, functionInvoke));

                int outPos = outStart;
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    if (method.Parameters[i].ParameterType.IsByReference)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldsfld, fieldReference));
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldc_I4, outPos));
                        int arg_pos = param_start + i;
                        if (arg_pos < ldargs.Length)
                        {
                            processor.InsertBefore(insertPoint, processor.Create(ldargs[arg_pos]));
                        }
                        else if (arg_pos < 256)
                        {
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_S, (byte)arg_pos));
                        }
                        else
                        {
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg, (short)arg_pos));
                        }
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, MakeGenericMethod(outParam,
                            ((ByReferenceType)method.Parameters[i].ParameterType).ElementType)));
                        outPos++;
                    }
                }
                if (statefulConstructor)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_0));
                }
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldsfld, fieldReference));
                if (isVoid)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, invokeSessionEnd));
                }
                else
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, MakeGenericMethod(invokeSessionEndWithResult, returnType)));
                }
                if (statefulConstructor)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stfld, stateTable));
                }
                if (isFinalize && hotfixType == 1)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_0));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldnull));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stfld, stateTable));
                }
                if (!method.IsConstructor && !isFinalize)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ret));
                }

                if (!method.IsConstructor)
                {
                    break;
                }
                insertPoint = findNextRet(method.Body.Instructions, insertPoint);
            }

            if (isFinalize)
            {
                method.Body.ExceptionHandlers[0].TryStart = method.Body.Instructions[0];
            }

            return true;
        }
    }
}
#else
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace XLua
{
    public static class Hotfix
    {
        [PostProcessScene]
        [MenuItem("XLua/Hotfix Inject In Editor", false, 3)]
        public static void HotfixInject()
        {
            if (EditorApplication.isCompiling || Application.isPlaying)
            {
                return;
            }

#if UNITY_EDITOR_OSX
			var mono_path = Path.Combine(Path.GetDirectoryName(typeof(UnityEngine.Debug).Module.FullyQualifiedName),
				"../MonoBleedingEdge/bin/mono");
#elif UNITY_EDITOR_WIN
            var mono_path = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                "Data/MonoBleedingEdge/bin/mono.exe");
#endif
            var inject_tool_path = "./Tools/XLuaHotfixInject.exe";
            if (!File.Exists(inject_tool_path))
            {
                UnityEngine.Debug.LogError("please install the Tools");
                return;
            }

            var assembly_csharp_path = "./Library/ScriptAssemblies/Assembly-CSharp.dll";

            List<string> args = new List<string>() { inject_tool_path, assembly_csharp_path,
#if NO_HOTFIX_GEN
                "nogen"
#else
                "gen"
#endif
            };

            foreach (var path in
                (from asm in AppDomain.CurrentDomain.GetAssemblies() select asm.ManifestModule.FullyQualifiedName)
                 .Distinct())
            {
                try
                {
                    args.Add(System.IO.Path.GetDirectoryName(path));
                }
                catch (Exception)
                {

                }
            }

            Process hotfix_injection = new Process();
            hotfix_injection.StartInfo.FileName = mono_path;
			hotfix_injection.StartInfo.Arguments = "\"" + String.Join("\" \"", args.Distinct().ToArray()) + "\"";
            hotfix_injection.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            hotfix_injection.StartInfo.RedirectStandardOutput = true;
            hotfix_injection.StartInfo.UseShellExecute = false;
            hotfix_injection.StartInfo.CreateNoWindow = true;
            hotfix_injection.Start();
            hotfix_injection.WaitForExit();
            UnityEngine.Debug.Log(hotfix_injection.StandardOutput.ReadToEnd());
        }
    }
}
#endif
#endif
