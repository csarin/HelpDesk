using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Identity;
using Helpdesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Helpdesk.Infrastructure.Tests
{
    public class DbInitializerTests
    {
        [Fact]
        public async Task SeedRoles_CreatesAllRoles_AndIsIdempotent()
        {
            using var provider = BuildProvider();
            using var scope = provider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await InvokePrivateAsync("SeedRolesAsync", roleManager);
            await InvokePrivateAsync("SeedRolesAsync", roleManager);

            foreach (var role in AppRoles.All)
            {
                Assert.True(await roleManager.RoleExistsAsync(role));
            }
        }

        [Fact]
        public async Task SeedAdmin_WhenDisabled_DoesNothing()
        {
            using var provider = BuildProvider();
            using var scope = provider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = BuildConfiguration(("SeedData:Admin:Enabled", "false"));

            await InvokePrivateAsync("SeedAdminAsync", userManager, configuration);

            var user = await userManager.FindByEmailAsync("admin@helpdesk.local");
            Assert.Null(user);
        }

        [Fact]
        public async Task SeedAdmin_WithMissingCredentials_Throws()
        {
            using var provider = BuildProvider();
            using var scope = provider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = BuildConfiguration(
                ("SeedData:Admin:Enabled", "true"),
                ("SeedData:Admin:Email", ""),
                ("SeedData:Admin:Password", ""));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                InvokePrivateAsync("SeedAdminAsync", userManager, configuration));
        }

        [Fact]
        public async Task SeedAdmin_CreatesUser_AndAssignsAdminRole()
        {
            using var provider = BuildProvider();
            using var scope = provider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await InvokePrivateAsync("SeedRolesAsync", roleManager);

            var configuration = BuildConfiguration(
                ("SeedData:Admin:Enabled", "true"),
                ("SeedData:Admin:Email", "admin@helpdesk.local"),
                ("SeedData:Admin:Password", "Admin123!"),
                ("SeedData:Admin:DisplayName", "Administrador"));

            await InvokePrivateAsync("SeedAdminAsync", userManager, configuration);

            var user = await userManager.FindByEmailAsync("admin@helpdesk.local");
            Assert.NotNull(user);
            Assert.Equal("Administrador", user.DisplayName);
            Assert.True(await userManager.IsInRoleAsync(user, AppRoles.Admin));
        }

        [Fact]
        public async Task SeedBaseCategories_CreatesBaseSet_AndIsIdempotent()
        {
            using var provider = BuildProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var configuration = BuildConfiguration(("SeedData:CreateBaseCategories", "true"));

            await InvokePrivateAsync("SeedBaseCategoriesAsync", db, configuration, CancellationToken.None);
            await InvokePrivateAsync("SeedBaseCategoriesAsync", db, configuration, CancellationToken.None);

            var names = await db.Categories
                .Select(x => x.Name)
                .OrderBy(x => x)
                .ToListAsync();

            Assert.Equal(3, names.Count);
            Assert.Contains("Support", names);
            Assert.Contains("Incident", names);
            Assert.Contains("Request", names);
        }

        private static ServiceProvider BuildProvider()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            return services.BuildServiceProvider();
        }

        private static IConfiguration BuildConfiguration(params (string Key, string Value)[] pairs)
        {
            var dict = pairs.ToDictionary(x => x.Key, x => x.Value);
            return new ConfigurationBuilder()
                .AddInMemoryCollection(dict!)
                .Build();
        }

        private static async Task InvokePrivateAsync(string methodName, params object[] args)
        {
            var method = typeof(DbInitializer).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            var task = method!.Invoke(null, args) as Task;
            Assert.NotNull(task);
            await task!;
        }
    }
}
