using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XLua;

/// <summary>
/// xLua 默认配置
/// </summary>
static class XLuaUnityDefaultConfig
{

#if UNITY_2022_1_OR_NEWER
    static bool IsSpanType(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var genericDefinition = type.GetGenericTypeDefinition();

        return
            genericDefinition == typeof(Span<>) ||
            genericDefinition == typeof(ReadOnlySpan<>);
    }

    static bool IsSpanMember(MemberInfo memberInfo)
    {
        switch (memberInfo)
        {
            case FieldInfo fieldInfo:
                return IsSpanType(fieldInfo.FieldType);

            case PropertyInfo propertyInfo:
                return IsSpanType(propertyInfo.PropertyType);

            case ConstructorInfo constructorInfo:
                return constructorInfo.GetParameters().Any(p => IsSpanType(p.ParameterType));

            case MethodInfo methodInfo:
                return methodInfo.GetParameters().Any(p => IsSpanType(p.ParameterType)) || IsSpanType(methodInfo.ReturnType);

            default:
                return false;
        }
    }

    [BlackList]
    public static Func<MemberInfo, bool> SpanMembersFilter = IsSpanMember;

#endif
}