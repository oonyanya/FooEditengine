/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#if METRO || WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Automation.Text;
#if METRO
using FooEditEngine.Metro;
#else
using FooEditEngine.UWP;
#endif
#endif
#if WPF
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using FooEditEngine;
using FooEditEngine.WPF;
#endif
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Automation.Text;
using FooEditEngine.WinUI;
#endif

namespace FooEditEngine
{
#if ENABLE_AUTMATION
    /// <summary>
    /// Automation Peer class for CustomInputBox2.  
    /// 
    /// Note: The difference between this and CustomControl1AutomationPeer is that this one implements
    /// Text Pattern (ITextProvider) and Value Pattern (IValuePattern) interfaces.  So Touch keyboard shows 
    /// automatically when user taps on the control with Touch or Pen.
    /// </summary>
#if METRO || WINDOWS_UWP || WINUI
    sealed class FooTextBoxAutomationPeer : FrameworkElementAutomationPeer, ITextProvider, ITextProvider2, IValueProvider
#elif WPF
    sealed class FooTextBoxAutomationPeer : FrameworkElementAutomationPeer, ITextProvider, IValueProvider
#endif
    {
        private FooTextBox fooTextBox;
        private string accClass = "FooTextBox";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        public FooTextBoxAutomationPeer(FooTextBox owner)
            : base(owner)
        {
            this.fooTextBox = owner;
        }

        public void OnNotifyTextChanged()
        {
            this.RaiseAutomationEvent(AutomationEvents.TextPatternOnTextChanged);
        }

        public void OnNotifyCaretChanged()
        {
            this.RaiseAutomationEvent(AutomationEvents.TextPatternOnTextSelectionChanged);
        }

#if METRO || WINDOWS_UWP || WINUI
        /// <summary>
        /// Override GetPatternCore to return the object that supports the specified pattern.  In this case the Value pattern, Text
        /// patter and any base class patterns.
        /// </summary>
        /// <param name="patternInterface"></param>
        /// <returns>the object that supports the specified pattern</returns>
        protected override object GetPatternCore(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
            {
                return this;
            }
            else if (patternInterface == PatternInterface.Text)
            {
                return this;
            }
            else if (patternInterface == PatternInterface.Text2)
            {
                return this;
            }
            return base.GetPatternCore(patternInterface);
        }
#elif WPF
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
            {
                return this;
            }
            else if (patternInterface == PatternInterface.Text)
            {
                return this;
            }
            return base.GetPattern(patternInterface);
        }
#endif

        protected override string GetAutomationIdCore()
        {
            return this.accClass;
        }

        /// <summary>
        /// Override GetClassNameCore and set the name of the class that defines the type associated with this control.
        /// </summary>
        /// <returns>The name of the control class</returns>
        protected override string GetClassNameCore()
        {
            return this.accClass;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Edit;
        }

        protected override bool IsContentElementCore()
        {
            return true;
        }

