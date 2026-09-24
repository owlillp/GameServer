using AuthService.Contracts.Responses;
using AuthService.Domain;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Shared.Core.Abstractions;
using Shared.Core.Database;
using Shared.Core.Validation;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Auth.Commands.JwtRegister;

public sealed class JwtRegisterHandler(
    IValidator<JwtRegisterCommand> validator,
    ITransactionManager transactionManager,
    UserManager<Account> userManager)
    : ICommandHandler<RegisterResponse, JwtRegisterCommand>
{
    public async Task<Result<RegisterResponse, Error>> Handle(
        JwtRegisterCommand command,
        CancellationToken cancellationToken = new ())
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var transactionResult = await transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error;
        }

        var account = new Account(command.Request.Email, command.Request.UserName);

        var result = await userManager.CreateAsync(account, command.Request.Password);
        if (!result.Succeeded)
        {
            return MapIdentityErrors(result);
        }

        var commitResult = await transactionManager.CommitTransactionAsync(cancellationToken);
        if (commitResult.IsFailure)
        {
            return commitResult.Error;
        }

        return new RegisterResponse(account.Id);
    }

    private static Error MapIdentityErrors(IdentityResult result)
    {
        var messages = result.Errors
            .Select(e => new ErrorMessage(e.Code, e.Description))
            .ToArray();

        bool isDuplicate = result.Errors.Any(e =>
            e.Code.StartsWith("Duplicate", StringComparison.Ordinal));

        return isDuplicate
            ? Error.Conflict(messages)
            : Error.Validation(messages);
    }
}