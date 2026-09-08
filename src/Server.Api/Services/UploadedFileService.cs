using Microsoft.EntityFrameworkCore;
using Server.Api.Data;
using Server.Api.Data.Entities;
using Server.Api.Infrastructure;

namespace Server.Api.Services;

public sealed class UploadedFileService : IUploadedFileService
{
    private readonly AppDbContext _dbContext;

    public UploadedFileService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UploadedFileEntity> CreateAsync(
        CreateUploadedFileCommand command,
        CancellationToken cancellationToken = default)
    {
        var entity = new UploadedFileEntity
        {
            OriginalFileName = command.StoredFile.OriginalFileName,
            StoredFileName = command.StoredFile.StoredFileName,
            StoragePath = command.StoredFile.StoragePath,
            Size = command.StoredFile.Size,
            MachineName = command.MachineName.Trim(),
            Manufacturer = command.Manufacturer.Trim(),
            SentAtUtc = command.SentAtUtc,
            UploadedAtUtc = command.StoredFile.SavedAtUtc,
            UploadSource = command.UploadSource,
            UploadedByUserId = command.UploadedByUserId,
            IsDeleted = false
        };

        _dbContext.UploadedFiles.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<IReadOnlyList<(UploadedFileEntity File, string? UploadedByUsername)>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.UploadedFiles
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupJoin(
                _dbContext.Users.AsNoTracking(),
                f => f.UploadedByUserId,
                u => u.Id,
                (f, users) => new { File = f, Users = users })
            .SelectMany(
                x => x.Users.DefaultIfEmpty(),
                (x, user) => new { x.File, UploadedByUsername = user != null ? user.FullName : null })
            .OrderByDescending(x => x.File.UploadedAtUtc)
            .ThenByDescending(x => x.File.Id)
            .Select(x => new ValueTuple<UploadedFileEntity, string?>(x.File, x.UploadedByUsername))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<UploadedFileEntity?> GetActiveByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UploadedFiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public async Task<DeleteUploadedFileResult> SoftDeleteAsync(
        int id,
        int performedByUserId,
        string performedByUsername,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.UploadedFiles
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (entity is null)
        {
            return DeleteUploadedFileResult.NotFound;
        }

        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTimeOffset.UtcNow;
        entity.DeletedByUserId = performedByUserId;

        _dbContext.UploadedFileActions.Add(new UploadedFileActionEntity
        {
            UploadedFileId = entity.Id,
            ActionType = UploadedFileActionTypes.SoftDelete,
            PerformedByUserId = performedByUserId,
            PerformedByUsernameSnapshot = performedByUsername.Trim(),
            PerformedAtUtc = entity.DeletedAtUtc.Value
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return DeleteUploadedFileResult.Success;
    }
}
