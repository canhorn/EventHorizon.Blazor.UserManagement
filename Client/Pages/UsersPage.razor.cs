using EventHorizon.Blazor.UserManagement.Client.Shared;
using EventHorizon.Blazor.UserManagement.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventHorizon.Blazor.UserManagement.Client.Pages
{
    [Authorize]
    public class UsersPageModel
        : ManagementComponentBase
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public IEnumerable<UserProfileModel> Users { get; private set; } = new List<UserProfileModel>();
        public NewUserModel NewUser { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            if (await StartConnection())
            {
                _hubConnection.On(
                    "Changed",
                    HandleHubChanged
                );

                Users = await FetchAll();
            }
        }

        public async Task HandleCreateNewUser()
        {
            ErrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(NewUser.Email))
            {
                ErrorMessage = "Email is not valid";
                return;
            }
            var createdUser = await CreateNewUser(
                NewUser
            );

            if (!createdUser)
            {
                ErrorMessage = "Failed to create user, contact support.";
                return;
            }

            ErrorMessage = "Successfully created User!";
            NewUser = new();
        }

        private async Task HandleHubChanged()
        {
            Users = await FetchAll();
            await InvokeAsync(StateHasChanged);
        }
    }
}
