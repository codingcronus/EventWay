using Microsoft.Azure.Documents;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace EventWay.Infrastructure.CosmosDb
{
    /// <summary>
    /// Ref: https://blogs.msdn.microsoft.com/bigdatasupport/2015/09/02/dealing-with-requestratetoolarge-errors-in-azure-documentdb-and-testing-performance/
    /// </summary>
    internal class DocumentDbRetryPolicy
    {
        const int RetriesMax = 3;
        public static async Task<V> ExecuteWithRetries<V>(Func<Task<V>> function)
        {
            TimeSpan sleepTime = TimeSpan.Zero;

            var retries = 0;
            while (true)
            {
                retries++;
                try
                {
                    return await function();
                }
                catch (DocumentClientException de)
                {
                    if ((int)de.StatusCode != 429 || retries > RetriesMax)
                    {
                        throw;
                    }
                    sleepTime = de.RetryAfter;
                }
                catch (AggregateException ae)
                {
                    if (!(ae.InnerException is DocumentClientException))
                    {
                        throw;
                    }

                    var de = (DocumentClientException)ae.InnerException;
                    if ((int)de.StatusCode != 429 || retries > RetriesMax)
                    {
                        throw;
                    }
                    sleepTime = de.RetryAfter;
                }

                await Task.Delay(sleepTime);
            }
        }
    }
}
