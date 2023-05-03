using System.Linq.Expressions;
using Webinex.Asky;

namespace Webinex.Simply.Example;

public class UserFieldMap : IAskyFieldMap<User>
{
    public Expression<Func<User, object>>? this[string fieldId] => fieldId switch
    {
        "name" => x => x.Name,
        _ => throw new ArgumentOutOfRangeException(nameof(fieldId), fieldId, null)
    };
}