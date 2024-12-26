using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using LanguageExt;
using VideoGameApi;

namespace CleanArchitecture.Infrastructure.Interface;

public interface DBError {
  int Type { get; }  // Properties are defined with the `get` accessor
}

public interface IGenericRepository<T> where T : class
{


  Task<Either<string, List<T>>> GetAllAsync();

  Task<Either<string, T>> AddAsync(T entity);
  Task<Either<string, IEnumerable<T>>> AddRangeAsync(IEnumerable<T> entities);
  Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
  Task<bool> AnyAsync();
  Task<int> CountAsync(Expression<Func<T, bool>> filter);
  Task<int> CountAsync();
  Task<Either<string, T>> GetByIdAsync(object id);
  Task<Pagination<TResult>> ToPagination<TResult>(
      int pageIndex,
      int pageSize,
      Expression<Func<T, bool>>? filter = null,
      Func<IQueryable<T>, IQueryable<T>>? include = null,
      Expression<Func<T, object>>? orderBy = null,
      bool ascending = true,
      Expression<Func<T, TResult>> selector = null);
  Task<T?> FirstOrDefaultAsync(
      Expression<Func<T, bool>> filter,
      Func<IQueryable<T>, IQueryable<T>>? include = null);
  Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, Expression<Func<T, object>> sort, bool ascending = true);
  Either<string, T> Update(T entity);
  Either<string, IEnumerable<T>> UpdateRange(IEnumerable<T> entities);
  void Delete(T entity);
  Either<string, IEnumerable<T>> DeleteRange(IEnumerable<T> entities);
  Task DeleteAsync(object id);
  Task<Either<string, int>> SaveChangesAsync();
}


public class Pagination<T>(List<T> items, int count, int pageIndex, int pageSize)
{
  public int CurrentPage { get; private set; } = pageIndex;
  public int TotalPages { get; private set; } = (int)Math.Ceiling(count / (double)pageSize);
  public int PageSize { get; private set; } = pageSize;
  public int TotalCount { get; private set; } = count;
  public bool HasPrevious => CurrentPage > 1;
  public bool HasNext => CurrentPage < TotalPages;
  public List<T>? Items { get; private set; } = items;

  public static async Task<Pagination<T>> ToPagedList(IQueryable<T> source, int pageIndex, int pageSize)
  {
    pageIndex = pageIndex <= 0 ? 1 : pageIndex;
    pageSize = pageSize <= 0 ? 10 : pageSize;

    var count = source.Count();
    var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
    return new Pagination<T>(items, count, pageIndex, pageSize);
  }
}
