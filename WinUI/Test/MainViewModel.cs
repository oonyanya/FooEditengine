//#define USE_DISK_FOR_DOCUMENT
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FooEditEngine;
using FooEditEngine.WinUI;
using Windows.Storage;

namespace Test
{
    class TestHilighter : IHilighter
    {
        public int DoHilight(string text, int length, TokenSpilitHandeler action)
        {
            string target = "test";
            int index = text.IndexOf(target);
            if (index != -1)
                action(new TokenSpilitEventArgs(index, target.Length, TokenType.Keyword1));
            return 0;
        }

        public void Reset()
        {
        }
    }

    class MainViewModel : INotifyPropertyChanged
    {
        ObservableCollection<Document> _list;
        const int DOCUMENT_CACHE_SIZE = 16;

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
                if (this.CurrentDocumentChanged != null)
                    this.CurrentDocumentChanged(this, null);
            }
        }

        double _FontSize = 12;
        public double FontSize
        {
            get { return this._FontSize; }
            set
            {
                this._FontSize = value;
                this.OnPropertyChanged(this);
            }
        }

        int _GoToRow;
        public int GoToRow
        {
            get { return _GoToRow; }
            set
            {
                this._GoToRow = value;
                this.OnPropertyChanged(this);
                JumpToRow(value);
            }
        }

        void JumpToRow(int row)
        {
            if (row > this.CurrentDocument.TotalLineCount)
                return;
            this.CurrentDocument.LayoutLines.FetchLine(row);
            this.CurrentDocument.CaretPostion = new TextPoint(row, 0);
            this.CurrentDocument.RequestRedraw();
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

#if USE_DISK_FOR_DOCUMENT
            var doc = new Document(DOCUMENT_CACHE_SIZE) { Title = "test1" };
#else
            var doc = new Document(){ Title = "test1" };
#endif
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

#if USE_DISK_FOR_DOCUMENT
            doc = new Document(DOCUMENT_CACHE_SIZE) { Title = "test2" };
#else
            doc = new Document() { Title = "test2" };
#endif
            _list.Add(doc);

            this.CurrentDocument = _list[0];
        }

        public void AddDocument()
        {
#if USE_DISK_FOR_DOCUMENT
            var doc = new Document(DOCUMENT_CACHE_SIZE) { Title = "test" + _list.Count };
#else
            var doc = new Document() { Title = "test" + _list.Count };
#endif
            _list.Add(doc);
            this.CurrentDocument = _list.Last();
        }

        public void RemoveDocument(Document doc = null)
        {
            if(_list.Count > 1)
                _list.Remove(doc != null ? doc : this.CurrentDocument);
            this.CurrentDocument = _list[0];
        }

        public async Task AddDocumentFromFile(IStorageFile file)
        {
            if (file != null)
            {
#if USE_DISK_FOR_DOCUMENT
                var doc = new Document(DOCUMENT_CACHE_SIZE) { Title = "test" + _list.Count };
#else
                var doc = new Document() { Title = "test" + _list.Count };
#endif
                doc.ShowLineBreak = true;
                doc.ShowFullSpace = true;
                doc.ShowTab = true;
                using (var ws = await file.OpenAsync(FileAccessMode.Read))
                using (var fs = new StreamReader(ws.AsStream()))
                {
                    var prop = await file.GetBasicPropertiesAsync();
                    await doc.LoadAsync(fs, null,(int)prop.Size);
                }
                doc.RequestRedraw();
                _list.Add(doc);
                this.CurrentDocument = _list.Last();
                System.Diagnostics.Debug.WriteLine("Loaded TotalLine:" + this.CurrentDocument.TotalLineCount);
            }
        }

        private void OnPropertyChanged(object sender, [System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if(this.PropertyChanged != null)
                this.PropertyChanged(sender, new PropertyChangedEventArgs(name));
        }
    }
}
