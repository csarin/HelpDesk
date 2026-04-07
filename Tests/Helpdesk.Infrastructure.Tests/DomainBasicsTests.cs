using Helpdesk.Domain.Entities;

namespace Helpdesk.Infrastructure.Tests
{
    public class DomainBasicsTests
    {
        [Fact]
        public void Category_Ctor_TrimAndAssignName()
        {
            var category = new Category("  Soporte  ");

            Assert.Equal("Soporte", category.Name);
            Assert.True(category.IsActive);
        }

        [Fact]
        public void Category_Ctor_WithInvalidName_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Category("   "));
        }

        [Fact]
        public void Category_Rename_UpdatesTrimmedName()
        {
            var category = new Category("Inicial");

            category.Rename("  Nueva  ");

            Assert.Equal("Nueva", category.Name);
        }

        [Fact]
        public void Category_Rename_WithInvalidName_Throws()
        {
            var category = new Category("Inicial");

            Assert.Throws<ArgumentException>(() => category.Rename(" "));
        }

        [Fact]
        public void AppRoles_All_ContainsExpectedRoles()
        {
            Assert.Contains(AppRoles.Admin, AppRoles.All);
            Assert.Contains(AppRoles.Agent, AppRoles.All);
            Assert.Contains(AppRoles.User, AppRoles.All);
            Assert.Equal(3, AppRoles.All.Length);
        }
    }
}
