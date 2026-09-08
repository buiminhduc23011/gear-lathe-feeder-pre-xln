using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Api.Contracts.Requests;
using Server.Api.Data;
using Server.Api.Data.Entities;
using Server.Api.Exceptions;
using Server.Api.Infrastructure;

namespace Server.Api.Services;

public sealed class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<UserEntity> _passwordHasher;

    public UserService(AppDbContext dbContext, IPasswordHasher<UserEntity> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .OrderBy(x => x.Username)
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
    }

    public async Task<UserEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<UserEntity> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateRequestAsync(request.Username, request.Email, request.Role, null, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new UserEntity
        {
            Username = request.Username.Trim(),
            FullName = request.FullName.Trim(),
            Email = NormalizeOptional(request.Email),
            Role = request.Role.Trim(),
            IsActive = request.IsActive,
            IsSystemAccount = request.IsSystemAccount,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        entity.PasswordHash = _passwordHasher.HashPassword(entity, request.Password);

        _dbContext.Users.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<UserEntity?> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        await ValidateRequestAsync(request.Username, request.Email, request.Role, id, cancellationToken);

        entity.Username = request.Username.Trim();
        entity.FullName = request.FullName.Trim();
        entity.Email = NormalizeOptional(request.Email);

        // Protect system accounts: preserve Role and IsActive to prevent
        // accidental lock-out of the default admin/technician accounts.
        if (!entity.IsSystemAccount)
        {
            entity.Role = request.Role.Trim();
            entity.IsActive = request.IsActive;
        }

        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            entity.PasswordHash = _passwordHasher.HashPassword(entity, request.Password);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<DeleteUserResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return DeleteUserResult.NotFound;
        }

        if (entity.IsSystemAccount)
        {
            return DeleteUserResult.SystemAccountProtected;
        }

        _dbContext.Users.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return DeleteUserResult.Deleted;
    }

    private async Task ValidateRequestAsync(
        string username,
        string? email,
        string role,
        int? existingId,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var normalizedUsername = username.Trim();

        if (!AppRoles.IsSupported(role))
        {
            errors["role"] = ["Role must be one of Admin, Technician, Operator, or Viewer."];
        }

        if (await _dbContext.Users.AnyAsync(
                x => x.Username == normalizedUsername && (!existingId.HasValue || x.Id != existingId.Value),
                cancellationToken))
        {
            errors["username"] = ["Username must be unique."];
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            try
            {
                _ = new System.Net.Mail.MailAddress(email);
            }
            catch (FormatException)
            {
                errors["email"] = ["Email must be a valid email address."];
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationProblemException(errors);
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
