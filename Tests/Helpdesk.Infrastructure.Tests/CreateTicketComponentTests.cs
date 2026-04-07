using Bunit;
using Helpdesk.Application.Models;
using Helpdesk.Application.Services;
using Helpdesk.Domain.Entities;
using HelpDesk.Web.Components.Pages.Tickets;
using HelpDesk.Web.ViewModels.Tickets;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Reflection;

namespace Helpdesk.Infrastructure.Tests
{
    public class CreateTicketComponentTests : TestContext
    {
        [Fact]
        public void Render_ShowsMainSections()
        {
            Services.AddScoped<ITicketService, StubTicketService>();
            Services.AddScoped<ITicketAttachmentService, StubAttachmentService>();
            Services.AddScoped<AuthenticationStateProvider>(_ =>
                new StubAuthStateProvider(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddScoped<CreateTicketViewModel>();

            var cut = RenderComponent<CreateTicket>();

            Assert.Contains("Nuevo ticket", cut.Markup);
            Assert.Contains("Información básica", cut.Markup);
            Assert.Contains("Descripción del problema", cut.Markup);
            Assert.Contains("Contexto", cut.Markup);
            Assert.Contains("Archivos adjuntos", cut.Markup);
        }

        [Fact]
        public void ChangingTitle_UpdatesTicketSummary()
        {
            Services.AddScoped<ITicketService, StubTicketService>();
            Services.AddScoped<ITicketAttachmentService, StubAttachmentService>();
            Services.AddScoped<AuthenticationStateProvider>(_ =>
                new StubAuthStateProvider(Authenticated("u1")));
            Services.AddScoped<CreateTicketViewModel>();

            var cut = RenderComponent<CreateTicket>();

            cut.Find("#ticket-title").Change("Error en impresora");

            Assert.Contains("Asunto: Error en impresora", cut.Markup);
        }

        [Fact]
        public void SubmitUnauthenticated_ShowsErrorMessage()
        {
            Services.AddScoped<ITicketService, StubTicketService>();
            Services.AddScoped<ITicketAttachmentService, StubAttachmentService>();
            Services.AddScoped<AuthenticationStateProvider>(_ =>
                new StubAuthStateProvider(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddScoped<CreateTicketViewModel>();

            var cut = RenderComponent<CreateTicket>();
            cut.Find("#ticket-title").Change("Prueba");

            cut.Find("form").Submit();

            cut.WaitForAssertion(() =>
                Assert.Contains("User is not authenticated.", cut.Markup));
        }

        [Fact]
        public void ChangingCategoryAndPriority_UpdatesSummary()
        {
            Services.AddScoped<ITicketService, StubTicketService>();
            Services.AddScoped<ITicketAttachmentService, StubAttachmentService>();
            Services.AddScoped<AuthenticationStateProvider>(_ =>
                new StubAuthStateProvider(Authenticated("u1")));
            Services.AddScoped<CreateTicketViewModel>();

            var cut = RenderComponent<CreateTicket>();

            cut.Find("#ticket-category").Change("2");
            cut.Find("#ticket-priority").Change("Crítica");

            Assert.Contains("Categoría: Hardware", cut.Markup);
            Assert.Contains("Prioridad: Crítica", cut.Markup);
            Assert.Contains("2 horas", cut.Markup);
        }

        [Fact]
        public void SubmitAuthenticated_ShowsSuccessMessage()
        {
            Services.AddScoped<ITicketService, StubTicketService>();
            Services.AddScoped<ITicketAttachmentService, StubAttachmentService>();
            Services.AddScoped<AuthenticationStateProvider>(_ =>
                new StubAuthStateProvider(Authenticated("u1")));
            Services.AddScoped<CreateTicketViewModel>();

            var cut = RenderComponent<CreateTicket>();
            cut.Find("#ticket-title").Change("Incidencia de red");

            cut.Find("form").Submit();

            cut.WaitForAssertion(() =>
                Assert.Contains("Ticket creado correctamente con ID", cut.Markup));
        }

        [Fact]
        public void InitialRender_ShowsDefaultSlaAndSummaryDashes()
        {
            Services.AddScoped<ITicketService, StubTicketService>();
            Services.AddScoped<ITicketAttachmentService, StubAttachmentService>();
            Services.AddScoped<AuthenticationStateProvider>(_ =>
                new StubAuthStateProvider(Authenticated("u1")));
            Services.AddScoped<CreateTicketViewModel>();

            var cut = RenderComponent<CreateTicket>();

            Assert.Contains("SLA estimado:", cut.Markup);
            Assert.Contains("4 horas", cut.Markup);
            Assert.Contains("Asunto: -", cut.Markup);
            Assert.Contains("Categoría: -", cut.Markup);
        }

        [Fact]
        public void Private_GetSla_ReturnsExpectedValues()
        {
            Services.AddScoped<ITicketService, StubTicketService>();
            Services.AddScoped<ITicketAttachmentService, StubAttachmentService>();
            Services.AddScoped<AuthenticationStateProvider>(_ =>
                new StubAuthStateProvider(Authenticated("u1")));
            Services.AddScoped<CreateTicketViewModel>();

            var cut = RenderComponent<CreateTicket>();
            var type = cut.Instance.GetType();
            var method = type.GetMethod("GetSla", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            Assert.Equal("2 horas", method!.Invoke(null, ["Crítica"]));
            Assert.Equal("4 horas", method.Invoke(null, ["Alta"]));
            Assert.Equal("8 horas", method.Invoke(null, ["Media"]));
            Assert.Equal("24 horas", method.Invoke(null, ["Baja"]));
            Assert.Equal("24 horas", method.Invoke(null, ["otro"]));
        }

        [Fact]
        public void Private_GetTextOrDash_ReturnsExpectedValues()
        {
            Services.AddScoped<ITicketService, StubTicketService>();
            Services.AddScoped<ITicketAttachmentService, StubAttachmentService>();
            Services.AddScoped<AuthenticationStateProvider>(_ =>
                new StubAuthStateProvider(Authenticated("u1")));
            Services.AddScoped<CreateTicketViewModel>();

            var cut = RenderComponent<CreateTicket>();
            var type = cut.Instance.GetType();
            var method = type.GetMethod("GetTextOrDash", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            Assert.Equal("-", method!.Invoke(null, [null]));
            Assert.Equal("-", method.Invoke(null, ["   "]));
            Assert.Equal("abc", method.Invoke(null, ["  abc  "]));
        }

        [Fact]
        public async Task Private_BuildAttachmentModelsAsync_WhenNoFiles_ReturnsEmpty()
        {
            Services.AddScoped<ITicketService, StubTicketService>();
            Services.AddScoped<ITicketAttachmentService, StubAttachmentService>();
            Services.AddScoped<AuthenticationStateProvider>(_ =>
                new StubAuthStateProvider(Authenticated("u1")));
            Services.AddScoped<CreateTicketViewModel>();

            var cut = RenderComponent<CreateTicket>();
            var type = cut.Instance.GetType();
            var method = type.GetMethod("BuildAttachmentModelsAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);

            var task = method!.Invoke(cut.Instance, null) as Task<IReadOnlyCollection<CreateTicketAttachmentModel>>;
            Assert.NotNull(task);
            var result = await task!;
            Assert.Empty(result);
        }

        [Fact]
        public void Private_FormatFileSize_FormatsExpectedValues()
        {
            Services.AddScoped<ITicketService, StubTicketService>();
            Services.AddScoped<ITicketAttachmentService, StubAttachmentService>();
            Services.AddScoped<AuthenticationStateProvider>(_ =>
                new StubAuthStateProvider(Authenticated("u1")));
            Services.AddScoped<CreateTicketViewModel>();

            var cut = RenderComponent<CreateTicket>();
            var type = cut.Instance.GetType();
            var method = type.GetMethod("FormatFileSize", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            Assert.Equal("123 B", method!.Invoke(null, [123L]));
            Assert.Equal("2 KB", method.Invoke(null, [2048L]));
            Assert.Equal("2 MB", method.Invoke(null, [2L * 1024L * 1024L]));
        }

        private static ClaimsPrincipal Authenticated(string userId) =>
            new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

        private sealed class StubAuthStateProvider(ClaimsPrincipal user) : AuthenticationStateProvider
        {
            public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
                Task.FromResult(new AuthenticationState(user));
        }

        private sealed class StubTicketService : ITicketService
        {
            public Task<Ticket> CreateTicketAsync(string title, string? description, int? categoryId, string createdByUserId)
            {
                return Task.FromResult(new Ticket
                {
                    Id = 10,
                    Title = title,
                    Description = description,
                    CategoryId = categoryId,
                    CreatedByUserId = createdByUserId
                });
            }
        }

        private sealed class StubAttachmentService : ITicketAttachmentService
        {
            public Task UploadAttachmentsAsync(int ticketId, string uploadedByUserId, IReadOnlyCollection<CreateTicketAttachmentModel> attachments, CancellationToken cancellationToken = default) =>
                Task.CompletedTask;
        }
    }
}
