﻿using System;
using System.Linq;

namespace FooEditEngine
{
    class FoldingGenerator : ILineInfoGenerator
    {
        const int HUGE_FILE_LENGTH = 1024 * 1024 * 10;

        public FoldingCollection FoldingCollection = new FoldingCollection();
        const long AllowCallTicks = 1000 * 10000;   //see.DateTime.Ticks プロパティ
        long lastUpdateTicks = DateTime.Now.Ticks;
        IFoldingStrategy _folding;
        bool _IsSync = true;

        public IFoldingStrategy FoldingStrategy
        {
            get
            {
                return this._folding;
            }
            set
            {
                this._folding = value;
                if (value == null)
                    this.FoldingCollection.Clear();
            }
        }

        public void Clear(LineToIndexTable lti)
        {
            this.FoldingCollection.Clear();
            this._IsSync = false;
        }

        public bool Generate(Document doc,LineToIndexTable lti, bool force = true)
        {
            if (doc.Length == 0)
                return false;
            long nowTick = DateTime.Now.Ticks;
            bool sync = force || !this._IsSync;
            //巨大ファイルは解析に時間がかかるので指示がない限りは実行しない
            if (force == false && doc.Length > HUGE_FILE_LENGTH)
                return false;
            if (sync && Math.Abs(nowTick - this.lastUpdateTicks) >= AllowCallTicks)
            {
                this.GenerateFolding(doc, lti, 0, doc.Length - 1);
                this.lastUpdateTicks = nowTick;
                this._IsSync = true;
                return true;
            }
            return false;
        }

        void GenerateFolding(Document doc, LineToIndexTable lti, long start, long end)
        {
            if (start > end)
                throw new ArgumentException("start <= endである必要があります");
            if (this.FoldingStrategy != null)
            {
                //再生成するとすべて展開状態になってしまうので、閉じてるやつだけを保存しておく
                FoldingItem[] closed_items = this.FoldingCollection.Where((e) => { return !e.Expand; }).ToArray();

                this.FoldingCollection.Clear();

                var items = this.FoldingStrategy.AnalyzeDocument(doc, start, end)
                    .Select((item) => item);
                this.FoldingCollection.AddRange(items);

                this.FoldingCollection.ApplyExpandStatus(closed_items);
            }
        }

        public void Update(Document doc, long startIndex, long insertLength, long removeLength)
        {
            this.FoldingCollection.UpdateData(doc, startIndex, insertLength, removeLength);
            this._IsSync = false;
            this.lastUpdateTicks = DateTime.Now.Ticks;
        }
    }
}
