using Bunit;
using Helpdesk.Application.Models;
using Helpdesk.Application.Services;
using Helpdesk.Domain.Entities;
using HelpDesk.Web.Components.Pages.Tickets;
using HelpDesk.Web.ViewModels.Tickets;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

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
