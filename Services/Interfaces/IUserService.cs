using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using e09.Models;
using e09.Dtos;

namespace e09.Services.Interfaces
{
    public interface IUserService
    {
        Task<IReadOnlyCollection<User>> GetUsersAsync();
        Task<User> GetUserAsync(Guid id);
        Task<User> CreateUserAsync(UserDto user);
        Task DeleteUserAsync(Guid id);
        Task UpdateUserAsync(Guid id, UserDto user);
    }
}
