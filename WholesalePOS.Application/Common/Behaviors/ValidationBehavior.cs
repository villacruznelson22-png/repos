using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalePOS.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));


            //flatten out the errors into a flat list
            var failures = validationResults.SelectMany(r => r.Errors)
                            .Where(f => f != null)
                            .ToList();

            //If there is an error
            if (failures.Any())
            {
                //The pipeline stops here, Handler never executes
                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
