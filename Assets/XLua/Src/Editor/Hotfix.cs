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

        static TypeReference objType = null;
        static TypeReference luaTableType = null;

        static void init(AssemblyDefinition assembly)
        {
            objType = assembly.MainModule.Import(typeof(object));

            luaTableType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.LuaTable");
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
                        var type_left = (param_left.ParameterType.IsByReference || param_left.ParameterType.IsValueType) ? param_left.ParameterType : objType;
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

            var hotfixDelegateAttributeType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.HotfixDelegateAttribute");
            hotfix_delegates = (from module in assembly.Modules
                                from type in module.Types
                                where type.CustomAttributes.Any(ca => ca.AttributeType == hotfixDelegateAttributeType)
                                select type).ToList();

            var hotfixAttributeType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.HotfixAttribute");
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
                        if (method.Name != ".cctor")
                        {
                            if (!InjectCode(assembly, method, hotfixType, stateTable))
                            {
                                return;
                            }
                        }
                    }
                }
            }

            assembly.Write(INTERCEPT_ASSEMBLY_PATH);

            Debug.Log("hotfix inject finish!");
        }
        
        static OpCode[] ldargs = new OpCode[] { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };

        static readonly int MAX_OVERLOAD = 100;

        static bool InjectCode(AssemblyDefinition assembly, MethodDefinition method, int hotfixType, FieldDefinition stateTable)
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
            if (luaDelegateName == null)
            {
                Debug.LogError("too many overload!");
                return false;
            }
            if (method.HasGenericParameters)
            {
                return true;
            }
            TypeReference delegateType = null;
            MethodReference invoke = null;

            int param_count = method.Parameters.Count + (method.IsStatic ? 0 : 1);

            if (!findHotfixDelegate(assembly, method, out delegateType, out invoke, hotfixType))
            {
                Debug.LogWarning("can not find delegate for " + method.DeclaringType + "." + method.Name + "! try re-genertate code.");
                return false;
            }

            if (delegateType == null || invoke == null)
            {
                throw new Exception("unknow exception!");
            }
            FieldDefinition fieldDefinition = new FieldDefinition(luaDelegateName, Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.Private,
                delegateType);
            type.Fields.Add(fieldDefinition);

            bool statefulConstructor = (hotfixType == 1) && method.IsConstructor && !method.IsStatic;

            
            var firstIns = method.Body.Instructions[0];
            var processor = method.Body.GetILProcessor();

            processor.InsertBefore(firstIns, processor.Create(OpCodes.Ldsfld, fieldDefinition));
            processor.InsertBefore(firstIns, processor.Create(OpCodes.Brfalse, firstIns));

            if (statefulConstructor)
            {
                processor.InsertBefore(firstIns, processor.Create(OpCodes.Ldarg_0));
            }
            processor.InsertBefore(firstIns, processor.Create(OpCodes.Ldsfld, fieldDefinition));
            for (int i = 0; i < param_count; i++)
            {
                if (i < ldargs.Length)
                {
                    processor.InsertBefore(firstIns, processor.Create(ldargs[i]));
                }
                else
                {
                    processor.InsertBefore(firstIns, processor.Create(OpCodes.Ldarg, (short)i));
                }
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

            return true;
        }
    }
}
#endif
