using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;
    }
}
