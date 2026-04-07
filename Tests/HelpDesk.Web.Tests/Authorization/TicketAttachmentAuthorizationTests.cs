using Helpdesk.Domain.Entities;
using HelpDesk.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HelpDesk.Web.Tests.Authorization;

public class TicketAttachmentAccessHandlerTests
{
    [Fact]
    public async Task HandleRequirement_WhenAdmin_Succeeds()
    {
        var user = Authenticated("other", AppRoles.Admin);
        var ticket = new Ticket { Id = 1, Title = "T1", CreatedByUserId = "u1" };
        var requirement = new TicketAttachmentAccessRequirement();
        var context = new AuthorizationHandlerContext([requirement], user, ticket);
        var handler = new TicketAttachmentAccessHandler();

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirement_WhenCreator_Succeeds()
    {
        var user = Authenticated("u1");
        var ticket = new Ticket { Id = 1, Title = "T1", CreatedByUserId = "u1" };
        var requirement = new TicketAttachmentAccessRequirement();
        var context = new AuthorizationHandlerContext([requirement], user, ticket);
        var handler = new TicketAttachmentAccessHandler();

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirement_WhenAssignedAgent_Succeeds()
    {
        var user = Authenticated("agent1");
        var ticket = new Ticket { Id = 1, Title = "T1", CreatedByUserId = "u1" };
        ticket.AssignTo("agent1");
        var requirement = new TicketAttachmentAccessRequirement();
        var context = new AuthorizationHandlerContext([requirement], user, ticket);
        var handler = new TicketAttachmentAccessHandler();

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirement_WhenOtherUser_Fails()
    {
        var user = Authenticated("other");
        var ticket = new Ticket { Id = 1, Title = "T1", CreatedByUserId = "u1" };
        ticket.AssignTo("agent1");
        var requirement = new TicketAttachmentAccessRequirement();
        var context = new AuthorizationHandlerContext([requirement], user, ticket);
        var handler = new TicketAttachmentAccessHandler();

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirement_WhenAnonymous_Fails()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var ticket = new Ticket { Id = 1, Title = "T1", CreatedByUserId = "u1" };
        var requirement = new TicketAttachmentAccessRequirement();
        var context = new AuthorizationHandlerContext([requirement], user, ticket);
        var handler = new TicketAttachmentAccessHandler();

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static ClaimsPrincipal Authenticated(string userId, string? role = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
