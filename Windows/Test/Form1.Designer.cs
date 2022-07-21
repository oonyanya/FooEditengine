namespace Test.Windows
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.印刷ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rTLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lineNumberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hilightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fooTextBox1 = new FooEditEngine.Windows.FooTextBox();
            this.setPaddingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(362, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.印刷ToolStripMenuItem,
            this.rTLToolStripMenuItem,
            this.lineNumberToolStripMenuItem,
            this.loadToolStripMenuItem,
            this.hilightToolStripMenuItem,
            this.setPaddingToolStripMenuItem});
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.testToolStripMenuItem.Text = "Test";
            // 
            // 印刷ToolStripMenuItem
            // 
            this.印刷ToolStripMenuItem.Name = "印刷ToolStripMenuItem";
            this.印刷ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.印刷ToolStripMenuItem.Text = "印刷";
            this.印刷ToolStripMenuItem.Click += new System.EventHandler(this.button1_Click);
            // 
            // rTLToolStripMenuItem
            // 
            this.rTLToolStripMenuItem.Name = "rTLToolStripMenuItem";
            this.rTLToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.rTLToolStripMenuItem.Text = "RTL";
            this.rTLToolStripMenuItem.Click += new System.EventHandler(this.button2_Click);
            // 
            // lineNumberToolStripMenuItem
            // 
            this.lineNumberToolStripMenuItem.Name = "lineNumberToolStripMenuItem";
            this.lineNumberToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.lineNumberToolStripMenuItem.Text = "LineNumber";
            this.lineNumberToolStripMenuItem.Click += new System.EventHandler(this.lineNumberToolStripMenuItem_Click);
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // hilightToolStripMenuItem
            // 
            this.hilightToolStripMenuItem.Name = "hilightToolStripMenuItem";
            this.hilightToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.hilightToolStripMenuItem.Text = "Hilight";
            this.hilightToolStripMenuItem.Click += new System.EventHandler(this.hilightToolStripMenuItem_Click);
            // 
            // fooTextBox1
            // 
            this.fooTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fooTextBox1.Background = System.Drawing.SystemColors.Control;
            this.fooTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fooTextBox1.Comment = System.Drawing.Color.Green;
            this.fooTextBox1.ControlChar = System.Drawing.Color.Gray;
            this.fooTextBox1.FoldingStrategy = null;
            this.fooTextBox1.Foreground = System.Drawing.SystemColors.ControlText;
            this.fooTextBox1.Hilight = System.Drawing.Color.DeepSkyBlue;
            this.fooTextBox1.Hilighter = null;
            this.fooTextBox1.InsertCaret = System.Drawing.Color.Black;
            this.fooTextBox1.InsertMode = false;
            this.fooTextBox1.Keyword1 = System.Drawing.Color.Blue;
            this.fooTextBox1.Keyword2 = System.Drawing.Color.DarkCyan;
            this.fooTextBox1.LineMarker = System.Drawing.Color.WhiteSmoke;
            this.fooTextBox1.LineNumber = System.Drawing.Color.DimGray;
            this.fooTextBox1.Literal = System.Drawing.Color.Brown;
            this.fooTextBox1.Location = new System.Drawing.Point(0, 27);
            this.fooTextBox1.Name = "fooTextBox1";
            this.fooTextBox1.OverwriteCaret = System.Drawing.Color.Black;
            this.fooTextBox1.SelectedText = null;
            this.fooTextBox1.ShowTab = true;
            this.fooTextBox1.Size = new System.Drawing.Size(362, 229);
            this.fooTextBox1.TabIndex = 2;
            this.fooTextBox1.TabStops = 8;
            this.fooTextBox1.Text = "fooTextBox1";
            this.fooTextBox1.TextAntialiasMode = FooEditEngine.TextAntialiasMode.Default;
            this.fooTextBox1.Url = System.Drawing.Color.Blue;
            // 
            // setPaddingToolStripMenuItem
            // 
            this.setPaddingToolStripMenuItem.Name = "setPaddingToolStripMenuItem";
            this.setPaddingToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.setPaddingToolStripMenuItem.Text = "Set Padding";
            this.setPaddingToolStripMenuItem.Click += new System.EventHandler(this.setPaddingToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 257);
            this.Controls.Add(this.fooTextBox1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 印刷ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rTLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lineNumberToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hilightToolStripMenuItem;
        private FooEditEngine.Windows.FooTextBox fooTextBox1;
        private System.Windows.Forms.ToolStripMenuItem setPaddingToolStripMenuItem;
    }
}

