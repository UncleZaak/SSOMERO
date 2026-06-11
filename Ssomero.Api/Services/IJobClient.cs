using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ssomero.Api.Services;

public interface IJobClient
{
    // Enqueue a background job that invokes the given async method on the target service type.
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);
}
