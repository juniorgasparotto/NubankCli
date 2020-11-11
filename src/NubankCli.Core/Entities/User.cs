using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NubankCli.Core.Extensions;
using NubankCli.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NubankCli.Core.Entities
{
    [DebuggerDisplay("UserName: {UserName}")]
    public class User
    {
        public string UserName { get; }

        [JsonIgnore]
        public string UserInfoPath => Path.Combine(GetPath(), "user-info.json");

        public class UserInfo
        {
            public string Token { get; set; }
            public Dictionary<string, string> AutenticatedUrls { get; set; }

            public DateTime GetExpiredDate()
            {
                if (Token == null)
                    return DateTime.MinValue;
                    
                var jobject = JwtDecoder.GetClaims(Token);
                var exp = jobject["exp"].Value<int>();
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(exp);
                return dateTimeOffset.LocalDateTime;
            }

            public bool IsValid()
            {
                var exp = GetExpiredDate();
                if (exp > DateTime.Now)
                    return true;

                return false;
            }
        }

        public User(string userName)
        {
            UserName = userName;
        }

        public string GetPath()
        {
            return Path.Combine(EnvironmentExtensions.ProjectRootOrExecutionDirectory, "UsersData", UserName);
        }

        public UserInfo GetUserInfo()
        {
            if (File.Exists(UserInfoPath))
            {
                string json = File.ReadAllText(UserInfoPath);
                return JsonConvert.DeserializeObject<UserInfo>(json);
            }

            return null;
        }

        //public void SaveUserInfo(UserInfo userInfo)
        //{
        //    File.WriteAllText(UserInfoPath, JsonConvert.SerializeObject(userInfo, Formatting.Indented));
        //}

        //public void ClearUserInfo()
        //{
        //    File.Delete(UserInfoPath);
        //}
    }
}
