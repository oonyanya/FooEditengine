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

namespace FooEditEngine
{
    interface ICommand
    {
        /// <summary>
        /// アンドゥする
        /// </summary>
        void undo();
        /// <summary>
        /// リドゥする
        /// </summary>
        void redo();
        /// <summary>
        /// マージする
        /// </summary>
        /// <param name="a"></param>
        /// <returns>マージできた場合は真、そうでない場合は偽を返す</returns>
        bool marge(ICommand a);
        /// <summary>
        /// コマンドを結合した結果が空なら真。そうでないなら偽を返す
        /// </summary>
        /// <returns></returns>
        bool isempty();

    }

    sealed class BeginActionCommand : ICommand
    {
        #region ICommand メンバー

        public void undo()
        {
        }

        public void redo()
        {
        }

        public bool marge(ICommand a)
        {
            return false;
        }

        public bool isempty()
        {
            return false;
        }
        #endregion
    }

    sealed class EndActionCommand : ICommand
    {
        #region ICommand メンバー

        public void undo()
        {
        }

        public void redo()
        {
        }

        public bool marge(ICommand a)
        {
            return false;
        }

        public bool isempty()
        {
            return false;
        }
        #endregion
    }

    /// <summary>
    /// アンドゥバッファーを管理するクラス
    /// </summary>
    public sealed class UndoManager
    {
        private bool locked = false;
        private Stack<ICommand> UndoStack = new Stack<ICommand>();
        private Stack<ICommand> RedoStack = new Stack<ICommand>();
        private int groupLevel = 0;

        /// <summary>
        /// コンストラクター
        /// </summary>
        internal UndoManager()
        {
            this.Grouping = false;
        }

        /// <summary>
        /// 操作を履歴として残します
        /// </summary>
        /// <param name="cmd">ICommandインターフェイス</param>
        internal void push(ICommand cmd)
        {
            if (this.locked == true)
                return;
            ICommand last = null;
            if (this.AutoMerge && UndoStack.Count() > 0)
                last = UndoStack.First();
            if(last == null || last.marge(cmd) == false)
                UndoStack.Push(cmd);
            if (last != null && last.isempty())
                UndoStack.Pop();
            if (this.RedoStack.Count > 0)
                RedoStack.Clear();
        }

        /// <summary>
        /// 履歴として残される操作が一連のグループとして追加されるなら真を返し、そうでなければ偽を返す
        /// </summary>
        internal bool Grouping
        {
            get;
            set;
        }


        /// <summary>
        /// アクションを結合するなら真。そうでないなら偽
        /// </summary>
        internal bool AutoMerge
        {
            get;
            set;
        }

        /// <summary>
        /// 一連のアンドゥアクションの開始を宣言します
        /// </summary>
        public void BeginUndoGroup()
        {
            if (this.Grouping)
            {
                this.groupLevel++;
            }
            else
            {
                this.push(new BeginActionCommand());
                this.Grouping = true;
                this.AutoMerge = true;
            }
        }

        /// <summary>
        /// 一連のアンドゥアクションの終了を宣言します
        /// </summary>
        public void EndUndoGroup()
        {
            if (this.Grouping == false)
                throw new InvalidOperationException("BeginUndoGroup()を呼び出してください");
            if (this.groupLevel > 0)
            {
                this.groupLevel--;
            }
            else
            {
                ICommand last = UndoStack.First();
                if (last != null && last is BeginActionCommand)
                    this.UndoStack.Pop();
                else
                    this.push(new EndActionCommand());
                this.Grouping = false;
                this.AutoMerge = false;
            }
        }

        /// <summary>
        /// 元に戻します
        /// </summary>
        public void undo()
        {
            if (this.UndoStack.Count == 0 || this.locked == true)
                return; 
  
            ICommand cmd;
            bool isGrouped = false;

            do
            {
                cmd = this.UndoStack.Pop();
                this.RedoStack.Push(cmd);
                this.BeginLock();
                cmd.undo();
                this.EndLock();
                //アンドゥスタック上ではEndActionCommand,...,BeginActionCommandの順番になる
                if (cmd is EndActionCommand)
                    isGrouped = true;
                else if (cmd is BeginActionCommand)
                    isGrouped = false;
            } while (isGrouped);

        }

        /// <summary>
        /// 元に戻した動作をやり直します
        /// </summary>
        public void redo()
        {
            if (this.RedoStack.Count == 0 || this.locked == true)
                return;
            ICommand cmd;
            bool isGrouped = false;

            do
            {
                cmd = this.RedoStack.Pop();
                this.UndoStack.Push(cmd);
                this.BeginLock();
                cmd.redo();
                this.EndLock();
                //リドゥスタック上ではBeginActionCommand,...,EndActionCommandの順番になる
                if (cmd is BeginActionCommand)
                    isGrouped = true;
                else if (cmd is EndActionCommand)
                    isGrouped = false;
            } while (isGrouped);
        }

        /// <summary>
        /// 操作履歴をすべて削除します
        /// </summary>
        public void clear()
        {
            if (this.locked == true)
                return;
            this.UndoStack.Clear();
            this.RedoStack.Clear();
        }
        /// <summary>
        /// 以後の操作をアンドゥ不能にする
        /// </summary>
        public void BeginLock()
        {
            this.locked = true;
        }

        /// <summary>
        /// 以後の操作をアンドゥ可能にする
        /// </summary>
        public void EndLock()
        {
            this.locked = false;
        }
    }
}
