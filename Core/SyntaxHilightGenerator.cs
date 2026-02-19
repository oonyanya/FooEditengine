using System;
using System.Collections.Generic;
using System.Text;

namespace FooEditEngine
{
    class SyntaxHilightGenerator : ILineInfoGenerator
    {
        const int HUGE_FILE_LENGTH = 1024 * 1024 * 10;

        const long AllowCallTicks = 1000 * 10000;   //see.DateTime.Ticks プロパティ
        long lastUpdateTicks = DateTime.Now.Ticks;
        bool _IsSync = true;

        /// <summary>
        /// シンタックスハイライター
        /// </summary>
        internal IHilighter Hilighter { get; set; }

        public void Clear(LineToIndexTable lti)
        {
            lti.ClearLayoutCache();
            this._IsSync = false;
        }

        public bool Generate(Document doc, LineToIndexTable lti, bool force = true)
        {
            if (this.Hilighter == null)
                return false;

            //巨大ファイルは解析に時間がかかるので指示がない限りは実行しない
            if (force == false && doc.Length > HUGE_FILE_LENGTH)
                return false;

            long nowTick = DateTime.Now.Ticks;
            bool not_sync = force || (!this._IsSync && Math.Abs(nowTick - this.lastUpdateTicks) >= AllowCallTicks);
            if (not_sync)
            {
                doc.SyntaxInfoCollection.Clear();

                for (int i = 0; i < lti.Count; i++)
                {
                    this.HilightLine(doc, lti, i);
                }

                this.Hilighter.Reset();
                lti.ClearLayoutCache();

                this.lastUpdateTicks = nowTick;

                this._IsSync = true;

                return true;
            }
            return false;
        }

        private void HilightLine(Document doc, LineToIndexTable lti, int row)
        {
            //シンタックスハイライトを行う
            var line = lti.GetRaw(row);
            var lineHeadIndex = lti.GetLineHeadLongIndex(row);
            int level = this.Hilighter.DoHilight(doc, lineHeadIndex, line.length, (s) =>
            {
                if (s.type == TokenType.None || s.type == TokenType.Control)
                    return;
                var linFeedLength = Util.GetNewLineLengthInTail(doc.Slice(lineHeadIndex + s.index,s.length));
                if (linFeedLength > 0)
                    s.length -= linFeedLength;
                doc.SyntaxInfoCollection.Add(new SyntaxInfo(lineHeadIndex + s.index, s.length, s.type));
            });
        }

        public void Update(Document doc, long startIndex, long insertLength, long removeLength)
        {
            this.lastUpdateTicks = DateTime.Now.Ticks;
            this._IsSync = false;
        }
    }
}
