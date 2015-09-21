using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;
using System.Text.RegularExpressions;
using System.Media;

namespace Help12306
{
    public partial class FormCode : Form
    {
        #region 服务器地址
        //初始页面地址
        private const string Url_Init = "https://kyfw.12306.cn/otn/leftTicket/init";
        //获取登录验证码地址
        private const string Url_LoginCode = "https://kyfw.12306.cn/otn/passcodeNew/getPassCodeNew?module=login&rand=sjrand";
        //获取确认订单验证码地址
        private const string Url_OrderCode = "https://kyfw.12306.cn/otn/passcodeNew/getPassCodeNew?module=passenger&rand=randp&";
        //验证码识别地址
        private const string Url_CheckCode = "https://kyfw.12306.cn/otn/passcodeNew/checkRandCodeAnsyn";
        //用户登录地址
        //private const string Url_Login = "https://kyfw.12306.cn/otn/login/loginUserAsyn";
        private const string Url_Login = "https://kyfw.12306.cn/otn/login/loginAysnSuggest";
        //查询车次
        private const string Url_Query = "https://kyfw.12306.cn/otn/leftTicket/query?leftTicketDTO.train_date={0}&leftTicketDTO.from_station={1}&leftTicketDTO.to_station={2}&purpose_codes=ADULT";
        //订单提交地址
        private const string Url_Submit = "https://kyfw.12306.cn/otn/leftTicket/submitOrderRequest";
        //确认订单初始化地址
        private const string Url_ConfirmInit = "https://kyfw.12306.cn/otn/confirmPassenger/initDc";
        //订单生成地址
        private const string Url_Confirm = "https://kyfw.12306.cn/otn/confirmPassenger/confirmSingleForQueue";
        //检查订单信息
        private const string Url_CheckOrderInfo = "https://kyfw.12306.cn/otn/confirmPassenger/checkOrderInfo";
        #endregion

        #region 成员变量
        private SoundPlayer spMsg = new SoundPlayer(Help12306.Properties.Resources.msg);
        private Info info;  //外部购票信息
        private ClientInfo clientInfo; //客户端保存信息
        private WebBrowser browser;
        private TrainInfo submitTrain;     //提交车次
        private dynamic ticketInfoForPassengerForm;   //隐藏表单
        private string globalRepeatSubmitToken;  //全局请求凭据
        private bool isLogin;
        private string fromCode;
        private string toCode;
        private string passengerTicketStr;
        private string oldPassengerStr;
        //string passengerTicketStr = "O%2C0%2C1%2C%E8%AE%B8%E7%81%B5%E9%B9%A4%2C1%2C420822199004134008%2C18571475918%2CN_O%2C0%2C1%2C%E8%82%96%E5%81%A5%2C1%2C421002198709210514%2C13871484332%2CN";
        //string oldPassengerStr = "%E8%AE%B8%E7%81%B5%E9%B9%A4%2C1%2C420822199004134008%2C1_%E8%82%96%E5%81%A5%2C1%2C421002198709210514%2C1_";
        //验证码图片矩阵
        private Point[] CheckCodeArray = new Point[8] { new Point(40, 45), new Point(110, 45), new Point(190, 45), new Point(260, 45), new Point(40, 115), new Point(110, 115), new Point(190, 115), new Point(260, 115) };
        //验证码提交字符串
        private string Code
        {
            get
            {
                return _code;
            }
            set
            {
                _code = "";
                var arr = value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var a in arr)
                {
                    var p = CheckCodeArray[Int32.Parse(a) - 1];
                    _code += string.Format("{0},{1},", p.X, p.Y);
                }
                if (!string.IsNullOrEmpty(_code) && _code.Length > 0)
                    _code = _code.Substring(0, _code.Length - 1);
            }
        }
        private string _code;
        #endregion

        public FormCode()
        {
            InitializeComponent();
        }

