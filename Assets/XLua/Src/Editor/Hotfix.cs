/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#if HOTFIX_ENABLE
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

#if XLUA_GENERAL
using Mono.Cecil;
using Mono.Cecil.Cil;
#endif

#if !XLUA_GENERAL
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Diagnostics;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif
#endif

namespace XLua
{
    public static class HotfixConfig
    {
        //返回-1表示没有标签
        static int getHotfixType(MemberInfo memberInfo)
        {
            try
            {
                foreach (var ca in memberInfo.GetCustomAttributes(false))
                {
                    var ca_type = ca.GetType();
                    if (ca_type.ToString() == "XLua.HotfixAttribute")
                    {
                        return (int)(ca_type.GetProperty("Flag").GetValue(ca, null));
                    }
                }
            }
            catch { }
            return -1;
        }

        static void mergeConfig(MemberInfo test, Type cfg_type, Func<IEnumerable<Type>> get_cfg, Action<Type, int> on_cfg)
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
                    if ((type.Namespace == null || (type.Namespace != "XLua" && !type.Namespace.StartsWith("XLua."))))
                    {
                        on_cfg(type, hotfixType);
                    }
                }
            }
        }

        public static void GetConfig(Dictionary<string, int> hotfixCfg, IEnumerable<Type> cfg_check_types)
        {
            if (cfg_check_types != null)
            {
                Action<Type, int> on_cfg = (type, hotfixType) =>
                {
                    string key = type.FullName.Replace('+', '/');
                    if (!hotfixCfg.ContainsKey(key))
                    {
                        hotfixCfg.Add(key, hotfixType);
                    }
                };
                foreach (var type in cfg_check_types.Where(t => !t.IsGenericTypeDefinition && t.IsAbstract && t.IsSealed))
                {
                    var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                    foreach (var field in type.GetFields(flags))
                    {
                        mergeConfig(field, field.FieldType, () => field.GetValue(null) as IEnumerable<Type>, on_cfg);
                    }
                    foreach (var prop in type.GetProperties(flags))
                    {
                        mergeConfig(prop, prop.PropertyType, () => prop.GetValue(null, null) as IEnumerable<Type>, on_cfg);
                    }
                }
            }
        }

        public static List<Assembly> GetHotfixAssembly()
        {
            var projectPath = Assembly.Load("Assembly-CSharp").ManifestModule.FullyQualifiedName;
            Regex rgx = new Regex(@"^(.*)[\\/]Library[\\/]ScriptAssemblies[\\/]Assembly-CSharp.dll$");
            MatchCollection matches = rgx.Matches(projectPath);
            projectPath = matches[0].Groups[1].Value;

            List<Type> types = new List<Type>();
            Action<Type, int> on_cfg = (type, hotfixType) =>
            {
                types.Add(type);
            };

            foreach (var assmbly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in (from type in assmbly.GetTypes()
                                          where !type.IsGenericTypeDefinition
                                          select type))
                    {
                        if (getHotfixType(type) != -1)
                        {
                            types.Add(type);
                        }
                        else
                        {
                            var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                            foreach (var field in type.GetFields(flags))
                            {
                                mergeConfig(field, field.FieldType, () => field.GetValue(null) as IEnumerable<Type>, on_cfg);
                            }
                            foreach (var prop in type.GetProperties(flags))
                            {
                                mergeConfig(prop, prop.PropertyType, () => prop.GetValue(null, null) as IEnumerable<Type>, on_cfg);
                            }
                        }
                    }
                }
                catch { } // 防止有的工程有非法的dll导致中断
            }
            return types.Select(t => t.Assembly).Distinct()
                .Where(a => a.ManifestModule.FullyQualifiedName.IndexOf(projectPath) == 0)
                .ToList();
        }

        public static List<string> GetHotfixAssemblyPaths()
        {
            return GetHotfixAssembly().Select(a => a.ManifestModule.FullyQualifiedName).Distinct().ToList();
        }
    }
}
#if XLUA_GENERAL

namespace XLua
{
    [Flags]
    enum HotfixFlagInTool
    {
        Stateless = 0,
        Stateful = 1,
        ValueTypeBoxing = 2,
        IgnoreProperty = 4,
        IgnoreNotPublic = 8,
        Inline = 16,
        IntKey = 32,
        AdaptByDelegate = 64,
        IgnoreCompilerGenerated = 128,
        NoBaseProxy = 256,
    }

    static class ExtentionMethods
    {
        public static bool HasFlag(this HotfixFlagInTool toCheck, HotfixFlagInTool flag)
        {
            return (toCheck != HotfixFlagInTool.Stateless) && ((toCheck & flag) == flag);
        }

        public static MethodReference MakeGenericMethod(this MethodReference self, params TypeReference[] arguments)
        {
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceMethod(self);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        public static FieldReference GetGeneric(this FieldDefinition definition)
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

        public static TypeReference TryImport(this ModuleDefinition module, TypeReference type)
        {
            if (module.Assembly.FullName == type.Module.Assembly.FullName
                && module.FullyQualifiedName == type.Module.FullyQualifiedName)
            {
                return type;
            }
            else
            {
#if XLUA_GENERAL
                return module.ImportReference(type);
#else
                return module.Import(type);
#endif
            }
        }

        public static MethodReference TryImport(this ModuleDefinition module, MethodReference method)
        {
            if (module.Assembly.FullName == method.Module.Assembly.FullName
                && module.FullyQualifiedName == method.Module.FullyQualifiedName)
            {
                return method;
            }
            else
            {
#if XLUA_GENERAL
                return module.ImportReference(method);
#else
                return module.Import(method);
#endif
            }
        }
    }

    public class Hotfix
    {
        private TypeReference objType = null;
        private TypeReference delegateBridgeType = null;
        private AssemblyDefinition injectAssembly = null;

        private MethodReference delegateBridgeGetter = null;
        private MethodReference hotfixFlagGetter = null;
        private MethodReference invokeSessionStart = null;
        private MethodReference functionInvoke = null;
        private MethodReference invokeSessionEnd = null;
        private MethodReference invokeSessionEndWithResult = null;
        private MethodReference inParam = null;
        private MethodReference inParams = null;
        private MethodReference outParam = null;

        private Dictionary<string, int> hotfixCfg = null;
        private List<MethodDefinition> hotfixBridgesDef = null;
        private Dictionary<MethodDefinition, MethodDefinition> hotfixBridgeToDelegate = null;

        private List<MethodDefinition> bridgeIndexByKey = null;

        private bool isTheSameAssembly = false;

        private int delegateId = 0;

