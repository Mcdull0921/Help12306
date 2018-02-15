using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Help12306
{
    class CityDictionary
    {
        Dictionary<string, string> value;
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

        public string this[string key]
        {
            get
            {
                if (value.ContainsKey(key))
                    return value[key];
                return null;
            }
        }
    }
}
