using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;

namespace Help12306
{
    class MessageResult<T>
    {
        public string validateMessagesShowId { get; set; }
        public bool status { get; set; }
        public int httpstatus { get; set; }
        public T data { get; set; }
        public string[] messages { get; set; }
    }

    class CodeResult
    {
        public string result_message { get; set; }
        public int result_code { get; set; }
    }

    class LoginResult : CodeResult
    {
        public string uamtk { get; set; }
    }

    class AppTokenResult : CodeResult
    {
        public string apptk { get; set; }
        public string newapptk { get; set; }
    }

    class DeviceResult
    {
        public string exp { get; set; }
        public string dfp { get; set; }
    }

    public class TrainData
    {
        public string[] result;
    }
}
