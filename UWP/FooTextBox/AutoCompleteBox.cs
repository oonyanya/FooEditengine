using System;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace FooEditEngine.UWP
{
    public class AutoCompleteBox : AutoCompleteBoxBase
    {
        private string inputedWord;
        private ListBox listBox1 = new ListBox();
        private Popup popup = new Popup();
        private Document doc;

        public const int CompleteListBoxHeight = 200;

        public AutoCompleteBox(Document doc) : base(doc)
        {
            //リストボックスを追加する
            this.popup.Child = this.listBox1;
            this.listBox1.DoubleTapped += ListBox1_DoubleTapped;
            this.listBox1.KeyDown += listBox1_KeyDown;
            this.listBox1.Height = CompleteListBoxHeight;
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

        protected override bool IsCloseCompleteBox
        {
            get
            {
                return !this.popup.IsOpen;
            }
        }

        protected override void RequestShowCompleteBox(ShowingCompleteBoxEventArgs ev)
        {
            this.inputedWord = ev.inputedWord;
            this.listBox1.SelectedIndex = ev.foundIndex;
            this.listBox1.ScrollIntoView(this.listBox1.SelectedItem);
            this.popup.IsOpen = true;
            Canvas.SetLeft(this.popup, ev.CaretPostion.X);
            Canvas.SetTop(this.popup, ev.CaretPostion.Y);
        }

        protected override void RequestCloseCompleteBox()
        {
            this.popup.IsOpen = false;
        }

        public bool ProcessKeyDown(FooTextBox textbox, KeyRoutedEventArgs e,bool isCtrl,bool isShift)
        {
            if (this.popup.IsOpen == false)
            {
                if (e.Key == VirtualKey.Space && isCtrl)
                {
                    this.OpenCompleteBox(string.Empty);
                    e.Handled = true;

                    return true;
                }
                return false;
            }

            switch (e.Key)
            {
                case VirtualKey.Escape:
                    this.RequestCloseCompleteBox();
                    textbox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                    e.Handled = true;
                    return true;
                case VirtualKey.Down:
                    if (this.listBox1.SelectedIndex + 1 >= this.listBox1.Items.Count)
                        this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
                    else
                        this.listBox1.SelectedIndex++;
                    this.listBox1.ScrollIntoView(this.listBox1.SelectedItem);
                    e.Handled = true;
                    return true;
                case VirtualKey.Up:
                    if (this.listBox1.SelectedIndex - 1 < 0)
                        this.listBox1.SelectedIndex = 0;
                    else
                        this.listBox1.SelectedIndex--;
                    this.listBox1.ScrollIntoView(this.listBox1.SelectedItem);
                    e.Handled = true;
                    return true;
                case VirtualKey.Tab:
                case VirtualKey.Enter:
                    this.RequestCloseCompleteBox();
                    CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
                    this.SelectItem(this, new SelectItemEventArgs(selWord, this.inputedWord, this.Document));
                    e.Handled = true;
                    return true;
            }

            return false;
        }

        private void ListBox1_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.popup.IsOpen = false;
            CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
            this.SelectItem(this, new SelectItemEventArgs(selWord, this.inputedWord, this.Document));
        }

        void listBox1_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                this.popup.IsOpen = false;
                CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
                this.SelectItem(this, new SelectItemEventArgs(selWord, this.inputedWord, this.Document));
                e.Handled = true;
            }
        }
    }
}
