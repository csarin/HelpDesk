using Helpdesk.Application.Models;
using Helpdesk.Application.Services;
using Helpdesk.Domain.Entities;
using HelpDesk.Web.ViewModels.Tickets;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Helpdesk.Infrastructure.Tests
{
    public class CreateTicketViewModelTests
    {
        [Fact]
        public async Task CreateAsync_WhenUserIsNotAuthenticated_SetsError()
        {
            var ticketService = new StubTicketService();
            var attachmentService = new StubAttachmentService();
            var authProvider = new StubAuthStateProvider(new ClaimsPrincipal(new ClaimsIdentity()));
            var vm = new CreateTicketViewModel(ticketService, attachmentService, authProvider);

            await vm.CreateAsync();

            Assert.Equal("User is not authenticated.", vm.Error);
            Assert.Equal(0, ticketService.Calls);
        }

        [Fact]
        public async Task CreateAsync_WhenUserIdMissing_SetsError()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Email, "a@b.com")], "test"));
            var vm = new CreateTicketViewModel(
                new StubTicketService(),
                new StubAttachmentService(),
                new StubAuthStateProvider(user));

            await vm.CreateAsync();

            Assert.Equal("User id not found.", vm.Error);
        }

        [Fact]
        public async Task CreateAsync_Success_ClearsFormAndSetsTicketId()
        {
            var ticketService = new StubTicketService
            {
                TicketToReturn = new Ticket { Id = 123, Title = "t", CreatedByUserId = "u1" }
            };
            var vm = new CreateTicketViewModel(
                ticketService,
                new StubAttachmentService(),
                new StubAuthStateProvider(AuthenticatedUser("u1")));

            vm.Model.Title = " My title ";
            vm.Model.Description = "desc";
            vm.Model.CategoryId = 2;

            await vm.CreateAsync();

            Assert.Equal(123, vm.CreatedTicketId);
            Assert.Equal(string.Empty, vm.Model.Title);
            Assert.Null(vm.Model.Description);
            Assert.Null(vm.Model.CategoryId);
            Assert.Null(vm.Error);
            Assert.False(vm.IsBusy);
        }

        [Fact]
        public async Task CreateAsync_WithAttachments_CallsAttachmentService()
        {
            var ticketService = new StubTicketService
            {
                TicketToReturn = new Ticket { Id = 200, Title = "t", CreatedByUserId = "u1" }
            };
            var attachmentService = new StubAttachmentService();
            var vm = new CreateTicketViewModel(
                ticketService,
                attachmentService,
                new StubAuthStateProvider(AuthenticatedUser("u1")));

            var attachments = new[]
            {
                new CreateTicketAttachmentModel
                {
                    FileName = "x.txt",
                    ContentType = "text/plain",
                    Content = [1, 2]
                }
            };

            await vm.CreateAsync(attachments);

            Assert.Equal(1, attachmentService.Calls);
            Assert.Equal(200, attachmentService.LastTicketId);
            Assert.Equal("u1", attachmentService.LastUserId);
            Assert.Single(attachmentService.LastAttachments!);
        }

        [Fact]
        public async Task CreateAsync_WhenServiceThrows_SetsErrorAndResetsBusy()
        {
            var ticketService = new StubTicketService
            {
                ExceptionToThrow = new InvalidOperationException("boom")
            };
            var vm = new CreateTicketViewModel(
                ticketService,
                new StubAttachmentService(),
                new StubAuthStateProvider(AuthenticatedUser("u1")));
            vm.Model.Title = "X";

            await vm.CreateAsync();

            Assert.Equal("boom", vm.Error);
            Assert.False(vm.IsBusy);
        }

        private static ClaimsPrincipal AuthenticatedUser(string id) =>
            new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id)], "test"));

        private sealed class StubAuthStateProvider(ClaimsPrincipal user) : AuthenticationStateProvider
        {
            public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
                Task.FromResult(new AuthenticationState(user));
        }

        private sealed class StubTicketService : ITicketService
        {
            public int Calls { get; private set; }
            public Ticket TicketToReturn { get; set; } = new() { Id = 1, Title = "t", CreatedByUserId = "u1" };
            public Exception? ExceptionToThrow { get; set; }

            public Task<Ticket> CreateTicketAsync(string title, string? description, int? categoryId, string createdByUserId)
            {
                Calls++;
                if (ExceptionToThrow is not null) throw ExceptionToThrow;
                return Task.FromResult(TicketToReturn);
            }
        }

        private sealed class StubAttachmentService : ITicketAttachmentService
        {
            public int Calls { get; private set; }
            public int LastTicketId { get; private set; }
            public string? LastUserId { get; private set; }
            public IReadOnlyCollection<CreateTicketAttachmentModel>? LastAttachments { get; private set; }

            public Task UploadAttachmentsAsync(int ticketId, string uploadedByUserId, IReadOnlyCollection<CreateTicketAttachmentModel> attachments, CancellationToken cancellationToken = default)
            {
                Calls++;
                LastTicketId = ticketId;
                LastUserId = uploadedByUserId;
                LastAttachments = attachments;
                return Task.CompletedTask;
            }
        }
    }
}
