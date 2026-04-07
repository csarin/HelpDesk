using Helpdesk.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HelpDesk.Web.Authorization
{
    public sealed class TicketAttachmentAccessHandler : AuthorizationHandler<TicketAttachmentAccessRequirement, Ticket>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TicketAttachmentAccessRequirement requirement,
            Ticket resource)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return Task.CompletedTask;
            }

            if (context.User.IsInRole(AppRoles.Admin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Task.CompletedTask;
            }

            if (string.Equals(userId, resource.CreatedByUserId, StringComparison.Ordinal)
                || string.Equals(userId, resource.AssignedToUserId, StringComparison.Ordinal))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
