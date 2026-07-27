using Server.Api.Contracts.Requests;
using Server.Api.Data.Entities;

namespace Server.Api.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<UserEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<UserEntity> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<UserEntity?> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<DeleteUserResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
