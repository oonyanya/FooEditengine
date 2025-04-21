using System;
using System.Collections.Generic;
using System.Text;

namespace FooEditEngine
{
    class SyntaxHilightGenerator : ILineInfoGenerator
    {
        const long AllowCallTicks = 1000 * 10000;   //see.DateTime.Ticks プロパティ
        long lastUpdateTicks = DateTime.Now.Ticks;
        bool _IsSync = true;

        /// <summary>
        /// シンタックスハイライター
        /// </summary>
        internal IHilighter Hilighter { get; set; }

        public void Clear(LineToIndexTable lti)
        {
            for (int i = 0; i < lti.Count; i++)
                lti.GetRaw(i).Syntax = null;
            lti.ClearLayoutCache();
            this._IsSync = false;
        }

        public bool Generate(Document doc, LineToIndexTable lti, bool force = true)
        {
            if (this.Hilighter == null)
                return false;

            long nowTick = DateTime.Now.Ticks;
            bool sync = force || !this._IsSync;
            if (sync || Math.Abs(nowTick - this.lastUpdateTicks) >= AllowCallTicks)
            {
                for (int i = 0; i < lti.Count; i++)
                    this.HilightLine(lti, i);

                this.Hilighter.Reset();
                lti.ClearLayoutCache();

                this.lastUpdateTicks = nowTick;

                this._IsSync = true;

                return true;
            }
            return false;
        }

        private void HilightLine(LineToIndexTable lti, int row)
        {
            //シンタックスハイライトを行う
            List<SyntaxInfo> syntax = new List<SyntaxInfo>();
            string str = lti[row];
            int level = this.Hilighter.DoHilight(str, str.Length, (s) =>
            {
                if (s.type == TokenType.None || s.type == TokenType.Control)
                    return;
                if (str[s.index + s.length - 1] == Document.NewLine)
                    s.length--;
                syntax.Add(new SyntaxInfo(s.index, s.length, s.type));
            });

            LineToIndexTableData lineData = lti.GetRaw(row);
            lineData.Syntax = syntax.ToArray();

        }

        public void Update(Document doc, long startIndex, long insertLength, long removeLength)
        {
            this.lastUpdateTicks = DateTime.Now.Ticks;
        }
    }
}
