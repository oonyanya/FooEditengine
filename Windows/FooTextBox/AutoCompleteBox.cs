using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using FooEditEngine;

namespace FooEditEngine.Windows
{
    /// <summary>
    /// 自動補完用クラス
    /// </summary>
    public class AutoCompleteBox : AutoCompleteBoxBase
    {
        private string inputedWord;
        private ListBox listBox1 = new ListBox();
        private Document doc;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="textbox">対象となるテキストボックス</param>
        internal AutoCompleteBox(FooTextBox textbox) : base(textbox.Document)
        {
            //リストボックスを追加する
            this.listBox1.MouseDoubleClick += ListBox1_MouseDoubleClick;
            this.listBox1.KeyDown += listBox1_KeyDown;
            this.listBox1.Height = 200;
            this.listBox1.Visible = false;
            textbox.Controls.Add(this.listBox1);
            this.doc = textbox.Document;
        }

        /// <summary>
        /// オートコンプリートの対象となる単語のリスト
        /// </summary>
        public override CompleteCollection<ICompleteItem> Items
        {
            get
            {
                return (CompleteCollection<ICompleteItem>)this.listBox1.DataSource;
        }
            set
            {
                this.listBox1.DisplayMember = CompleteCollection<ICompleteItem>.ShowMember;
                this.listBox1.ValueMember = CompleteCollection<ICompleteItem>.ShowMember;
                this.listBox1.DataSource = value;
            }
        }

        /// <summary>
        /// 自動補完リストが表示されているかどうか
        /// </summary>
        protected override bool IsCloseCompleteBox
        {
            get
            {
                return !this.listBox1.Visible;
            }
        }

        /// <summary>
        /// 補完候補の表示要求を処理する
        /// </summary>
        /// <param name="ev"></param>
        protected override void RequestShowCompleteBox(ShowingCompleteBoxEventArgs ev)
        {
            this.inputedWord = ev.inputedWord;
            this.listBox1.SelectedItem = ((CompleteCollection<ICompleteItem>)this.listBox1.DataSource)[ev.foundIndex];
            this.listBox1.Visible = true;
            this.listBox1.Location = ev.CaretPostion;
        }

        /// <summary>
        /// 補完候補の非表示要求を処理する
        /// </summary>
        protected override void RequestCloseCompleteBox()
        {
            this.listBox1.Visible = false;
        }

        internal bool ProcessKeyPress(FooTextBox textbox, KeyPressEventArgs e)
        {
            if (this.isReqForceComplete && e.KeyChar == ' ')
            {
                this.OpenCompleteBox(string.Empty);
                return true;
            } else if (!this.IsCloseCompleteBox && (e.KeyChar == '\r')){
                this.RequestCloseCompleteBox();
                CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
                this.SelectItem(this, new SelectItemEventArgs(selWord, this.inputedWord, this.Document));
                textbox.Refresh();
                return true;
            }
            return false;
        }

        bool isReqForceComplete = false;

        internal bool ProcessKeyDown(FooTextBox textbox, KeyEventArgs e,bool isCtrl,bool isShift)
        {
            if (this.IsCloseCompleteBox)
            {
                if (e.KeyCode == Keys.Space && isCtrl)
                {
                    this.isReqForceComplete = true;
                    return true;
                }
                return false;
            }

            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.RequestCloseCompleteBox();
                    textbox.Focus();
                    return true;
                case Keys.Down:
                    if (this.listBox1.SelectedIndex + 1 >= this.listBox1.Items.Count)
                        this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
                    else
                        this.listBox1.SelectedIndex++;
                    return true;
                case Keys.Up:
                    if (this.listBox1.SelectedIndex - 1 < 0)
                        this.listBox1.SelectedIndex = 0;
                    else
                        this.listBox1.SelectedIndex--;
                    return true;
                case Keys.Tab:
                    this.RequestCloseCompleteBox();
                    CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
                    this.SelectItem(this, new SelectItemEventArgs(selWord, this.inputedWord, this.Document));
                    textbox.Refresh();
                    return true;
            }

            return false;
        }

        private void ListBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.listBox1.Visible = false;
            CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
            this.SelectItem(this, new SelectItemEventArgs(selWord, this.inputedWord, this.Document));
        }

        void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.listBox1.Visible = false;
                CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
                this.SelectItem(this, new SelectItemEventArgs(selWord, this.inputedWord, this.Document));
                e.Handled = true;
            }
        }
    }
}
