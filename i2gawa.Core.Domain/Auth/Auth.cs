using i2gawa.Core.DataContract.Response;
using i2gawa.Core.Domain.User;
using i2gawa.Core.Model.Profile;
using i2gawa.Core.Model.User;
using i2gawa.Core.Service.Common;
using i2gawa.Core.Service.Interface;
using i2gawa.Core.Util.BlowFish;
using i2gawa.Core.Util.DbAccess;
using i2gawa.Core.Util.Extentions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace i2gawa.Core.Domain.Auth
{
    public class Auth : IAuth
    {

        private static Db db = new Db();
        private readonly string key;

        public Auth(string key)
        {
            this.key = key;
        }
        

        private static readonly Func<IDataReader, Users> Make = reader =>
           new Users
           {             
               Id = reader["id"].AsInt(),
               userid = reader["userid"].AsString(),
               employeeid = reader["employeeid"].AsString(),
               password = reader["password"].AsString(),
               lastlogin = reader["notes"].AsDateTime(),
               startdate = reader["startdate"].AsDateTime(),
               enddate = reader["enddate"].AsDateTime(),
               emailconfirmed = reader["emailconfirmed"].AsInt(),
               deleted = reader["deleted"].AsInt(),
               active = reader["active"].AsInt(),
               islock = reader["islock"].AsInt(),
               usertype = reader["usertype"].AsInt(),
               notes = reader["notes"].AsString(),
               createdby = reader["createdby"].AsString(),
               createddate = reader["createddate"].AsDateTime(),
               changeby = reader["changeby"].AsString(),
               changedate = reader["changedate"].AsDateTime()                         
           };

        public ServiceResult<bool> Authentication(string username, string password)
        {
            i2gawaAuthenticationResult result = new i2gawaAuthenticationResult();
            var tokenuser = "";            
            string json = "";
            var nowdate = DateTime.Now;
            String Message = "";
            bool success = false;
            i2gawaBlowfish encrypt = new i2gawaBlowfish();
            var PassEncrypt = encrypt.EncryptString(password);
            string Sql = @"SELECT id
                          ,userid
                          ,employeeid
                          ,password
                          ,lastlogin
                          ,startdate
                          ,enddate
                          ,emailconfirmed
                          ,deleted
                          ,active
                          ,islock
                          ,usertype
                          ,notes
                          ,createdby
                          ,createddate
                          ,changeby
                          ,changedate
                      FROM sysuser where userid = '" + username + "' and password = '" + PassEncrypt + "'";
            var res = db.ReadSync(Sql, CommandType.Text, Make).ToList();
            var UserData = res.FirstOrDefault();
            if (UserData != null)
            {
                Message = "Login Success";
                success = true;
                
                if (UserData.active == 0)
                {
                    Message = "User Not Active";
                    success = false;
                }
                if (UserData.deleted == 1)
                {
                    Message = "User Is Deleted";
                    success = false;
                }
                if (UserData.islock == 1)
                {
                    Message = "User Locked";
                    success = false;
                }

                var LasLogin = UserData.lastlogin;
                if(LasLogin.Year > 1)
                {
                    var lastlogindays = nowdate - UserData.lastlogin;
                    if (lastlogindays.Days > 90)
                    {
                        Message = "User Locked";
                        success = false;
                        updateLock(username);
                    }
                }



                // 1. Create Security Token Handler
                var tokenHandler = new JwtSecurityTokenHandler();

                // 2. Create Private Key to Encrypted
                var tokenKey = Encoding.ASCII.GetBytes(key);

                //3. Create JETdescriptor
                var tokenDescriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(
                        new Claim[]
                        {
                        new Claim(ClaimTypes.Name, username)
                        }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
                };
                //4. Create Token
                var token = tokenHandler.CreateToken(tokenDescriptor);

                // 5. Return Token from method
                tokenuser = tokenHandler.WriteToken(token);
                if(UserData.active == 0 || UserData.deleted == 1)
                {
                    tokenuser = "";
                }
                else
                {
                    updatetoken(username, tokenuser);
                    updatelogin(username);
                    var MenuAccess = GetProfileMenu(username);
                    string DataContMenu = JsonConvert.SerializeObject(MenuAccess, Formatting.Indented);
                    var MenuJson = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(DataContMenu);
                    result.Menu = MenuJson;
                }
            }
            else
            {
                Message = "Login Failed, Invalid Username or Password";
                success = false;
            }
            
            string DataCont = JsonConvert.SerializeObject(res, Formatting.Indented);
            var dictionaries = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(DataCont);
            result.Message = Message;
            result.Success = success;
            result.Status = success;
            result.Data = dictionaries;            
            result.Token = tokenuser;
            json = JsonConvert.SerializeObject(result, Formatting.Indented);
            var i2gawaresult = new ServiceResult<bool>(success, json, Message);
            return i2gawaresult;
        }

        public void updateLock(string userid)
        {          
            string Sql = @"update sysuser set islock = 1 where userid = @userid";
            var res = db.Update(Sql, CommandType.Text, Take(userid));            
        }

        public void updatetoken(string userid, string token)
        {
            DateTime lastlogin = DateTime.Now;
            string Sql = @"update sysuser set token = '" + token + "' where userid = @userid";
            var res = db.Update(Sql, CommandType.Text, Take(userid));
        }

        public void updatelogin(string userid)
        {
            var lastlogin = DateTime.Now;
            string Sql = @"update sysuser set lastlogin = '" + lastlogin + "' where userid = @userid";
            var res = db.Update(Sql, CommandType.Text, Take(userid));
        }

        public 

        object[] Take(string userid)
        {
            return new object[]
            {
                "@userid", userid
            };
        }

        public List<ProfileMenuUsers> GetProfileMenu(string username)
        {
            string Sql = @"SELECT DISTINCT
	                        sysuser.userid,
	                        sysuser.employeeid,
	                        sysuserrole.roleid,
	                        sysrole.rolename,
	                        sysroleprofile.profileid,
	                        sysprofile.profilename,
	                        sysprofilemenu.menuid,
	                        sysmenutext.menu,
	                        sysmenu.parentmenuid,
	                        sysmenu.ismenugroup,
	                        sysmenu.sequence,
	                        sysmenu.targeturl,
	                        sysmenu.iconcls,
	                        sysmenu.notes,
	                        sysmenu.menuactiveclass,
	                        sysprofilemenu.actionadd,
	                        sysprofilemenu.actionedit,
	                        sysprofilemenu.actiondelete,
	                        sysprofilemenu.actionread 
                        FROM
	                        dbo.sysuser
	                        INNER JOIN dbo.sysuserrole ON sysuser.userid = sysuserrole.userid and sysuserrole.active = 1
	                        INNER JOIN dbo.sysrole ON sysuserrole.roleid = sysrole.roleid and sysrole.active = 1
	                        INNER JOIN dbo.sysroleprofile ON sysrole.roleid = sysroleprofile.roleid and sysroleprofile.active = 1
	                        INNER JOIN dbo.sysprofile ON sysroleprofile.profileid = sysprofile.profileid and sysprofile.active = 1
	                        INNER JOIN dbo.sysprofilemenu ON sysprofile.profileid = sysprofilemenu.profileid and sysprofilemenu.active = 1
	                        INNER JOIN dbo.sysmenu ON sysprofilemenu.menuid = sysmenu.menuid and sysmenu.active = 1
	                        INNER JOIN dbo.sysmenutext ON sysmenu.menuid = sysmenutext.menuid and sysmenutext.active = 1
                        WHERE
	                        dbo.sysuser.userid = @username
	                        ORDER BY sysmenu.sequence ASC";
            var res = db.ReadSync(Sql, CommandType.Text, MakeProfileMenu, TakeUserid(username)).ToList();
            return res;
        }

        private static readonly Func<IDataReader, ProfileMenuUsers> MakeProfileMenu = reader =>
          new ProfileMenuUsers
          {
              userid = reader["userid"].AsString(),
              employeeid = reader["employeeid"].AsString(),
              roleid = reader["roleid"].AsString(),
              rolename = reader["rolename"].AsString(),
              profileid = reader["profileid"].AsString(),
              profilename = reader["profilename"].AsString(),
              menuid = reader["menuid"].AsString(),
              menu = reader["menu"].AsString(),
              parentmenuid = reader["parentmenuid"].AsString(),
              ismenugroup = reader["ismenugroup"].AsInt(),
              sequence = reader["sequence"].AsInt(),
              targeturl = reader["targeturl"].AsString(),
              iconcls = reader["iconcls"].AsString(),
              notes = reader["notes"].AsString(),
              menuactiveclass = reader["menuactiveclass"].AsString(),
              Actionadd = reader["Actionadd"].AsInt(),
              Actionedit = reader["Actionedit"].AsInt(),
              Actiondelete = reader["Actiondelete"].AsInt(),
              Actionread = reader["Actionread"].AsInt()
          };

        object[] TakeUserid(string username)
        {
            return new object[]
            {
                "@username", username
            };
        }
    }
}
