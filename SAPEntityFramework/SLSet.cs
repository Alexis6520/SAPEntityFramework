using SAPEntityFramework.Extensions.Http;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace SAPEntityFramework
{
    /// <summary>
    /// Proporciona acceso a un recurso de Service Layer
    /// </summary>
    /// <typeparam name="T">Tipo en el que se basa el recurso</typeparam>
    public class SLSet<T> : IQueryable<T>
    {
        private readonly SLContext _slContext;
        private readonly string _path;

        public SLSet(SLContext slContext, string path)
        {
            _slContext = slContext;
            _path = path;
            Expression = Expression.Constant(this);
            Provider = new SLQueryProvider(_slContext, path);
        }

        public SLSet(SLQueryProvider queryProvider, Expression predicate)
        {
            _slContext = queryProvider.Context;
            _path = queryProvider.Path;
            Expression = predicate;
            Provider = queryProvider;
        }

        public Type ElementType => typeof(T);

        public Expression Expression { get; internal set; }

        public IQueryProvider Provider { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
