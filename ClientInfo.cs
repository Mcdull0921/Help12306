using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Help12306
{
    class ClientInfo
    {
        public string name { get; private set; }
        public string value { get; private set; }
        public CookieContainer cookieContainer { get; private set; }
        public ClientInfo()
        {
            cookieContainer = new CookieContainer();
        }
        public ClientInfo(string input, string cookie)
            : this(cookie)
        {
            SetValue(input);
        }

        public ClientInfo(string cookie)
        {
            cookieContainer = new CookieContainer();
            var arr_c = cookie.Split(';');
            foreach (var c in arr_c)
            {
                var a = c.Trim().Split('=');
                var co = new Cookie(a[0], a[1], "/", ".12306.cn");
                cookieContainer.Add(co);
            }
        }

        public void SetCookie(string name, string value)
        {
            cookieContainer.SetCookies(new Uri("https://kyfw.12306.cn"), (name + "=" + value));
        }

        public void SetValue(string input)
        {
            var arr_v = input.Split(":::".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            var arr_vv = arr_v[0].Split(",-,".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            name = arr_vv[0];
            value = arr_vv[1];
        }
    }
}
