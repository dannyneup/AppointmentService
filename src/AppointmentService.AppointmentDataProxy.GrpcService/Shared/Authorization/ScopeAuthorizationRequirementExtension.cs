using Microsoft.AspNetCore.Authorization;

namespace AppointmentService.AppointmentDataProxy.GrpcService.Shared.Authorization;

public static class ScopeAuthorizationRequirementExtensions
{
    public static AuthorizationPolicyBuilder RequireScope(
        this AuthorizationPolicyBuilder authorizationPolicyBuilder,
        params string[] requiredScopes)
    {
        authorizationPolicyBuilder.AddRequirements(new ScopeAuthorizationRequirement(requiredScopes));
        return authorizationPolicyBuilder;
    }
}