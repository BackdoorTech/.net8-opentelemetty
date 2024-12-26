using System.Linq.Expressions;
using CleanArchitecture.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;
using VideoGameApi;
using LanguageExt;
using static LanguageExt.Prelude;
using LanguageExt.UnsafeValueAccess;

public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : class
{
  protected DbSet<T> _dbSet = context.Set<T>();

  public async Task<Either<string, T>> AddAsync(T entity)
  {
    try
    {
      await _dbSet.AddAsync(entity);
      return Right(entity);  // Success: return the added entity
    }
    catch (Exception ex)
    {
      return Left<string, T>(ex.Message);  // Failure: return the error message
    }
  }

  public async Task<Either<string, IEnumerable<T>>> AddRangeAsync(IEnumerable<T> entities)
  {
    try
    {
      await _dbSet.AddRangeAsync(entities);
      return Right(entities); // Success: return the added entities
    }
    catch (Exception ex)
    {
      return Left<string, IEnumerable<T>>(ex.Message); // Failure: return the error message
    }
  }


  public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
  {
    return await _dbSet.AnyAsync(filter);
  }

  public async Task<bool> AnyAsync()
  {
    return await _dbSet.AnyAsync();
  }

  public async Task<int> CountAsync(Expression<Func<T, bool>> filter)
  {
    return filter == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(filter);
  }

  public async Task<int> CountAsync()
  {
    return await _dbSet.CountAsync();
  }

  public async Task<Either<string, T>> GetByIdAsync(object id)
  {
    try
    {
      var result = await _dbSet.FindAsync(id);

      if(result is not null) {
        return Right(result);  // Success: return the entity
      } else {
        return Left<string, T>("not found");
      }

    }
    catch (Exception ex)
    {
      return Left<string, T>(ex.Message);  // Failure: return the error message
    }
  }

  public async Task<Either<string, List<T>>> GetAllAsync()
  {
    try
    {
      var all = await _dbSet.AsNoTracking().ToListAsync();
      return Right(all);  // Success: return the list of entities
    }
    catch (Exception ex)
    {
      return Left<string, List<T>>(ex.Message);  // Failure: return the error message
    }
  }

  public async Task<Pagination<TResult>> ToPagination<TResult>(
    int pageIndex,
    int pageSize,
    Expression<Func<T, bool>>? filter = null,
    Func<IQueryable<T>, IQueryable<T>>? include = null,
    Expression<Func<T, object>>? orderBy = null,
    bool ascending = true,
    Expression<Func<T, TResult>> selector = null)
  {
    IQueryable<T> query = _dbSet.AsNoTracking();

    if (include != null)
    {
        query = include(query);
    }

    if (filter != null)
    {
        query = query.Where(filter);
    }

    orderBy ??= x => EF.Property<object>(x, "Id");

    query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

    var projectedQuery = query.Select(selector);

    var result = await Pagination<TResult>.ToPagedList(projectedQuery, pageIndex, pageSize);

    return result;
  }

  public async Task<T?> FirstOrDefaultAsync(
      Expression<Func<T, bool>> filter,
      Func<IQueryable<T>, IQueryable<T>>? include = null)
  {
    IQueryable<T> query = _dbSet.IgnoreQueryFilters().AsNoTracking();

    if (include != null)
    {
      query = include(query);
    }

    return await query.FirstOrDefaultAsync(filter);
  }

  public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter,
      Expression<Func<T, object>> sort, bool ascending = true)
  {
    var query = _dbSet.IgnoreQueryFilters()
      .AsNoTracking()
      .Where(filter);

    query = ascending ? query.OrderBy(sort) : query.OrderByDescending(sort);

    return await query.FirstOrDefaultAsync();
  }



  public Either<string, T> Update(T entity)
  {
    try
    {
      _dbSet.Update(entity);
      return Right(entity); // Success: return the updated entity
    }
    catch (Exception ex)
    {
      return Left<string, T>(ex.Message); // Failure: return the error message
    }
  }

  public Either<string, IEnumerable<T>> UpdateRange(IEnumerable<T> entities)
  {
    try
    {
      _dbSet.UpdateRange(entities);
      return Right(entities); // Success: return the updated entities
    }
    catch (Exception ex)
    {
      return Left<string, IEnumerable<T>>(ex.Message); // Failure: return the error message
    }
  }

  public async Task  DeleteAsync(object id)
  {
    var entityResult = await GetByIdAsync(id);

    if(entityResult.IsRight && entityResult.ValueUnsafe() is not null) {
      _dbSet.Remove(entityResult.ValueUnsafe());
      Right(entityResult.ValueUnsafe());
    }
  }

  public Either<string, IEnumerable<T>> DeleteRange(IEnumerable<T> entities)
  {
    try
    {
      _dbSet.RemoveRange(entities);
      return Right(entities); // Success: return the deleted entities
    }
    catch (Exception ex)
    {
      return Left<string, IEnumerable<T>>(ex.Message); // Failure: return the error message
    }
  }

  public void Delete(T entity)
  {
    _dbSet.Remove(entity);
  }

  public async Task<Either<string, int>> SaveChangesAsync() {
    try {
      int number = await context.SaveChangesAsync();
      return Right(number);
    } catch (Exception ex) {
      return Left<string, int>(ex.Message);
    }

  }


}
