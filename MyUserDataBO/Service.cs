using System;

namespace MyUserDataBO
{
    public class Service
    {
        public string GetDateTime()
        {
            return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss fff");
        }
    }
}
