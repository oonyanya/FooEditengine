using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FooEditEngine;

namespace FooEditEngine.WPF
{
    /// <summary>
    /// 自動補完用クラス
    /// </summary>
    public class AutoCompleteBox : AutoCompleteBoxBase
    {
        private string inputedWord;
        private ListBox listBox1 = new ListBox();
        private Popup popup;
        private Document doc;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="doc">ドキュメント</param>
        public AutoCompleteBox(Document doc) : base(doc)
        {
            //リストボックスを追加する
            this.listBox1.MouseDoubleClick += listBox1_MouseDoubleClick;
            this.listBox1.KeyDown += listBox1_KeyDown;
            this.listBox1.Height = 200;
            this.doc = doc;
        }

        /// <summary>
        /// オートコンプリートの対象となる単語のリスト
        /// </summary>
        public override CompleteCollection<ICompleteItem> Items
        {
            get
            {
                return (CompleteCollection<ICompleteItem>)this.listBox1.ItemsSource;
            }
            set
            {
                this.listBox1.ItemsSource = value;
                this.listBox1.DisplayMemberPath = CompleteCollection<ICompleteItem>.ShowMember;
            }
        }

        /// <summary>
        /// 自動補完リストが表示されているかどうか
        /// </summary>
        protected override bool IsCloseCompleteBox
        {
            get
            {
                return !this.popup.IsOpen;
            }
        }

        internal Popup TargetPopup
        {
            get
            {
                return this.popup;
            }
            set
            {
                this.popup = value;
                if(value != null)
                {
                    this.popup.Child = this.listBox1;
                    this.popup.Height = 200;
                }
            }
        }

        /// <summary>
        /// 補完候補の表示要求を処理する
        /// </summary>
        /// <param name="ev"></param>
        protected override void RequestShowCompleteBox(ShowingCompleteBoxEventArgs ev)
        {
            this.inputedWord = ev.inputedWord;
            this.listBox1.SelectedIndex = ev.foundIndex;
            this.listBox1.ScrollIntoView(this.listBox1.SelectedItem);
            this.popup.Placement = PlacementMode.Absolute;
            this.popup.PlacementRectangle = new Rect(ev.CaretPostion, new Size(listBox1.ActualWidth, listBox1.Height));
            this.popup.IsOpen = true;
        }

        /// <summary>
        /// 補完候補の非表示要求を処理する
        /// </summary>
        protected override void RequestCloseCompleteBox()
        {
            this.popup.IsOpen = false;
        }

        void DecideListBoxLocation(Document doc, ListBox listbox, Point p)
        {
        }

        internal bool ProcessKeyDown(FooTextBox textbox, KeyEventArgs e,bool isCtrl,bool isShift)
        {
            if (this.popup.IsOpen == false)
            {
                if (e.Key == Key.Space && isCtrl)
                {
                    this.OpenCompleteBox(string.Empty);
                    e.Handled = true;

                    return true;
                }
                return false;
            }

            switch (e.Key)
            {
                case Key.Escape:
                    this.RequestCloseCompleteBox();
                    textbox.Focus();
                    e.Handled = true;
                    return true;
                case Key.Down:
                    if (this.listBox1.SelectedIndex + 1 >= this.listBox1.Items.Count)
                        this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
                    else
                        this.listBox1.SelectedIndex++;
                    this.listBox1.ScrollIntoView(this.listBox1.SelectedItem);
                    e.Handled = true;
                    return true;
                case Key.Up:
                    if (this.listBox1.SelectedIndex - 1 < 0)
                        this.listBox1.SelectedIndex = 0;
                    else
                        this.listBox1.SelectedIndex--;
                    this.listBox1.ScrollIntoView(this.listBox1.SelectedItem);
                    e.Handled = true;
                    return true;
                case Key.Tab:
                case Key.Enter:
                    this.RequestCloseCompleteBox();
                    CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
                    this.SelectItem(this, new SelectItemEventArgs(selWord, this.inputedWord, this.Document));
                    e.Handled = true;
                    return true;
            }

            return false;
        }

        private void listBox1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.popup.IsOpen = false;
            CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
            this.SelectItem(this, new SelectItemEventArgs(selWord, this.inputedWord, this.Document));
        }

        void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.popup.IsOpen = false;
                CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
                this.SelectItem(this, new SelectItemEventArgs(selWord, this.inputedWord, this.Document));
                e.Handled = true;
            }
        }
    }
}
