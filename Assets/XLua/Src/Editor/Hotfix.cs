/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#if HOTFIX_ENABLE
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor.Callbacks;

namespace XLua
{
    public static class Hotfix
    {
        static readonly string INTERCEPT_ASSEMBLY_PATH = "./Library/ScriptAssemblies/Assembly-CSharp.dll";

        static TypeReference[] actions = null;
        static TypeReference[] funcs = null;
        static TypeReference objType = null;
        static TypeReference luaTableType = null;

        static void init(AssemblyDefinition assembly)
        {
            actions = new Type[] { typeof(Action), typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), typeof(Action<,,,>) }
              .Select(type => assembly.MainModule.Import(type))
              .ToArray();

            funcs = new Type[] { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>) }
              .Select(type => assembly.MainModule.Import(type))
              .ToArray();

            objType = assembly.MainModule.Import(typeof(object));

            luaTableType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.LuaTable");
        }

        static List<Type> cs_call_lua_delegate = null;

        static Type toSystemType(TypeReference tr)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(tr.FullName, false);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }
        static bool isSameType(TypeReference left, Type right)
        {
            return toSystemType(left) == right;
        }

        static bool isAssignableFrom(Type left, TypeReference right)
        {
            Type right_type = toSystemType(right);
            if (right_type == null)
            {
                return false;
            }
            else
            {
                return left.IsAssignableFrom(right_type);
            }
        }
        static bool findGenDelegate(AssemblyDefinition assembly, MethodDefinition method, out TypeReference delegateType, out MethodReference invoke, int hotfixType)
        {
            for (int i = 0; i < cs_call_lua_delegate.Count; i++)
            {
                MethodInfo delegate_invoke = cs_call_lua_delegate[i].GetMethod("Invoke");
                var returnType = (hotfixType == 1 && method.IsConstructor) ? luaTableType : method.ReturnType;
                if (isSameType(returnType, delegate_invoke.ReturnType))
                {
                    var parametersOfDelegate = delegate_invoke.GetParameters();
                    int compareOffset = 0;
                    if (!method.IsStatic)
                    {
                        if (parametersOfDelegate.Length == 0)
                        {
                            continue;
                        }
                        if(hotfixType == 1 && !method.IsConstructor)
                        {
                            if (parametersOfDelegate[0].ParameterType != typeof(LuaTable))
                            {
                                continue;
                            }
                        }
                        else if (parametersOfDelegate[0].ParameterType.IsByRef || !isAssignableFrom(parametersOfDelegate[0].ParameterType, method.DeclaringType))
                        {
                            continue;
                        }
                        compareOffset = 1;
                    }
                    if (method.Parameters.Count != (parametersOfDelegate.Length - compareOffset))
                    {
                        continue;
                    }
                    for(int j = 0; j < method.Parameters.Count; j++)
                    {
                        var param_left = method.Parameters[j];
                        var param_right = parametersOfDelegate[compareOffset];
                        if (param_left.IsOut != param_right.IsOut
                            || param_left.ParameterType.IsByReference != param_right.ParameterType.IsByRef)
                        {
                            continue;
                        }
                        if ((param_left.ParameterType.IsByReference || param_left.ParameterType.IsValueType) ? 
                            !isSameType(param_left.ParameterType, param_right.ParameterType)
                            : !isAssignableFrom(param_right.ParameterType, param_left.ParameterType))
                        {
                            continue;
                        }
                        compareOffset++;
                    }


                    var type = cs_call_lua_delegate[i];
                    if (type.Module.Assembly.FullName == assembly.FullName &&
                        type.Module.FullyQualifiedName == assembly.MainModule.FullyQualifiedName)
                    {
                        delegateType = assembly.MainModule.Types.Single(t => t.FullName == type.FullName);
                        invoke = delegateType.Resolve().Methods.Single(m => m.Name == "Invoke");
                    }
                    else
                    {
                        delegateType = assembly.MainModule.Import(type);
                        invoke = assembly.MainModule.Import(delegate_invoke);
                    }
                    return true;
                }
            }
            delegateType = null;
            invoke = null;
            return false;
        }

        [DidReloadScripts]
        [PostProcessScene]
        //[UnityEditor.MenuItem("XLua/Hotfix Inject In Editor", false, 3)]
        public static void HotfixInject()
        {
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(INTERCEPT_ASSEMBLY_PATH);
            init(assembly);

            if (assembly.MainModule.Types.Any(t => t.Name == "__XLUA_GEN_FLAG")) return;

            assembly.MainModule.Types.Add(new TypeDefinition("__XLUA_GEN", "__XLUA_GEN_FLAG", Mono.Cecil.TypeAttributes.Class,
                objType));

            CSObjectWrapEditor.Generator.GetGenConfig();// load config
            cs_call_lua_delegate = CSObjectWrapEditor.Generator.CSharpCallLua.Where(type => typeof(Delegate).IsAssignableFrom(type)).ToList();

            var hotfixAttributeType = assembly.MainModule.Types.Single(t => t.Name == "HotfixAttribute");
            foreach (var type in (from module in assembly.Modules from type in module.Types select type))
            {
                CustomAttribute hotfixAttr = type.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == hotfixAttributeType);
                if (hotfixAttr != null)
                {
                    int hotfixType = (int)hotfixAttr.ConstructorArguments[0].Value;
                    FieldDefinition stateTable = null;
                    if (hotfixType == 1)
                    {
                        if (type.IsAbstract && type.IsSealed)
                        {
                            throw new InvalidOperationException(type.FullName + " is static, can not be mark as Stateful!");
                        }
                        stateTable = new FieldDefinition("__Hitfix_xluaStateTable", Mono.Cecil.FieldAttributes.Private, luaTableType);
                        type.Fields.Add(stateTable );
                    }
                    foreach (var method in type.Methods)
                    {
                        if (method.Name != ".cctor" && method.Name != "Finalize")
                        {
                            InjectCode(assembly, method, hotfixType, stateTable);
                        }
                    }
                }
            }

            assembly.Write(INTERCEPT_ASSEMBLY_PATH);

            Debug.Log("hotfix inject finish!");
        }

        static TypeReference getParamType(TypeReference type)
        {
            return type.IsValueType ? type : objType;
        }

        static MethodReference makeInvokeInstance( MethodReference invoke, TypeReference delegateType)
        {
            var ret = new MethodReference(
                invoke.Name,
                invoke.ReturnType,
                delegateType)
            {
                HasThis = invoke.HasThis,
                ExplicitThis = invoke.ExplicitThis,
                CallingConvention = invoke.CallingConvention
            };

            foreach (var parameter in invoke.Parameters)
            {
                ret.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            foreach (var genericParam in invoke.GenericParameters)
            {
                ret.GenericParameters.Add(new GenericParameter(genericParam.Name, ret));
            }

            return ret;
        }

        static void genDelegateType(AssemblyDefinition assembly, MethodDefinition method, out TypeReference delegateType, out MethodReference invoke, int hotfixType)
        {
            List<TypeReference> paramTypes = new List<TypeReference>();

            if (!method.IsStatic) paramTypes.Add((hotfixType == 1 && !method.IsConstructor) ? luaTableType : getParamType(method.DeclaringType));
            paramTypes.AddRange(method.Parameters.Select(p => getParamType(p.ParameterType)));

            bool statefulConstructor = (hotfixType == 1) && method.IsConstructor;
            if (!statefulConstructor && method.ReturnType.ToString() == "System.Void")
            {
                if (paramTypes.Count == 0)
                {
                    delegateType = actions[0];
                    invoke = assembly.MainModule.Import(delegateType.Resolve().Methods.Single(m => m.Name == "Invoke"));
                }
                else
                {
                    GenericInstanceType gtype = new GenericInstanceType(actions[paramTypes.Count]);

                    foreach (var paramType in paramTypes)
                    {
                        gtype.GenericArguments.Add(paramType);
                    }
                    delegateType = assembly.MainModule.Import(gtype);
                    invoke = assembly.MainModule.Import(makeInvokeInstance(delegateType.Resolve().Methods.Single(m => m.Name == "Invoke"), delegateType));
                }
            }
            else
            {
                GenericInstanceType gtype = new GenericInstanceType(funcs[paramTypes.Count]);

                foreach (var paramType in paramTypes)
                {
                    gtype.GenericArguments.Add(paramType);
                }
                if (statefulConstructor)
                {
                    gtype.GenericArguments.Add(luaTableType);
                }
                else
                {
                    gtype.GenericArguments.Add(method.ReturnType);
                }

                delegateType = assembly.MainModule.Import(gtype);
                invoke = assembly.MainModule.Import(makeInvokeInstance(delegateType.Resolve().Methods.Single(m => m.Name == "Invoke"), delegateType));
            }
        }

        static OpCode[] ldargs = new OpCode[] { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };

        static readonly int MAX_OVERLOAD = 100;

        static void InjectCode(AssemblyDefinition assembly, MethodDefinition method, int hotfixType, FieldDefinition stateTable)
        {
            string fieldName = (method.IsConstructor ? "XLuaConstructor" : method.Name);
            string luaDelegateName = null;
            var type = method.DeclaringType;
            for (int i = 0; i < MAX_OVERLOAD; i++)
            {
                string tmp = "__Hitfix" + i + "_" + fieldName;
                if (!type.Fields.Any(f => f.Name == tmp)) // injected
                {
                    luaDelegateName = tmp;
                    break;
                }
            }
            if (luaDelegateName == null)
            {
                throw new Exception("too many overload!");
            }
            if (method.HasGenericParameters)
            {
                Debug.LogWarning(method.Name + " has generic parameter! skiped");
                return;
            }
            TypeReference delegateType = null;
            MethodReference invoke = null;

            int param_count = method.Parameters.Count + (method.IsStatic ? 0 : 1);
            if (param_count > 4)
            {
                Debug.LogWarning("you must declare delegate for " + method.DeclaringType + "." + method.Name + " and mark as CSharpCallLua");
                return;
            }
            foreach(var param in method.Parameters)
            {
                if (param.ParameterType.IsByReference)
                {
                    if (!findGenDelegate(assembly, method, out delegateType, out invoke, hotfixType))
                    {
                        Debug.LogWarning("you must declare delegate for " + method.DeclaringType + "." + method.Name + " and mark as CSharpCallLua");
                        return;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (delegateType == null && invoke == null)
            {
                genDelegateType(assembly, method, out delegateType, out invoke, hotfixType);
            }
            if (delegateType == null || invoke == null)
            {
                throw new Exception("unknow exception!");
            }
            FieldDefinition fieldDefinition = new FieldDefinition(luaDelegateName, Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.Private,
                delegateType);
            type.Fields.Add(fieldDefinition);

            bool statefulConstructor = (hotfixType == 1) && method.IsConstructor;

            var firstIns = method.Body.Instructions[0];
            var processor = method.Body.GetILProcessor();
            processor.InsertBefore(firstIns, processor.Create(OpCodes.Ldsfld, fieldDefinition));
            processor.InsertBefore(firstIns, processor.Create(OpCodes.Brfalse, firstIns));

            if (statefulConstructor)
            {
                processor.InsertBefore(firstIns, processor.Create(OpCodes.Ldarg_0));
            }
            processor.InsertBefore(firstIns, processor.Create(OpCodes.Ldsfld, fieldDefinition));
            for(int i = 0; i < param_count; i++)
            {
                processor.InsertBefore(firstIns, processor.Create(ldargs[i]));
                if (i == 0 && hotfixType == 1 && !method.IsStatic && !method.IsConstructor)
                {
                    processor.InsertBefore(firstIns, processor.Create(OpCodes.Ldfld, stateTable));
                }
            }

            processor.InsertBefore(firstIns, processor.Create(OpCodes.Call, invoke));
            if (statefulConstructor)
            {
                processor.InsertBefore(firstIns, processor.Create(OpCodes.Stfld, stateTable));
            }
            processor.InsertBefore(firstIns, processor.Create(OpCodes.Ret));

        }

        static Type[] action_types = new Type[] { typeof(Action), typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), typeof(Action<,,,>) };
        static Type[] func_types = new Type[] { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>) };

        static Type genericType(Type type)
        {
            return type.IsValueType ? type : typeof(object);
        }

        static Type delegateType(MethodBase method)
        {
            var hotfixType = ((method.DeclaringType.GetCustomAttributes(typeof(HotfixAttribute), false)[0]) as HotfixAttribute).Flag;
            int param_count = method.GetParameters().Length + (method.IsStatic ? 0 : 1);
            bool is_void = method.IsConstructor || ((method as MethodInfo).ReturnType == typeof(void));
            if (is_void && param_count == 0)
            {
                return action_types[0];
            }

            List<Type> genericParams = new List<Type>();
            if (!method.IsStatic)
            {
                genericParams.Add((hotfixType == HotfixFlag.Stateful && !method.IsConstructor) ? typeof(LuaTable) : genericType(method.DeclaringType));
            }
            foreach (var param in method.GetParameters())
            {
                genericParams.Add(genericType(param.ParameterType));
            }
            bool statefulConstructor = (hotfixType == HotfixFlag.Stateful) && method.IsConstructor;
            if (!is_void || statefulConstructor)
            {
                genericParams.Add(statefulConstructor? typeof(LuaTable) : (method as MethodInfo).ReturnType);
                return func_types[param_count].MakeGenericType(genericParams.ToArray());
            }
            else
            {
                return action_types[param_count].MakeGenericType(genericParams.ToArray());
            }
        }

        [CSharpCallLua]
        static IEnumerable<Type> HotfixDelegate
        {
            get
            {
                var need_gen_types = (from type in Utils.GetAllTypes() where type.IsDefined(typeof(HotfixAttribute), false) select type)
                    .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic).Cast<MethodBase>().Concat(type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Cast<MethodBase>()))
                    .Where(method => !method.ContainsGenericParameters && method.GetParameters().Length <= (method.IsStatic ? 4 : 3) && !method.GetParameters().Any(p => p.ParameterType.IsByRef))
                    .Select(method => delegateType(method)).Distinct().ToList();
                need_gen_types.Sort((t1, t2) => t1.Name.CompareTo(t2.Name));
                return need_gen_types;
            }
        }
    }
}
#endif
