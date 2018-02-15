using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Media;
using System.Drawing;
using Newtonsoft.Json;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace Help12306
{
    static class Program
    {
        #region 服务器地址
        //验证码识别地址
        //private const string Url_CheckCode = "https://kyfw.12306.cn/otn/passcodeNew/checkRandCodeAnsyn";
        private const string Url_CheckCode = "https://kyfw.12306.cn/passport/captcha/captcha-check";
        //用户登录地址
        //private const string Url_Login = "https://kyfw.12306.cn/otn/login/loginAysnSuggest";
        private const string Url_Login = "https://kyfw.12306.cn/passport/web/login";
        //查询车次
        // private const string Url_Query = "https://kyfw.12306.cn/otn/leftTicket/query?leftTicketDTO.train_date={0}&leftTicketDTO.from_station={1}&leftTicketDTO.to_station={2}&purpose_codes=ADULT";
        private const string Url_Query = "https://kyfw.12306.cn/otn/leftTicket/queryZ?leftTicketDTO.train_date={0}&leftTicketDTO.from_station={1}&leftTicketDTO.to_station={2}&purpose_codes=ADULT";
        //订单提交地址
        private const string Url_Submit = "https://kyfw.12306.cn/otn/leftTicket/submitOrderRequest";
        //确认订单初始化地址
        private const string Url_ConfirmInit = "https://kyfw.12306.cn/otn/confirmPassenger/initDc";
        //订单生成地址
        private const string Url_Confirm = "https://kyfw.12306.cn/otn/confirmPassenger/confirmSingleForQueue";
        //检查订单信息
        private const string Url_CheckOrderInfo = "https://kyfw.12306.cn/otn/confirmPassenger/checkOrderInfo";
        //query到目标车次后，请求提交页面Url_Submit，成功后加载Url_ConfirmInit从页面上获取token，加载验证码获取code之后提交Url_CheckOrderInfo成功后提交Url_Confirm
        #endregion

        #region 成员变量
        private static SoundPlayer spSuccess = new SoundPlayer(Help12306.Properties.Resources.success);
        private static SoundPlayer spError = new SoundPlayer(Help12306.Properties.Resources.error);
        private static Info info;  //外部购票信息
        private static ClientInfo clientInfo; //客户端保存信息
        private static TrainInfo submitTrain;     //提交车次
        private static Info.Train selectTrain;    //选择的配置车次
        private static dynamic ticketInfoForPassengerForm;   //隐藏表单
        private static string globalRepeatSubmitToken;  //全局请求凭据
        private static string passengerTicketStr;
        private static string oldPassengerStr;

        private static WebBrowser browser;
        #endregion
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                info = Info.GetInfo(Environment.CurrentDirectory + "\\Info.xml");
            }
            catch (Exception ex)
            {
                Console.WriteLine("信息读取失败，" + ex.Message);
                return;
            }
            if (info == null)
            {
                Console.WriteLine("信息读取失败，请检查配置文件Info.xml是否存在或格式是否有误！");
                return;
            }
            Console.WriteLine("输入y开始抢票其他退出程序：");
            if (Console.ReadLine() == "y")
            {
                if (Init())
                {
                    browser = new WebBrowser();
                    browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);

                    Form form = new Form();
                    form.WindowState = FormWindowState.Minimized;
                    form.Visible = false;
                    form.ShowInTaskbar = false;
                    form.Controls.Add(browser);
                    form.Name = "Browser";
                    form.Load += new EventHandler(form_Load);
                    Application.Run(form);
                }
                else
                    Console.WriteLine("初始化失败");
            }
        }

        static void form_Load(object sender, EventArgs e)
        {
            browser.Navigate("https://kyfw.12306.cn/otn/login/init");
        }

        static void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (browser.Document.Cookie.Contains("RAIL_DEVICEID"))
            {
                Console.WriteLine("设备信息获取成功，开始执行登录。。");
                clientInfo = new ClientInfo(browser.Document.Cookie);
                browser.Stop();
                browser.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);
                Console.WriteLine("请输入验证码");
                frmCode f = new frmCode(true, clientInfo.cookieContainer, CheckCode);
                if (f.ShowDialog() == DialogResult.OK)
                {
                    Console.WriteLine("开始登录..");
                    if (Login(f.Code) && AuthClient())
                    {
                        BeginQuery();
                    }
                }
            }
            else
                Console.WriteLine("设备信息获取失败！");
        }

        private static bool AuthClient()
        {
            var token = JsonConvert.DeserializeObject<AppTokenResult>(HttpHelper.GetHtml("https://kyfw.12306.cn/passport/web/auth/uamtk", "appid=otn", true, clientInfo.cookieContainer));
            var html = HttpHelper.GetHtml("https://kyfw.12306.cn/otn/uamauthclient", "tk=" + token.newapptk, true, clientInfo.cookieContainer, "https://kyfw.12306.cn/otn/passport?redirect=/otn/login/userLogin", false);
            if (html.Equals("302"))
                html = HttpHelper.GetHtml("https://kyfw.12306.cn/otn/uamauthclient", "tk=" + token.newapptk, true, clientInfo.cookieContainer, "https://kyfw.12306.cn/otn/passport?redirect=/otn/login/userLogin");
            var result = JsonConvert.DeserializeObject<AppTokenResult>(html);
            if (result.result_code == 0)
            {
                Console.WriteLine("验证成功！");
                return true;
            }
            else
                Console.WriteLine("验证失败!");
            return false;
        }

        private static void BeginQuery()
        {
            Console.WriteLine("开始查询车次..");
            while (true)
            {
                if (QueryByTask())
                    break;
                else
                {
                    Console.WriteLine("5秒后再次查询..时间{0:HH:mm:ss}", DateTime.Now);
                    Thread.Sleep(5000);
                }
            }
            bool error = true;
            int retry = 5;
            while (retry-- > 0)
            {
                Console.WriteLine("开始提交订单..");
                if (Submit() && GetToken()) //SubmitByTask()
                {
                    error = false;
                    break;
                }
                //if (error && retry > 0)
                //{
                //    Console.WriteLine("重新登录，请输入验证码");
                //    frmCode fc = new frmCode(true, clientInfo.cookieContainer, CheckCode);
                //    if (fc.ShowDialog() == DialogResult.OK)
                //    {
                //        Console.WriteLine("开始登录..");
                //        Login(fc.Code);
                //    }
                //}
            }
            if (error)
            {
                Console.WriteLine("订单提交失败，抢票结束!");
                spError.Play();
                return;
            }
            Console.WriteLine("订单提交成功!");
            //Console.WriteLine("请输入验证码并确认订单");
            //frmCode f = new frmCode(false, clientInfo.cookieContainer, CheckCode);
            //if (f.ShowDialog() == DialogResult.OK)
            //{
            if (Confirm(""))
            {
                Console.WriteLine("抢票成功！");
                spSuccess.Play();
            }
            else
            {
                Console.WriteLine("抢票失败！");
                spError.Play();
            }
            // }
        }

        private static bool Init()
        {
            try
            {
                foreach (var p in info.passengers)
                {
                    //seat_type,0,ticket_type,name,id_type,id_no,phone_no,save_status(N,Y)_
                    //seat_type:1硬座 2软座 3硬卧卧 O二等座 M一等座
                    //ticket_type:1成人票 2儿童票 3学生票 4残军票
                    passengerTicketStr += string.Format("{3},0,{4},{0},1,{1},{2},N_", p.name, p.idCard, p.mobile, p.seat, p.ticket);
                    //c.name,c.id_type,c.id_no,c.passenger_type_
                    oldPassengerStr += string.Format("{0},1,{1},1_", p.name, p.idCard);
                }
                passengerTicketStr = passengerTicketStr.Substring(0, passengerTicketStr.Length - 1);
                return true;
            }
            catch { return false; }
        }

        private static bool CheckCode(bool login, string code)
        {
            Console.WriteLine("验证码正在提交中..");
            string data = login ? string.Format("answer={0}&rand=sjrand&login_site=E", code) : string.Format("randCode={0}&rand=randp&_json_att=&REPEAT_SUBMIT_TOKEN={1}", code, globalRepeatSubmitToken);
            var html = HttpHelper.GetHtml(Url_CheckCode, data, true, clientInfo.cookieContainer);
            CodeResult result = null;
            try
            {
                result = JsonConvert.DeserializeObject<CodeResult>(html);
            }
            catch
            {
                if (html.Contains("验证码校验成功"))
                    result = new CodeResult
                    {
                        result_code = 4,
                        result_message = "验证码校验成功"
                    };
            }
            if (result != null && result.result_code == 4)
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

        private static bool Login(string code)
        {
            var postData = string.Format("username={0}&password={1}&appid=otn", info.user, info.password);
            var html = HttpHelper.GetHtml(Url_Login, postData, true, clientInfo.cookieContainer);
            try
            {
                var result = JsonConvert.DeserializeObject<LoginResult>(html);
                if (result.result_code == 0)
                {
                    Console.WriteLine("登录成功");
                    return true;
                }
            }
            catch { }
            Console.WriteLine("登录失败");
            return false;
        }

        /// <summary>
        /// 多线程任务查询车次
        /// </summary>
        /// <returns></returns>
        private static bool QueryByTask()
        {
            foreach (var train in info.trains)
            {
                var url = string.Format(Url_Query, train.date, train.fromCode, train.toCode);
                int read = 1;
                Func<dynamic> query = () =>
                {
                    while (Thread.VolatileRead(ref read) == 1)
                    {
                        var html = HttpHelper.GetHtml(url, null, false, clientInfo.cookieContainer);
                        MessageResult<TrainData> result = null;
                        try
                        {
                            result = JsonConvert.DeserializeObject<MessageResult<TrainData>>(html);
                        }
                        catch
                        {
                            continue;
                        }
                        if (result == null || result.data == null)
                        {
                            continue;
                        }
                        return result.data;
                    }
                    return null;
                };
                TaskFactory<dynamic> taskFactory = new TaskFactory<dynamic>();
                Task<dynamic>[] tasks = new Task<dynamic>[10];
                for (int i = 0; i < tasks.Length; i++)
                    tasks[i] = taskFactory.StartNew(query);
                //ContinueWhenAny执行到这里只是启动一个新线程，原线程如果没有执行完仍然会执行
                var resultTask = new TaskFactory<bool>().ContinueWhenAny(tasks, task =>
                {
                    Thread.VolatileWrite(ref read, 0);
                    #region 任意晚于指定时间的车次
                    submitTrain = null;
                    selectTrain = null;
                    foreach (var i in task.Result.result)
                    {
                        TrainInfo tf = new TrainInfo(i);
                        int startTime = Int32.Parse(tf.startTime.Replace(":", ""));
                        if (startTime >= train.start && startTime <= train.end && (tf.traincode[0] == 'D' || tf.traincode[0] == 'G'))
                        {
                            Console.WriteLine("已查到车次{0}，出发时间{1}，到达时间{2}，剩余二等座车票{3}，剩余一等座车票{4}，无座{5}，硬座{6}", tf.traincode, tf.startTime, tf.arriveTime, tf.ze_num, tf.zy_num, tf.wz_num, tf.yz_num);
                            int ze_num = 0;
                            int zy_num = 0;
                            int wz_num = 0;
                            int yz_num = 0;
                            Int32.TryParse(tf.ze_num, out ze_num);
                            Int32.TryParse(tf.zy_num, out zy_num);
                            Int32.TryParse(tf.wz_num, out wz_num);
                            Int32.TryParse(tf.yz_num, out yz_num);
                            bool hasZE = tf.ze_num.Equals("有") || ze_num >= info.passengers.Length;
                            bool hasZY = tf.zy_num.Equals("有") || zy_num >= info.passengers.Length;
                            if (!string.IsNullOrEmpty(tf.secretStr) && (hasZE || hasZY))
                            {
                                submitTrain = tf;
                                selectTrain = train;
                                if (!hasZE && hasZY) //二等座改为一等座
                                {
                                    passengerTicketStr = "";
                                    foreach (var p in info.passengers)
                                    {
                                        passengerTicketStr += string.Format("M,0,1,{0},1,{1},{2},N_", p.name, p.idCard, p.mobile);
                                    }
                                    passengerTicketStr = HttpHelper.ConvertToUrlUtf8(passengerTicketStr.Substring(0, passengerTicketStr.Length - 1));
                                    Console.WriteLine("二等座改为一等座.");
                                }
                                Console.WriteLine("选择目标车次{0}", submitTrain.traincode);
                                return true;
                            }
                        }
                    }
                    #endregion
                    Console.WriteLine("未能提交目标车次");
                    return false;
                });
                if (resultTask.Result)
                    return true;
            }
            return false;
        }

        private static bool Submit()
        {
            //clientInfo.SetCookie("current_captcha_type", "Z");
            //clientInfo.SetCookie("_jc_save_fromStation", HttpHelper.CharacterToCoding(info.from) + "%2C" + fromCode);
            //clientInfo.SetCookie("_jc_save_toStation", HttpHelper.CharacterToCoding(info.to) + "%2C" + toCode);
            //clientInfo.SetCookie("_jc_save_fromDate", info.traindate);
            //clientInfo.SetCookie("_jc_save_toDate", "2018-01-18");
            //clientInfo.SetCookie("_jc_save_wfdc_flag", "dc");
            var postData = string.Format("secretStr={2}&train_date={3}&back_train_date=2018-01-18&tour_flag=dc&purpose_codes=ADULT&query_from_station_name={4}&query_to_station_name={5}&undefined",
                    clientInfo.name, clientInfo.value, submitTrain.secretStr, selectTrain.date, selectTrain.from, selectTrain.to);
            string html = HttpHelper.GetHtml(Url_Submit, postData, true, clientInfo.cookieContainer);
            try
            {
                var result = JsonConvert.DeserializeObject<MessageResult<dynamic>>(html);
                if (result == null)
                {
                    Console.WriteLine("网络请求失败，无数据返回");
                    return false;
                }
                if (result.status)
                {
                    if (result.messages.Length == 0)
                        return true;
                    else
                        foreach (var s in result.messages)
                            Console.WriteLine(s);
                }
                else if (result.messages.Length > 0)
                {
                    foreach (var s in result.messages)
                    {
                        Console.WriteLine(s);
                    }
                }
            }
            catch
            {
                Console.WriteLine("服务器返回失败：");
                //Console.WriteLine(html);
            }
            return false;
        }

        private static bool GetToken()
        {
            var html = HttpHelper.GetHtml(Url_ConfirmInit + "?rd=" + DateTime.Now.Ticks, null, false, clientInfo.cookieContainer);
            Regex rx = new Regex("(?<=var ticketInfoForPassengerForm=).*;");
            var m = rx.Match(html);
            if (string.IsNullOrEmpty(m.Value))
            {
                Console.WriteLine("发生错误！Token获取失败");
                return false;
            }
            var s = m.Value.Substring(0, m.Value.Length - 1);
            ticketInfoForPassengerForm = JsonConvert.DeserializeObject<dynamic>(s);

            rx = new Regex("var globalRepeatSubmitToken.*(?=;)");
            m = rx.Match(html);
            if (string.IsNullOrEmpty(m.Value))
            {
                Console.WriteLine("发生错误！Token获取失败");
                return false;
            }
            var d = m.Value.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            globalRepeatSubmitToken = d[1].Trim(new char[] { ' ', '\'' });
            return true;
        }

        private static bool Confirm(string code)
        {
            var postData = string.Format("cancel_flag=2&bed_level_order_num=000000000000000000000000000000&passengerTicketStr={0}&oldPassengerStr={1}&tour_flag=dc&randCode={2}&_json_att=&REPEAT_SUBMIT_TOKEN={3}"
               , HttpHelper.ConvertToUrlUtf8(passengerTicketStr), HttpHelper.ConvertToUrlUtf8(oldPassengerStr), code, globalRepeatSubmitToken);
            var html = HttpHelper.GetHtml(Url_CheckOrderInfo, postData, true, clientInfo.cookieContainer);
            var result = JsonConvert.DeserializeObject<MessageResult<dynamic>>(html);
            if (result != null && result.status && result.data != null && result.data.submitStatus.Value)
            {
                GetQueueCount();
                if (FinalConfirm(code))
                    return true;
                return false;
            }
            else if (result.data != null && result.data.errMsg != null)
            {
                Console.WriteLine("Confirm:" + result.data.errMsg.Value);
            }
            else
            {
                Console.WriteLine("Confirm:" + "订单确认失败！");
            }
            return false;
        }

        private static void GetQueueCount()
        {
            var postData = string.Format("train_date={0:r}&train_no={1}&stationTrainCode={2}&seatType={3}&fromStationTelecode={4}&toStationTelecode={5}&leftTicket={6}&purpose_codes=00&train_location={7}&REPEAT_SUBMIT_TOKEN={8}", DateTime.Parse(selectTrain.date), submitTrain.train_no, submitTrain.traincode, "O", submitTrain.from_station_telecode, submitTrain.to_station_telecode, ticketInfoForPassengerForm.leftTicketStr, ticketInfoForPassengerForm.train_location, globalRepeatSubmitToken);
            var html = HttpHelper.GetHtml("https://kyfw.12306.cn/otn/confirmPassenger/getQueueCount", postData, true, clientInfo.cookieContainer);
            var result = JsonConvert.DeserializeObject<MessageResult<dynamic>>(html);
            Console.WriteLine("GetQueueCount返回：" + result.status);
        }

        private static bool FinalConfirm(string code)
        {
            var postData = string.Format("passengerTicketStr={0}&oldPassengerStr={1}&randCode={2}&purpose_codes=00&key_check_isChange={3}&leftTicketStr={4}&train_location={5}&choose_seats=&seatDetailType=000&whatsSelect=1&roomType=00&dwAll=N&_json_att=&REPEAT_SUBMIT_TOKEN={6}", passengerTicketStr, oldPassengerStr, code, ticketInfoForPassengerForm.key_check_isChange, HttpHelper.ConvertToUrlUtf8(ticketInfoForPassengerForm.leftTicketStr.Value), ticketInfoForPassengerForm.train_location, globalRepeatSubmitToken);
            var html = HttpHelper.GetHtml(Url_Confirm, postData, true, clientInfo.cookieContainer, Url_ConfirmInit);
            var result = JsonConvert.DeserializeObject<MessageResult<dynamic>>(html);
            if (result != null && result.status && result.data != null && result.data.submitStatus.Value)
            {
                return true;
            }
            else if (result.messages != null && result.messages.Length > 0)
            {
                foreach (var s in result.messages)
                {
                    Console.WriteLine("FinalConfirm:" + s);
                }
            }
            else if (result.data != null && result.data.GetType() == typeof(string))
            {
                Console.WriteLine("FinalConfirm:" + result.data);
            }
            else if (result.data != null && result.data.errMsg != null)
            {
                Console.WriteLine("FinalConfirm:" + result.data.errMsg.Value);
            }
            else
            {
                Console.WriteLine("FinalConfirm:返回空");
            }
            return false;
        }
    }
}
