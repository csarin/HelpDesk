using Microsoft.AspNetCore.Authorization;

namespace HelpDesk.Web.Authorization
{
    public sealed class TicketAttachmentAccessRequirement : IAuthorizationRequirement
    {
        public static string PolicyName => "TicketAttachmentAccess";
    }
}
