using i2gawa.Core.Model.Profile;
using i2gawa.Core.Service.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace i2gawa.Core.Domain.Auth
{
    public interface IAuth
    {        
        ServiceResult<bool> Authentication(string username, string password);
        List<ProfileMenuUsers> GetProfileMenu(string username);
    }
}
