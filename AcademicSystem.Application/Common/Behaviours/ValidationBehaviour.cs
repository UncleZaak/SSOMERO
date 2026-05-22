using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using MediatR;

namespace AcademicSystem.Application.Common.Behaviours
{
    /// <summary>
    /// MediatR pipeline behaviour that runs FluentValidation validators for a request.
    /// If validation errors exist, throws a ValidationException from FluentValidation.
    /// </summary>
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var failures = new List<FluentValidation.Results.ValidationFailure>();

            foreach (var validator in _validators)
            {
                var result = await validator.ValidateAsync(context, cancellationToken);
                if (result != null && result.Errors != null && result.Errors.Any())
                {
                    failures.AddRange(result.Errors);
                }
            }

            if (failures.Any())
            {
                throw new FluentValidation.ValidationException(failures);
            }

            return await next();
        }
    }
}
