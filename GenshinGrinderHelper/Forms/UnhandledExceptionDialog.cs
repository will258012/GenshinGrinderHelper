using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace GenshinGrinderHelper.Forms
{
    public partial class UnhandledExceptionDialog : Form
    {
        /// <summary>
        /// 当（不幸地）发生了未处理错误时显示的窗口。
        /// </summary>
        /// <param name="ex"></param>
        public UnhandledExceptionDialog(Exception ex)
        {
            InitializeComponent();
            textBox1.Text = string.Format("异常类型：{0}\r\n异常消息：{1}\r\n 堆栈跟踪：\r\n {2} \r\n",
                    ex.GetType().FullName, ex.Message, ex.StackTrace);//生成错误报告

            label2.Text = Assembly.GetExecutingAssembly().GetName().Name + " " + Assembly.GetExecutingAssembly().GetName().Version;//获取版本号
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var startinfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = "https://github.com/will258012/GenshinGrinderHelper/issues/new",
            };
            Process.Start(startinfo);
        }
        private void label1_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void UnhandledExceptionDialog_Load(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
    }
}
