using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Common.Database.Infrastructure.Extensions;

public static class DbFunctions
{
    private static TOut Assert<TIn, TOut>(string name, Func<TIn, TOut>? alternateFunc, TIn input)
    {
        if (ExecutionEnvironment.IsInMemoryDatabase && alternateFunc != null) return alternateFunc(input);
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(name));
    }

    public static int? DatePart(string datePartArg, DateTime? date)
    {
        return Assert<DateTime?, int?>(nameof(DatePart), null, default);
    }

    public static int? DatePart(string datePartArg, DateTime date)
    {
        return Assert<DateTime, int?>(nameof(DatePart), null, default);
    }

    public static int? DatePart(string datePartArg, DateTimeOffset? date)
    {
        return Assert<DateTimeOffset?, int?>(nameof(DatePart), null, default);
    }

    public static int? DatePart(string datePartArg, DateTimeOffset date)
    {
        return Assert<DateTimeOffset, int?>(nameof(DatePart), null, default);
    }

    /// <summary>
    ///     Use only in ef queries. Make sure than @@DATEFIRST in your database is 7
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DayOfWeek? DayOfWeekDb(this DateTime date)
    {
        return Assert<DateTime, DayOfWeek?>(nameof(DayOfWeekDb), e => e.DayOfWeek, date);
    }

    /// <summary>
    ///     Use only in ef queries. Make sure than @@DATEFIRST in your database is 7
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DayOfWeek? DayOfWeekDb(this DateTime? date)
    {
        return Assert(nameof(DayOfWeekDb), e => e?.DayOfWeek, date);
    }

    /// <summary>
    ///     Use only in ef queries. Make sure than @@DATEFIRST in your database is 7
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DayOfWeek? DayOfWeekDb(this DateTimeOffset date)
    {
        return Assert<DateTimeOffset, DayOfWeek?>(nameof(DayOfWeekDb), e => e.DayOfWeek, date);
    }

    /// <summary>
    ///     Use only in ef queries. Make sure than @@DATEFIRST in your database is 7
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DayOfWeek? DayOfWeekDb(this DateTimeOffset? date)
    {
        return Assert(nameof(DayOfWeekDb), e => e?.DayOfWeek, date);
    }

    public static void AddDbFunctions<TContext>(this TContext dbContext, ModelBuilder modelBuilder)
        where TContext : DbContext, ICommonDbContext<TContext>
    {
        foreach (
            var methodInfo in new[]
            {
                typeof(DbFunctions).GetRuntimeMethod(nameof(DatePart), new[] { typeof(string), typeof(DateTime?) })!,
                typeof(DbFunctions).GetRuntimeMethod(nameof(DatePart), new[] { typeof(string), typeof(DateTime) })!,
                typeof(DbFunctions).GetRuntimeMethod(nameof(DatePart),
                    new[] { typeof(string), typeof(DateTimeOffset?) })!,
                typeof(DbFunctions).GetRuntimeMethod(nameof(DatePart),
                    new[] { typeof(string), typeof(DateTimeOffset) })!
            })
            modelBuilder
                .HasDbFunction(methodInfo)
                .HasTranslation(args =>
                    new SqlFunctionExpression(nameof(DatePart),
                        new[]
                        {
                            new SqlFragmentExpression((args.ToArray()[0] as SqlConstantExpression)!.Value!.ToString()!),
                            args.ToArray()[1]
                        },
                        true,
                        new[] { false, false },
                        typeof(int?),
                        null
                    )
                );

        foreach (
            var methodInfo in new[]
            {
                typeof(DbFunctions).GetRuntimeMethod(nameof(DayOfWeekDb), new[] { typeof(DateTime?) })!,
                typeof(DbFunctions).GetRuntimeMethod(nameof(DayOfWeekDb), new[] { typeof(DateTime) })!,
                typeof(DbFunctions).GetRuntimeMethod(nameof(DayOfWeekDb), new[] { typeof(DateTimeOffset?) })!,
                typeof(DbFunctions).GetRuntimeMethod(nameof(DayOfWeekDb), new[] { typeof(DateTimeOffset) })!
            })
            modelBuilder
                .HasDbFunction(methodInfo)
                .HasTranslation(args => new SqlBinaryExpression(
                    ExpressionType.Subtract,
                    new SqlFunctionExpression(nameof(DatePart),
                        new[]
                        {
                            new SqlFragmentExpression("weekday"),
                            args.ToArray()[0]
                        },
                        true,
                        new[] { false, false },
                        typeof(int?),
                        null
                    ),
                    new SqlConstantExpression(
                        Expression.Constant(1),
                        null),
                    typeof(DayOfWeek?),
                    null)
                );
    }
}