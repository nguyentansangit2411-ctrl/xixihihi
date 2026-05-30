using System.Collections.Generic;
using System.Threading;

namespace Xixihihi.Application.Interfaces;

public record EmailJob(string To, string Subject, string Body);

public interface IEmailQueue
{
    void Enqueue(EmailJob job);
    IAsyncEnumerable<EmailJob> DequeueAllAsync(CancellationToken cancellationToken);
}
