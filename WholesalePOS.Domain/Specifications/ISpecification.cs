using System.Linq.Expressions;

namespace WholesalePOS.Domain.Specifications;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
}