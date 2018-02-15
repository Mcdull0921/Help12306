using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Help12306
{
    abstract class Info
    {
        public class Passenger
        {
            public string name { get; set; }
            public string idCard { get; set; }
            public string mobile { get; set; }
            public string seat { get; set; }
            public string ticket { get; set; }
        }

        public class Train
        {
            public string from { get; set; }
            public string to { get; set; }
            public string date { get; set; }
            public int start { get; set; }
            public int end { get; set; }
            public string fromCode { get; set; }
            public string toCode { get; set; }
        }
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
        public Passenger[] passengers { get; private set; }
        public Train[] trains { get; private set; }

        public static Info GetInfo(string xmlPath)
        {
            CityDictionary cd = new CityDictionary();
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            info.user = doc.DocumentElement["User"].InnerText;
            info.password = doc.DocumentElement["Password"].InnerText;
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
            List<Train> trains = new List<Train>();
            foreach (XmlNode p in doc.DocumentElement["Trains"].ChildNodes)
            {
                var t = new Train
                {
                    date = p.Attributes["date"].Value,
                    from = p.Attributes["from"].Value,
                    to = p.Attributes["to"].Value,
                    start = int.Parse(p.Attributes["start"].Value),
                    end = int.Parse(p.Attributes["end"].Value)
                };
                t.fromCode = cd[t.from];
                t.toCode = cd[t.to];
                if (string.IsNullOrEmpty(t.fromCode) || string.IsNullOrEmpty(t.toCode))
                    throw new Exception("未能找到城市代码");
                trains.Add(t);
            }
            info.trains = trains.ToArray();
            return info;
        }
    }
}
