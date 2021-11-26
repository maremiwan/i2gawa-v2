using i2gawa.Core.DataContract.Response;
using i2gawa.Core.Model.Field;
using i2gawa.Core.Service.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace i2gawa.Application.Interface
{
    public interface IFieldRepository
    {
        Task<Response> AddAsync(Field entity);
        Task<Response> UpdateAsync(Field entity);
        Task<Response> DeleteAsync(int id);
        Response Get();
        Response GetById(int id);
        Response GetByMenuId(string menuid);
    }
}
