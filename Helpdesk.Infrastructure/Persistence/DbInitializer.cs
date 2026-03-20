using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await db.Database.MigrateAsync(cancellationToken);

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, configuration);
        await SeedBaseCategoriesAsync(db, configuration, cancellationToken);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in AppRoles.All)
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;

            var result = await roleManager.CreateAsync(new IdentityRole(role));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Role could not be created '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        var enabled = configuration.GetValue<bool>("SeedData:Admin:Enabled");
        if (!enabled)
            return;

        var email = configuration["SeedData:Admin:Email"];
        var password = configuration["SeedData:Admin:Password"];
        var displayName = configuration["SeedData:Admin:DisplayName"] ?? "Administrator";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "SeedData:Admin account is enabled, but the email address or password is missing.");
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName
            };

            var createResult = await userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Initial admin account could not be created: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }
        else if (user.DisplayName != displayName)
        {
            user.DisplayName = displayName;

            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Initial admin display name could not be updated: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(user, AppRoles.Admin);

            if (!addToRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Admin role could not be assigned to the initial user: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static async Task SeedBaseCategoriesAsync(
        AppDbContext db,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var enabled = configuration.GetValue<bool>("SeedData:CreateBaseCategories");
        if (!enabled)
            return;

        string[] baseCategories =
        [
            "Support",
            "Incident",
            "Request"
        ];

        var existingNames = await db.Categories
            .AsNoTracking()
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var toInsert = baseCategories
            .Where(name => !existingNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            .Select(name => new Category(name))
            .ToList();

        if (toInsert.Count == 0)
            return;

        db.Categories.AddRange(toInsert);
        await db.SaveChangesAsync(cancellationToken);
    }
}