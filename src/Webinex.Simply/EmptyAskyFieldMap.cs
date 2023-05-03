using System.Linq.Expressions;
using Webinex.Asky;

namespace Webinex.Simply;

internal class EmptyAskyFieldMap<TEntity> : IAskyFieldMap<TEntity>
{
    public Expression<Func<TEntity, object>> this[string fieldId] => null!;
}