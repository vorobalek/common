using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Common.Infrastructure.Extensions;

public static class TypeExtensions
{
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

    private static readonly Dictionary<Type, (dynamic Min, dynamic Max)> CommonTypeMinMaxValueDictionary = new()
    {
        { typeof(int), (int.MinValue, int.MaxValue) },
        { typeof(uint), (uint.MinValue, uint.MaxValue) },
        { typeof(Guid), (Guid.Empty, Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")) },
        { typeof(DateTime), (DateTime.MinValue, DateTime.MaxValue) },
        { typeof(DateTimeOffset), (DateTimeOffset.MinValue, DateTimeOffset.MaxValue) },
        { typeof(long), (long.MinValue, long.MaxValue) },
        { typeof(ulong), (ulong.MinValue, ulong.MaxValue) },
        { typeof(bool), (false, true) },
        { typeof(decimal), (decimal.MinValue, decimal.MaxValue) },
        { typeof(double), (double.MinValue, double.MaxValue) },
        { typeof(float), (float.MinValue, float.MaxValue) },
        { typeof(short), (short.MinValue, short.MaxValue) },
        { typeof(ushort), (ushort.MinValue, ushort.MaxValue) },
        { typeof(byte), (byte.MinValue, byte.MaxValue) },
        { typeof(sbyte), (sbyte.MinValue, sbyte.MaxValue) },
        { typeof(char), (char.MinValue, char.MaxValue) }
    };

    public static dynamic? MinValue(this Type type)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlyingType) return underlyingType.MinValue();

        return CommonTypeMinMaxValueDictionary.TryGetValue(type, out var value)
            ? value.Min
            : null;
    }

    public static dynamic? MaxValue(this Type type)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlyingType) return underlyingType.MaxValue();

        return CommonTypeMinMaxValueDictionary.TryGetValue(type, out var value)
            ? value.Max
            : null;
    }

    public static decimal? MaxValueForJson(this decimal? value)
    {
        if (!value.HasValue) return null;

        return Math.Min(9999999999999998m, value.Value);
    }

    public static decimal? MinValueForJson(this decimal? value)
    {
        if (!value.HasValue) return null;

        return Math.Max(-9999999999999998m, value.Value);
    }

    public static bool IsNullable(this Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }

    public static bool CanBeSet(this MemberInfo propertyOrField)
    {
        return propertyOrField is FieldInfo field
            ? !field.IsInitOnly
            : ((PropertyInfo)propertyOrField).CanWrite;
    }
}