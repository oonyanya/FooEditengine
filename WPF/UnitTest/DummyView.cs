using FooEditEngine;

namespace UnitTest
{
    class DummyView : ViewBase
    {
        public DummyView(Document doc, IEditorRender render)
            : base(doc,render,new Padding(0,0,0,0))
        {
        }

        protected override void CalculateClipRect()
        {
            this.render.TextArea = this.PageBound;
        }

        public override void CalculateLineCountOnScreen()
        {
            this.LineCountOnScreen = (int)(this.PageBound.Height / DummyTextLayout.TestLineHeight);
        }
    }
}
