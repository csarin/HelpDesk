using System.ComponentModel;
using System.Runtime.CompilerServices;
using Helpdesk.Application.Models;
using Helpdesk.Application.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace HelpDesk.Web.ViewModels.Tickets
{
    public class CreateTicketViewModel : INotifyPropertyChanged
    {
        private readonly ITicketService _ticketService;
        private readonly ITicketAttachmentService _ticketAttachmentService;
        private readonly AuthenticationStateProvider _authProvider;

        public CreateTicketModel Model { get; } = new CreateTicketModel();

        private int? _createdTicketId;
        public int? CreatedTicketId
        {
            get => _createdTicketId;
            private set { if (_createdTicketId != value) { _createdTicketId = value; OnPropertyChanged(); } }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set { if (_isBusy != value) { _isBusy = value; OnPropertyChanged(); } }
        }

        private string? _error;
        public string? Error
        {
            get => _error;
            private set { if (_error != value) { _error = value; OnPropertyChanged(); } }
        }

        public CreateTicketViewModel(
            ITicketService ticketService,
            ITicketAttachmentService ticketAttachmentService,
            AuthenticationStateProvider authProvider)
        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _ticketAttachmentService = ticketAttachmentService ?? throw new ArgumentNullException(nameof(ticketAttachmentService));
            _authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
        }

        public async Task CreateAsync(IReadOnlyCollection<CreateTicketAttachmentModel>? attachments = null)
        {
            IsBusy = true;
            Error = null;
            try
            {
                var auth = await _authProvider.GetAuthenticationStateAsync();
                var user = auth.User;
                if (!user.Identity?.IsAuthenticated ?? true)
                {
                    Error = "User is not authenticated.";
                    return;
                }

                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    Error = "User id not found.";
                    return;
                }

                var ticket = await _ticketService.CreateTicketAsync(Model.Title, Model.Description, Model.CategoryId, userId);
                CreatedTicketId = ticket.Id;

                if (attachments is { Count: > 0 })
                {
                    await _ticketAttachmentService.UploadAttachmentsAsync(ticket.Id, userId, attachments);
                }

                Model.Title = string.Empty;
                Model.Description = null;
                Model.CategoryId = null;
                OnPropertyChanged(nameof(Model));
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
