using Microsoft.AspNetCore.Authorization;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.Authorization;

public class ScopeAuthorizationRequirement : AuthorizationHandler<ScopeAuthorizationRequirement>, IAuthorizationRequirement
{
    private IEnumerable<string> RequiredScopes { get; }

    public ScopeAuthorizationRequirement(IEnumerable<string> requiredScopes)
    {
        var enumeratedRequiredScopes = requiredScopes as string[] ?? requiredScopes.ToArray();
        if (enumeratedRequiredScopes.Length == 0)
        {
            throw new ArgumentException($"{nameof(requiredScopes)} must contain at least one value.", nameof(requiredScopes));
        }

        RequiredScopes = enumeratedRequiredScopes;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeAuthorizationRequirement requirement)
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        var scopeClaim = context.User?.Claims.FirstOrDefault(
            claim => string.Equals(claim.Type, "scope", StringComparison.OrdinalIgnoreCase)
            );

        if (scopeClaim == null)
            return Task.CompletedTask;
        var scopes = scopeClaim.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (requirement.RequiredScopes.All(requiredScope => scopes.Contains(requiredScope, StringComparer.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}