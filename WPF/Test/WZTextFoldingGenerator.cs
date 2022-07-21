using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FooEditEngine;

namespace Test
{
    public sealed class OutlineItem : FoldingItem
    {
        /// <summary>
        /// コンストラクター
        /// </summary>
        public OutlineItem(int start, int end, int level)
            : base(start, end)
        {
            this.Level = level;
        }

        /// <summary>
        /// アウトラインレベル
        /// </summary>
        public int Level
        {
            get;
            set;
        }
    }

    sealed class WZTextFoldingGenerator : IFoldingStrategy
    {
        struct TextLevelInfo
        {
            public int Index;
            public int Level;
            public TextLevelInfo(int index, int level)
            {
                this.Index = index;
                this.Level = level;
            }
        }
        public IEnumerable<FoldingItem> AnalyzeDocument(Document doc, int start, int end)
        {
            Stack<TextLevelInfo> beginIndexs = new Stack<TextLevelInfo>();
            int lineHeadIndex = start;
            foreach (string lineStr in doc.GetLines(start, end))
            {
                int level = GetWZTextLevel(lineStr);
                if (level != -1)
                {
                    foreach(FoldingItem item in GetFoldings(beginIndexs,level, lineHeadIndex))
                        yield return item;
                    beginIndexs.Push(new TextLevelInfo(lineHeadIndex, level));
                }
                lineHeadIndex += lineStr.Length;
            }
            foreach (FoldingItem item in GetFoldings(beginIndexs, 0, lineHeadIndex))
                yield return item;
        }

        IEnumerable<FoldingItem> GetFoldings(Stack<TextLevelInfo> beginIndexs,int level,int lineHeadIndex)
        {
            while (beginIndexs.Count > 0)
            {
                TextLevelInfo begin = beginIndexs.Peek();
                if (level > begin.Level)
                    break;
                beginIndexs.Pop();
                int endIndex = lineHeadIndex - 1;
                if (begin.Index < endIndex)
                    yield return new OutlineItem(begin.Index, endIndex,begin.Level);
            }
        }

        /// <summary>
        /// WZText形式ののアウトラインレベルを取得する
        /// </summary>
        int GetWZTextLevel(string str)
        {
            int level = -1;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '.')
                    level++;
                else
                    break;
            }
            return level;
        }
    }
}