        public void Init(AssemblyDefinition injectAssembly, AssemblyDefinition xluaAssembly, IEnumerable<string> searchDirectorys, Dictionary<string, int> hotfixCfg)
        {
            isTheSameAssembly = injectAssembly == xluaAssembly;
            this.injectAssembly = injectAssembly;
            this.hotfixCfg = hotfixCfg;
            var injectModule = injectAssembly.MainModule;
            objType = injectModule.TypeSystem.Object;

            var delegateBridgeTypeDef = xluaAssembly.MainModule.Types.Single(t => t.FullName == "XLua.DelegateBridge");
            delegateBridgeType = injectModule.TryImport(delegateBridgeTypeDef);
            delegateBridgeGetter = injectModule.TryImport(xluaAssembly.MainModule.Types.Single(t => t.FullName == "XLua.HotfixDelegateBridge")
                .Methods.Single(m => m.Name == "Get"));
            hotfixFlagGetter = injectModule.TryImport(xluaAssembly.MainModule.Types.Single(t => t.FullName == "XLua.HotfixDelegateBridge")
                .Methods.Single(m => m.Name == "xlua_get_hotfix_flag"));

            //luaFunctionType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.LuaFunction");
            invokeSessionStart = injectModule.TryImport(delegateBridgeTypeDef.Methods.Single(m => m.Name == "InvokeSessionStart"));
            functionInvoke = injectModule.TryImport(delegateBridgeTypeDef.Methods.Single(m => m.Name == "Invoke"));
            invokeSessionEnd = injectModule.TryImport(delegateBridgeTypeDef.Methods.Single(m => m.Name == "InvokeSessionEnd"));
            invokeSessionEndWithResult = injectModule.TryImport(delegateBridgeTypeDef.Methods.Single(m => m.Name == "InvokeSessionEndWithResult"));
            inParam = injectModule.TryImport(delegateBridgeTypeDef.Methods.Single(m => m.Name == "InParam"));
            inParams = injectModule.TryImport(delegateBridgeTypeDef.Methods.Single(m => m.Name == "InParams"));
            outParam = injectModule.TryImport(delegateBridgeTypeDef.Methods.Single(m => m.Name == "OutParam"));

            hotfixBridgesDef = (from method in delegateBridgeTypeDef.Methods
                              where method.Name.StartsWith("__Gen_Delegate_Imp")
                              select method).ToList();
            hotfixBridgeToDelegate = new Dictionary<MethodDefinition, MethodDefinition>();
            delegateId = 0;

            //hotfixBridges = hotfixBridgesDef.Select(m => injectModule.TryImport(m)).ToList();

            bridgeIndexByKey = new List<MethodDefinition>();

            var resolverOfInjectAssembly = injectAssembly.MainModule.AssemblyResolver as BaseAssemblyResolver;
            var resolverOfXluaAssembly = xluaAssembly.MainModule.AssemblyResolver as BaseAssemblyResolver;
            if (!isTheSameAssembly)
            {
                resolverOfXluaAssembly.AddSearchDirectory(Path.GetDirectoryName(injectAssembly.MainModule.FullyQualifiedName));
            }
            Action<string> addSearchDirectory = (string dir) =>
            {
                resolverOfInjectAssembly.AddSearchDirectory(dir);
                if (!isTheSameAssembly)
                {
                    resolverOfXluaAssembly.AddSearchDirectory(dir);
                }
            };
            addSearchDirectory("./Library/ScriptAssemblies/");
            foreach (var path in
                (from asm in AppDomain.CurrentDomain.GetAssemblies() select asm.ManifestModule.FullyQualifiedName)
                 .Distinct())
            {
                try
                {
                    addSearchDirectory(Path.GetDirectoryName(path));
                }
                catch(Exception)
                {

                }
            }

            if (searchDirectorys != null)
            {
                foreach(var directory in searchDirectorys.Distinct())
                {
                    addSearchDirectory(directory);
                }
            }

            var nameToOpcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .ToDictionary(f => f.Name, f => ((OpCode)f.GetValue(null)));
            foreach(var kv in nameToOpcodes)
            {
                if (kv.Key.EndsWith("_S"))
                {
                    shortToLong[kv.Value] = nameToOpcodes[kv.Key.Substring(0, kv.Key.Length - 2)];
                }
            }
        }

        static string getAssemblyFullName(IMetadataScope scope)
        {
            if (scope == null) return null;
            switch(scope.MetadataScopeType)
            {
                case MetadataScopeType.ModuleDefinition:
                    {
                        ModuleDefinition md = scope as ModuleDefinition;
                        return md.Assembly.FullName;
                    }
                case MetadataScopeType.AssemblyNameReference:
                    {
                        AssemblyNameReference anr = scope as AssemblyNameReference;
                        return anr.FullName;
                    }
            }
            return null;
        }

        static bool isSameType(TypeReference left, TypeReference right)
        {
            if (left.FullName != right.FullName)
            {
                return false;
            }

            //TypeReference.Module.FullyQualifiedName不验证
            if (left.Module.Assembly.FullName == right.Module.Assembly.FullName)
            {
                return true;
            }
            else
            {
                var lafn = getAssemblyFullName(left.Scope);
                var rafn = getAssemblyFullName(right.Scope);
                if (lafn != null && lafn == rafn)
                {
                    return true;
                }
                var lr = left.Resolve();
                var rr = right.Resolve();
                if (lr == null || rr == null) return false;
                return lr.Module.Assembly.FullName == rr.Module.Assembly.FullName;
            }
        }

        MethodDefinition createDelegateFor(MethodDefinition method, AssemblyDefinition assembly, string delegateName, bool ignoreValueType)
        {
            var voidType = assembly.MainModule.TypeSystem.Void;
            var objectType = assembly.MainModule.TypeSystem.Object;
            var nativeIntType = assembly.MainModule.TypeSystem.IntPtr;
            var asyncResultType = assembly.MainModule.Import(typeof(IAsyncResult));
            var asyncCallbackType = assembly.MainModule.Import(typeof(AsyncCallback));

            Mono.Cecil.MethodAttributes delegateMethodAttributes = Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.Virtual | Mono.Cecil.MethodAttributes.VtableLayoutMask;

            var delegateDef = new TypeDefinition("XLua", delegateName, Mono.Cecil.TypeAttributes.Sealed | Mono.Cecil.TypeAttributes.Public,
                    assembly.MainModule.Import(typeof(MulticastDelegate)));
            List<TypeReference> argTypes = new List<TypeReference>();
            TypeReference self = null;
            if (!method.IsStatic)
            {
                self = (!ignoreValueType && method.DeclaringType.IsValueType) ? method.DeclaringType : objType;
            }
            foreach(var parameter in method.Parameters)
            {
                bool isparam = parameter.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.Name == "ParamArrayAttribute") != null;
                argTypes.Add((isparam || parameter.ParameterType.IsByReference || (!ignoreValueType && parameter.ParameterType.IsValueType)) ? parameter.ParameterType : objType);
            }

