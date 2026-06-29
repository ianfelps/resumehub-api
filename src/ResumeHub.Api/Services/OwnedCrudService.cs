using Microsoft.EntityFrameworkCore;
using ResumeHub.Api.Common;
using ResumeHub.Domain.Entities;
using ResumeHub.Infrastructure.Persistence;

namespace ResumeHub.Api.Services;

/// <summary>
/// Generic CRUD contract for inventory items scoped to the current user.
/// </summary>
public interface IOwnedCrudService<in TCreate, in TUpdate, TResponse>
{
    Task<IReadOnlyList<TResponse>> GetAllAsync();
    Task<TResponse> GetByIdAsync(Guid id);
    Task<TResponse> CreateAsync(TCreate dto);
    Task<TResponse> UpdateAsync(Guid id, TUpdate dto);
    Task DeleteAsync(Guid id);
}

/// <summary>
/// Base implementation: every query is filtered by the authenticated user's id,
/// so a user can only read/modify their own inventory (ownership enforced here).
/// </summary>
public abstract class OwnedCrudService<TEntity, TCreate, TUpdate, TResponse>(
    ResumeHubDbContext db, ICurrentUser currentUser)
    : IOwnedCrudService<TCreate, TUpdate, TResponse>
    where TEntity : OwnedEntity
{
    protected readonly ResumeHubDbContext Db = db;
    protected readonly ICurrentUser CurrentUser = currentUser;

    protected abstract DbSet<TEntity> Set { get; }
    protected abstract TEntity FromCreate(TCreate dto);
    protected abstract void ApplyUpdate(TUpdate dto, TEntity entity);
    protected abstract TResponse ToResponse(TEntity entity);

    /// <summary>Default ordering for list results; override per entity if needed.</summary>
    protected virtual IQueryable<TEntity> OrderListing(IQueryable<TEntity> query)
        => query.OrderByDescending(e => e.CreatedAt);

    public async Task<IReadOnlyList<TResponse>> GetAllAsync()
    {
        var items = await OrderListing(Set.Where(e => e.UserId == CurrentUser.Id))
            .AsNoTracking()
            .ToListAsync();
        return items.Select(ToResponse).ToList();
    }

    public async Task<TResponse> GetByIdAsync(Guid id)
        => ToResponse(await FindOwnedAsync(id));

    public async Task<TResponse> CreateAsync(TCreate dto)
    {
        var entity = FromCreate(dto);
        entity.UserId = CurrentUser.Id;
        Set.Add(entity);
        await Db.SaveChangesAsync();
        return ToResponse(entity);
    }

    public async Task<TResponse> UpdateAsync(Guid id, TUpdate dto)
    {
        var entity = await FindOwnedAsync(id);
        ApplyUpdate(dto, entity);
        await Db.SaveChangesAsync();
        return ToResponse(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindOwnedAsync(id);
        Set.Remove(entity);
        await Db.SaveChangesAsync();
    }

    protected async Task<TEntity> FindOwnedAsync(Guid id)
        => await Set.FirstOrDefaultAsync(e => e.Id == id && e.UserId == CurrentUser.Id)
            ?? throw new NotFoundException($"{typeof(TEntity).Name} '{id}' not found.");
}
