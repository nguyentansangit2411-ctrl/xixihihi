using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using Xixihihi.Application.Interfaces;

namespace Xixihihi.Infrastructure.Services;

public class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailJob> _channel = Channel.CreateUnbounded<EmailJob>();

    public void Enqueue(EmailJob job) => _channel.Writer.TryWrite(job);

    public async IAsyncEnumerable<EmailJob> DequeueAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(cancellationToken))
            yield return job;
    }
}
