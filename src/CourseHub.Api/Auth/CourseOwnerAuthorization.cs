using CourseHub.Core.Domain;
using Microsoft.AspNetCore.Authorization;

namespace CourseHub.Api.Auth;

public sealed class CourseOwnerRequirement : IAuthorizationRequirement;

public sealed class CourseOwnerHandler : AuthorizationHandler<CourseOwnerRequirement, Course>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CourseOwnerRequirement requirement,
        Course course)
    {
        var sub = CallerId.From(context.User);
        if (!string.IsNullOrEmpty(sub) && sub == course.InstructorId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
