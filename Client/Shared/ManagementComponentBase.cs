using EventHorizon.Blazor.UserManagement.Server.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventHorizon.Blazor.UserManagement.Client.Shared
{
    /// <summary>
    /// This is a Base component that allows for the easier setup and usage of the SignalR Connection.
    /// Includes methods that can be used to call up to the SignalR Hub.
    /// </summary>
    public class ManagementComponentBase
        : ComponentBase, 
        IAsyncDisposable
    {
        [Inject]
        public IAccessTokenProvider TokenProvider { get; set; } = null!;
        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        protected string LoggedInUserId { get; private set; } = string.Empty;

        protected HubConnection? _hubConnection;

        protected async Task<bool> StartConnection()
        {
            // Here we are getting the Access Token for the current User
            // This is used by the Server Hub to validate that the user is Authorized.
            var accessTokenResult = await TokenProvider.RequestAccessToken();
            if (accessTokenResult.TryGetToken(
                out var accessToken
            ))
            {
                var auth = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var sub = auth.User.Claims.FirstOrDefault(a => a.Type == "sub");

                // Track the Logged In User Id.
                // This way management pages can know who is editing the Users.
                LoggedInUserId = sub?.Value ?? string.Empty;

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(
                        // Here we assume that our Hub is also hosted from our current client domain
                        $"{NavigationManager.BaseUri}user-management",
                        options =>
                        {
                            // We use our AccessToken for Authentication
                            // This also has the added benefit, on the server, to validate reqeuests.
                            options.AccessTokenProvider = () => Task.FromResult(
                                accessToken.Value
                            );
                        }
                    ).Build();

                // Start the connection!
                // So when we return the caller can setup any On Actions.
                await _hubConnection.StartAsync();

                return true;
            }

            return false;
        }

        public async ValueTask DisposeAsync()
        {
            // This is wired into the Blazor runtime management,
            // it will cleanup our open connection and remove any handlers.
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }

        protected async Task<UserProfileModel> ReadUser(
            string id
        )
        {
            return await _hubConnection.InvokeAsync<UserProfileModel>(
                "ReadUser",
                id
            );
        }

        protected async Task<EditUserModel> ReadEditUser(
            string id
        )
        {
            return await _hubConnection.InvokeAsync<EditUserModel>(
                "ReadEditUser",
                id
            );
        }

        protected async Task<bool> UpdateUser(
            EditUserModel updatedUser
        )
        {
            return await _hubConnection.InvokeAsync<bool>(
                "UpdateUser",
                updatedUser
            );
        }

        protected async Task<bool> DeleteUser(
            string userId
        )
        {
            return await _hubConnection.InvokeAsync<bool>(
                "DeleteUser",
                userId
            );
        }

        protected async Task<IEnumerable<UserProfileModel>> FetchAll()
        {
            return await _hubConnection.InvokeAsync<IEnumerable<UserProfileModel>>(
                "Fetch"
            );
        }

        protected async Task<bool> CreateNewUser(
            NewUserModel user
        )
        {
            return await _hubConnection.InvokeAsync<bool>(
                "CreateNewUser",
                user
            );
        }
    }
}
