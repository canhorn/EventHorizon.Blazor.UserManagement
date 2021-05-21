using EventHorizon.Blazor.UserManagement.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventHorizon.Blazor.UserManagement.Server.Hubs
{
    [Authorize]
    public class UserManagementHub
        : Hub
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UserManagementHub(
            UserManager<ApplicationUser> userManager
        )
        {
            this.userManager = userManager;
        }

        public IEnumerable<UserProfileModel> Fetch()
        {
            var userList = userManager.Users.ToList();
            return userList.Select(
                a => new UserProfileModel
                {
                    UserId = a.Id,
                    Email = a.Email,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                }
            );
        }

        public UserProfileModel ReadUser(
            string id
        )
        {
            return userManager.Users.Where(
                a => a.Id == id
            ).Select(
                a => new UserProfileModel
                {
                    UserId = a.Id,
                    Email = a.Email,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                }
            ).FirstOrDefault();
        }

        public EditUserModel ReadEditUser(
            string id
        )
        {
            return userManager.Users.Where(
                a => a.Id == id
            ).Select(
                a => new EditUserModel
                {
                    UserId = a.Id,
                    Email = a.Email,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                }
            ).FirstOrDefault();
        }

        public async Task<bool> UpdateUser(
            EditUserModel user
        )
        {
            var userToUpdate = userManager.Users.FirstOrDefault(
                a => a.Id == user.UserId
            );

            if (userToUpdate is null)
            {
                return false;
            }

            userToUpdate.FirstName = user.FirstName;
            userToUpdate.LastName = user.LastName;

            var result = await userManager.UpdateAsync(
                userToUpdate
            );

            if (result.Succeeded)
            {
                await Clients.All.SendAsync("Changed");
            }

            return result.Succeeded;
        }


        public async Task<bool> CreateNewUser(
            NewUserModel user
        )
        {
            var result = await userManager.CreateAsync(
                new ApplicationUser
                {
                    Email = user.Email,
                    UserName = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                }
            );

            if (result.Succeeded)
            {
                await Clients.All.SendAsync("Changed");
            }

            return result.Succeeded;
        }

        public async Task<bool> DeleteUser(
            string userId
        )
        {
            var userToDelete = userManager.Users.FirstOrDefault(
                a => a.Id == userId
            );

            if (userToDelete is null)
            {
                return true;
            }

            var result = await userManager.DeleteAsync(
                userToDelete
            );

            if (result.Succeeded)
            {
                await Clients.All.SendAsync("Changed");
            }

            return result.Succeeded;
        }
    }
}
