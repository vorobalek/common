using System;

namespace Common.Infrastructure;

public static class ExecutionEnvironment
{
    private static bool? _isTesting;
    private static bool? _isEntityFrameworkTools;
    private static bool? _isInMemoryDatabase;

    public static bool IsApplication => !IsTesting && !IsEntityFrameworkTools;
    public static bool IsTesting => _isTesting.GetValueOrDefault();

    public static bool IsEntityFrameworkTools => _isEntityFrameworkTools ??=
        Environment.StackTrace.Contains("Microsoft.EntityFrameworkCore.Design");

    public static bool IsInMemoryDatabase => _isInMemoryDatabase.GetValueOrDefault();

    public static void SetTesting(bool testing)
    {
        _isTesting = testing;
    }

    public static void SetInMemoryDatabase(bool isInMemoryDatabase)
    {
        _isInMemoryDatabase = isInMemoryDatabase;
    }
}