using EventHorizon.Blazor.UserManagement.Client.Shared;
using EventHorizon.Blazor.UserManagement.Server.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

namespace EventHorizon.Blazor.UserManagement.Client.Pages
{
    public class UserEditPageModel
        : ManagementComponentBase
    {
        [Parameter]
        public string UserId { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
        public string LoggedinUserId { get; set; } = string.Empty;
        public EditUserModel EditUser { get; set; } = new EditUserModel();

        protected override async Task OnInitializedAsync()
        {
            if (await StartConnection())
            {
                _hubConnection.On(
                    "Changed",
                    HandleHubChanged
                );

                EditUser = await ReadEditUser(
                    UserId
                );
            }
        }

        public async Task HandleUpdateUser()
        {
            Message = string.Empty;
            var updatedUser = await UpdateUser(
                EditUser
            );

            if (!updatedUser)
            {
                Message = "Failed to update user, contact support.";
                return;
            }

            Message = "Successfully Updated User!";
        }

        private async Task HandleHubChanged()
        {
            EditUser = await ReadEditUser(
                UserId
            );
            if (EditUser is null)
            {
                NavigationManager.NavigateTo("/users");
                return;
            }
            await InvokeAsync(StateHasChanged);
        }
    }
}
