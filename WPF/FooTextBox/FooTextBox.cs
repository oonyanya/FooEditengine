/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Threading;
using DotNetTextStore;
using DotNetTextStore.UnmanagedAPI.TSF;
using DotNetTextStore.UnmanagedAPI.WinDef;
using Microsoft.Win32;

namespace FooEditEngine.WPF
{
    /// <summary>
    /// WPFでのFooTextBoxの実装
    /// </summary>
    public sealed class FooTextBox : Control, IDisposable
    {
        const double MaxFontSize = 72.0f;
        const double MinFontSize = 1;

        EditView _View;
        Controller _Controller;
        D2DRender Render;
        Image image;
        ScrollBar verticalScrollBar, horizontalScrollBar;
        TextStore textStore;
        DispatcherTimer timer;
        bool disposed = false;
        FooTextBoxAutomationPeer peer;
        bool isNotifyChanged = false;
        Document _Document;
        Popup popup;

        const int Interval = 96;
        const int IntervalWhenLostFocuse = 160;

        static FooTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FooTextBox), new FrameworkPropertyMetadata(typeof(FooTextBox)));
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(FooTextBox), new FrameworkPropertyMetadata(true));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(FooTextBox), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        public FooTextBox()
        {
            this.popup = new Popup();

            this.image = new Image();
            this.image.Stretch = Stretch.Fill;
            this.image.HorizontalAlignment = HorizontalAlignment.Left;
            this.image.VerticalAlignment = VerticalAlignment.Top;

            this.textStore = new TextStore();
            this.textStore.IsLoading += textStore_IsLoading;
            this.textStore.IsReadOnly += textStore_IsReadOnly;
            this.textStore.GetStringLength += () => {
                return this.Document.LayoutLines.GetRaw(this.Document.CaretPostion.row).Length;
            };
            this.textStore.GetString += _textStore_GetString;
            this.textStore.GetSelectionIndex += _textStore_GetSelectionIndex;
            this.textStore.SetSelectionIndex += _textStore_SetSelectionIndex;
            this.textStore.InsertAtSelection += _textStore_InsertAtSelection;
            this.textStore.GetHWnd += _textStore_GetHWnd;
            this.textStore.GetScreenExtent += _textStore_GetScreenExtent;
            this.textStore.GetStringExtent += _textStore_GetStringExtent;
            this.textStore.CompositionStarted += textStore_CompositionStarted;
            this.textStore.CompositionUpdated += textStore_CompositionUpdated;
            this.textStore.CompositionEnded += textStore_CompositionEnded;

            this.Render = new D2DRender(this, 200, 200,this.image);

            this.Document = new Document();

            this._View = new EditView(this.Document, this.Render, new Padding(5, 5, 5, 5));
            this._View.SrcChanged += View_SrcChanged;
            this._View.InsertMode = this.InsertMode;
            this.Document.DrawLineNumber = this.DrawLineNumber;
            this._View.HideCaret = !this.DrawCaret;
            this._View.HideLineMarker = !this.DrawCaretLine;
            this.Document.HideRuler = !this.DrawRuler;
            this.Document.UrlMark = this.MarkURL;
            this.Document.TabStops = this.TabChars;
            this.Document.ShowFullSpace = this.ShowFullSpace;
            this.Document.ShowHalfSpace = this.ShowHalfSpace;
            this.Document.ShowTab = this.ShowTab;

            this._Controller = new Controller(this.Document, this._View);
            this._Document.SelectionChanged += new EventHandler(Controller_SelectionChanged);

            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, CopyCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, CutCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, PasteCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, DeleteCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, SelectAllCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, UndoCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, RedoCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(EditingCommands.ToggleInsert, ToggleInsertCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(FooTextBoxCommands.ToggleRectSelectMode, ToggleRectSelectCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(FooTextBoxCommands.ToggleFlowDirection, ToggleFlowDirectionCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(FooTextBoxCommands.ToggleCodePoint, ToggleCodePointCommand, CanExecute));

            this.InputBindings.Add(new InputBinding(ApplicationCommands.Copy, new KeyGesture(Key.C, ModifierKeys.Control)));
            this.InputBindings.Add(new InputBinding(ApplicationCommands.Cut, new KeyGesture(Key.X, ModifierKeys.Control)));
            this.InputBindings.Add(new InputBinding(ApplicationCommands.Paste, new KeyGesture(Key.V, ModifierKeys.Control)));
            this.InputBindings.Add(new InputBinding(ApplicationCommands.Delete, new KeyGesture(Key.Delete, ModifierKeys.None)));
            this.InputBindings.Add(new InputBinding(ApplicationCommands.SelectAll, new KeyGesture(Key.A, ModifierKeys.Control)));
            this.InputBindings.Add(new InputBinding(ApplicationCommands.Undo, new KeyGesture(Key.Z, ModifierKeys.Control)));
            this.InputBindings.Add(new InputBinding(ApplicationCommands.Redo, new KeyGesture(Key.Y, ModifierKeys.Control)));
            this.InputBindings.Add(new InputBinding(EditingCommands.ToggleInsert, new KeyGesture(Key.Insert, ModifierKeys.None)));
            this.InputBindings.Add(new InputBinding(FooTextBoxCommands.ToggleCodePoint, new KeyGesture(Key.X, ModifierKeys.Alt)));

            this.timer = new DispatcherTimer();
            this.timer.Interval = new TimeSpan(0, 0, 0, 0, Interval);
            this.timer.Tick += new EventHandler(timer_Tick);

            this.Loaded += new RoutedEventHandler(FooTextBox_Loaded);

            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);

            this.SystemEvents_UserPreferenceChanged(null, new UserPreferenceChangedEventArgs(UserPreferenceCategory.Keyboard));

            this.CaretMoved += (s, e) => { };

            this.IsManipulationEnabled = true;
        }

        /// <summary>
        /// ファイナライザー
        /// </summary>
        ~FooTextBox()
        {
            //Dispose(false)を呼び出すと落ちる
            this.Dispose(false);
        }

        /// <summary>
        /// テンプレートを適用します
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Grid grid = this.GetTemplateChild("PART_Grid") as Grid;
            if (grid != null)
            {
                Grid.SetRow(this.image, 0);
                Grid.SetColumn(this.image, 0);
                grid.Children.Add(this.image);

                Grid.SetRow(this.popup, 0);
                Grid.SetColumn(this.popup, 0);
                grid.Children.Add(this.popup);
                //this.popup.PlacementTarget = this;
                this.popup.Placement = PlacementMode.Absolute;
            }

            this.horizontalScrollBar = this.GetTemplateChild("PART_HorizontalScrollBar") as ScrollBar;
            if (this.horizontalScrollBar != null)
            {
                this.horizontalScrollBar.SmallChange = 10;
                this.horizontalScrollBar.LargeChange = 100;
                this.horizontalScrollBar.Maximum = this.horizontalScrollBar.LargeChange + 1;
                this.horizontalScrollBar.Scroll += new ScrollEventHandler(horizontalScrollBar_Scroll);
            }
            this.verticalScrollBar = this.GetTemplateChild("PART_VerticalScrollBar") as ScrollBar;
            if (this.verticalScrollBar != null)
            {
                this.verticalScrollBar.SmallChange = 1;
                this.verticalScrollBar.LargeChange = 10;
                this.verticalScrollBar.Maximum = this.Document.LayoutLines.Count - 1;
                this.verticalScrollBar.Scroll += new ScrollEventHandler(verticalScrollBar_Scroll);
            }
        }

        /// <summary>
        /// ドキュメントを選択する
        /// </summary>
        /// <param name="start">開始インデックス</param>
        /// <param name="length">長さ</param>
        public void Select(int start, int length)
        {
            this.Document.Select(start, length);
            this.textStore.NotifySelectionChanged();
        }

        /// <summary>
        /// キャレットを指定した行に移動させます
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <remarks>このメソッドを呼び出すと選択状態は解除されます</remarks>
        public void JumpCaret(int index)
        {
            this._Controller.JumpCaret(index);
        }
        /// <summary>
        /// キャレットを指定した行と桁に移動させます
        /// </summary>
        /// <param name="row">行番号</param>
        /// <param name="col">桁</param>
        /// <remarks>このメソッドを呼び出すと選択状態は解除されます</remarks>
        public void JumpCaret(int row, int col)
        {
            this._Controller.JumpCaret(row, col);
        }

        /// <summary>
        /// 選択中のテキストをクリップボードにコピーします
        /// </summary>
        public void Copy()
        {
            string text = this._Controller.SelectedText;
            if (text != null && text != string.Empty)
                Clipboard.SetText(text);
        }

        /// <summary>
        /// 選択中のテキストをクリップボードに切り取ります
        /// </summary>
        public void Cut()
        {
            string text = this._Controller.SelectedText;
            if (text != null && text != string.Empty)
            {
                Clipboard.SetText(text);
                this._Controller.SelectedText = "";
            }
        }

        /// <summary>
        /// 選択中のテキストを貼り付けます
        /// </summary>
        public void Paste()
        {
            if (Clipboard.ContainsText() == false)
                return;
            string text = Clipboard.GetText();
            this._Controller.SelectedText = text;
        }

        /// <summary>
        /// 選択を解除する
        /// </summary>
        public void DeSelectAll()
        {
            this._Controller.DeSelectAll();
            this.textStore.NotifySelectionChanged();
        }

        /// <summary>
        /// 対応する座標を返します
        /// </summary>
        /// <param name="tp">テキストポイント</param>
        /// <returns>座標</returns>
        /// <remarks>テキストポイントがクライアント領域の原点より外にある場合、返される値は原点に丸められます</remarks>
        public System.Windows.Point GetPostionFromTextPoint(TextPoint tp)
        {
            if (this.Document.FireUpdateEvent == false)
                throw new InvalidOperationException("");
            return this.image.TranslatePoint(this._View.GetPostionFromTextPoint(tp),this);
        }

        /// <summary>
        /// 対応するテキストポイントを返します
        /// </summary>
        /// <param name="p">クライアント領域の原点を左上とする座標</param>
        /// <returns>テキストポイント</returns>
        public TextPoint GetTextPointFromPostion(System.Windows.Point p)
        {
            if (this.Document.FireUpdateEvent == false)
                throw new InvalidOperationException("");
            System.Windows.Point relP = this.TranslatePoint(p, this.image);
            return this._View.GetTextPointFromPostion(p);
        }

        /// <summary>
        /// 行の高さを取得します
        /// </summary>
        /// <param name="row">レイアウト行</param>
        /// <returns>行の高さ</returns>
        public double GetLineHeight(int row)
        {
            if (this.Document.FireUpdateEvent == false)
                throw new InvalidOperationException("");
            return this._View.LayoutLines.GetLayout(row).Height;;
        }

        /// <summary>
        /// インデックスに対応する座標を得ます
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>座標を返す</returns>
        public System.Windows.Point GetPostionFromIndex(int index)
        {
            if (this.Document.FireUpdateEvent == false)
                throw new InvalidOperationException("");
            TextPoint tp = this._View.GetLayoutLineFromIndex(index);
            return this._View.GetPostionFromTextPoint(tp);
        }

        /// <summary>
        /// 座標からインデックスに変換します
        /// </summary>
        /// <param name="p">座標</param>
        /// <returns>インデックスを返す</returns>
        public int GetIndexFromPostion(System.Windows.Point p)
        {
            if (this.Document.FireUpdateEvent == false)
                throw new InvalidOperationException("");
            TextPoint tp = this._View.GetTextPointFromPostion(p);
            return this._View.GetIndexFromLayoutLine(tp);
        }

        /// <summary>
        /// 再描写する
        /// </summary>
        public void Refresh()
        {
            this.Refresh(this._View.PageBound);
        }

        /// <summary>
        /// レイアウト行をすべて破棄し、再度レイアウトを行う
        /// </summary>
        public void PerfomLayouts()
        {
            this.Document.PerformLayout();
        }

        /// <summary>
        /// 指定行までスクロールする
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="alignTop">指定行を画面上に置くなら真。そうでないなら偽</param>
        public void ScrollIntoView(int row, bool alignTop)
        {
            this._View.ScrollIntoView(row, alignTop);
        }

        /// <summary>
        /// ストリームからドキュメントを構築する
        /// </summary>
        /// <param name="tr">TextReader</param>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns>Taskオブジェクト</returns>
        public async Task LoadAsync(System.IO.TextReader tr, System.Threading.CancellationTokenSource token)
        {
            await this.Document.LoadAsync(tr, token);
        }

        /// <summary>
        /// ファイルからドキュメントを構築する
        /// </summary>
        /// <param name="filepath">ファイルパス</param>
        /// <param name="enc">エンコード</param>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns>Taskオブジェクト</returns>
        public async Task LoadFileAsync(string filepath, Encoding enc,System.Threading.CancellationTokenSource token)
        {
            var fs = new System.IO.StreamReader(filepath, enc);
            await this.Document.LoadAsync(fs, token);
            fs.Close();
        }

        private void Document_LoadProgress(object sender, ProgressEventArgs e)
        {
            if (e.state == ProgressState.Start)
            {
                this.IsEnabled = false;
            }
            else if (e.state == ProgressState.Complete)
            {
                TextStoreHelper.NotifyTextChanged(this.textStore, 0, 0, this.Document.Length);
                if (this.verticalScrollBar != null)
                    this.verticalScrollBar.Maximum = this._View.LayoutLines.Count;
                this._View.CalculateWhloeViewPort();
                this._View.CalculateLineCountOnScreen();
                this.IsEnabled = true;
                this.Refresh(this._View.PageBound);
            }
        }

        /// <summary>
        /// ドキュメントの内容をファイルに保存する
        /// </summary>
        /// <param name="filepath">ファイルパス</param>
        /// <param name="newLine">改行コード</param>
        /// <param name="enc">エンコード</param>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns>Taskオブジェクト</returns>
        public async Task SaveFile(string filepath, Encoding enc,string newLine, System.Threading.CancellationTokenSource token)
        {
            var fs = new System.IO.StreamWriter(filepath, false , enc);
            fs.NewLine = newLine;
            await this.Document.SaveAsync(fs, token);
            fs.Close();
        }

        /// <summary>
        /// アンマネージドリソースを開放する
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
                return;
            this.Dispose(true);
            GC.SuppressFinalize(this);
            this.disposed = true;
        }

        /// <summary>
        /// リソースを開放する
        /// </summary>
        /// <param name="disposing">真ならマネージドリソースも開放し、そうでないならアンマネージドリソースのみを開放する</param>
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.textStore.Dispose();
                this.timer.Stop();
                this._View.Dispose();
                this.Render.Dispose();
            }
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
        }
        
        void Refresh(Rectangle updateRect)
        {
            if (this.disposed || this.Visibility == Visibility.Collapsed)
                return;

            this.Render.DrawContent(this._View, this.IsEnabled, updateRect);
            this.Document.IsRequestRedraw = false;
        }

        #region Commands
        void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.IsEnabled;
        }

        void ToggleCodePointCommand(object sender, RoutedEventArgs e)
        {
            if (!this._Controller.ConvertToChar())
                this._Controller.ConvertToCodePoint();
            this.Refresh();
        }

        void CopyCommand(object sender, RoutedEventArgs e)
        {
            this.Copy();
        }

        void CutCommand(object sender, RoutedEventArgs e)
        {
            this.Cut();
            this.Refresh();
        }

        void PasteCommand(object sender, RoutedEventArgs e)
        {
            this.Paste();
            this.Refresh();
        }

        void DeleteCommand(object sender, RoutedEventArgs e)
        {
            int oldLength = this.Document.Length;
            this._Controller.DoDeleteAction();
            this.Refresh();
        }

        void SelectAllCommand(object sender, RoutedEventArgs e)
        {
            this.Select(0, this.Document.Length);
            this.Refresh();
        }

        void UndoCommand(object sender, RoutedEventArgs e)
        {
            int oldLength = this.Document.Length;
            this.Document.UndoManager.undo();
            this.Refresh();
        }

        void RedoCommand(object sender, RoutedEventArgs e)
        {
            int oldLength = this.Document.Length;
            this.Document.UndoManager.redo();
            this.Refresh();
        }

        void ToggleInsertCommand(object sender, RoutedEventArgs e)
        {
            if (this.InsertMode)
                this.InsertMode = false;
            else
                this.InsertMode = true;
            this.Refresh();
        }

        void ToggleRectSelectCommand(object sender, RoutedEventArgs e)
        {
            if (this.RectSelectMode)
                this.RectSelectMode = false;
            else
                this.RectSelectMode = true;
            this.Refresh();
        }
        void ToggleFlowDirectionCommand(object sender, RoutedEventArgs e)
        {
            if (this.FlowDirection == System.Windows.FlowDirection.LeftToRight)
                this.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                this.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            this.Refresh();
        }
        #endregion
        #region TSF
        internal TextStore TextStore
        {
            get { return this.textStore; }
        }

        bool textStore_IsReadOnly()
        {
            return false;
        }

        bool textStore_IsLoading()
        {
            return false;
        }

        void textStore_CompositionEnded()
        {
            TextStoreHelper.EndCompostion(this.Document);
            this.Refresh();
        }

        void textStore_CompositionUpdated(int start, int end)
        {
            int inputImeStartIndex = this.Document.LayoutLines.GetLineHeadIndex(this.Document.CaretPostion.row);
            if (TextStoreHelper.ScrollToCompstionUpdated(this.textStore, this._View, inputImeStartIndex + start, inputImeStartIndex + end))
                this.Refresh();
        }
        bool textStore_CompositionStarted()
        {
            bool result = TextStoreHelper.StartCompstion(this.Document);
            if (!result)
                System.Media.SystemSounds.Beep.Play();
            return result;
        }

        string _textStore_GetString(int start, int length)
        {
            int inputImeStartIndex = this.Document.LayoutLines.GetLineHeadIndex(this.Document.CaretPostion.row);
            return this.Document.ToString(inputImeStartIndex + start, length);
        }

        IntPtr _textStore_GetHWnd()
        {
            var hwndSource = HwndSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
                return hwndSource.Handle;
            else
                return IntPtr.Zero;
        }

        void _textStore_GetStringExtent(
            int i_startIndex,
            int i_endIndex,
            out POINT o_topLeft,
            out POINT o_bottomRight
        )
        {
            Point startPos, endPos;
            int inputImeStartIndex = this.Document.LayoutLines.GetLineHeadIndex(this.Document.CaretPostion.row);
            TextStoreHelper.GetStringExtent(this.Document, this._View, inputImeStartIndex + i_startIndex, inputImeStartIndex + i_endIndex, out startPos, out endPos);

            double scale = this.Render.GetScale();
            
            startPos = PointToScreen(this.TranslatePoint(startPos.Scale(scale), this));
            endPos = PointToScreen(this.TranslatePoint(endPos.Scale(scale), this));
            
            o_topLeft = new POINT((int)startPos.X, (int)startPos.Y);
            o_bottomRight = new POINT((int)endPos.X, (int)endPos.Y);
        }

        void _textStore_GetScreenExtent(out POINT o_topLeft, out POINT o_bottomRight)
        {
            var pointTopLeft = new Point(0, 0);
            var pointBottomRight = new Point(this.RenderSize.Width, this.RenderSize.Height);

            pointTopLeft = PointToScreen(pointTopLeft);
            pointBottomRight = PointToScreen(pointBottomRight);

            o_topLeft = new POINT((int)pointTopLeft.X, (int)pointTopLeft.Y);
            o_bottomRight = new POINT((int)pointBottomRight.X, (int)pointBottomRight.Y);
        }

        void _textStore_GetSelectionIndex(int start_index, int max_count, out DotNetTextStore.TextSelection[] sels)
        {
            TextRange selRange;
            TextStoreHelper.GetSelection(this._Controller, this._View.Selections, out selRange);

            int inputImeStartIndex = this.Document.LayoutLines.GetLineHeadIndex(this.Document.CaretPostion.row);

            sels = new DotNetTextStore.TextSelection[1];
            sels[0] = new DotNetTextStore.TextSelection();
            sels[0].start = selRange.Index - inputImeStartIndex;
            sels[0].end = selRange.Index + selRange.Length - inputImeStartIndex;
        }

        void _textStore_SetSelectionIndex(DotNetTextStore.TextSelection[] sels)
        {
            int inputImeStartIndex = this.Document.LayoutLines.GetLineHeadIndex(this.Document.CaretPostion.row);
            TextStoreHelper.SetSelectionIndex(this._Controller, this._View, sels[0].start + inputImeStartIndex, sels[0].end + inputImeStartIndex);
            this.Refresh();
        }

        void _textStore_InsertAtSelection(string i_value, ref int o_startIndex, ref int o_endIndex)
        {
            int inputImeStartIndex = this.Document.LayoutLines.GetLineHeadIndex(this.Document.CaretPostion.row);
            TextStoreHelper.InsertTextAtSelection(this._Controller, i_value,o_startIndex + inputImeStartIndex, o_endIndex + inputImeStartIndex);
            this.Refresh();
        }

        /// <summary>
        /// キーボードフォーカスが取得されたときに呼ばれます
        /// </summary>
        /// <param name="e">イベントデーター</param>
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            this.textStore.SetFocus();
            this._View.IsFocused = true;
            this.timer.Interval = new TimeSpan(0,0,0,0,Interval);
            this.Refresh();
        }

        /// <summary>
        /// キーボードフォーカスが失われたときに呼ばれます
        /// </summary>
        /// <param name="e">イベントデーター</param>
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            this._View.IsFocused = false;
            this.timer.Interval = new TimeSpan(0, 0, 0, 0, IntervalWhenLostFocuse);
            this.Refresh();
        }
        #endregion
        #region Event
        /// <summary>
        /// キャレットが移動したときに通知されるイベント
        /// </summary>
        public event EventHandler CaretMoved;

        /// <inheritdoc/>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            this.peer = new FooTextBoxAutomationPeer(this);
            return this.peer;
        }


        /// <inheritdoc/>
        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                this._Controller.DoEnterAction();
            }
            else if (e.Text == "\b")
            {
                this._Controller.DoBackSpaceAction();
            }
            else
            {
                if(this.IsInputString(e.Text))
                    this._Controller.DoInputString(e.Text);
            }
            this.Refresh();
            base.OnTextInput(e);
            e.Handled = true;
        }

        bool IsInputString(string s)
        {
            foreach (char charCode in s)
            {
                if ((0x20 <= charCode && charCode <= 0x7e)
                    || 0x7f < charCode)
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (this.textStore.IsLocked())
                return;

            ModifierKeys modiferKeys = e.KeyboardDevice.Modifiers;

            var autocomplete = this.Document.AutoComplete as AutoCompleteBox;
            if (autocomplete != null &&
                autocomplete.ProcessKeyDown(this,e, this.IsPressedModifierKey(modiferKeys, ModifierKeys.Control), this.IsPressedModifierKey(modiferKeys, ModifierKeys.Shift)))
            {
                e.Handled = true;
                return;
            }

            bool movedCaret = false;
            double alignedPage = (int)(this.Render.TextArea.Height / this.Render.emSize.Height) * this.Render.emSize.Height;
            switch (e.Key)
            {
                case Key.Up:
                    this._Controller.MoveCaretVertical(-1, this.IsPressedModifierKey(modiferKeys, ModifierKeys.Shift));
                    this.Refresh();
                    e.Handled = true;
                    movedCaret = true;
                    break;
                case Key.Down:
                    this._Controller.MoveCaretVertical(+1, this.IsPressedModifierKey(modiferKeys, ModifierKeys.Shift));
                    this.Refresh();
                    e.Handled = true;
                    movedCaret = true;
                    break;
                case Key.Left:
                    this._Controller.MoveCaretHorizontical(-1, this.IsPressedModifierKey(modiferKeys, ModifierKeys.Shift), this.IsPressedModifierKey(modiferKeys, ModifierKeys.Control));
                    this.Refresh();
                    e.Handled = true;
                    movedCaret = true;
                    break;
                case Key.Right:
                    this._Controller.MoveCaretHorizontical(1, this.IsPressedModifierKey(modiferKeys, ModifierKeys.Shift), this.IsPressedModifierKey(modiferKeys, ModifierKeys.Control));
                    this.Refresh();
                    e.Handled = true;
                    movedCaret = true;
                    break;
                case Key.PageUp:
                    this._Controller.ScrollByPixel(ScrollDirection.Up, alignedPage, this.IsPressedModifierKey(modiferKeys, ModifierKeys.Shift), true);
                    this.Refresh();
                    movedCaret = true;
                    break;
                case Key.PageDown:
                    this._Controller.ScrollByPixel(ScrollDirection.Down, alignedPage, this.IsPressedModifierKey(modiferKeys, ModifierKeys.Shift), true);
                    this.Refresh();
                    movedCaret = true;
                    break;
                case Key.Home:
                    if (this.IsPressedModifierKey(modiferKeys, ModifierKeys.Control))
                        this._Controller.JumpToHead(this.IsPressedModifierKey(modiferKeys, ModifierKeys.Shift));
                    else
                        this._Controller.JumpToLineHead(this.Document.CaretPostion.row, this.IsPressedModifierKey(modiferKeys, ModifierKeys.Shift));
                    this.Refresh();
                    movedCaret = true;
                    break;
                case Key.End:
                    if (this.IsPressedModifierKey(modiferKeys, ModifierKeys.Control))
                        this._Controller.JumpToEnd(this.IsPressedModifierKey(modiferKeys, ModifierKeys.Shift));
                    else
                        this._Controller.JumpToLineEnd(this.Document.CaretPostion.row, this.IsPressedModifierKey(modiferKeys, ModifierKeys.Shift));
                    this.Refresh();
                    movedCaret = true;
                    break;
                case Key.Tab:
                    int oldLength = this.Document.Length;
                    if (this.Selection.Length == 0)
                        this._Controller.DoInputChar('\t');
                    else if(this.IsPressedModifierKey(modiferKeys,ModifierKeys.Shift))
                        this._Controller.DownIndent();
                    else
                        this._Controller.UpIndent();
                    this.Refresh();
                    e.Handled = true;
                    break;
            }
            if (movedCaret && this.peer != null)
                this.peer.OnNotifyCaretChanged();
            base.OnKeyDown(e);
        }

        bool IsPressedModifierKey(ModifierKeys keys, ModifierKeys pressed)
        {
            if (keys == pressed)
                return true;
            if ((keys & pressed) == pressed)
                return true;
            return false;
        }

        /// <summary>
        /// ダブルクリックされたときに呼ばれます
        /// </summary>
        /// <param name="e">イベントパラメーター</param>
        /// <remarks>
        /// イベントパラメーターはFooMouseEventArgsにキャスト可能です。
        /// e.Handledを真にした場合、単語単位の選択が行われなくなります
        /// </remarks>
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            var p = this.GetDipFromPoint(e.GetPosition(this));
            TextPoint tp = this._View.GetTextPointFromPostion(p);
            if (tp == TextPoint.Null)
                return;
            int index = this._View.LayoutLines.GetIndexFromTextPoint(tp);

            FooMouseButtonEventArgs newEventArgs = new FooMouseButtonEventArgs(e.MouseDevice,
                e.Timestamp,
                e.ChangedButton,
                e.StylusDevice,
                index);
            newEventArgs.RoutedEvent = e.RoutedEvent;
            base.OnMouseDoubleClick(newEventArgs);

            if (newEventArgs.Handled)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (p.X < this.Render.TextArea.X)
                    this.Document.SelectLine((int)index);
                else
                    this.Document.SelectWord((int)index);

                this.textStore.NotifySelectionChanged();
                if(this.peer != null)
                    this.peer.OnNotifyCaretChanged();
                this.Refresh();
            }
        }

        /// <summary>
        /// マウスボタンが押されたときに呼ばれます
        /// </summary>
        /// <param name="e">イベントパラメーター</param>
        /// <remarks>
        /// イベントパラメーターはFooMouseEventArgsにキャスト可能です。
        /// e.Handledを真にした場合、キャレットの移動処理が行われなくなります
        /// </remarks>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            this.CaptureMouse();

            var p = this.GetDipFromPoint(e.GetPosition(this));
            TextPoint tp = this._View.GetTextPointFromPostion(p);
            if (tp == TextPoint.Null)
                return;
            int index = this._View.LayoutLines.GetIndexFromTextPoint(tp);

            FooMouseButtonEventArgs newEventArgs = new FooMouseButtonEventArgs(e.MouseDevice,
                e.Timestamp,
                e.ChangedButton,
                e.StylusDevice,
                index);
            newEventArgs.RoutedEvent = e.RoutedEvent;
            base.OnMouseDown(newEventArgs);

            if (newEventArgs.Handled)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                FoldingItem foldingData = this._View.HitFoldingData(p.X,tp.row);
                if (foldingData != null)
                {
                    if (foldingData.Expand)
                        this._View.LayoutLines.FoldingCollection.Collapse(foldingData);
                    else
                        this._View.LayoutLines.FoldingCollection.Expand(foldingData);
                    this._Controller.JumpCaret(foldingData.Start,false);
                }
                else
                {
                    this._Controller.JumpCaret(tp.row, tp.col, false);
                }
                if (this.peer != null)
                    this.peer.OnNotifyCaretChanged();
                this._View.IsFocused = true;
                this.Focus();
                this.Document.SelectGrippers.BottomLeft.Enabled = false;
                this.Document.SelectGrippers.BottomRight.Enabled = false;
                this.Refresh();
            }
        }

        /// <summary>
        /// マウスのボタンが離されたときに呼ばれます
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            base.OnMouseUp(e);
        }

        /// <summary>
        /// マウスが移動したときに呼ばれます
        /// </summary>
        /// <param name="e">イベントパラメーター</param>
        /// <remarks>
        /// イベントパラメーターはFooMouseEventArgsにキャスト可能です。
        /// e.Handledを真にした場合、選択処理と状況に応じたカーソルの変化が行われなくなります
        /// </remarks>
        protected override void  OnMouseMove(MouseEventArgs e)
        {
            bool leftPressed = e.LeftButton == MouseButtonState.Pressed;

            var p = this.GetDipFromPoint(e.GetPosition(this));

            TextPointSearchRange searchRange;
            if (this._View.HitTextArea(p.X, p.Y))
            {
                searchRange = TextPointSearchRange.TextAreaOnly;
            }
            else if (leftPressed)
            {
                searchRange = TextPointSearchRange.Full;
            }
            else
            {
                this.Cursor = Cursors.Arrow;
                base.OnMouseMove(e);
                return;
            }

            TextPoint tp = this._View.GetTextPointFromPostion(p, searchRange);

            if (tp == TextPoint.Null)
            {
                this.Cursor = Cursors.Arrow;
                base.OnMouseMove(e);
                return;
            }

            int index = this._View.GetIndexFromLayoutLine(tp);

            FooMouseEventArgs newEventArgs = new FooMouseEventArgs(e.MouseDevice, e.Timestamp, e.StylusDevice, index);
            newEventArgs.RoutedEvent = e.RoutedEvent;
            base.OnMouseMove(newEventArgs);

            if (newEventArgs.Handled)
                return;

            //この状態のときはカーソルがテキストエリア内にある
            if (searchRange == TextPointSearchRange.TextAreaOnly)
            {
                if (this._Controller.IsMarker(tp, HilightType.Url))
                    this.Cursor = Cursors.Hand;
                else
                    this.Cursor = Cursors.IBeam;
            }
            else
            {
                this.Cursor = Cursors.Arrow;
            }

            //スクロールバーを押した場合はキャレットを移動させる必要がない
            if (leftPressed && e.OriginalSource.GetType() == typeof(FooTextBox))
            {
                bool controlPressed = (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) == KeyStates.Down;
                this._Controller.MoveCaretAndSelect(tp, controlPressed);
                if (this.peer != null)
                    this.peer.OnNotifyCaretChanged();
                this.Refresh();
            }
        }

        Gripper hittedGripper;
        bool touchScrolled = false;

        /// <inheritdoc/>
        protected override void OnTouchDown(TouchEventArgs e)
        {
            var p = this.GetDipFromPoint(e.GetTouchPoint(this).Position);
            this.hittedGripper = this._View.HitGripperFromPoint(p);
            this.CaptureTouch(e.TouchDevice);
        }

        /// <inheritdoc/>
        protected override void OnTouchUp(TouchEventArgs e)
        {
            this.ReleaseTouchCapture(e.TouchDevice);
            if(this.hittedGripper != null || this.touchScrolled)
            {
                this.hittedGripper = null;
                this.touchScrolled = false;
                return;
            }

            var p = this.GetDipFromPoint(e.GetTouchPoint(this).Position);
            TextPoint tp = this._View.GetTextPointFromPostion(p);
            if (tp == TextPoint.Null)
                return;
            int index = this._View.LayoutLines.GetIndexFromTextPoint(tp);

            FoldingItem foldingData = this._View.HitFoldingData(p.X, tp.row);
            if (foldingData != null)
            {
                if (foldingData.Expand)
                    this._View.LayoutLines.FoldingCollection.Collapse(foldingData);
                else
                    this._View.LayoutLines.FoldingCollection.Expand(foldingData);
                this._Controller.JumpCaret(foldingData.Start, false);
            }
            else
            {
                this._Controller.JumpCaret(tp.row, tp.col, false);
            }
            if (this.peer != null)
                this.peer.OnNotifyCaretChanged();
            this._View.IsFocused = true;
            this.Focus();
            this.Document.SelectGrippers.BottomLeft.Enabled = false;
            this.Document.SelectGrippers.BottomRight.Enabled = true;
            this.Refresh();
        }

        /// <inheritdoc/>
        protected override void OnTouchMove(TouchEventArgs e)
        {
            var p = this.GetDipFromPoint(e.GetTouchPoint(this).Position);
            if (this.Controller.MoveCaretAndGripper(p, this.hittedGripper))
            {
                if (this.peer != null)
                    this.peer.OnNotifyCaretChanged();
                this.Refresh();
            }
        }

        /// <inheritdoc/>
        protected override void OnManipulationInertiaStarting(ManipulationInertiaStartingEventArgs e)
        {
        }

        /// <inheritdoc/>
        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            if (this.hittedGripper != null)
                return;

            Point translation = new Point(e.DeltaManipulation.Translation.X, e.DeltaManipulation.Translation.Y);

            //Xの絶対値が大きければ横方向のスクロールで、そうでなければ縦方向らしい
            if (Math.Abs(e.CumulativeManipulation.Translation.X) < Math.Abs(e.CumulativeManipulation.Translation.Y))
            {
                int deltay = (int)Math.Abs(Math.Ceiling(translation.Y));
                if (translation.Y < 0)
                    this._Controller.ScrollByPixel(ScrollDirection.Down, deltay, false, false);
                else
                    this._Controller.ScrollByPixel(ScrollDirection.Up, deltay, false, false);
                this.touchScrolled = true;
                this.Refresh();
                return;
            }

            int deltax = (int)Math.Abs(Math.Ceiling(translation.X));
            if (deltax != 0)
            {
                if (translation.X < 0)
                    this._Controller.Scroll(ScrollDirection.Left, deltax, false, false);
                else
                    this._Controller.Scroll(ScrollDirection.Right, deltax, false, false);
                this.touchScrolled = true;
                this.Refresh();
            }
        }

        private Point GetDipFromPoint(Point p)
        {
            float dpi;
            this.Render.GetDpi(out dpi,out dpi);
            double scale = dpi / 96.0;
            return p.Scale(1 / scale);
        }

        /// <inheritdoc/>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if(Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Delta > 0)
                    this._Controller.ScrollByPixel(ScrollDirection.Up, SystemParameters.WheelScrollLines * this.Render.emSize.Height, false, false);
                else
                    this._Controller.ScrollByPixel(ScrollDirection.Down, SystemParameters.WheelScrollLines * this.Render.emSize.Height, false, false);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double newFontSize = this.Render.FontSize;
                if (e.Delta > 0)
                    newFontSize++;
                else
                    newFontSize--;
                if (newFontSize > MaxFontSize)
                    newFontSize = 72;
                else if (newFontSize < MinFontSize)
                    newFontSize = 1;
                this.Render.FontSize = newFontSize;
                SetValue(MagnificationPowerPropertyKey, this.Render.FontSize / this.FontSize);
            }
            this.Refresh();
            base.OnMouseWheel(e);
        }

        void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Keyboard)
            {
                int blinkTime = (int)NativeMethods.GetCaretBlinkTime();
                this._View.CaretBlink = blinkTime >= 0;
                this._View.CaretBlinkTime = blinkTime * 2;
            }
            if (e.Category == UserPreferenceCategory.General)
            {
                this._View.CaretWidthOnInsertMode = SystemParameters.CaretWidth;
            }
        }

        void Document_Update(object sender, DocumentUpdateEventArgs e)
        {
            if (this.textStore.IsLocked())
                return;
            if(e.type == UpdateType.Replace)
                TextStoreHelper.NotifyTextChanged(this.textStore, e.startIndex, e.removeLength, e.insertLength);
            if(this.peer != null)
                this.peer.OnNotifyTextChanged();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (this.image.ActualWidth == 0 || this.image.ActualHeight == 0)
                return;
            if (this.Resize(this.image.ActualWidth, this.image.ActualHeight))
            {
                this.Refresh(this._View.PageBound);
                return;
            }

            bool updateAll = this._View.LayoutLines.HilightAll() || this._View.LayoutLines.GenerateFolding() || this.Document.IsRequestRedraw;

            this.Refresh(this._View.PageBound);
            /*
             * HIGHDPIだとうまく動かない
            if (updateAll)
                this.Refresh(this._View.PageBound);
            else
                this.Refresh(this._View.GetCurrentCaretRect());
            */
        }

        void horizontalScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (this.horizontalScrollBar == null)
                return;
            double toX;
            if (this.FlowDirection == System.Windows.FlowDirection.LeftToRight)
                toX = this.horizontalScrollBar.Value;
            else
                toX = -this.horizontalScrollBar.Value;
            this.Controller.ScrollByPixel(ScrollDirection.Left, (int)toX, false, false);
            this.Refresh();
        }

        void verticalScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (this.verticalScrollBar == null)
                return;
            this.Controller.Scroll(this.Document.Src.X, (int)this.verticalScrollBar.Value, false, false);
            this.Refresh();
        }

        void View_SrcChanged(object sender, EventArgs e)
        {
            if (this.horizontalScrollBar == null || this.verticalScrollBar == null)
                return;
            EditView view = this._View;
            if (view.Src.Row > this.Document.LayoutLines.Count)
                this.verticalScrollBar.Maximum = this.Document.LayoutLines.Count - 1;
            double absoulteX = Math.Abs(view.Src.X);
            if(absoulteX > this.horizontalScrollBar.Maximum)
                this.horizontalScrollBar.Maximum = absoulteX + view.PageBound.Width + 1;
            if(view.Src.Row != this.verticalScrollBar.Value)
                this.verticalScrollBar.Value = view.Src.Row;
            if (view.Src.X != this.horizontalScrollBar.Value)
                this.horizontalScrollBar.Value = Math.Abs(view.Src.X);
        }

        void Controller_SelectionChanged(object sender, EventArgs e)
        {
            this._View.CaretBlink = this._View.CaretBlink;
            this.CaretMoved(this, null);
            //こうしないと選択できなくなってしまう
            this.isNotifyChanged = true;
            SetValue(SelectedTextProperty, this._Controller.SelectedText);
            SetValue(SelectionProperty, new TextRange(this._Controller.SelectionStart, this._Controller.SelectionLength));
            SetValue(CaretPostionProperty, this.Document.CaretPostion);
            this.isNotifyChanged = false;
            if (this.textStore.IsLocked() == false)
                this.textStore.NotifySelectionChanged();
        }

        void FooTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            this.Resize(this.image.ActualWidth, this.image.ActualHeight);
            this.Focus();
            this.timer.Start();
        }

        bool Resize(double width, double height)
        {
            if (width == 0 || height == 0)
                throw new ArgumentOutOfRangeException();
            if (this.Render.Resize(width, height))
            {
                this._View.PageBound = new Rectangle(0, 0, width, height);

                if (this.horizontalScrollBar != null)
                {
                    this.horizontalScrollBar.LargeChange = this._View.PageBound.Width;
                    this.horizontalScrollBar.Maximum = this._View.LongestWidth + this.horizontalScrollBar.LargeChange + 1;
                }
                if (this.verticalScrollBar != null)
                {
                    this.verticalScrollBar.LargeChange = this._View.LineCountOnScreen;
                    this.verticalScrollBar.Maximum = this._View.LayoutLines.Count + this.verticalScrollBar.LargeChange + 1;
                }
                return true;
            }
            return false;
        }

        private void SetDocument(Document value)
        {
            if (value == null)
                return;

            Document old_doc = this._Document;
            int oldLength = 0;
            if (this._Document != null)
            {
                old_doc.Update -= new DocumentUpdateEventHandler(Document_Update);
                old_doc.LoadProgress -= Document_LoadProgress;
                old_doc.SelectionChanged -= new EventHandler(Controller_SelectionChanged);
                old_doc.AutoCompleteChanged -= _Document_AutoCompleteChanged;
                oldLength = old_doc.Length;
                if (this._Document.AutoComplete != null)
                {
                    ((AutoCompleteBox)this._Document.AutoComplete).TargetPopup = null;
                    this._Document.AutoComplete.GetPostion = null;
                    this._Document.AutoComplete = null;
                }
            }

            this._Document = value;
            this._Document.LayoutLines.Render = this.Render;
            this._Document.Update += new DocumentUpdateEventHandler(Document_Update);
            this._Document.LoadProgress += Document_LoadProgress;
            this._Document.AutoCompleteChanged += _Document_AutoCompleteChanged;
            if (this._Document.AutoComplete != null && this.Document.AutoComplete.GetPostion == null)
                this._Document_AutoCompleteChanged(this.Document, null);
            //初期化が終わっていればすべて存在する
            if (this.Controller != null && this._View != null && this.textStore != null)
            {
                this._Document.SelectionChanged += new EventHandler(Controller_SelectionChanged);

                this.Controller.Document = value;
                this._View.Document = value;
                this.Controller.AdjustCaret();
                this.textStore.NotifyTextChanged(oldLength, value.Length);

                //依存プロパティとドキュメント内容が食い違っているので再設定する
                this.ShowFullSpace = value.ShowFullSpace;
                this.ShowHalfSpace = value.ShowHalfSpace;
                this.ShowLineBreak = value.ShowLineBreak;
                this.ShowTab = value.ShowTab;
                this.FlowDirection = value.RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                this.IndentMode = value.IndentMode;
                this.DrawCaretLine = !value.HideLineMarker;
                this.InsertMode = value.InsertMode;
                this.DrawRuler = !value.HideRuler;
                this.DrawLineNumber = value.DrawLineNumber;
                this.MarkURL = value.UrlMark;
                this.LineBreakMethod = value.LineBreak;
                this.LineBreakCharCount = value.LineBreakCharCount;
                this.TabChars = value.TabStops;

                this.Refresh();
            }
        }

        private void _Document_AutoCompleteChanged(object sender, EventArgs e)
        {
            Document doc = (Document)sender;
            ((AutoCompleteBox)this._Document.AutoComplete).TargetPopup = this.popup;
            this._Document.AutoComplete.GetPostion = (tp, edoc) =>
            {
                var p = this._View.GetPostionFromTextPoint(tp);
                int height = (int)this.Render.emSize.Height;
                p.Y += height;
                return PointToScreen(this.TranslatePoint(p.Scale(Util.GetScale()), this));
            };
        }

        /// <summary>
        /// プロパティーが変更されたときに呼ばれます
        /// </summary>
        /// <param name="e">イベントパラメーター</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Document":
                    this.SetDocument(this.Document);
                    break;
                case "Hilighter":
                    this._View.Hilighter = this.Hilighter;
                    break;
                case "TextAntialiasMode":
                    this.Render.TextAntialiasMode = this.TextAntialiasMode;
                    break;
                case "FoldingStrategy":
                    this._View.LayoutLines.FoldingStrategy = this.FoldingStrategy;
                    break;
                case "SelectedText":
                    if (!this.isNotifyChanged)
                        this._Controller.SelectedText = this.SelectedText;
                    break;
                case "IndentMode":
                    this._Controller.IndentMode = this.IndentMode;
                    break;
                case "Selection":
                    if(!this.isNotifyChanged)
                        this.Select(this.Selection.Index, this.Selection.Length);
                    break;
                case "CaretPostion":
                    if (!this.isNotifyChanged)
                        this.JumpCaret(this.CaretPostion.row, this.CaretPostion.col);
                    break;
                case "LineBreakMethod":
                    this.Document.LineBreak = this.LineBreakMethod;
                    break;
                case "LineBreakCharCount":
                    this.Document.LineBreakCharCount = this.LineBreakCharCount;
                    break;
                case "InsertMode":
                    this._View.InsertMode = this.InsertMode;
                    break;
                case "TabChars":
                    this.Document.TabStops = this.TabChars;
                    break;
                case "RectSelectMode":
                    this._Controller.RectSelection = this.RectSelectMode;
                    break;
                case "DrawCaret":
                    this._View.HideCaret = !this.DrawCaret;
                    break;
                case "DrawCaretLine":
                    this._View.HideLineMarker = !this.DrawCaretLine;
                    break;
                case "DrawLineNumber":
                    this.Document.DrawLineNumber = this.DrawLineNumber;
                    break;
                case "FontFamily":
                    this.Render.FontFamily = this.FontFamily;
                    break;
                case "FontSize":
                    this.Render.FontSize = this.FontSize;
                    break;
                case "FontStyle":
                    this.Render.FontStyle = this.FontStyle;
                    break;
                case "FontWeight":
                    this.Render.FontWeigth = this.FontWeight;
                    break;
                case "Foreground":
                    this.Render.Foreground = D2DRender.ToColor4(this.Foreground);
                    break;
                case "HilightForeground":
                    this.Render.HilightForeground = D2DRender.ToColor4(this.HilightForeground);
                    break;
                case "Background":
                    this.Render.Background = D2DRender.ToColor4(this.Background);
                    break;
                case "ControlChar":
                    this.Render.ControlChar =D2DRender.ToColor4( this.ControlChar);
                    break;
                case "Hilight":
                    this.Render.Hilight = D2DRender.ToColor4(this.Hilight);
                    break;
                case "Keyword1":
                    this.Render.Keyword1 = D2DRender.ToColor4(this.Keyword1);
                    break;
                case "Keyword2":
                    this.Render.Keyword2 = D2DRender.ToColor4(this.Keyword2);
                    break;
                case "Comment":
                    this.Render.Comment = D2DRender.ToColor4(this.Comment);
                    break;
                case "Literal":
                    this.Render.Literal = D2DRender.ToColor4(this.Literal);
                    break;
                case "URL":
                    this.Render.Url = D2DRender.ToColor4(this.URL);
                    break;
                case "InsertCaret":
                    this.Render.InsertCaret = D2DRender.ToColor4(this.InsertCaret);
                    break;
                case "OverwriteCaret":
                    this.Render.OverwriteCaret = D2DRender.ToColor4(this.OverwriteCaret);
                    break;
                case "Padding":
                    this._View.Padding = new Padding((int)this.Padding.Left, (int)this.Padding.Top, (int)this.Padding.Right, (int)this.Padding.Bottom);
                    break;
                case "LineMarker":
                    this.Render.LineMarker = D2DRender.ToColor4(this.LineMarker);
                    break;
                case "MarkURL":
                    this.Document.UrlMark = this.MarkURL;
                    break;
                case "ShowFullSpace":
                    this.Document.ShowFullSpace = this.ShowFullSpace;
                    break;
                case "ShowHalfSpace":
                    this.Document.ShowHalfSpace = this.ShowHalfSpace;
                    break;
                case "ShowTab":
                    this.Document.ShowTab = this.ShowTab;
                    break;
                case "ShowLineBreak":
                    this.Document.ShowLineBreak = this.ShowLineBreak;
                    break;
                case "FlowDirection":
                    this.Document.RightToLeft = this.FlowDirection == System.Windows.FlowDirection.RightToLeft;
                    this.horizontalScrollBar.FlowDirection = this.FlowDirection;
                    break;
                case "DrawRuler":
                    this.Document.HideRuler = !this.DrawRuler;
                    this._Controller.JumpCaret(this.Document.CaretPostion.row, this.Document.CaretPostion.col);
                    break;
                case "UpdateArea":
                    this.Render.UpdateArea = D2DRender.ToColor4(this.UpdateArea);
                    break;
                case "LineNumber":
                    this.Render.LineNumber = D2DRender.ToColor4(this.LineNumber);
                    break;
            }
            base.OnPropertyChanged(e);
        }
        #endregion
        #region property

        internal EditView View
        {
            get
            {
                return this._View;
            }
        }

        internal Controller Controller
        {
            get
            {
                return this._Controller;
            }
        }

        /// <summary>
        /// 文字列の描写に使用されるアンチエイリアシング モードを表します
        /// </summary>
        public TextAntialiasMode TextAntialiasMode
        {
            get { return (TextAntialiasMode)GetValue(TextAntialiasModeProperty); }
            set { SetValue(TextAntialiasModeProperty, value); }
        }

        /// <summary>
        /// TextAntialiasModeの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty TextAntialiasModeProperty =
            DependencyProperty.Register("TextAntialiasMode", typeof(TextAntialiasMode), typeof(FooTextBox), new PropertyMetadata(TextAntialiasMode.Default));

        /// <summary>
        /// シンタックスハイライターを表す
        /// </summary>
        public IHilighter Hilighter
        {
            get { return (IHilighter)GetValue(HilighterProperty); }
            set { SetValue(HilighterProperty, value); }
        }

        /// <summary>
        /// Hilighterの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty HilighterProperty =
            DependencyProperty.Register("Hilighter", typeof(IHilighter), typeof(FooTextBox), new PropertyMetadata(null));

        /// <summary>
        /// フォールティングを作成するインターフェイスを表す
        /// </summary>
        public IFoldingStrategy FoldingStrategy
        {
            get { return (IFoldingStrategy)GetValue(FoldingStrategyProperty); }
            set { SetValue(FoldingStrategyProperty, value); }
        }

        /// <summary>
        /// FoldingStrategyの依存プロパティ
        /// </summary>
        public static readonly DependencyProperty FoldingStrategyProperty =
            DependencyProperty.Register("FoldingStrategy", typeof(IFoldingStrategy), typeof(FooTextBox), new PropertyMetadata(null));


        /// <summary>
        /// マーカーパターンセット
        /// </summary>
        public MarkerPatternSet MarkerPatternSet
        {
            get
            {
                return this.Document.MarkerPatternSet;
            }
        }

        /// <summary>
        /// ドキュメント表す
        /// </summary>
        public Document Document
        {
            get { return (Document)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        /// <summary>
        /// ドキュメント添付プロパティ
        /// </summary>
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(Document), typeof(FooTextBox), new PropertyMetadata(null));


        /// <summary>
        /// レイアウト行を表す
        /// </summary>
        public LineToIndexTable LayoutLineCollection
        {
            get { return this._View.LayoutLines; }
        }

        /// <summary>
        /// 選択中の文字列を表す
        /// </summary>
        public string SelectedText
        {
            get { return (string)GetValue(SelectedTextProperty); }
            set { SetValue(SelectedTextProperty, value); }
        }

        /// <summary>
        /// SelectedTextの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.Register("SelectedText", typeof(string), typeof(FooTextBox), new PropertyMetadata(null));

        /// <summary>
        /// インデントの方法を表す
        /// </summary>
        public IndentMode IndentMode
        {
            get { return (IndentMode)GetValue(IndentModeProperty); }
            set { SetValue(IndentModeProperty, value); }
        }

        /// <summary>
        /// IndentModeの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty IndentModeProperty =
            DependencyProperty.Register("IndentMode", typeof(IndentMode), typeof(FooTextBox), new PropertyMetadata(IndentMode.Tab));

        /// <summary>
        /// 選択範囲を表す
        /// </summary>
        /// <remarks>
        /// Lengthが0の場合はキャレット位置を表します。
        /// 矩形選択モードの場合、選択範囲の文字数ではなく、開始位置から終了位置までの長さとなります
        /// </remarks>
        public TextRange Selection
        {
            get { return (TextRange)GetValue(SelectionProperty); }
            set { SetValue(SelectionProperty, value); }
        }

        /// <summary>
        /// Selectionの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty SelectionProperty =
            DependencyProperty.Register("Selection", typeof(TextRange), typeof(FooTextBox), new PropertyMetadata(TextRange.Null));

        /// <summary>
        /// 拡大率を表す
        /// </summary>
        public double MagnificationPower
        {
            get { return (double)GetValue(MagnificationPowerPropertyKey.DependencyProperty); }
        }

        /// <summary>
        /// 拡大率を表す依存プロパティ
        /// </summary>
        public static readonly DependencyPropertyKey MagnificationPowerPropertyKey =
            DependencyProperty.RegisterReadOnly("MagnificationPower", typeof(double), typeof(FooTextBox), new PropertyMetadata(1.0));

        /// <summary>
        /// レタリング方向を表す
        /// </summary>
        public new FlowDirection FlowDirection
        {
            get { return (FlowDirection)GetValue(FlowDirectionProperty); }
            set { SetValue(FlowDirectionProperty, value); }
        }

        /// <summary>
        /// レタリング方向を表す。これは依存プロパティです
        /// </summary>
        public new static readonly DependencyProperty FlowDirectionProperty =
            DependencyProperty.Register("FlowDirection", typeof(FlowDirection), typeof(FooTextBox), new PropertyMetadata(FlowDirection.LeftToRight));        

        /// <summary>
        /// キャレット位置を表す。これは依存プロパティです
        /// </summary>
        public TextPoint CaretPostion
        {
            get { return (TextPoint)GetValue(CaretPostionProperty); }
            set { SetValue(CaretPostionProperty, value); }
        }

        /// <summary>
        /// CaretPostionの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty CaretPostionProperty =
            DependencyProperty.Register("CaretPostion", typeof(TextPoint), typeof(FooTextBox), new PropertyMetadata(TextPoint.Null));
        
        /// <summary>
        /// デフォルトの文字色を表す。これは依存プロパティです
        /// </summary>
        public new System.Windows.Media.Color Foreground
        {
            get { return (System.Windows.Media.Color)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        /// <summary>
        /// Foregroundの依存プロパティを表す
        /// </summary>
        public new static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(System.Windows.Media.Color), typeof(FooTextBox), new FrameworkPropertyMetadata(SystemColors.WindowTextColor));

        /// <summary>
        /// 背景色を表す。これは依存プロパティです
        /// </summary>
        public new System.Windows.Media.Color Background
        {
            get { return (System.Windows.Media.Color)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        /// <summary>
        /// Backgroundの依存プロパティを表す
        /// </summary>
        public new static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(System.Windows.Media.Color), typeof(FooTextBox), new FrameworkPropertyMetadata(SystemColors.WindowColor));

        /// <summary>
        /// 選択時の文字色を表す。これは依存プロパティです
        /// </summary>
        public System.Windows.Media.Color HilightForeground
        {
            get { return (System.Windows.Media.Color)GetValue(HilightForegroundProperty); }
            set { SetValue(HilightForegroundProperty, value); }
        }

        /// <summary>
        /// ControlCharの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty HilightForegroundProperty =
            DependencyProperty.Register("HilightForeground", typeof(System.Windows.Media.Color), typeof(FooTextBox), new FrameworkPropertyMetadata(Colors.White));

        /// <summary>
        /// コントロールコードの文字色を表す。これは依存プロパティです
        /// </summary>
        public System.Windows.Media.Color ControlChar
        {
            get { return (System.Windows.Media.Color)GetValue(ControlCharProperty); }
            set { SetValue(ControlCharProperty, value); }
        }

        /// <summary>
        /// ControlCharの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty ControlCharProperty =
            DependencyProperty.Register("ControlChar", typeof(System.Windows.Media.Color), typeof(FooTextBox), new FrameworkPropertyMetadata(Colors.Gray));
        
        /// <summary>
        /// 選択時の背景色を表す。これは依存プロパティです
        /// </summary>
        public System.Windows.Media.Color Hilight
        {
            get { return (System.Windows.Media.Color)GetValue(HilightProperty); }
            set { SetValue(HilightProperty, value); }
        }

        /// <summary>
        /// Hilightの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty HilightProperty =
            DependencyProperty.Register("Hilight", typeof(System.Windows.Media.Color), typeof(FooTextBox), new FrameworkPropertyMetadata(Colors.DeepSkyBlue));
        
        /// <summary>
        /// キーワード１の文字色を表す。これは依存プロパティです
        /// </summary>
        public System.Windows.Media.Color Keyword1
        {
            get { return (System.Windows.Media.Color)GetValue(Keyword1Property); }
            set { SetValue(Keyword1Property, value); }
        }

        /// <summary>
        /// Keyword1の依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty Keyword1Property =
            DependencyProperty.Register("Keyword1", typeof(System.Windows.Media.Color), typeof(FooTextBox), new FrameworkPropertyMetadata(Colors.Blue));

        /// <summary>
        /// キーワード2の文字色を表す。これは依存プロパティです
        /// </summary>
        public System.Windows.Media.Color Keyword2
        {
            get { return (System.Windows.Media.Color)GetValue(Keyword2Property); }
            set { SetValue(Keyword2Property, value); }
        }

        /// <summary>
        /// Keyword2の依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty Keyword2Property =
            DependencyProperty.Register("Keyword2", typeof(System.Windows.Media.Color), typeof(FooTextBox), new FrameworkPropertyMetadata(Colors.DarkCyan));

        /// <summary>
        /// コメントの文字色を表す。これは依存プロパティです
        /// </summary>
        public System.Windows.Media.Color Comment
        {
            get { return (System.Windows.Media.Color)GetValue(CommentProperty); }
            set { SetValue(CommentProperty, value); }
        }

        /// <summary>
        /// Commentの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty CommentProperty =
            DependencyProperty.Register("Comment", typeof(System.Windows.Media.Color), typeof(FooTextBox), new FrameworkPropertyMetadata(Colors.Green));

        /// <summary>
        /// 文字リテラルの文字色を表す。これは依存プロパティです
        /// </summary>
        public System.Windows.Media.Color Literal
        {
            get { return (System.Windows.Media.Color)GetValue(LiteralProperty); }
            set { SetValue(LiteralProperty, value); }
        }

        /// <summary>
        /// Literalの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty LiteralProperty =
            DependencyProperty.Register("Literal", typeof(System.Windows.Media.Color), typeof(FooTextBox), new FrameworkPropertyMetadata(Colors.Brown));

        /// <summary>
        /// URLの文字色を表す。これは依存プロパティです
        /// </summary>
        public System.Windows.Media.Color URL
        {
            get { return (System.Windows.Media.Color)GetValue(URLProperty); }
            set { SetValue(URLProperty, value); }
        }

        /// <summary>
        /// URLの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty URLProperty =
            DependencyProperty.Register("URL", typeof(System.Windows.Media.Color), typeof(FooTextBox), new FrameworkPropertyMetadata(Colors.Blue));


        /// <summary>
        /// ラインマーカーの色を表す
        /// </summary>
        public System.Windows.Media.Color LineMarker
        {
            get { return (System.Windows.Media.Color)GetValue(LineMarkerProperty); }
            set { SetValue(LineMarkerProperty, value); }
        }

        /// <summary>
        /// LineMarkerの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty LineMarkerProperty =
            DependencyProperty.Register("LineMarker", typeof(System.Windows.Media.Color), typeof(FooTextBox), new FrameworkPropertyMetadata(Colors.Silver));

        /// <summary>
        /// 挿入モード時のキャレットの色を表す
        /// </summary>
        public System.Windows.Media.Color InsertCaret
        {
            get { return (System.Windows.Media.Color)GetValue(InsertCaretProperty); }
            set { SetValue(InsertCaretProperty, value); }
        }

        /// <summary>
        /// InsertCaretの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty InsertCaretProperty =
            DependencyProperty.Register("InsertCaret", typeof(System.Windows.Media.Color), typeof(FooTextBox), new FrameworkPropertyMetadata(SystemColors.WindowTextColor));

        /// <summary>
        /// 行更新フラグの色を表す
        /// </summary>
        public System.Windows.Media.Color UpdateArea
        {
            get { return (System.Windows.Media.Color)GetValue(UpdateAreaProperty); }
            set { SetValue(UpdateAreaProperty, value); }
        }

        /// <summary>
        /// UpdateAreaの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty UpdateAreaProperty =
            DependencyProperty.Register("UpdateArea", typeof(System.Windows.Media.Color), typeof(FooTextBox), new PropertyMetadata(Colors.MediumSeaGreen));        

        /// <summary>
        /// 上書きモード時のキャレット職を表す
        /// </summary>
        public System.Windows.Media.Color OverwriteCaret
        {
            get { return (System.Windows.Media.Color)GetValue(OverwriteCaretProperty); }
            set { SetValue(OverwriteCaretProperty, value); }
        }
        
        /// <summary>
        /// OverwriteCaretの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty OverwriteCaretProperty =
            DependencyProperty.Register("OverwriteCaret", typeof(System.Windows.Media.Color), typeof(FooTextBox), new FrameworkPropertyMetadata(SystemColors.WindowTextColor));

        /// <summary>
        /// 行番号の色を表す
        /// </summary>
        public System.Windows.Media.Color LineNumber
        {
            get { return (System.Windows.Media.Color)GetValue(LineNumberProperty); }
            set { SetValue(LineNumberProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for LineNumber.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty LineNumberProperty =
            DependencyProperty.Register("LineNumber", typeof(System.Windows.Media.Color), typeof(FooTextBox), new PropertyMetadata(Colors.DimGray));

        /// <summary>
        /// 挿入モードなら真を返し、そうでないなら、偽を返す。これは依存プロパティです
        /// </summary>
        public bool InsertMode
        {
            get { return (bool)GetValue(InsertModeProperty); }
            set { SetValue(InsertModeProperty, value); }
        }

        /// <summary>
        /// InsertModeの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty InsertModeProperty =
            DependencyProperty.Register("InsertMode",
            typeof(bool),
            typeof(FooTextBox),
            new FrameworkPropertyMetadata(true));

        /// <summary>
        /// タブの文字数を表す。これは依存プロパティです
        /// </summary>
        public int TabChars
        {
            get { return (int)GetValue(TabCharsProperty); }
            set { SetValue(TabCharsProperty, value); }
        }

        /// <summary>
        /// TabCharsの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty TabCharsProperty =
            DependencyProperty.Register("TabChars",
            typeof(int),
            typeof(FooTextBox),
            new FrameworkPropertyMetadata(4));

        /// <summary>
        /// 矩形選択モードなら真を返し、そうでないなら偽を返す。これは依存プロパティです
        /// </summary>
        public bool RectSelectMode
        {
            get { return (bool)GetValue(RectSelectModeProperty); }
            set { SetValue(RectSelectModeProperty, value); }
        }

        /// <summary>
        /// RectSelectModeの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty RectSelectModeProperty =
            DependencyProperty.Register("RectSelectMode", typeof(bool), typeof(FooTextBox), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// 折り返しの方法を指定する
        /// </summary>
        /// <remarks>
        /// 変更した場合、レイアウトの再構築を行う必要があります
        /// </remarks>
        public LineBreakMethod LineBreakMethod
        {
            get { return (LineBreakMethod)GetValue(LineBreakProperty); }
            set { SetValue(LineBreakProperty, value); }
        }

        /// <summary>
        /// LineBreakMethodの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty LineBreakProperty =
            DependencyProperty.Register("LineBreakMethod", typeof(LineBreakMethod), typeof(FooTextBox), new PropertyMetadata(LineBreakMethod.None));


        /// <summary>
        /// 折り返しの幅を指定する。LineBreakMethod.CharUnit以外の時は無視されます
        /// </summary>
        /// <remarks>
        /// 変更した場合、レイアウトの再構築を行う必要があります
        /// </remarks>
        public int LineBreakCharCount
        {
            get { return (int)GetValue(LineBreakCharCountProperty); }
            set { SetValue(LineBreakCharCountProperty, value); }
        }

        /// <summary>
        /// LineBreakCharCountの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty LineBreakCharCountProperty =
            DependencyProperty.Register("LineBreakCharCount", typeof(int), typeof(FooTextBox), new PropertyMetadata(80));

        /// <summary>
        /// キャレットを描くなら真。そうでないなら偽を返す。これは依存プロパティです
        /// </summary>
        public bool DrawCaret
        {
            get { return (bool)GetValue(DrawCaretProperty); }
            set { SetValue(DrawCaretProperty, value); }
        }

        /// <summary>
        /// DrawCaretの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty DrawCaretProperty =
            DependencyProperty.Register("DrawCaret", typeof(bool), typeof(FooTextBox), new FrameworkPropertyMetadata(true));

        
        /// <summary>
        /// キャレットラインを描くなら真。そうでないなら偽を返す。これは依存プロパティです
        /// </summary>
        public bool DrawCaretLine
        {
            get { return (bool)GetValue(DrawCaretLineProperty); }
            set { SetValue(DrawCaretLineProperty, value); }
        }

        /// <summary>
        /// DrawCaretLineの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty DrawCaretLineProperty =
            DependencyProperty.Register("DrawCaretLine", typeof(bool), typeof(FooTextBox), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// 行番号を描くなら真。そうでなければ偽。これは依存プロパティです
        /// </summary>
        public bool DrawLineNumber
        {
            get { return (bool)GetValue(DrawLineNumberProperty); }
            set { SetValue(DrawLineNumberProperty, value); }
        }

        /// <summary>
        /// ルーラーを描くなら真。そうでなければ偽。これは依存プロパティです
        /// </summary>
        public bool DrawRuler
        {
            get { return (bool)GetValue(DrawRulerProperty); }
            set { SetValue(DrawRulerProperty, value); }
        }

        /// <summary>
        /// DrawRulerの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty DrawRulerProperty =
            DependencyProperty.Register("DrawRuler", typeof(bool), typeof(FooTextBox), new PropertyMetadata(false));

        
        /// <summary>
        /// DrawLineNumberの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty DrawLineNumberProperty =
            DependencyProperty.Register("DrawLineNumber", typeof(bool), typeof(FooTextBox), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// URLに下線を引くなら真。そうでないなら偽を表す。これは依存プロパティです
        /// </summary>
        public bool MarkURL
        {
            get { return (bool)GetValue(MarkURLProperty); }
            set { SetValue(MarkURLProperty, value); }
        }

        /// <summary>
        /// MarkURLの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty MarkURLProperty =
            DependencyProperty.Register("MarkURL", typeof(bool), typeof(FooTextBox), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// 全角スペースを表示するなら真。そうでないなら偽
        /// </summary>
        public bool ShowFullSpace
        {
            get { return (bool)GetValue(ShowFullSpaceProperty); }
            set { SetValue(ShowFullSpaceProperty, value); }
        }

        /// <summary>
        /// ShowFullSpaceの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty ShowFullSpaceProperty =
            DependencyProperty.Register("ShowFullSpace", typeof(bool), typeof(FooTextBox), new UIPropertyMetadata(false));

        /// <summary>
        /// 半角スペースを表示するなら真。そうでないなら偽
        /// </summary>
        public bool ShowHalfSpace
        {
            get { return (bool)GetValue(ShowHalfSpaceProperty); }
            set { SetValue(ShowHalfSpaceProperty, value); }
        }

        /// <summary>
        /// ShowHalfSpaceの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty ShowHalfSpaceProperty =
            DependencyProperty.Register("ShowHalfSpace", typeof(bool), typeof(FooTextBox), new UIPropertyMetadata(false));

        /// <summary>
        /// タブを表示するなら真。そうでないなら偽
        /// </summary>
        public bool ShowTab
        {
            get { return (bool)GetValue(ShowTabProperty); }
            set { SetValue(ShowTabProperty, value); }
        }

        /// <summary>
        /// ShowTabの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty ShowTabProperty =
            DependencyProperty.Register("ShowTab", typeof(bool), typeof(FooTextBox), new UIPropertyMetadata(false));

        /// <summary>
        /// 改行マークを表示するなら真。そうでないなら偽
        /// </summary>
        public bool ShowLineBreak
        {
            get { return (bool)GetValue(ShowLineBreakProperty); }
            set { SetValue(ShowLineBreakProperty, value); }
        }

        /// <summary>
        /// ShowLineBreakの依存プロパティを表す
        /// </summary>
        public static readonly DependencyProperty ShowLineBreakProperty =
            DependencyProperty.Register("ShowLineBreak", typeof(bool), typeof(FooTextBox), new PropertyMetadata(false));
        
        #endregion
    }
    /// <summary>
    /// マウスボタン関連のイベントクラス
    /// </summary>
    public sealed class FooMouseButtonEventArgs : MouseButtonEventArgs
    {
        /// <summary>
        /// イベントが発生したドキュメントのインデックス
        /// </summary>
        public int Index
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="mouse">マウスデバイス</param>
        /// <param name="timestamp">タイムスタンプ</param>
        /// <param name="button">ボタン</param>
        /// <param name="stylusDevice">スタイラスデバイス</param>
        /// <param name="index">インデックス</param>
        public FooMouseButtonEventArgs(MouseDevice mouse, int timestamp, MouseButton button, StylusDevice stylusDevice, int index)
            : base(mouse, timestamp, button, stylusDevice)
        {
            this.Index = index;
        }
    }
    /// <summary>
    /// マウス関連のイベントクラス
    /// </summary>
    public sealed class FooMouseEventArgs : MouseEventArgs
    {
        /// <summary>
        /// イベントが発生したドキュメントのインデックス
        /// </summary>
        public int Index
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="mouse">マウスデバイス</param>
        /// <param name="timestamp">タイムスタンプ</param>
        /// <param name="stylusDevice">スタイラスデバイス</param>
        /// <param name="index">インデックス</param>
        public FooMouseEventArgs(MouseDevice mouse,
            int timestamp,
            StylusDevice stylusDevice,
            int index)
            : base(mouse, timestamp, stylusDevice)
        {
            this.Index = index;
        }
    }
}