            var constructor = new MethodDefinition(".ctor", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName, voidType);
            constructor.Parameters.Add(new ParameterDefinition("objectInstance", Mono.Cecil.ParameterAttributes.None, objectType));
            constructor.Parameters.Add(new ParameterDefinition("functionPtr", Mono.Cecil.ParameterAttributes.None, nativeIntType));
            constructor.ImplAttributes = Mono.Cecil.MethodImplAttributes.Runtime;
            delegateDef.Methods.Add(constructor);

            var beginInvoke = new MethodDefinition("BeginInvoke", delegateMethodAttributes, asyncResultType);
            if (self != null)
            {
                beginInvoke.Parameters.Add(new ParameterDefinition(self));
            }
            for (int i = 0; i < argTypes.Count; i++)
            {
                beginInvoke.Parameters.Add(new ParameterDefinition(method.Parameters[i].Name, (method.Parameters[i].IsOut ? Mono.Cecil.ParameterAttributes.Out : Mono.Cecil.ParameterAttributes.None), argTypes[i]));
            }
            beginInvoke.Parameters.Add(new ParameterDefinition("callback", Mono.Cecil.ParameterAttributes.None, asyncCallbackType));
            beginInvoke.Parameters.Add(new ParameterDefinition("object", Mono.Cecil.ParameterAttributes.None, objectType));
            beginInvoke.ImplAttributes = Mono.Cecil.MethodImplAttributes.Runtime;
            delegateDef.Methods.Add(beginInvoke);

            var endInvoke = new MethodDefinition("EndInvoke", delegateMethodAttributes, method.ReturnType);
            for (int i = 0; i < argTypes.Count; i++)
            {
                if (argTypes[i].IsByReference)
                {
                    endInvoke.Parameters.Add(new ParameterDefinition(method.Parameters[i].Name, (method.Parameters[i].IsOut ? Mono.Cecil.ParameterAttributes.Out : Mono.Cecil.ParameterAttributes.None), argTypes[i]));
                }
            }
            endInvoke.Parameters.Add(new ParameterDefinition("result", Mono.Cecil.ParameterAttributes.None, asyncResultType));
            endInvoke.ImplAttributes = Mono.Cecil.MethodImplAttributes.Runtime;
            delegateDef.Methods.Add(endInvoke);

            var invoke = new MethodDefinition("Invoke", delegateMethodAttributes, method.ReturnType);
            if (self != null)
            {
                invoke.Parameters.Add(new ParameterDefinition(self));
            }
            for(int i = 0; i < argTypes.Count; i++)
            {
                invoke.Parameters.Add(new ParameterDefinition(method.Parameters[i].Name, (method.Parameters[i].IsOut ? Mono.Cecil.ParameterAttributes.Out : Mono.Cecil.ParameterAttributes.None), argTypes[i]));
            }
            invoke.ImplAttributes = Mono.Cecil.MethodImplAttributes.Runtime;
            delegateDef.Methods.Add(invoke);

            assembly.MainModule.Types.Add(delegateDef);

            return invoke;
        }

        MethodDefinition getDelegateInvokeFor(MethodDefinition method, MethodDefinition bridgeDef, bool ignoreValueType)
        {
            MethodDefinition ret;
            if (!hotfixBridgeToDelegate.TryGetValue(bridgeDef, out ret))
            {
                ret = createDelegateFor(method, injectAssembly, ("__XLua_Gen_Delegate" + (delegateId++)), ignoreValueType);
                hotfixBridgeToDelegate.Add(bridgeDef, ret);
            }

            return ret;
        }

