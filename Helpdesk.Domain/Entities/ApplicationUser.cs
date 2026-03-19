using Microsoft.AspNetCore.Identity;

namespace Helpdesk.Domain.Entities
{
    public class ApplicationUser: IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
    }
}
