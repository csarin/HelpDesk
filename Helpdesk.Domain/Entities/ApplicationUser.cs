using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Domain.Entities
{
    public class ApplicationUser: IdentityUser
    {
        [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;
    }
}
