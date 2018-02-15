using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Help12306
{

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
            swz_num = string.IsNullOrEmpty(data[32]) ? "无" : data[32];
            zy_num = string.IsNullOrEmpty(data[31]) ? "无" : data[31];
            ze_num = string.IsNullOrEmpty(data[30]) ? "无" : data[30];
            yz_num = string.IsNullOrEmpty(data[20]) ? "无" : data[20];
            yw_num = string.IsNullOrEmpty(data[28]) ? "无" : data[28];
            wz_num = string.IsNullOrEmpty(data[26]) ? "无" : data[26];
            rz_num = string.IsNullOrEmpty(data[24]) ? "无" : data[24];
            rw_num = string.IsNullOrEmpty(data[23]) ? "无" : data[23];
            tz_num = string.IsNullOrEmpty(data[25]) ? "无" : data[25];
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
