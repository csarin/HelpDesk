using Bunit;
using HelpDesk.Web.Components.Layout;
using HelpDesk.Web.Components.Pages;

namespace Helpdesk.Infrastructure.Tests
{
    public class BasicComponentsTests : TestContext
    {
        [Fact]
        public void Index_Render_ShowsHeadingAndCreateLink()
        {
            var cut = RenderComponent<HelpDesk.Web.Components.Pages.Index>();

            Assert.Contains("Hello, world!", cut.Markup);
            Assert.Contains("/tickets/create", cut.Markup);
        }

        [Fact]
        public void MainLayout_RendersBodyInsideMain()
        {
            var cut = RenderComponent<MainLayout>();

            Assert.Contains("site-main", cut.Markup);
        }
    }
}
