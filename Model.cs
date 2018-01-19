using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;

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

    class CityDictionary
    {
        public Dictionary<string, string> value { get; private set; }
        public CityDictionary()
        {
            value = new Dictionary<string, string>();
            value.Add("荆州", "JBN");
            value.Add("汉口", "HKN");
            value.Add("荆门", "JMN");
            value.Add("武汉", "WHN");
            value.Add("北京", "BJP");
            value.Add("上海", "SHH");
            value.Add("天津", "TJP");
            value.Add("重庆", "CQW");
            value.Add("长沙", "CSQ");
            value.Add("长春", "CCT");
            value.Add("杭州", "HZH");
            value.Add("深圳", "SZQ");
            value.Add("广州", "GZQ");
            value.Add("广州南", "IZQ");
            value.Add("潜江", "QJN");
        }
    }

    class Passenger
    {
        public string name { get; set; }
        public string idCard { get; set; }
        public string mobile { get; set; }
        public string seat { get; set; }
        public string ticket { get; set; }
    }

    abstract class Info
    {
        class Private_Info : Info
        {

        }
        private static Private_Info info;
        static Info()
        {
            info = new Private_Info();
        }

        public string user { get; private set; }
        public string password { get; private set; }
        public List<string> traincodes { get; private set; }
        public string traindate { get; private set; }
        public string from { get; private set; }
        public string to { get; private set; }
        public Passenger[] passengers { get; private set; }

        public static Info GetInfo(string xmlPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                info.user = doc.DocumentElement["User"].InnerText;
                info.password = doc.DocumentElement["Password"].InnerText;
                info.traincodes = new List<string>();
                foreach (XmlNode p in doc.DocumentElement["Traincodes"].ChildNodes)
                {
                    info.traincodes.Add(p.InnerText);
                }
                info.traindate = doc.DocumentElement["Traindate"].InnerText;
                info.from = doc.DocumentElement["From"].InnerText;
                info.to = doc.DocumentElement["To"].InnerText;
                List<Passenger> list = new List<Passenger>();
                foreach (XmlNode p in doc.DocumentElement["Passengers"].ChildNodes)
                {
                    list.Add(new Passenger
                    {
                        name = p.Attributes["name"].Value,
                        idCard = p.Attributes["idCard"].Value,
                        mobile = p.Attributes["mobile"].Value,
                        seat = p.Attributes["seat"].Value,
                        ticket = p.Attributes["ticket"].Value,
                    });
                }
                info.passengers = list.ToArray();
                return info;
            }
            catch
            {
                return null;
            }
        }
    }

    class TrainInfo
    {
        public TrainInfo(string obj)
        {
            //train_no = obj.queryLeftNewDTO.train_no.Value;
            //traincode = obj.queryLeftNewDTO.station_train_code.Value;
            //from_station_telecode = obj.queryLeftNewDTO.from_station_telecode;
            //to_station_telecode = obj.queryLeftNewDTO.to_station_telecode;
            //secretStr = obj.secretStr.Value;
            //startTime = obj.queryLeftNewDTO.start_time.Value;
            //arriveTime = obj.queryLeftNewDTO.arrive_time.Value;
            //swz_num = obj.queryLeftNewDTO.swz_num.Value;
            //zy_num = obj.queryLeftNewDTO.zy_num.Value;
            //ze_num = obj.queryLeftNewDTO.ze_num.Value;
            //yz_num = obj.queryLeftNewDTO.yz_num.Value;
            //yw_num = obj.queryLeftNewDTO.yw_num.Value;
            //wz_num = obj.queryLeftNewDTO.wz_num.Value;
            //rz_num = obj.queryLeftNewDTO.rz_num.Value;
            //rw_num = obj.queryLeftNewDTO.rw_num.Value;
            //tz_num = obj.queryLeftNewDTO.tz_num.Value;


            var data = obj.Split("|".ToCharArray());
            secretStr = data[0];
            train_no = data[2];
            traincode = data[3];
            from_station_telecode = data[6];
            to_station_telecode = data[7];
            startTime = data[8];
            arriveTime = data[9];
            swz_num = data[32];
            zy_num = data[31];
            ze_num = data[30];
            yz_num = data[20];
            yw_num = data[28];
            wz_num = data[26];
            rz_num = data[24];
            rw_num = data[23];
            tz_num = data[25];
        }

        public string from_station_telecode { get; private set; }
        public string to_station_telecode { get; private set; }
        /// <summary>
        /// 编号
        /// </summary>
        public string train_no { get; private set; }
        /// <summary>
        /// 车次
        /// </summary>
        public string traincode { get; private set; }
        /// <summary>
        /// 车次提交串码
        /// </summary>
        public string secretStr { get; private set; }
        /// <summary>
        /// 发车时间
        /// </summary>
        public string startTime { get; private set; }
        /// <summary>
        /// 到站时间
        /// </summary>
        public string arriveTime { get; private set; }
        /// <summary>
        /// 商务座
        /// </summary>
        public string swz_num { get; private set; }
        /// <summary>
        /// 一等座
        /// </summary>
        public string zy_num { get; private set; }
        /// <summary>
        /// 二等座
        /// </summary>
        public string ze_num { get; private set; }
        /// <summary>
        /// 硬座
        /// </summary>
        public string yz_num { get; private set; }
        /// <summary>
        /// 硬卧
        /// </summary>
        public string yw_num { get; private set; }
        /// <summary>
        /// 无座
        /// </summary>
        public string wz_num { get; private set; }
        /// <summary>
        /// 软座
        /// </summary>
        public string rz_num { get; private set; }
        /// <summary>
        /// 软卧
        /// </summary>
        public string rw_num { get; private set; }
        /// <summary>
        /// 特等座
        /// </summary>
        public string tz_num { get; private set; }
    }
}
