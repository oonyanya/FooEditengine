using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FooEditEngine;
using FooEditEngine.UWP;
using Windows.Storage;

namespace Test
{
    class TestHilighter : HilighterLegacyBase
    {
        public override int DoHilight(string text, int length, TokenSpilitHandeler action)
        {
            string target = "test";
            int index = text.IndexOf(target);
            if (index != -1)
                action(new TokenSpilitEventArgs(index, target.Length, TokenType.Keyword1));
            return 0;
        }

        public override void Reset()
        {
        }
    }

    class MainViewModel : INotifyPropertyChanged
    {
        ObservableCollection<Document> _list;

        public MainViewModel()
        {
        }

        public ObservableCollection<Document> DocumentList
        {
            get
            {
                if(_list == null)
                {
                    _list = new ObservableCollection<Document>();
                }
                return this._list;
            }
        }

        Document _currentDocument;
        public Document CurrentDocument
        {
            get
            {
                return this._currentDocument;
            }
            set
            {
                this._currentDocument = value;
                this.OnPropertyChanged(this);
                if(this.CurrentDocumentChanged != null)
                    this.CurrentDocumentChanged(this, null);
            }
        }

        public event EventHandler CurrentDocumentChanged;
        
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initalize()
        {
            var complete_collection = new CompleteCollection<ICompleteItem>();
            CompleteHelper.AddComleteWord(complete_collection, "int");
            CompleteHelper.AddComleteWord(complete_collection, "float");
            CompleteHelper.AddComleteWord(complete_collection, "double");
            CompleteHelper.AddComleteWord(complete_collection, "char");
            CompleteHelper.AddComleteWord(complete_collection, "byte");
            CompleteHelper.AddComleteWord(complete_collection, "var");
            CompleteHelper.AddComleteWord(complete_collection, "short");

            var doc = new Document() { Title = "test1" };
            doc.AutoComplete = new AutoCompleteBox(doc);
            doc.AutoComplete.Items = complete_collection;
            doc.AutoComplete.Enabled = true;
            doc.ShowLineBreak = true;
            doc.ShowHalfSpace = true;
            doc.ShowFullSpace = true;
            doc.ShowTab = true;
            doc.DrawLineNumber = true;
            doc.HideRuler = false;
            doc.HideLineMarker = false;
            doc.LayoutLines.Hilighter = new TestHilighter();
            _list.Add(doc);

            doc = new Document() { Title = "test2" };
            _list.Add(doc);

            this.CurrentDocument = _list[0];
        }

        public void AddDocument()
        {
            var doc = new Document() { Title = "test" + _list.Count };
            _list.Add(doc);
            this.CurrentDocument = _list.Last();
        }

        public async Task AddDocumentFromFile(IStorageFile file)
        {
            if (file != null)
            {
                var doc = new Document() { Title = "test" + _list.Count };
                doc.ShowLineBreak = true;
                doc.ShowFullSpace = true;
                doc.ShowTab = true;
                using (var ws = await file.OpenAsync(FileAccessMode.Read))
                using (var fs = new StreamReader(ws.AsStream()))
                {
                    var prop = await file.GetBasicPropertiesAsync();
                    await doc.LoadAsync(fs.BaseStream, fs.CurrentEncoding);
                }
                doc.RequestRedraw();
                _list.Add(doc);
                this.CurrentDocument = _list.Last();
            }
        }

        private void OnPropertyChanged(object sender, [System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if(this.PropertyChanged != null)
                this.PropertyChanged(sender, new PropertyChangedEventArgs(name));
        }
    }
}
