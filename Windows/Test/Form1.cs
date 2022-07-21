using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FooEditEngine;
using FooEditEngine.Windows;
using FooEditEngine.Test;

namespace Test.Windows
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.fooTextBox1.ShowTab = true;
            this.fooTextBox1.ShowFullSpace = true;
            this.fooTextBox1.ShowLineBreak = true;
            var collection = new CompleteCollection<ICompleteItem>();
            collection.Add(new CompleteWord("int"));
            collection.Add(new CompleteWord("float"));
            collection.Add(new CompleteWord("double"));
            collection.Add(new CompleteWord("byte"));
            collection.Add(new CompleteWord("char"));
            this.fooTextBox1.Document.AutoComplete.Items = collection;
            this.fooTextBox1.Document.AutoComplete.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FooPrintText fooPrint;
            fooPrint = new FooPrintText();
            fooPrint.Document = this.fooTextBox1.Document;
            fooPrint.DrawLineNumber = this.fooTextBox1.DrawLineNumber;
            fooPrint.Font = this.fooTextBox1.Font;
            fooPrint.LineBreakMethod = this.fooTextBox1.LineBreakMethod == LineBreakMethod.None ? LineBreakMethod.PageBound : this.fooTextBox1.LineBreakMethod;
            fooPrint.LineBreakCharCount = this.fooTextBox1.LineBreakCharCount;
            fooPrint.RightToLeft = this.fooTextBox1.RightToLeft == System.Windows.Forms.RightToLeft.Yes;
            fooPrint.Header = "header";
            fooPrint.Footer = "footer";
            fooPrint.Foreground = this.fooTextBox1.Foreground;
            PrintPreviewDialog dialog = new PrintPreviewDialog();
            dialog.Document = fooPrint.PrintDocument;
            dialog.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.fooTextBox1.RightToLeft == System.Windows.Forms.RightToLeft.No)
                this.fooTextBox1.RightToLeft = RightToLeft.Yes;
            else
                this.fooTextBox1.RightToLeft = RightToLeft.No;
            this.fooTextBox1.Refresh();
        }

        private void lineNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.lineNumberToolStripMenuItem.Checked)
                this.fooTextBox1.DrawLineNumber = false;
            else
                this.fooTextBox1.DrawLineNumber = true;
            this.lineNumberToolStripMenuItem.Checked = this.fooTextBox1.DrawLineNumber;
            this.fooTextBox1.Refresh();
        }

        private async void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                await this.fooTextBox1.LoadFileAsync(ofd.FileName, Encoding.Default,null);
                this.fooTextBox1.Refresh();
            }
        }

        private void hilightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.fooTextBox1.Hilighter = new XmlHilighter();
            this.fooTextBox1.LayoutLines.HilightAll();
        }

        private void setPaddingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.fooTextBox1.Padding = new System.Windows.Forms.Padding(20);
            this.fooTextBox1.Refresh();
        }
    }
}
