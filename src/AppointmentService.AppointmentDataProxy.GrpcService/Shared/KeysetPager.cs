using System.Collections.ObjectModel;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared;

internal static class KeysetPager
    {
        public static async IAsyncEnumerable<TOut> StreamAsync<TRow, TKey, TOut>(
            Func<CancellationToken, Task<TKey?>> fetchCutoffAsync,
            Func<TKey?, TKey, int, CancellationToken, Task<ReadOnlyCollection<TRow>>> fetchPageAsync,
            Func<TRow, TKey> keySelector,
            Func<TRow, TOut> map,
            int batchSize,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var cutoff = await fetchCutoffAsync(cancellationToken);
            if (cutoff is null)
                yield break;

            TKey? after = default;
            while (true)
            {
                var rows = await fetchPageAsync(after, cutoff, batchSize, cancellationToken);
                if (rows.Count == 0)
                    yield break;

                foreach (var row in rows)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return map(row);
                }

                if (rows.Count < batchSize) yield break;

                after = keySelector(rows[^1]);
            }
        }
    }