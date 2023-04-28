using System.Reflection;

namespace Webinex.Simply.AspNetCore;

internal static class ParameterUtil
{
    private const int NOT_NULLABLE_VALUE = 1;

    public static bool IsNullable(ParameterInfo param)
    {
        if (IsNullableReferenceTypesDisabled(param))
            return true;

        var nullability = GetParamNullableAttributeValue(param) ?? GetDeclaringTypeNullabilityContext(param);
        return nullability != NOT_NULLABLE_VALUE;
    }

    private static bool IsNullableReferenceTypesDisabled(ParameterInfo param)
    {
        return GetDeclaringTypeNullableContextAttribute(param) == null;
    }

    private static int? GetDeclaringTypeNullabilityContext(ParameterInfo param)
    {
        var attribute = GetDeclaringTypeNullableContextAttribute(param);
        return attribute != null ? GetNullableAttributeValue(attribute) : null;
    }

    private static CustomAttributeData? GetDeclaringTypeNullableContextAttribute(ParameterInfo param)
    {
        var declaringType = param.Member.DeclaringType!;
        return declaringType.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "NullableContextAttribute");
    }

    private static int? GetParamNullableAttributeValue(ParameterInfo param)
    {
        var attribute = GetParamNullableAttribute(param);
        return attribute != null ? GetNullableAttributeValue(attribute) : null;
    }

    private static CustomAttributeData? GetParamNullableAttribute(ParameterInfo param)
    {
        var attributes = param.CustomAttributes.ToArray();
        return attributes.FirstOrDefault(x => x.AttributeType.Name == "NullableAttribute");
    }

    private static int GetNullableAttributeValue(CustomAttributeData attribute)
    {
        return (byte)attribute.ConstructorArguments.First(x => x.ArgumentType == typeof(byte)).Value!;
    }
}