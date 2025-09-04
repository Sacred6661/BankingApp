using System.Linq.Expressions;

namespace ProfileService.Helpers
{
    public static class PaginationHelper
    {
        public static IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortBy, string sortDirection)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.PropertyOrField(parameter, sortBy);
            var lambda = Expression.Lambda(property, parameter);

            string methodName = sortDirection.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";

            var method = typeof(Queryable).GetMethods().First(m => m.Name == methodName && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), property.Type);

            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, lambda })!;
        }
    }
}
