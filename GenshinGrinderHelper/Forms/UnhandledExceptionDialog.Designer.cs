namespace GenshinGrinderHelper.Forms
{
    partial class UnhandledExceptionDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            textBox1 = new TextBox();
            linkLabel1 = new LinkLabel();
            label2 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("微软雅黑", 9F);
            label1.Location = new Point(36, 15);
            label1.Name = "label1";
            label1.Size = new Size(298, 72);
            label1.TabIndex = 0;
            label1.Text = "检测到程序发生了未经处理的异常。\r\n这或许是一个我们未发现的Bug。\r\n错误信息：";
            label1.Click += label1_Click;
            // 
            // textBox1
            // 
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Location = new Point(36, 91);
            textBox1.Margin = new Padding(3, 4, 3, 4);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.ScrollBars = ScrollBars.Horizontal;
            textBox1.Size = new Size(704, 300);
            textBox1.TabIndex = 1;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Cursor = Cursors.Hand;
            linkLabel1.Font = new Font("微软雅黑", 9F);
            linkLabel1.Location = new Point(32, 434);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(82, 24);
            linkLabel1.TabIndex = 2;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "报告错误";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // label2
            // 
            label2.AutoEllipsis = true;
            label2.AutoSize = true;
            label2.ForeColor = SystemColors.AppWorkspace;
            label2.Location = new Point(511, 434);
            label2.Name = "label2";
            label2.Size = new Size(64, 24);
            label2.TabIndex = 3;
            label2.Text = "版本号";
            label2.Click += label2_Click;
            // 
            // UnhandledExceptionDialog
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(772, 479);
            Controls.Add(label2);
            Controls.Add(linkLabel1);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Font = new Font("微软雅黑", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UnhandledExceptionDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "发生未经处理的异常";
            TopMost = true;
            Load += UnhandledExceptionDialog_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label2;
    }
}