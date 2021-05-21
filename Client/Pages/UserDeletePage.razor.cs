namespace EventHorizon.Blazor.UserManagement.Client.Pages
{
    using EventHorizon.Blazor.UserManagement.Client.Shared;
    using EventHorizon.Blazor.UserManagement.Server.Models;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.SignalR.Client;
    using System.Threading.Tasks;

    public class UserDeletePageModel
        : ManagementComponentBase
    {
        [Parameter]
        public string UserId { get; set; } = string.Empty;

        public string Message { get; private set; } = string.Empty;
        public UserProfileModel UserModel { get; private set; } = new UserProfileModel();

        protected override async Task OnInitializedAsync()
        {
            if (await StartConnection())
            {
                _hubConnection.On(
                    "Change",
                    HandleChange
                );
                var user = await ReadUser(
                    UserId
                );
                if (user is null)
                {
                    NavigationManager.NavigateTo("/users");
                    return;
                }
                UserModel = user;
            }
        }

        public async Task HandleDeleteUser()
        {
            var successful = await DeleteUser(
                UserId
            );
            if (!successful)
            {
                Message = "Failed to Delete User, contact support.";
                return;
            }

            NavigationManager.NavigateTo("/users");
        }

        public void HandleBackToUsers()
        {
            NavigationManager.NavigateTo("/users");
        }

        private async Task HandleChange()
        {
            UserModel = await ReadUser(
                UserId
            );
        }
    }
}
