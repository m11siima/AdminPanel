using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Requests;

namespace AdminPanel.Domain.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetUsersAsync();
    Task<User> CreateUserAsync(CreateUserRequest request);
    Task<User> UpdateUserAsync(Guid userId, UpdateUserRequest request);
}

