using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;

namespace Ssomero.Api.Services;

public class HangfireJobClient : IJobClient
{
    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }
}
