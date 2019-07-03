using MyUserDataModels.Models;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MyUserViewObjects;

namespace MyUserDataBO
{
    public class Service
    {
        //private MyBusinessDemoDBContext _context;
        //public Service(MyBusinessDemoDBContext context)
        //{
        //    _context = context;
        //}
        public string GetDateTime()
        {
            return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss fff");
        }

        public IEnumerable<User> GetUsers()
        {
            MyBusinessDemoDBContext context = new MyBusinessDemoDBContext();

            var result = context.User.ToList();

            return result;
        }

        public bool Login(UserVO login)
        {
            return login != null && !string.IsNullOrEmpty(login.UserId);
        }
    }
}
