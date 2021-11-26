using i2gawa.Core.Service.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace i2gawa.Application.Interface
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<T> GetByIdSyncYield(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        List<T> GetAllSyncYield();
        Task<List<T>> GetAllAsyncYield();
        Task<int> AddAsync(T entity);        
        Task<int> UpdateAsync(T entity);
        Task<int> DeleteAsync(int id);
        Task<ServiceResult<bool>> AddEntityAsync(T entity);
        Task<ServiceResult<bool>> UpdateEntityAsync(T entity);
        Task<ServiceResult<bool>> DeleteEntityAsync(int id);
        ServiceResult<bool> GetAll();
        ServiceResult<bool> GetAllById(int id);        
    }
}
