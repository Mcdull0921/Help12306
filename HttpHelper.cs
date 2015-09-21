using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Help12306
{
    class HttpHelper
    {

        private static int delay = 1000;
        private static int maxTry = 300;
        private static Encoding encoding = Encoding.GetEncoding("utf-8");

        public static int NetworkDelay
        {
            get
            {
                Random r = new Random();
                return (r.Next(delay, delay * 2));
            }
            set
            {
                delay = value;
            }
        }

        /// <summary>
        /// Content-type Http标头的值
        /// </summary>
        private static string ContentType
        {
            get
            {
                return "application/x-www-form-urlencoded";
            }
        }

        /// <summary>
        /// Accept Http标头的值
        /// </summary>
        private static string Accept
        {
            get
            {
                return "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/x-silverlight, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, application/x-silverlight-2-b1, */*";
            }
        }

        /// <summary>
        /// User-agent Http标头的值
        /// </summary>
        private static string UserAgent
        {
            get
            {
                return "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022)";
            }
        }


        #region
        /// <summary>
        /// 获取HTML
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="postData">post 提交的字符串</param>
        /// <param name="isPost">是否是post</param>
        /// <param name="cookieContainer">CookieContainer</param>
        /// <returns>html </returns>
        public static string GetHtml(string url, string postData, bool isPost, CookieContainer cookieContainer)
        {
            Thread.Sleep(NetworkDelay);

            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.ContentType = ContentType;
                httpWebRequest.ServicePoint.ConnectionLimit = maxTry;
                httpWebRequest.Referer = url;
                httpWebRequest.Accept = Accept;
                httpWebRequest.UserAgent = UserAgent;
                httpWebRequest.Method = isPost ? "POST" : "GET";
                httpWebRequest.ContentLength = 0;
                if (!string.IsNullOrEmpty(postData))
                {
                    byte[] byteRequest = Encoding.Default.GetBytes(postData);
                    httpWebRequest.ContentLength = byteRequest.Length;
                    Stream stream = httpWebRequest.GetRequestStream();
                    stream.Write(byteRequest, 0, byteRequest.Length);
                    stream.Dispose();
                    stream.Close();
                }

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, encoding);
                string html = streamReader.ReadToEnd();
                //StringBuilder sb = new StringBuilder();
                //char[] buffer = new char[1024];
                //int index = streamReader.Read(buffer, 0, 1024);
                //while (index > 0)
                //{
                //    var s = new string(buffer);
                //    sb.Append(s);
                //    index = streamReader.Read(buffer, 0, 1024);
                //}
                streamReader.Dispose();
                streamReader.Close();
                responseStream.Dispose();
                responseStream.Close();

                return html;
            }
            catch
            {
                if (httpWebRequest != null)
                {
                    httpWebRequest.Abort();
                }
                if (httpWebResponse != null)
                {
                    httpWebResponse.Close();
                }
                return string.Empty;
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
        #endregion

        #region 获取数据流
        public static Stream GetStream(string url, CookieContainer cookieContainer)
        {
            //Thread.Sleep(delay); 

            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;

            try
            {
                //获取图片流，这里是https并且证书未认证需加上这个
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.ContentType = ContentType;
                httpWebRequest.ServicePoint.ConnectionLimit = maxTry;
                httpWebRequest.Accept = Accept;
                httpWebRequest.UserAgent = UserAgent;
                httpWebRequest.Referer = url;
                httpWebRequest.Method = "GET";

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                return responseStream;
            }
            catch
            {
                if (httpWebRequest != null)
                {
                    httpWebRequest.Abort();
                }
                if (httpWebResponse != null)
                {
                    httpWebResponse.Close();
                }
                return null;
            }
        }
        #endregion

        /// <summary>
        /// 中文字符转html编码
        /// </summary>
        /// <param name="unicodeString"></param>
        /// <returns></returns>
        public static string ConvertToUrlUtf8(string unicodeString)
        {
            Byte[] encodedBytes = Encoding.UTF8.GetBytes(unicodeString);
            unicodeString = "";
            foreach (Byte b in encodedBytes)
            {
                //[0-9a-zA-Z_]
                if ((b >= 48 && b <= 57) || (b >= 65 && b <= 90) || (b >= 97 && b <= 122) || b == 95)
                    unicodeString += (char)b;
                else
                    unicodeString += "%" + Convert.ToString(b, 16).ToUpper();
            }
            return unicodeString;
        }

        /// <summary>
        /// 发送邮件（只发送单篇邮件）
        /// </summary>
        /// <param name="title">邮件标题</param>
        /// <param name="content">邮件正文</param>
        /// <param name="receiveAddress">收件人地址</param>
        /// <param name="receiver">收件人姓名</param>
        /// <param name="sender">发件人姓名</param>
        /// <returns>是否发送成功</returns>
        public static bool SendEmail(string title, string content, string sender)
        {
            string SmtpServer = "smtp.163.com";
            string User = "alphatestmail@163.com";
            string Pwd = "alpha123456";
            try
            {
                System.Net.Mail.SmtpClient client;                             //邮件客户端
                client = new System.Net.Mail.SmtpClient(SmtpServer);           //实例化对象，参数为smtp服务器
                client.Timeout = 60000;                                        //邮件发送延迟一分钟
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(User, Pwd);    //登录名与密码
                client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();


                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.From = new System.Net.Mail.MailAddress(User, sender, System.Text.Encoding.UTF8);  // 【发件人地址,发件人称呼[发件人姓名]】--发件人地址需要与SMTP登录名保持一致
                message.To.Add(new System.Net.Mail.MailAddress("53455375@qq.com", "", System.Text.Encoding.UTF8));
                message.IsBodyHtml = false;
                message.Subject = title.Replace("\r", "").Replace("\n", "").Trim();
                message.Body = content;

                client.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
