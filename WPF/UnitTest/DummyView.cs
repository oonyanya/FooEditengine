using FooEditEngine;

namespace UnitTest
{
    class DummyView : ViewBase
    {
        public DummyView(Document doc, IEditorRender render)
            : base(doc,render,new Padding(0,0,0,0))
        {
        }
    }
}
