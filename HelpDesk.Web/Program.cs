using Helpdesk.Infrastructure.DependencyInjection;
using Helpdesk.Infrastructure.Identity;
using Helpdesk.Infrastructure.Persistence;
using Helpdesk.Application.Services;
using HelpDesk.Web.Authorization;
using HelpDesk.Web;
using HelpDesk.Web.ViewModels.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string does not exist 'DefaultConnection'.");

builder.Services.AddRazorPages(options => options.RootDirectory = "/Components/Pages");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<CreateTicketViewModel>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        TicketAttachmentAccessRequirement.PolicyName,
        policy => policy.Requirements.Add(new TicketAttachmentAccessRequirement()));
});
builder.Services.AddSingleton<IAuthorizationHandler, TicketAttachmentAccessHandler>();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Register infrastructure services via extension
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/tickets/{ticketId:int}/attachments/{attachmentId:int}", async (
    int ticketId,
    int attachmentId,
    HttpContext httpContext,
    ITicketService ticketService,
    ITicketAttachmentQueryService attachmentQueryService,
    IAuthorizationService authorizationService,
    CancellationToken cancellationToken) =>
{
    if (httpContext.User.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    var ticket = await ticketService.GetByIdAsync(ticketId, cancellationToken);
    if (ticket is null)
    {
        return Results.NotFound();
    }

    var authResult = await authorizationService.AuthorizeAsync(
        httpContext.User,
        ticket,
        TicketAttachmentAccessRequirement.PolicyName);
    if (!authResult.Succeeded)
    {
        return Results.Forbid();
    }

    var attachment = await attachmentQueryService.DownloadAsync(ticketId, attachmentId, cancellationToken);
    if (attachment is null)
    {
        return Results.NotFound();
    }

    return Results.File(attachment.Content, attachment.ContentType, attachment.FileName);
});

app.MapGet("/tickets/{ticketId:int}/attachments/{attachmentId:int}/preview", async (
    int ticketId,
    int attachmentId,
    HttpContext httpContext,
    ITicketService ticketService,
    ITicketAttachmentQueryService attachmentQueryService,
    IAuthorizationService authorizationService,
    CancellationToken cancellationToken) =>
{
    if (httpContext.User.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    var ticket = await ticketService.GetByIdAsync(ticketId, cancellationToken);
    if (ticket is null)
    {
        return Results.NotFound();
    }

    var authResult = await authorizationService.AuthorizeAsync(
        httpContext.User,
        ticket,
        TicketAttachmentAccessRequirement.PolicyName);
    if (!authResult.Succeeded)
    {
        return Results.Forbid();
    }

    var attachment = await attachmentQueryService.DownloadAsync(ticketId, attachmentId, cancellationToken);
    if (attachment is null)
    {
        return Results.NotFound();
    }

    return Results.File(attachment.Content, attachment.ContentType);
});

await DbInitializer.InitializeAsync(app.Services);

app.Run();