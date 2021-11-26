using System;
using System.Collections.Generic;
using System.Text;

namespace i2gawa.Core.DataContract.Response
{
    public class i2gawaResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
        public List<Dictionary<string, object>> Data { get; set; }        
    }

    public class i2gawaAuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public bool Status { get; set; }
        public List<Dictionary<string, object>> Data { get; set; }
        public List<Dictionary<string, object>> Menu { get; set; }
    }

    public class i2gawaExecuteResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }                
    }

    public class Response
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public Response(bool status, string message, object data)
        {
            IsSuccess = status;
            Message = message;
            Data = data;
        }
    }
}
