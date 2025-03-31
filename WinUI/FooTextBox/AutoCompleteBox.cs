using System;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace FooEditEngine.WinUI
{
    public class AutoCompleteBox : AutoCompleteBoxBase
    {
        private string inputedWord;
        private ListBox listBox1 = new ListBox();
        private Flyout flyout = new Flyout();
        private Document doc;
        private FrameworkElement owner;

        public const int CompleteListBoxHeight = 200;

        public AutoCompleteBox(Document doc) : base(doc)
        {
            //リストボックスを追加する
            this.flyout.Content = this.listBox1;
            this.listBox1.DoubleTapped += ListBox1_DoubleTapped;
            this.listBox1.PreviewKeyDown += ListBox1_PreviewKeyDown;
            this.listBox1.Height = CompleteListBoxHeight;
            this.doc = doc;
        }

        public FrameworkElement Target
        {
            get
            {
                return this.owner;
            }
            set
            {
                if(this.owner != value)
                {
                    FlyoutBase.SetAttachedFlyout(value, this.flyout);
                    this.owner = value;
                }
            }
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
                return !this.flyout.IsOpen;
            }
        }

        protected override void RequestShowCompleteBox(ShowingCompleteBoxEventArgs ev)
        {
            this.inputedWord = ev.inputedWord;
            this.listBox1.SelectedIndex = ev.foundIndex;
            this.listBox1.ScrollIntoView(this.listBox1.SelectedItem);

            this.flyout.ShowAt(owner, new FlyoutShowOptions() { Position = ev.CaretPostion,ShowMode = FlyoutShowMode.Standard, Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft});
        }

        protected override void RequestCloseCompleteBox()
        {
            this.flyout.Hide();
        }

        public bool ProcessKeyDown(FooTextBox textbox, KeyRoutedEventArgs e,bool isCtrl,bool isShift)
        {
            if (this.flyout.IsOpen == false)
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
                    textbox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
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
            this.flyout.Hide();
            CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
            this.SelectItem(this, new SelectItemEventArgs(selWord, this.inputedWord, this.Document));
        }


        void ListBox1_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                this.flyout.Hide();
                CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
                this.SelectItem(this, new SelectItemEventArgs(selWord, this.inputedWord, this.Document));
                e.Handled = true;
            }
        }
    }
}
