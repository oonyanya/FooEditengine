using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FooEditEngine
{
    /// <summary>
    /// 補完候補を表すインターフェイス
    /// </summary>
    public interface ICompleteItem : INotifyPropertyChanged
    {
        /// <summary>
        /// 補完対象の単語を表す
        /// </summary>
        string word { get; }
    }

    /// <summary>
    /// 補完候補
    /// </summary>
    public class CompleteWord : ICompleteItem
    {
        private string _word;
        /// <summary>
        /// コンストラクター
        /// </summary>
        public CompleteWord(string w)
        {
            this._word = w;
            this.PropertyChanged += new PropertyChangedEventHandler((s,e)=>{});
        }

        /// <summary>
        /// 補完候補となる単語を表す
        /// </summary>
        public string word
        {
            get { return this._word; }
            set { this._word = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// プロパティが変更されたことを通知する
        /// </summary>
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// プロパティが変更されたことを通知する
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// 補完候補リスト
    /// </summary>
    /// <typeparam name="T">ICompleteItemを継承したクラス</typeparam>
    public sealed class CompleteCollection<T> : ObservableCollection<T> where T : ICompleteItem
    {
        internal const string ShowMember = "word";

        /// <summary>
        /// 補完対象の単語を表す
        /// </summary>
        public CompleteCollection()
        {
            this.LongestItem = default(T);
        }

        /// <summary>
        /// 最も長い単語を表す
        /// </summary>
        public T LongestItem
        {
            get;
            private set;
        }

        /// <summary>
        /// 補完候補を追加する
        /// </summary>
        /// <param name="collection"></param>
        public void AddRange(IEnumerable<T> collection)
        {
            foreach (T s in collection)
                this.Add(s);
        }

        /// <summary>
        /// 補完候補を追加する
        /// </summary>
        /// <param name="s"></param>
        public new void Add(T s)
        {
            if (this.LongestItem == null)
                this.LongestItem = s;
            if (s.word.Length > this.LongestItem.word.Length)
                this.LongestItem = s;
            base.Add(s);
        }

        /// <summary>
        /// 補完候補を挿入する
        /// </summary>
        /// <param name="index"></param>
        /// <param name="s"></param>
        public new void Insert(int index, T s)
        {
            if (this.LongestItem == null)
                this.LongestItem = s;
            if (s.word.Length > this.LongestItem.word.Length)
                this.LongestItem = s;
            base.Insert(index, s);
        }

        /// <summary>
        /// 補完候補をすべて削除する
        /// </summary>
        public new void Clear()
        {
            this.LongestItem = default(T);
            base.Clear();
        }
    }
}
