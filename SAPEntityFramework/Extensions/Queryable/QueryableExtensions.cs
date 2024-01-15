﻿namespace SAPEntityFramework.Extensions.Queryable
{
    public static class QueryableExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken = default)
        {
            if (source.Provider is SLQueryProvider slProvider)
            {
                return await slProvider.ExecuteToListAsync<T>(source.Expression, cancellationToken);
            }

            throw new NotSupportedException("Metodo no soportado para este tipo de proveedor");
        }

        public static async Task<T> FirsttOrDefaultAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken = default)
        {
            if (source.Provider is SLQueryProvider slProvider)
            {
                return await slProvider.ExecuteFirstOrDefaultAsync<T>(source.Expression, cancellationToken);
            }

            throw new NotSupportedException("Metodo no soportado para este tipo de proveedor");
        }
    }
}
