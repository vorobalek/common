using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Infrastructure.Extensions;

public static class TypeExtensions
{
    private static readonly Dictionary<Type, dynamic> CommonTypeDictionary = new()
    {
        { typeof(int), default(int) },
        { typeof(uint), default(uint) },
        { typeof(Guid), default(Guid) },
        { typeof(DateTime), default(DateTime) },
        { typeof(DateTimeOffset), default(DateTimeOffset) },
        { typeof(long), default(long) },
        { typeof(ulong), default(ulong) },
        { typeof(bool), default(bool) },
        { typeof(decimal), default(decimal) },
        { typeof(double), default(double) },
        { typeof(float), default(float) },
        { typeof(short), default(short) },
        { typeof(ushort), default(ushort) },
        { typeof(byte), default(byte) },
        { typeof(sbyte), default(sbyte) },
        { typeof(char), default(char) }
    };

    public static string GetName(this Type type)
    {
        if (!type.IsGenericType) return type.Name;
        var genericTypesNames = type.GenericTypeArguments.Select(item => item.GetName());
        return $"{type.Name.Split('`')[0]}<{string.Join(", ", genericTypesNames)}>";
    }

    public static string GetFullName(this Type type)
    {
        if (!type.IsGenericType) return type.FullName ?? type.Name;
        var genericTypesNames = type.GenericTypeArguments.Select(item => item.GetFullName());
        return $"{type.Name.Split('`')[0]}<{string.Join(", ", genericTypesNames)}>";
    }

    public static void AssertIsAssignableTo(this Type sourceType, Type destinationType)
    {
        if (sourceType == null)
            throw new ArgumentNullException(nameof(sourceType));

        if (destinationType == null)
            throw new ArgumentNullException(nameof(destinationType));

        if (!sourceType.IsAssignableTo(destinationType))
            throw new ArgumentException(
                $"The type {sourceType.FullName} is not assignable to {destinationType.FullName}");
    }

    public static Type[] GetGenericTypeArgumentsOfSpecificParent(this Type? targetType, Type specificGeneric)
    {
        while (targetType != null && targetType != typeof(object))
        {
            if (targetType.IsGenericType && specificGeneric == targetType.GetGenericTypeDefinition())
                return targetType.GetGenericArguments();

            targetType = targetType.BaseType;
        }

        return Array.Empty<Type>();
    }

    public static bool HasInterface(this Type targetType, Type interfaceType)
    {
        if (!interfaceType.IsInterface)
            throw new ArgumentException($"The {nameof(interfaceType)} must be an interface");

        return targetType
            .GetInterfaces()
            .Select(i => i.IsGenericType ? i.GetGenericTypeDefinition() : i)
            .Any(i =>
                i == interfaceType ||
                (i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType) ||
                (i.IsGenericType && interfaceType.IsGenericType &&
                 i.GetGenericTypeDefinition() == interfaceType.GetGenericTypeDefinition()));
    }

    public static dynamic? GetDefaultValue(this Type type)
    {
        if (!type.IsValueType) return null;

        // A bit of perf code to avoid calling Activator.CreateInstance for common types and
        // to avoid boxing on every call. This is about 50% faster than just calling CreateInstance
        // for all value types.
        return CommonTypeDictionary.TryGetValue(type, out var value)
            ? value
            : Activator.CreateInstance(type);
    }
}