        protected override bool IsKeyboardFocusableCore()
        {
            return true;
        }

#if METRO || WINDOWS_UWP || WINUI
        protected override Windows.Foundation.Rect GetBoundingRectangleCore()
        {
            double scale = Util.GetScale();
            Point left = Util.GetPointInWindow(new Point(0, 0),this.fooTextBox).Scale(scale);
            Point bottom = Util.GetPointInWindow(new Point(this.fooTextBox.ActualWidth, this.fooTextBox.ActualHeight),this.fooTextBox).Scale(scale);
#elif WPF
        protected override System.Windows.Rect GetBoundingRectangleCore()
        {
            Point left = Util.GetScreentPoint(new Point(0, 0), this.fooTextBox);
            Point bottom = Util.GetScreentPoint(new Point(this.fooTextBox.ActualWidth, this.fooTextBox.ActualHeight), this.fooTextBox);
#endif
            return new Rectangle(left,bottom);
        }

        #region Implementation for ITextPattern and ITextPattern2 interface
        // Complete implementation of the ITextPattern is beyond the scope of this sample.  The implementation provided
        // is specific to this sample's custom control, so it is unlikely that they are directly transferable to other 
        // custom control.

        public ITextRangeProvider DocumentRange
        {
            // A real implementation of this method is beyond the scope of this sample.
            // If your custom control has complex text involving both readonly and non-readonly ranges, 
            // it will need a smarter implementation than just returning a fixed range
            get
            {
                return new FooTextBoxRangeProvider(this.fooTextBox, this);
            }
        }

        public ITextRangeProvider[] GetSelection()
        {
            ITextRangeProvider[] ret = new ITextRangeProvider[1];
            int selStart = this.fooTextBox.Selection.Index;
            int selLength = this.fooTextBox.Selection.Length;
            ret[0] = new FooTextBoxRangeProvider(this.fooTextBox, selStart, selLength, this);
            return ret;
        }

        public ITextRangeProvider[] GetVisibleRanges()
        {
            ITextRangeProvider[] ret = new ITextRangeProvider[1];
            if (this.fooTextBox.LayoutLineCollection.Count == 0)
            {
                ret[0] = new FooTextBoxRangeProvider(this.fooTextBox, 0, 0, this);
            }
            else
            {
                EditView view = this.fooTextBox.View;
                
                int startIndex = view.GetIndexFromLayoutLine(new TextPoint(view.Src.Row,0));
                int endIndex = view.GetIndexFromLayoutLine(new TextPoint(view.Src.Row + view.LineCountOnScreen, 0));
                ret[0] = new FooTextBoxRangeProvider(this.fooTextBox, startIndex, endIndex - startIndex, this);
            }
            return ret;
        }

        public ITextRangeProvider RangeFromChild(IRawElementProviderSimple childElement)
        {
            return new FooTextBoxRangeProvider(this.fooTextBox,0,0, this);
        }

#if METRO || WINDOWS_UWP || WINUI
        public ITextRangeProvider RangeFromPoint(Windows.Foundation.Point screenLocation)
#elif WPF
        public ITextRangeProvider RangeFromPoint(System.Windows.Point screenLocation)
#endif
        {
            Point pt = Util.GetClientPoint(screenLocation, fooTextBox);
            EditView view = this.fooTextBox.View;

            TextPoint tp = view.GetTextPointFromPostion(pt);
            if (tp == TextPoint.Null)
                tp = new TextPoint(view.Src.Row,0);
            int index = view.GetIndexFromLayoutLine(tp);
            int length = 1;
            if (index == this.fooTextBox.Document.Length)
                length = 0;

            return new FooTextBoxRangeProvider(this.fooTextBox, index, length, this);
        }

        public SupportedTextSelection SupportedTextSelection
        {
            get { return SupportedTextSelection.Single; }
        }

        public ITextRangeProvider RangeFromAnnotation(IRawElementProviderSimple annotationElement)
        {
            throw new NotImplementedException();
        }

        public ITextRangeProvider GetCaretRange(out bool isActive)
        {

            EditView view = this.fooTextBox.View;
            Document doc = this.fooTextBox.Document;
            isActive = true;
            int index = view.GetIndexFromLayoutLine(doc.CaretPostion);
            int length = 1;
            if (index == this.fooTextBox.Document.Length)
                length = 0;

            return new FooTextBoxRangeProvider(this.fooTextBox, index, length, this);
        }
        #endregion

        #region Implementation for IValueProvider interface
        // Complete implementation of the IValueProvider is beyond the scope of this sample.  The implementation provided
        // is specific to this sample's custom control, so it is unlikely that they are directly transferable to other 
        // custom control.

        /// <summary>
        /// The value needs to be false for the Touch keyboard to be launched automatically because Touch keyboard
        /// does not appear when the input focus is in a readonly UI control.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        public void SetValue(string value)
        {
            string oldText = this.fooTextBox.Document.ToString(0);
            this.fooTextBox.Document.Replace(0,this.fooTextBox.Document.Length,value);
            this.RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldText, this.fooTextBox.Document.ToString(0));
        }

        public string Value
        {
            get
            {
                return this.fooTextBox.Document.ToString(0,this.fooTextBox.Document.Length);
            }
        }

    #endregion //Implementation for IValueProvider interface

        public IRawElementProviderSimple GetRawElementProviderSimple()
        {
            return ProviderFromPeer(this);
        }

    }

#endif
}
