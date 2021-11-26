using System;
using System.Collections.Generic;
using System.Text;

namespace i2gawa.Core.Service.Common
{
    public interface IServiceResult
    {
        bool Success { get; }
        string Data { get; }        
    }

    public class ServiceResult<TResult> : IServiceResult
    {
        public bool Success { get; }        
        public string Message { get; }
        public string Data { get; }

        public ServiceResult(bool res, string data, string mess)
        {
            Data = data;
            Message = mess;
            Success = res;
        }       
    }
}
