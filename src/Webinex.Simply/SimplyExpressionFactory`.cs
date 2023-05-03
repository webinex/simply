using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Webinex.Simply;

internal static class SimplyExpressionFactory
{
    public static Expression<Func<TEntity, TKey>> KeySelector<TEntity, TKey>(DbContext dbContext)
    {
        var parameter = Expression.Parameter(typeof(TEntity));
        var propertyAccessExpression = Expression.Property(parameter, GetKey<TEntity>(dbContext));

        return Expression.Lambda<Func<TEntity, TKey>>(propertyAccessExpression, parameter);
    }

    public static Expression<Func<TEntity, bool>> KeyIn<TEntity, TKey>(DbContext dbContext, IEnumerable<TKey> keys)
    {
        var parameter = Expression.Parameter(typeof(TEntity));
        var propertyAccessExpression = Expression.Property(parameter, GetKey<TEntity>(dbContext));

        var containsMethodInfo =
            typeof(Enumerable)
                .GetMethods()
                .Where(x => x.Name == nameof(Enumerable.Contains) && x.IsPublic && x.IsStatic)
                .Single(x => x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));

        var castMethodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(typeof(TKey));
        var typedValues = castMethodInfo.Invoke(null, new object[] { keys.ToArray() });

        return Expression.Lambda<Func<TEntity, bool>>(
            Expression.Call(
                containsMethodInfo,
                Expression.Constant(typedValues, typeof(IEnumerable<>).MakeGenericType(typeof(TKey))),
                propertyAccessExpression
            ),
            parameter);
    }

    private static PropertyInfo GetKey<TEntity>(DbContext dbContext)
    {
        var model = dbContext.Model.FindEntityType(typeof(TEntity));
        model = model ?? throw new InvalidOperationException($"{typeof(TEntity).Name} not registered in db context");

        var keys = model.GetKeys().ToArray();
        if (keys.Length > 1)
            throw new InvalidOperationException("Multiple key entities is not supported yet");

        var key = keys.Single();
        var properties = key.Properties;
        
        if (properties.Count > 1)
            throw new InvalidOperationException("Multiple key properties is not supported yet");

        var property = properties.Single();
        return property.PropertyInfo ??
               throw new InvalidOperationException("Key without property is not supported yet");
    }
}