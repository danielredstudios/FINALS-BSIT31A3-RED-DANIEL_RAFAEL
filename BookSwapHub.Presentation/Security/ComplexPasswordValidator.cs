using System.Text.RegularExpressions;
using BookSwapHub.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

namespace BookSwapHub.Presentation.Security;

public class ComplexPasswordValidator : IPasswordValidator<ApplicationUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Password is required." }));
        }

        int upper = password.Count(char.IsUpper);
        int digits = password.Count(char.IsDigit);
        int symbols = password.Count(c => char.IsSymbol(c) || char.IsPunctuation(c));

        var errors = new List<IdentityError>();
        if (upper < 2) errors.Add(new IdentityError { Description = "Password must contain at least 2 uppercase letters." });
        if (digits < 3) errors.Add(new IdentityError { Description = "Password must contain at least 3 numbers." });
        if (symbols < 3) errors.Add(new IdentityError { Description = "Password must contain at least 3 symbols." });

        return Task.FromResult(errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed([.. errors]));
    }
}