        private void FormCode_Load(object sender, EventArgs e)
        {
            this.Visible = false;
            this.ShowInTaskbar = false;
            this.isLogin = true;
            info = Info.GetInfo(Environment.CurrentDirectory + "\\Info.xml");
            if (info == null)
            {
                Console.WriteLine("信息读取失败，请检查配置文件Info.xml是否存在或格式是否有误！");
                return;
            }
            Console.WriteLine("出发地：" + info.from);
            Console.WriteLine("目的地：" + info.to);
            Console.WriteLine("备选车次：");
            for (int i = 0; i < info.traincodes.Count; i++)
                Console.WriteLine("{0}.{1}", i + 1, info.traincodes[i]);
            Console.WriteLine("出发日：" + info.traindate);
            Console.WriteLine("乘客信息：");
            for (int i = 0; i < info.passengers.Length; ++i)
            {
                Console.WriteLine(string.Format("第{0}位乘客，姓名：{1}，身份证号：{2}，手机号：{3}，座次：{4}，票类：{5}", i + 1, info.passengers[i].name, info.passengers[i].idCard, !string.IsNullOrEmpty(info.passengers[i].mobile) ? info.passengers[i].mobile : "无", info.passengers[i].seat, info.passengers[i].ticket));
            }
            Console.WriteLine("是否邮件通知：{0}", info.sendMail ? "是" : "否");
            browser = new WebBrowser();
            browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);
            Console.WriteLine("输入y开始抢票：");
            if (Console.Read() == 'y')
            {
                if (init())
                {
                    Console.WriteLine("重置页面，开始获取隐藏表单值");
                    browser.Navigate(Url_Init);
                }
                else
                    Console.WriteLine("初始化失败");
            }
        }

        private bool init()
        {
            try
            {
                CityDictionary cd = new CityDictionary();
                if (!cd.value.ContainsKey(info.from))
                    return false;
                fromCode = cd.value[info.from];
                if (!cd.value.ContainsKey(info.to))
                    return false;
                toCode = cd.value[info.to];
                foreach (var p in info.passengers)
                {
                    //seat_type,0,ticket_type,name,id_type,id_no,phone_no,save_status(N,Y)_
                    //seat_type:1硬座 2软座 3硬卧卧 O二等座 M一等座
                    //ticket_type:1成人票 2儿童票 3学生票 4残军票
                    passengerTicketStr += string.Format("{3},0,{4},{0},1,{1},{2},N_", p.name, p.idCard, p.mobile, p.seat, p.ticket);
                    //c.name,c.id_type,c.id_no,c.passenger_type_
                    oldPassengerStr += string.Format("{0},1,{1},1_", p.name, p.idCard);
                }
                passengerTicketStr = HttpHelper.ConvertToUrlUtf8(passengerTicketStr.Substring(0, passengerTicketStr.Length - 1));
                oldPassengerStr = HttpHelper.ConvertToUrlUtf8(oldPassengerStr);
                return true;
            }
            catch { return false; }
        }

        void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                if (isLogin)
                {
                    //string value = browser.Document.InvokeScript("submitForm").ToString(); ;
                    //if (!string.IsNullOrEmpty(value))
                    //{
                    //    clientInfo = new ClientInfo(value, browser.Document.Cookie);
                    //    browser.Stop();
                    //    browser.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);
                    //    Console.WriteLine("隐藏表单获取完成,开始获取验证码");
                    //    this.pictureCode.Image = Image.FromStream(HttpHelper.GetStream(Url_LoginCode, clientInfo.cookieContainer));
                    //    this.Visible = true;
                    //    this.txtCode.Text = "";
                    //    this.txtCode.Focus();
                    //    Console.WriteLine("请输入验证码");
                    //}
                    clientInfo = new ClientInfo(browser.Document.Cookie);
                    browser.Stop();
                    browser.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);
                    Console.WriteLine("隐藏表单获取完成,开始获取验证码");
                    this.pictureCode.Image = Image.FromStream(HttpHelper.GetStream(Url_LoginCode, clientInfo.cookieContainer));
                    this.Visible = true;
                    this.txtCode.Text = "";
                    this.txtCode.Focus();
                    Console.WriteLine("请输入验证码");
                }
                else
                {
                    //string value = browser.Document.InvokeScript("submitForm").ToString(); ;
                    //if (!string.IsNullOrEmpty(value))
                    //{
                    //    clientInfo.SetValue(value);
                    //    browser.Stop();
                    //    browser.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);

                    //    var html = browser.DocumentText;
                    //    Regex rx = new Regex("(?<=var ticketInfoForPassengerForm=).*;");
                    //    var m = rx.Match(html);
                    //    if (string.IsNullOrEmpty(m.Value))
                    //    {
                    //        Console.WriteLine("发生错误！");
                    //        return;
                    //    }
                    //    var s = m.Value.Substring(0, m.Value.Length - 1);
                    //    ticketInfoForPassengerForm = JsonConvert.DeserializeObject<dynamic>(s);

                    //    rx = new Regex("var globalRepeatSubmitToken.*(?=;)");
                    //    m = rx.Match(html);
                    //    if (string.IsNullOrEmpty(m.Value))
                    //    {
                    //        Console.WriteLine("发生错误！");
                    //        return;
                    //    }
                    //    var d = m.Value.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    //    globalRepeatSubmitToken = d[1].Trim(new char[] { ' ', '\'' });

                    //    this.pictureCode.Image = Image.FromStream(HttpHelper.GetStream(Url_OrderCode, clientInfo.cookieContainer));
                    //    this.Visible = true;
                    //    this.txtCode.Text = "";
                    //    this.txtCode.Focus();
                    //    Console.WriteLine("请输入验证码并确认订单");
                    //    spMsg.PlaySync();
                    //    //发送邮件
                    //    if (info.sendMail)
                    //        HttpHelper.SendEmail("火车票已抢到", string.Format("车次{0}已抢到，出发时间{1}，到达时间{2}，剩余二等座车票{3}，剩余一等座车票{4}赶紧输入验证码提交订单！", submitTrain.traincode, submitTrain.startTime, submitTrain.arriveTime, submitTrain.ze_num, submitTrain.zy_num), "mcdull");
                    //}
                    browser.Stop();
                    browser.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);

                    var html = browser.DocumentText;
                    Regex rx = new Regex("(?<=var ticketInfoForPassengerForm=).*;");
                    var m = rx.Match(html);
                    if (string.IsNullOrEmpty(m.Value))
                    {
                        Console.WriteLine("发生错误！");
                        return;
                    }
                    var s = m.Value.Substring(0, m.Value.Length - 1);
                    ticketInfoForPassengerForm = JsonConvert.DeserializeObject<dynamic>(s);

                    rx = new Regex("var globalRepeatSubmitToken.*(?=;)");
                    m = rx.Match(html);
                    if (string.IsNullOrEmpty(m.Value))
                    {
                        Console.WriteLine("发生错误！");
                        return;
                    }
                    var d = m.Value.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    globalRepeatSubmitToken = d[1].Trim(new char[] { ' ', '\'' });

                    this.pictureCode.Image = Image.FromStream(HttpHelper.GetStream(Url_OrderCode, clientInfo.cookieContainer));
                    this.Visible = true;
                    this.txtCode.Text = "";
                    this.txtCode.Focus();
                    Console.WriteLine("请输入验证码并确认订单");
                    spMsg.PlaySync();
                    //发送邮件
                    if (info.sendMail)
                        HttpHelper.SendEmail("火车票已抢到", string.Format("车次{0}已抢到，出发时间{1}，到达时间{2}，剩余二等座车票{3}，剩余一等座车票{4}赶紧输入验证码提交订单！", submitTrain.traincode, submitTrain.startTime, submitTrain.arriveTime, submitTrain.ze_num, submitTrain.zy_num), "mcdull");

                }
            }
            catch
            {

            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (isLogin)
            {
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    Console.WriteLine("开始登录..");
                    if (Login())
                    {
                        this.Invoke(new Action(() => { this.Visible = false; }));
                        Console.WriteLine("开始查询车次..");
                        #region 限制提交次数
                        //int time = 5;
                        //while (time > 0)
                        //{
                        //    if (Query())
                        //        break;
                        //    else
                        //    {
                        //        --time;
                        //        if (time <= 0)
                        //        {
                        //            Console.WriteLine("查询已结束");
                        //            return;
                        //        }
                        //        Console.WriteLine(string.Format("5秒后再次查询，剩余次数{0}..", time));
                        //        Thread.Sleep(5000);
                        //    }
                        //}
                        #endregion
                        while (true)
                        {
                            if (Query())
                                break;
                            else
                            {
                                Console.WriteLine("10秒后再次查询..时间{0:HH:mm:ss}", DateTime.Now);
                                Thread.Sleep(10000);
                            }
                        }
                        Console.WriteLine("开始提交订单..");
                        if (Submit())
                        {
                            Console.WriteLine("订单提交成功");
                            this.Invoke(new Action(() =>
                            {
                                isLogin = false;
                                browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);
                                browser.Navigate(Url_ConfirmInit);
                            }));
                        }
                    }
                }));
                thread.Start();
            }
            else
            {
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    if (Confirm() && info.sendMail)
                    {
                        HttpHelper.SendEmail("火车票已抢到", string.Format("抢票成功，车次{0}已抢到！", submitTrain.traincode), "mcdull");
                    }
                }));
                thread.Start();
            }
        }

        private void pictureCode_Click(object sender, EventArgs e)
        {
            this.pictureCode.Image = Image.FromStream(HttpHelper.GetStream(isLogin ? Url_LoginCode : Url_OrderCode, clientInfo.cookieContainer));
        }

        private bool Login()
        {
            if (checkCode(false))
            {
                //var postData = string.Format("loginUserDTO.user_name={0}&userDTO.password={1}&randCode={2}&randCode_validate=&{3}={4}&myversion=undefined", info.user, info.password, Code, clientInfo.name, clientInfo.value);
                var postData = string.Format("loginUserDTO.user_name={0}&userDTO.password={1}&randCode={2}", info.user, info.password, Code);
                var html = HttpHelper.GetHtml(Url_Login + "?random=" + DateTime.Now.Ticks, postData, true, clientInfo.cookieContainer);
                var result = JsonConvert.DeserializeObject<MessageResult>(html);
                if (result != null && result.status)
                {
                    Console.WriteLine("登录成功");
                    return true;
                }
                Console.WriteLine("登录失败");
            }
            return false;
        }

        private bool Query()
        {
            var url = string.Format(Url_Query, info.traindate, fromCode, toCode);
            var html = HttpHelper.GetHtml(url, null, false, clientInfo.cookieContainer);
            MessageResult result = null;
            try
            {
                result = JsonConvert.DeserializeObject<MessageResult>(html);
            }
            catch
            {
                Console.WriteLine("查询失败");
                return false;
            }
            if (result == null || result.data == null)
            {
                Console.WriteLine("查询失败");
                return false;
            }
            #region 配置中的目标车次
            //Dictionary<string, TrainInfo> dic = new Dictionary<string, TrainInfo>();
            //foreach (var i in result.data)
            //{
            //    TrainInfo tf = new TrainInfo(i);
            //    dic.Add(tf.traincode, tf);
            //}
            //submitTrain = null;
            //foreach (string t in info.traincodes)
            //{
            //    if (dic.ContainsKey(t))
            //    {
            //        Console.WriteLine("已查到车次{0}，出发时间{1}，到达时间{2}，剩余二等座车票{3}，剩余一等座车票{4}", dic[t].traincode, dic[t].startTime, dic[t].arriveTime, dic[t].ze_num, dic[t].zy_num);
            //        int ze_num = 0;
            //        int zy_num = 0;
            //        Int32.TryParse(dic[t].ze_num, out ze_num);
            //        Int32.TryParse(dic[t].zy_num, out zy_num);
            //        if (!string.IsNullOrEmpty(dic[t].secretStr) && (ze_num > 1 || zy_num > 1))
            //        {
            //            submitTrain = dic[t];
            //            Console.WriteLine("选择目标车次{0}", submitTrain.traincode);
            //            return true;
            //        }
            //    }
            //}
            #endregion
            #region 任意晚于16点的车次
            submitTrain = null;
            foreach (var i in result.data)
            {
                TrainInfo tf = new TrainInfo(i);
                if (Int32.Parse(tf.startTime.Replace(":", "")) >= 1600)
                {
                    Console.WriteLine("已查到车次{0}，出发时间{1}，到达时间{2}，剩余二等座车票{3}，剩余一等座车票{4}", tf.traincode, tf.startTime, tf.arriveTime, tf.ze_num, tf.zy_num);
                    int ze_num = 0;
                    int zy_num = 0;
                    Int32.TryParse(tf.ze_num, out ze_num);
                    Int32.TryParse(tf.zy_num, out zy_num);
                    if (!string.IsNullOrEmpty(tf.secretStr) && (ze_num > 1 || zy_num > 1))
                    {
                        submitTrain = tf;
                        if (zy_num > 1 && ze_num < 2) //二等座改为一等座
                        {
                            passengerTicketStr = "";
                            foreach (var p in info.passengers)
                            {
                                passengerTicketStr += string.Format("M,0,1,{0},1,{1},{2},N_", p.name, p.idCard, p.mobile);
                            }
                            passengerTicketStr = HttpHelper.ConvertToUrlUtf8(passengerTicketStr.Substring(0, passengerTicketStr.Length - 1));
                        }
                        Console.WriteLine("选择目标车次{0}", submitTrain.traincode);
                        return true;
                    }
                }
            }
            #endregion
            Console.WriteLine("未查询到可预定车次");
            return false;
        }

        private bool Submit()
        {
            var postData = string.Format("{0}={1}&myversion=undefined&secretStr={2}&train_date={3}&back_train_date=2014-12-30&tour_flag=dc&purpose_codes=ADULT&query_from_station_name={4}&query_to_station_name={5}&undefined",
                    clientInfo.name, clientInfo.value, submitTrain.secretStr, info.traindate, info.from, info.to);
            string html = HttpHelper.GetHtml(Url_Submit, postData, true, clientInfo.cookieContainer);
            var result = JsonConvert.DeserializeObject<MessageResult>(html);
            if (result.status)
            {
                return true;
            }
            else if (result.messages.Length > 0)
            {
                foreach (var s in result.messages)
                {
                    Console.WriteLine(s);
                }
            }
            return false;
        }

        private bool Confirm()
        {
            if (checkCode(true))
            {
                var postData = string.Format("cancel_flag=2&bed_level_order_num=000000000000000000000000000000&passengerTicketStr={0}&oldPassengerStr={1}&tour_flag=dc&randCode={2}&{3}={4}&_json_att=&REPEAT_SUBMIT_TOKEN={5}"
                   , passengerTicketStr, oldPassengerStr, Code, clientInfo.name, clientInfo.value, globalRepeatSubmitToken);
                var html = HttpHelper.GetHtml(Url_CheckOrderInfo, postData, true, clientInfo.cookieContainer);
                var result = JsonConvert.DeserializeObject<MessageResult>(html);
                if (result != null && result.status && result.data.submitStatus.Value)
                {
                    this.Invoke(new Action(() => { this.Visible = false; }));
                    int time = 100;
                    while (time > 0)
                    {
                        if (FinalConfirm())
                            return true;
                        else
                        {
                            if (--time <= 0)
                            {
                                Console.WriteLine("抢票结束，失败");
                                return false;
                            }
                            Console.WriteLine(string.Format("1秒后再次确认订单，剩余次数{0}..", time));
                            Thread.Sleep(1000);
                        }
                    }
                }
                else if (result.data.errMsg != null)
                    Console.WriteLine(result.data.errMsg.Value);
                else
                    Console.WriteLine("订单确认失败！");
            }
            return false;
        }

        private bool FinalConfirm()
        {
            var postData = string.Format("dwAll=N&passengerTicketStr={0}&oldPassengerStr={1}&randCode={2}&purpose_codes=00&key_check_isChange={3}&leftTicketStr={4}&train_location={5}"
                                , passengerTicketStr, oldPassengerStr, Code, ticketInfoForPassengerForm.key_check_isChange, ticketInfoForPassengerForm.leftTicketStr, ticketInfoForPassengerForm.train_location);
            var html = HttpHelper.GetHtml(Url_Confirm, postData, true, clientInfo.cookieContainer);
            var result = JsonConvert.DeserializeObject<MessageResult>(html);
            if (result != null && result.status && result.data.submitStatus.Value)
            {
                Console.WriteLine("抢票成功！");
                return true;
            }
            else if (result.messages.Length > 0)
            {
                foreach (var s in result.messages)
                {
                    Console.WriteLine(s);
                }
            }
            else if (result.data.errMsg != null)
            {
                Console.WriteLine(result.data.errMsg.Value);
            }
            return false;
        }

        private bool checkCode(bool hasLogin)
        {
            Console.WriteLine("验证码正在提交中..");
            Code = this.txtCode.Text;
            var html = HttpHelper.GetHtml(Url_CheckCode, string.Format("randCode={0}&rand={1}&randCode_validate=", Code, hasLogin ? "randp" : "sjrand"), true, clientInfo.cookieContainer);
            var result = JsonConvert.DeserializeObject<MessageResult>(html);
            if (result != null && result.data != null && result.data.result == 1)
            {
                Console.WriteLine("验证码输入正确");
                return true;
            }
            else
            {
                Console.WriteLine("验证码输入错误");
                return false;
            }
        }
    }
}