        bool findHotfixDelegate(MethodDefinition method, out MethodReference invoke, HotfixFlagInTool hotfixType)
        {
            bool ignoreValueType = hotfixType.HasFlag(HotfixFlagInTool.ValueTypeBoxing);

            bool isIntKey = hotfixType.HasFlag(HotfixFlagInTool.IntKey) && !method.DeclaringType.HasGenericParameters && isTheSameAssembly;

            bool isAdaptByDelegate = !isIntKey && hotfixType.HasFlag(HotfixFlagInTool.AdaptByDelegate);

            for (int i = 0; i < hotfixBridgesDef.Count; i++)
            {
                MethodDefinition hotfixBridgeDef = hotfixBridgesDef[i];
                var returnType = method.ReturnType;
                if (isSameType(returnType, hotfixBridgeDef.ReturnType))
                {
                    var parametersOfDelegate = hotfixBridgeDef.Parameters;
                    int compareOffset = 0;
                    if (!method.IsStatic)
                    {
                        var typeOfSelf = (!ignoreValueType && method.DeclaringType.IsValueType) ? method.DeclaringType : objType;
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
                        if (!ignoreValueType && param_left.ParameterType.IsValueType != param_right.ParameterType.IsValueType)
                        {
                            paramMatch = false;
                            break;
                        }
						bool isparam = param_left.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.Name == "ParamArrayAttribute") != null;
						var type_left = (isparam || param_left.ParameterType.IsByReference || (!ignoreValueType && param_left.ParameterType.IsValueType)) ? param_left.ParameterType : objType;
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
                    invoke = (isTheSameAssembly && !isAdaptByDelegate) ? hotfixBridgeDef : getDelegateInvokeFor(method, hotfixBridgeDef, ignoreValueType);
                    return true;
                }
            }
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

        static bool hasGenericParameter(MethodDefinition method)
        {
            if (!method.IsStatic && hasGenericParameter(method.DeclaringType)) return true;
            return hasGenericParameterSkipDelaringType(method);
        }

        static bool hasGenericParameterSkipDelaringType(MethodDefinition method)
        {
            if (method.HasGenericParameters) return true;
            //if (!method.IsStatic && hasGenericParameter(method.DeclaringType)) return true;
            if (hasGenericParameter(method.ReturnType)) return true;
            foreach (var paramInfo in method.Parameters)
            {
                if (hasGenericParameter(paramInfo.ParameterType)) return true;
            }
            return false;
        }

        static bool isNoPublic(TypeReference type)
        {
            if (type.IsByReference)
            {
                return isNoPublic(((ByReferenceType)type).ElementType);
            }
            if (type.IsArray)
            {
                return isNoPublic(((ArrayType)type).ElementType);
            }
            else
            {
                if (type.IsGenericInstance)
                {
                    foreach (var typeArg in ((GenericInstanceType)type).GenericArguments)
                    {
                        if (isNoPublic(typeArg))
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

        static bool genericInOut(MethodDefinition method, HotfixFlagInTool hotfixType)
        {
            bool ignoreValueType = hotfixType.HasFlag(HotfixFlagInTool.ValueTypeBoxing);

            if (hasGenericParameter(method.ReturnType) || isNoPublic(method.ReturnType))
            {
                return true;
            }
            var parameters = method.Parameters;

            if (!method.IsStatic 
                && (hasGenericParameter(method.DeclaringType) || ((!ignoreValueType && method.DeclaringType.IsValueType) && isNoPublic(method.DeclaringType))))
            {
                    return true;
            }
            for (int i = 0; i < parameters.Count; i++)
            {
                if ( hasGenericParameter(parameters[i].ParameterType) || (((!ignoreValueType && parameters[i].ParameterType.IsValueType) || parameters[i].ParameterType.IsByReference || parameters[i].CustomAttributes.Any(ca => ca.AttributeType.FullName == "System.ParamArrayAttribute")) && isNoPublic(parameters[i].ParameterType)))
                {
                    return true;
                }
            }
            return false;
        }

        public bool InjectType(TypeReference hotfixAttributeType, TypeDefinition type)
        {
            foreach(var nestedTypes in type.NestedTypes)
            {
                if (!InjectType(hotfixAttributeType, nestedTypes))
                {
                    return false;
                }
            }
            if (type.Name.Contains("<") || type.IsInterface || type.Methods.Count == 0) // skip anonymous type and interface
            {
                return true;
            }
            CustomAttribute hotfixAttr = type.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == hotfixAttributeType);
            HotfixFlagInTool hotfixType;
            if (hotfixAttr != null)
            {
                hotfixType = (HotfixFlagInTool)(int)hotfixAttr.ConstructorArguments[0].Value;
            }
            else
            {
                if (!hotfixCfg.ContainsKey(type.FullName))
                {
                    return true;
                }
                hotfixType = (HotfixFlagInTool)hotfixCfg[type.FullName];
            }

            bool ignoreProperty = hotfixType.HasFlag(HotfixFlagInTool.IgnoreProperty);
            bool ignoreCompilerGenerated = hotfixType.HasFlag(HotfixFlagInTool.IgnoreCompilerGenerated);
            bool ignoreNotPublic = hotfixType.HasFlag(HotfixFlagInTool.IgnoreNotPublic);
            bool isInline = hotfixType.HasFlag(HotfixFlagInTool.Inline);
            bool isIntKey = hotfixType.HasFlag(HotfixFlagInTool.IntKey);
            bool noBaseProxy = hotfixType.HasFlag(HotfixFlagInTool.NoBaseProxy);
            if (ignoreCompilerGenerated && type.CustomAttributes.Any(ca => ca.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
            {
                return true;
            }
            if (isIntKey && type.HasGenericParameters)
            {
                throw new InvalidOperationException(type.FullName + " is generic definition, can not be mark as IntKey!");
            }
            //isIntKey = !type.HasGenericParameters;

            foreach (var method in type.Methods)
            {
                if (ignoreNotPublic && !method.IsPublic)
                {
                    continue;
                }
                if (ignoreProperty && method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
                {
                    continue;
                }
                if (ignoreCompilerGenerated && method.CustomAttributes.Any(ca => ca.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
                {
                    continue;
                }
                if (method.Parameters.Any(pd => pd.ParameterType.IsPointer) || method.ReturnType.IsPointer)
                {
                    continue;
                }
                if (method.Name != ".cctor" && !method.IsAbstract && !method.IsPInvokeImpl && method.Body != null && !method.Name.Contains("<"))
                {
                    //Debug.Log(method);
                    if ((isInline || method.HasGenericParameters || genericInOut(method, hotfixType)) 
                        ? !injectGenericMethod(method, hotfixType) :
                        !injectMethod(method, hotfixType))
                    {
                        return false;
                    }
                }
            }

            if (!noBaseProxy)
            {
                List<MethodDefinition> toAdd = new List<MethodDefinition>();
                foreach (var method in type.Methods)
                {
                    if (ignoreNotPublic && !method.IsPublic)
                    {
                        continue;
                    }
                    if (ignoreProperty && method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
                    {
                        continue;
                    }
                    if (ignoreCompilerGenerated && method.CustomAttributes.Any(ca => ca.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
                    {
                        continue;
                    }
                    if (method.Parameters.Any(pd => pd.ParameterType.IsPointer) || method.ReturnType.IsPointer)
                    {
                        continue;
                    }
                    if (method.Name != ".cctor" && !method.IsAbstract && !method.IsPInvokeImpl && method.Body != null && !method.Name.Contains("<"))
                    {
                        var proxyMethod = tryAddBaseProxy(type, method);
                        if (proxyMethod != null) toAdd.Add(proxyMethod);
                    }
                }

                foreach (var md in toAdd)
                {
                    type.Methods.Add(md);
                }
            }

            return true;
        }

#if !XLUA_GENERAL
        [PostProcessScene]
        [MenuItem("XLua/Hotfix Inject In Editor", false, 3)]
        public static void HotfixInject()
        {
            if (EditorApplication.isCompiling || Application.isPlaying)
            {
                return;
            }
            var hotfixCfg = new Dictionary<string, int>();
            HotfixConfig.GetConfig(hotfixCfg, Utils.GetAllTypes());
            var xluaAssemblyPath = typeof(LuaEnv).Module.FullyQualifiedName;
            var idMapFileName = CSObjectWrapEditor.GeneratorConfig.common_path + "Resources/hotfix_id_map.lua.txt";
            var injectAssemblyPaths = HotfixConfig.GetHotfixAssemblyPaths();

            foreach (var injectAssemblyPath in injectAssemblyPaths)
            {
                Info("injecting " + injectAssemblyPath);
                if (injectAssemblyPaths.Count > 1)
                {
                    var injectAssemblyFileName = Path.GetFileName(injectAssemblyPath);
                    idMapFileName = CSObjectWrapEditor.GeneratorConfig.common_path + "Resources/hotfix_id_map_" + injectAssemblyFileName.Substring(0, injectAssemblyFileName.Length - 4) + ".lua.txt";
                }
                HotfixInject(injectAssemblyPath, xluaAssemblyPath, null, idMapFileName, hotfixCfg);
            }
            AssetDatabase.Refresh();
        }
#endif

        static AssemblyDefinition readAssembly(string assemblyPath)
        {
#if HOTFIX_SYMBOLS_DISABLE
            return AssemblyDefinition.ReadAssembly(assemblyPath);
#else
            if (File.Exists(assemblyPath + ".mdb"))
            {
                var readerParameters = new ReaderParameters { ReadSymbols = true };
                return AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters);
            }
            else
            {
                return AssemblyDefinition.ReadAssembly(assemblyPath);
            }
#endif
        }

        static void writeAssembly(AssemblyDefinition assembly, string assemblyPath)
        {
#if HOTFIX_SYMBOLS_DISABLE
            assembly.Write(assemblyPath);
#else
            if (File.Exists(assemblyPath + ".mdb"))
            {
                var writerParameters = new WriterParameters { WriteSymbols = true };
                assembly.Write(assemblyPath, writerParameters);
            }
            else
            {
                assembly.Write(assemblyPath);
            }
#endif
        }

        public static void HotfixInject(string injectAssemblyPath, string xluaAssemblyPath, IEnumerable<string> searchDirectorys, string idMapFilePath, Dictionary<string, int> hotfixConfig)
        {
            AssemblyDefinition injectAssembly = null;
            AssemblyDefinition xluaAssembly = null;
            try
            {
                injectAssembly = readAssembly(injectAssemblyPath);
                
                // injected flag check
                if (injectAssembly.MainModule.Types.Any(t => t.Name == "__XLUA_GEN_FLAG"))
                {
                    Info(injectAssemblyPath + " had injected!");
                    return;
                }
                injectAssembly.MainModule.Types.Add(new TypeDefinition("__XLUA_GEN", "__XLUA_GEN_FLAG", Mono.Cecil.TypeAttributes.Class,
                    injectAssembly.MainModule.TypeSystem.Object));

                xluaAssembly = (injectAssemblyPath == xluaAssemblyPath || injectAssembly.MainModule.FullyQualifiedName == xluaAssemblyPath) ? 
                    injectAssembly : readAssembly(xluaAssemblyPath);

                Hotfix hotfix = new Hotfix();
                hotfix.Init(injectAssembly, xluaAssembly, searchDirectorys, hotfixConfig);

                //var hotfixDelegateAttributeType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.HotfixDelegateAttribute");
                var hotfixAttributeType = xluaAssembly.MainModule.Types.Single(t => t.FullName == "XLua.HotfixAttribute");
                var toInject = (from module in injectAssembly.Modules from type in module.Types select type).ToList();
                foreach (var type in toInject)
                {
                    if (!hotfix.InjectType(hotfixAttributeType, type))
                    {
                        return;
                    }
                }
                Directory.CreateDirectory(Path.GetDirectoryName(idMapFilePath));
                hotfix.OutputIntKeyMapper(new FileStream(idMapFilePath, FileMode.Create, FileAccess.Write));
                File.Copy(idMapFilePath, idMapFilePath + "." + DateTime.Now.ToString("yyyyMMddHHmmssfff"));

                writeAssembly(injectAssembly, injectAssemblyPath);
                Info(injectAssemblyPath + " inject finish!");
            }
            catch(Exception e)
            {
                Error(injectAssemblyPath + " inject Exception! " + e);
            }
            finally
            {
                if (injectAssembly != null)
                {
                    Clean(injectAssembly);
                }
                if (xluaAssembly != null)
                {
                    Clean(xluaAssembly);
                }
            }
        }

        static void Info(string info)
        {
#if XLUA_GENERAL
            System.Console.WriteLine(info);
#else
            UnityEngine.Debug.Log(info);
#endif
        }

        static void Error(string info)
        {
#if XLUA_GENERAL
            System.Console.WriteLine("Error:" + info);
#else
            UnityEngine.Debug.LogError(info);
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
                string tmp = ccFlag + "__Hotfix" + i + "_" + fieldName;
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

        Dictionary<OpCode, OpCode> shortToLong = new Dictionary<OpCode, OpCode>();

        void fixBranch(ILProcessor processor, Mono.Collections.Generic.Collection<Instruction> instructions, Dictionary<Instruction, Instruction> originToNewTarget, HashSet<Instruction> noCheck)
        {
            foreach(var instruction in instructions)
            {
                Instruction target = instruction.Operand as Instruction;
                if (target != null && !noCheck.Contains(instruction))
                {
                    if (originToNewTarget.ContainsKey(target))
                    {
                        instruction.Operand = originToNewTarget[target];
                    }
                }
            }

            int offset = 0;
            for (int i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                instruction.Offset = offset;
                offset += instruction.GetSize();
            }

            for (int i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                Instruction target = instruction.Operand as Instruction;
                
                if (target != null)
                {
                    int diff = target.Offset - instruction.Offset;
                    if ((diff > sbyte.MaxValue || diff < sbyte.MinValue) && shortToLong.ContainsKey(instruction.OpCode))
                    {
                        instructions[i] = processor.Create(shortToLong[instruction.OpCode], target);
                    }
                }
            }
        }

        static MethodDefinition findOverride(TypeDefinition type, MethodReference vmethod)
        {
            foreach (var method in type.Methods)
            {
                if (method.Name == vmethod.Name && method.IsVirtual && !method.IsAbstract && (method.ReturnType.FullName == vmethod.ReturnType.FullName) && method.Parameters.Count == vmethod.Parameters.Count)
                {
                    bool isParamsMatch = true;
                    for (int i = 0; i < method.Parameters.Count; i++)
                    {
                        if (method.Parameters[i].Attributes != vmethod.Parameters[i].Attributes
                            || (method.Parameters[i].ParameterType.FullName != vmethod.Parameters[i].ParameterType.FullName))
                        {
                            isParamsMatch = false;
                            break;
                        }
                    }
                    if (isParamsMatch) return method;
                }
            }
            return null;
        }

        static MethodReference _findBase(TypeReference type, MethodDefinition method)
        {
            TypeDefinition td = type.Resolve();
            if (td == null)
            {
                return null;
            }
            //if (type.IsGenericInstance && 
            //    (method.Module.Assembly.FullName != type.Module.Assembly.FullName || method.Module.Assembly.FullName != td.Module.Assembly.FullName
            //    || method.Module.FullyQualifiedName != type.Module.FullyQualifiedName || method.Module.FullyQualifiedName != td.Module.FullyQualifiedName))
            //{
            //    return _findBase(td.BaseType, method);
            //}
            var m = findOverride(td, method);
            if (m != null)
            {
                if (type.IsGenericInstance)
                {
                    var reference = new MethodReference(m.Name, tryImport(method.DeclaringType, m.ReturnType), tryImport(method.DeclaringType, type))
                    {
                        HasThis = m.HasThis,
                        ExplicitThis = m.ExplicitThis,
                        CallingConvention = m.CallingConvention
                    };
                    foreach (var parameter in m.Parameters)
                        reference.Parameters.Add(new ParameterDefinition(tryImport(method.DeclaringType, parameter.ParameterType)));
                    foreach (var generic_parameter in m.GenericParameters)
                        reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));
                    return reference;
                }
                else
                {
                    return m;
                }
            }
            return _findBase(getBaseType(type), method);
        }

        static MethodReference findBase(TypeDefinition type, MethodDefinition method)
        {
            if (method.IsVirtual && !method.IsNewSlot) //表明override
            {
                try
                {
                    if (hasGenericParameter(method)) return null;
                    var b = _findBase(type.BaseType, method);
                    try
                    {
                        if (hasGenericParameterSkipDelaringType(b.Resolve())) return null;
                    }catch { }
                    return b;
                }
                catch { }
            }
            return null;
        }

        static TypeReference getBaseType(TypeReference typeReference)
        {
            var typeDefinition = typeReference.Resolve();
            var baseType = typeDefinition.BaseType;
            if (typeReference.IsGenericInstance && baseType.IsGenericInstance)
            {
                var genericType = typeReference as GenericInstanceType;
                var baseGenericType = baseType as GenericInstanceType;
                var genericInstanceType = new GenericInstanceType(tryImport(typeReference, baseGenericType.ElementType));
                foreach (var genericArgument in genericType.GenericArguments)
                {
                    genericInstanceType.GenericArguments.Add(genericArgument);
                }
                baseType = genericInstanceType;
            }
            return baseType;
        }

        const string BASE_RPOXY_PERFIX = "<>xLuaBaseProxy_";

        static TypeReference tryImport(TypeReference type, TypeReference toImport)
        {
            if (type.Module.Assembly.FullName == toImport.Module.Assembly.FullName
                && type.Module.FullyQualifiedName == toImport.Module.FullyQualifiedName)
            {
                return toImport;
            }
            else
            {
#if XLUA_GENERAL
                return type.Module.ImportReference(toImport);
#else
                return type.Module.Import(toImport);
#endif
            }
        }

        static MethodReference tryImport(TypeReference type, MethodReference toImport)
        {
            if (type.Module.Assembly.FullName == toImport.Module.Assembly.FullName
                && type.Module.FullyQualifiedName == toImport.Module.FullyQualifiedName)
            {
                return toImport;
            }
            else
            {
#if XLUA_GENERAL
                return type.Module.ImportReference(toImport);
#else
                return type.Module.Import(toImport);
#endif
            }
        }

        static MethodDefinition tryAddBaseProxy(TypeDefinition type, MethodDefinition method)
        {
            var mbase = findBase(type, method);
            if (mbase != null)
            {
                var proxyMethod = new MethodDefinition(BASE_RPOXY_PERFIX + method.Name, Mono.Cecil.MethodAttributes.Private, tryImport(type, method.ReturnType));
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    proxyMethod.Parameters.Add(new ParameterDefinition("P" + i, method.Parameters[i].IsOut ? Mono.Cecil.ParameterAttributes.Out : Mono.Cecil.ParameterAttributes.None, tryImport(type, method.Parameters[i].ParameterType)));
                }
                var instructions = proxyMethod.Body.Instructions;
                var processor = proxyMethod.Body.GetILProcessor();
                int paramCount = method.Parameters.Count + 1;
                for (int i = 0; i < paramCount; i++)
                {
                    if (i < ldargs.Length)
                    {
                        instructions.Add(processor.Create(ldargs[i]));
                    }
                    else if (i < 256)
                    {
                        instructions.Add(processor.Create(OpCodes.Ldarg_S, (byte)i));
                    }
                    else
                    {
                        instructions.Add(processor.Create(OpCodes.Ldarg, (short)i));
                    }
                    if (i == 0 && type.IsValueType)
                    {
                        instructions.Add(Instruction.Create(OpCodes.Ldobj, type));
                        instructions.Add(Instruction.Create(OpCodes.Box, type));
                    }
                }
                instructions.Add(Instruction.Create(OpCodes.Call, tryImport(type, mbase)));
                instructions.Add(Instruction.Create(OpCodes.Ret));
                return proxyMethod;
            }
            return null;
        }

        bool injectMethod(MethodDefinition method, HotfixFlagInTool hotfixType)
        {
            var type = method.DeclaringType;
            
            bool isFinalize = (method.Name == "Finalize" && method.IsSpecialName);

            MethodReference invoke = null;

            int param_count = method.Parameters.Count + (method.IsStatic ? 0 : 1);

            if (!findHotfixDelegate(method, out invoke, hotfixType))
            {
                Error("can not find delegate for " + method.DeclaringType + "." + method.Name + "! try re-genertate code.");
                return false;
            }

            if (invoke == null)
            {
                throw new Exception("unknow exception!");
            }

#if XLUA_GENERAL
            invoke = injectAssembly.MainModule.ImportReference(invoke);
#else
            invoke = injectAssembly.MainModule.Import(invoke);
#endif

            FieldReference fieldReference = null;
            VariableDefinition injection = null;
            bool isIntKey = hotfixType.HasFlag(HotfixFlagInTool.IntKey) && !type.HasGenericParameters && isTheSameAssembly;
            //isIntKey = !type.HasGenericParameters;

            if (!isIntKey)
            {
                injection = new VariableDefinition(invoke.DeclaringType);
                method.Body.Variables.Add(injection);

                var luaDelegateName = getDelegateName(method);
                if (luaDelegateName == null)
                {
                    Error("too many overload!");
                    return false;
                }

                FieldDefinition fieldDefinition = new FieldDefinition(luaDelegateName, Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.Private,
                    invoke.DeclaringType);
                type.Fields.Add(fieldDefinition);
                fieldReference = fieldDefinition.GetGeneric();
            }

            bool ignoreValueType = hotfixType.HasFlag(HotfixFlagInTool.ValueTypeBoxing);

            var insertPoint = method.Body.Instructions[0];
            var processor = method.Body.GetILProcessor();

            if (method.IsConstructor)
            {
                insertPoint = findNextRet(method.Body.Instructions, insertPoint);
            }

            Dictionary<Instruction, Instruction> originToNewTarget = new Dictionary<Instruction, Instruction>();
            HashSet<Instruction> noCheck = new HashSet<Instruction>();

            while (insertPoint != null)
            {
                Instruction firstInstruction;
                if (isIntKey)
                {
                    firstInstruction = processor.Create(OpCodes.Ldc_I4, bridgeIndexByKey.Count);
                    processor.InsertBefore(insertPoint, firstInstruction);
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Call, hotfixFlagGetter));
                }
                else
                {
                    firstInstruction = processor.Create(OpCodes.Ldsfld, fieldReference);
                    processor.InsertBefore(insertPoint, firstInstruction);
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stloc, injection));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
                }

                var jmpInstruction = processor.Create(OpCodes.Brfalse, insertPoint);
                processor.InsertBefore(insertPoint, jmpInstruction);

                if (isIntKey)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldc_I4, bridgeIndexByKey.Count));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Call, delegateBridgeGetter));
                }
                else
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
                }

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
                    if (i == 0 && !method.IsStatic && type.IsValueType)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldobj, type));
                        
                    }
                    if (ignoreValueType)
                    {
                        TypeReference paramType;
                        if (method.IsStatic)
                        {
                            paramType = method.Parameters[i].ParameterType;
                        }
                        else
                        {
                            paramType = (i == 0) ? type : method.Parameters[i - 1].ParameterType;
                        }
                        if (paramType.IsValueType)
                        {
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Box, paramType));
                        }
                    }
                }

                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Call, invoke));

                if (!method.IsConstructor && !isFinalize)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ret));
                }

                if (!method.IsConstructor)
                {
                    break;
                }
                else
                {
                    originToNewTarget[insertPoint] = firstInstruction;
                    noCheck.Add(jmpInstruction);
                }
                insertPoint = findNextRet(method.Body.Instructions, insertPoint);
            }

            if (method.IsConstructor)
            {
                fixBranch(processor, method.Body.Instructions, originToNewTarget, noCheck);
            }

            if (isFinalize)
            {
                if (method.Body.ExceptionHandlers.Count == 0)
                {
                    throw new InvalidProgramException("Finalize has not try-catch? Type :" + method.DeclaringType);
                }
                method.Body.ExceptionHandlers[0].TryStart = method.Body.Instructions[0];
            }
            if (isIntKey)
            {
                bridgeIndexByKey.Add(method);
            }
            return true;
        }

        bool injectGenericMethod(MethodDefinition method, HotfixFlagInTool hotfixType)
        {
            //如果注入的是xlua所在之外的Assembly的话，不支持该方式
            if (!isTheSameAssembly)
            {
                return true;
            }
            var type = method.DeclaringType;
            
            bool isFinalize = (method.Name == "Finalize" && method.IsSpecialName);
            bool isIntKey = hotfixType.HasFlag(HotfixFlagInTool.IntKey) && !type.HasGenericParameters;
            //isIntKey = !type.HasGenericParameters;

            FieldReference fieldReference = null;
            VariableDefinition injection = null;
            if (!isIntKey)
            {
                var luaDelegateName = getDelegateName(method);
                if (luaDelegateName == null)
                {
                    Error("too many overload!");
                    return false;
                }

                FieldDefinition fieldDefinition = new FieldDefinition(luaDelegateName, Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.Private,
                    delegateBridgeType);
                type.Fields.Add(fieldDefinition);

                fieldReference = fieldDefinition.GetGeneric();
            }

            injection = new VariableDefinition(delegateBridgeType);
            method.Body.Variables.Add(injection);

            int param_start = method.IsStatic ? 0 : 1;
            int param_count = method.Parameters.Count + param_start;
            var insertPoint = method.Body.Instructions[0];
            var processor = method.Body.GetILProcessor();

            if (method.IsConstructor)
            {
                insertPoint = findNextRet(method.Body.Instructions, insertPoint);
            }

            Dictionary<Instruction, Instruction> originToNewTarget = new Dictionary<Instruction, Instruction>();
            HashSet<Instruction> noCheck = new HashSet<Instruction>();

            while (insertPoint != null)
            {
                Instruction firstInstruction;
                Instruction jmpInstruction;
                if (isIntKey)
                {
                    firstInstruction = processor.Create(OpCodes.Ldc_I4, bridgeIndexByKey.Count);
                    processor.InsertBefore(insertPoint, firstInstruction);
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Call, hotfixFlagGetter));
                    jmpInstruction = processor.Create(OpCodes.Brfalse, insertPoint);
                    processor.InsertBefore(insertPoint, jmpInstruction);
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldc_I4, bridgeIndexByKey.Count));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Call, delegateBridgeGetter));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stloc, injection));
                }
                else
                {
                    firstInstruction = processor.Create(OpCodes.Ldsfld, fieldReference);
                    processor.InsertBefore(insertPoint, firstInstruction);
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stloc, injection));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
                    jmpInstruction = processor.Create(OpCodes.Brfalse, insertPoint);
                    processor.InsertBefore(insertPoint, jmpInstruction);
                }

                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));

                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, invokeSessionStart));

                TypeReference returnType = method.ReturnType;

                bool isVoid = returnType.FullName == "System.Void";

                int outCout = 0;

                for (int i = 0; i < param_count; i++)
                {
                    if (i == 0 && !method.IsStatic)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_0));
                        if (type.IsValueType)
                        {
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldobj, method.DeclaringType.GetGeneric()));
                        }
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, inParam.MakeGenericMethod(method.DeclaringType.GetGeneric())));
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
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));

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
                                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, inParams.MakeGenericMethod(((ArrayType)paramType).ElementType)));
                            }
                            else
                            {
                                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, inParam.MakeGenericMethod(paramType)));
                            }
                        }
                    }
                }

                int outStart = (isVoid ? 0 : 1);

                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldc_I4, outCout + outStart));
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, functionInvoke));

                int outPos = outStart;
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    if (method.Parameters[i].ParameterType.IsByReference)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
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
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, outParam.MakeGenericMethod(
                            ((ByReferenceType)method.Parameters[i].ParameterType).ElementType)));
                        outPos++;
                    }
                }

                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
                if (isVoid)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, invokeSessionEnd));
                }
                else
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, invokeSessionEndWithResult.MakeGenericMethod(returnType)));
                }

                if (!method.IsConstructor && !isFinalize)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ret));
                }

                if (!method.IsConstructor)
                {
                    break;
                }
                else
                {
                    originToNewTarget[insertPoint] = firstInstruction;
                    noCheck.Add(jmpInstruction);
                }

                insertPoint = findNextRet(method.Body.Instructions, insertPoint);
            }

            if (method.IsConstructor)
            {
                fixBranch(processor, method.Body.Instructions, originToNewTarget, noCheck);
            }

            if (isFinalize)
            {
                method.Body.ExceptionHandlers[0].TryStart = method.Body.Instructions[0];
            }

            if (isIntKey)
            {
                bridgeIndexByKey.Add(method);
            }

            return true;
        }

        public void OutputIntKeyMapper(Stream output)
        {
            using (StreamWriter writer = new StreamWriter(output))
            {
                writer.WriteLine("return {");
                var data = bridgeIndexByKey
                    .Select((md, idx) => new { Method = md, Index = idx})
                    .GroupBy(info => info.Method.DeclaringType)
                    .ToDictionary(group => group.Key, group =>
                    {
                        return group.GroupBy(info => info.Method.Name).ToDictionary(group_by_name => group_by_name.Key, group_by_name => group_by_name.Select(info => info.Index.ToString()).ToArray());
                    });
                foreach(var kv in data)
                {
                    writer.WriteLine("    [\"" + kv.Key.FullName.Replace('/', '+') + "\"] = {");
                    foreach(var kv2 in kv.Value)
                    {
                        writer.WriteLine("        [\"" + kv2.Key + "\"] = {");
                        writer.WriteLine("            " + string.Join(",", kv2.Value));
                        writer.WriteLine("        },");
                    }
                    writer.WriteLine("    },");
                }
                writer.WriteLine("}");
            }
        }
    }
}
#else

