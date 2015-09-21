namespace Help12306
{
    partial class FormCode
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureCode = new System.Windows.Forms.PictureBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureCode)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureCode
            // 
            this.pictureCode.Location = new System.Drawing.Point(12, 12);
            this.pictureCode.Name = "pictureCode";
            this.pictureCode.Size = new System.Drawing.Size(314, 180);
            this.pictureCode.TabIndex = 0;
            this.pictureCode.TabStop = false;
            this.pictureCode.Click += new System.EventHandler(this.pictureCode_Click);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(118, 198);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "确定";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // txtCode
            // 
            this.txtCode.Location = new System.Drawing.Point(12, 198);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(100, 21);
            this.txtCode.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(10, 228);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(323, 24);
            this.label1.TabIndex = 3;
            this.label1.Text = "注：输入图案的编号，从左至右，从上至下的顺序依次为1-8\r\n使用英文逗号分隔";
            // 
            // FormCode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(335, 259);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.pictureCode);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FormCode";
            this.Text = "请输入验证码";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormCode_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureCode)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureCode;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.Label label1;
    }
}

