using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading.Tasks;
using System.Media;

namespace Help12306
{
    public partial class frmCode : Form
    {
        private static SoundPlayer spMsg = new SoundPlayer(Help12306.Properties.Resources.msg);
        //获取登录验证码地址
        private const string Url_LoginCode = "https://kyfw.12306.cn/passport/captcha/captcha-image?login_site=E&module=login&rand=sjrand&";
        //获取确认订单验证码地址
        private const string Url_OrderCode = "https://kyfw.12306.cn/passport/captcha/captcha-image?login_site=E&module=passenger&rand=randp&";
        //验证码图片矩阵
        private Point[] CheckCodeArray = new Point[8] { new Point(40, 45), new Point(110, 45), new Point(190, 45), new Point(260, 45), new Point(40, 115), new Point(110, 115), new Point(190, 115), new Point(260, 115) };
        private bool _login;
        private CookieContainer _cookieContainer;
        private Func<bool, string, bool> _checkCode;
        //验证码提交字符串
        public string Code
        {
            get
            {
                return _code;
            }
        }
        private string _code;
        private const string number = "①②③④⑤⑥⑦⑧";
        public frmCode(bool login, CookieContainer cookie, Func<bool, string, bool> checkCode)
        {
            for (int i = 0; i < 8; i++)
            {
                Label lb = new Label();
                lb.Text = number[i].ToString();
                lb.AutoSize = true;
                lb.BackColor = Color.White;
                //lb.Font = new System.Drawing.Font("宋体", 9, FontStyle.Bold);
                lb.Location = new Point(i % 4 * 110 + 13, i / 4 * 110 + 60 + 13);
                this.Controls.Add(lb);
            }
            InitializeComponent();
            this._login = login;
            this._cookieContainer = cookie;
            this._checkCode = checkCode;
        }

        private void frmCode_Load(object sender, EventArgs e)
        {
            spMsg.PlaySync();
            this.Focus();
            txtCode.Focus();
            btnRefresh_Click(null, null);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            _code = "";
            //var arr = this.txtCode.Text.Split("".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (char a in this.txtCode.Text)
            {
                var p = CheckCodeArray[Int32.Parse(a.ToString()) - 1];
                _code += string.Format("{0},{1},", p.X, p.Y);
            }
            if (!string.IsNullOrEmpty(_code) && _code.Length > 0)
                _code = _code.Substring(0, _code.Length - 1);
            Task<bool> task = new Task<bool>(() => _checkCode(_login, _code));
            task.ContinueWith(t =>
            {
                if (t.Result)
                    this.DialogResult = DialogResult.OK;
                else
                {
                    this.Invoke(new Action(() =>
                    {
                        this.txtCode.Text = "";
                        btnRefresh_Click(null, null);
                    }));
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            task.Start();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.btnOk.Enabled = false;
            this.btnRefresh.Enabled = false;
            this.Text = "图片正在加载中";
            Task<System.IO.Stream> task = new Task<System.IO.Stream>(() => HttpHelper.GetStream((_login ? Url_LoginCode : Url_OrderCode) + "rd=" + DateTime.Now.Ticks, _cookieContainer));
            task.ContinueWith(t => this.Invoke(new Action<System.IO.Stream>(img =>
            {
                try
                {
                    if (img != null)
                    {
                        this.pictureCode.Image = Image.FromStream(img);
                        this.Text = "请输入验证码";
                    }
                    else
                    {
                        this.Text = "图片加载失败";
                    }
                }
                finally
                {
                    this.btnOk.Enabled = true;
                    this.btnRefresh.Enabled = true;
                }
            }), t.Result));
            task.Start();
        }

        private void txtCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b' && (e.KeyChar < '1' || e.KeyChar > '8'))//仅允许退格键和1-8
                e.Handled = true;
        }
    }
}