namespace XLua
{
#if UNITY_2019_1_OR_NEWER
    class MyCustomBuildProcessor : IPostBuildPlayerScriptDLLs
    {
        public int callbackOrder { get { return 0; } }
        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            var dir = Path.GetDirectoryName(report.files.Single(file => file.path.EndsWith("Assembly-CSharp.dll")).path);
            Hotfix.HotfixInject(dir);
        }
    }
#endif

    public static class Hotfix
    {
        static bool ContainNotAsciiChar(string s)
        {
            for (int i = 0; i < s.Length; ++i)
            {
                if (s[i] > 127)
                {
                    return true;
                }
            }
            return false;
        }

#if !UNITY_2019_1_OR_NEWER
        [PostProcessScene]
#endif
        [MenuItem("XLua/Hotfix Inject In Editor", false, 3)]
        public static void HotfixInject()
        {
            HotfixInject("./Library/ScriptAssemblies");
        }

        public static void HotfixInject(string assemblyDir)
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (EditorApplication.isCompiling)
            {
                UnityEngine.Debug.LogError("You can't inject before the compilation is done");
                return;
            }

#if UNITY_EDITOR_OSX
			var mono_path = Path.Combine(Path.GetDirectoryName(typeof(UnityEngine.Debug).Module.FullyQualifiedName),
				"../MonoBleedingEdge/bin/mono");
			if(!File.Exists(mono_path))
			{
				mono_path = Path.Combine(Path.GetDirectoryName(typeof(UnityEngine.Debug).Module.FullyQualifiedName),
					"../../MonoBleedingEdge/bin/mono");
			}
			if(!File.Exists(mono_path))
			{
				UnityEngine.Debug.LogError("can not find mono!");
			}
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

            var assembly_csharp_path = Path.Combine(assemblyDir, "Assembly-CSharp.dll");
            var id_map_file_path = CSObjectWrapEditor.GeneratorConfig.common_path + "Resources/hotfix_id_map.lua.txt";
            var hotfix_cfg_in_editor = CSObjectWrapEditor.GeneratorConfig.common_path + "hotfix_cfg_in_editor.data";

            Dictionary<string, int> editor_cfg = new Dictionary<string, int>();
            Assembly editor_assembly = typeof(Hotfix).Assembly;
            HotfixConfig.GetConfig(editor_cfg, Utils.GetAllTypes().Where(t => t.Assembly == editor_assembly));

            if (!Directory.Exists(CSObjectWrapEditor.GeneratorConfig.common_path))
            {
                Directory.CreateDirectory(CSObjectWrapEditor.GeneratorConfig.common_path);
            }

            using (BinaryWriter writer = new BinaryWriter(new FileStream(hotfix_cfg_in_editor, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(editor_cfg.Count);
                foreach (var kv in editor_cfg)
                {
                    writer.Write(kv.Key);
                    writer.Write(kv.Value);
                }
            }

#if UNITY_2019_1_OR_NEWER
            List<string> args = new List<string>() { assembly_csharp_path, assembly_csharp_path, id_map_file_path, hotfix_cfg_in_editor };
#else
            List<string> args = new List<string>() { assembly_csharp_path, typeof(LuaEnv).Module.FullyQualifiedName, id_map_file_path, hotfix_cfg_in_editor };
#endif

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
            var injectAssemblyPaths = HotfixConfig.GetHotfixAssemblyPaths();
            var idMapFileNames = new List<string>();
            foreach (var injectAssemblyPath in injectAssemblyPaths)
            {
                args[0] = Path.Combine(assemblyDir, Path.GetFileName(injectAssemblyPath));
                if (ContainNotAsciiChar(args[0]))
                {
                    throw new Exception("project path must contain only ascii characters");
                }

                if (injectAssemblyPaths.Count > 1)
                {
                    var injectAssemblyFileName = Path.GetFileName(injectAssemblyPath);
                    args[2] = CSObjectWrapEditor.GeneratorConfig.common_path + "Resources/hotfix_id_map_" + injectAssemblyFileName.Substring(0, injectAssemblyFileName.Length - 4) + ".lua.txt";
                    idMapFileNames.Add(args[2]);
                }
                Process hotfix_injection = new Process();
                hotfix_injection.StartInfo.FileName = mono_path;
#if UNITY_5_6_OR_NEWER
                hotfix_injection.StartInfo.Arguments = "--runtime=v4.0.30319 " + inject_tool_path + " \"" + String.Join("\" \"", args.ToArray()) + "\"";
#else
                hotfix_injection.StartInfo.Arguments = inject_tool_path + " \"" + String.Join("\" \"", args.ToArray()) + "\"";
#endif
                hotfix_injection.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                hotfix_injection.StartInfo.RedirectStandardOutput = true;
                hotfix_injection.StartInfo.UseShellExecute = false;
                hotfix_injection.StartInfo.CreateNoWindow = true;
                hotfix_injection.Start();
                UnityEngine.Debug.Log(hotfix_injection.StandardOutput.ReadToEnd());
                hotfix_injection.WaitForExit();
            }

            File.Delete(hotfix_cfg_in_editor);
            AssetDatabase.Refresh();
        }
    }
}
#endif
#endif
