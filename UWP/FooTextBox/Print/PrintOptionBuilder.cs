/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using Windows.Graphics.Printing.OptionDetails;
using System.Reflection;
using Windows.ApplicationModel.Resources.Core;

namespace FooEditEngine.UWP
{
    /// <summary>
    /// 表示に使用するリソースID
    /// </summary>
    public sealed class DisplayPrintOptionResourceIDAttribute : Attribute
    {
        public string ResourceID
        {
            get;
            private set;
        }
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="resourceID">リソースID</param>
        public DisplayPrintOptionResourceIDAttribute(string resourceID)
        {
            this.ResourceID = resourceID;
        }
    }
    /// <summary>
    /// IPrintPreviewSourceインターフェイス
    /// </summary>
    public interface IPrintPreviewSource
    {
        /// <summary>
        /// 再描写する
        /// </summary>
        void InvalidatePreview();
    }
    /// <summary>
    /// 印刷プレビューのカスタムオプションリストビルダー
    /// <para>
    /// 印刷プレビューに表示させたいプロパティには「DisplayPrintOptionResourceID」属性を付ける必要があります。
    /// また、このプロパティはenumを継承した型でなければならず、リソースに「型名.値」という形で印刷プレビューに表示される名前を定義する必要があります。
    /// 定義しない場合、値がそのまま表示されます
    /// </para>
    /// </summary>
    public sealed class PrintOptionBuilder<T> where T : IFooDocumentSource
    {
        T PrintDocument;
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="source">対象となるオブジェクト</param>
        public PrintOptionBuilder(T source)
        {
            this.PrintDocument = source;
        }

        /// <summary>
        /// 印刷プレビューのオプションを作成する
        /// </summary>
        /// <param name="details">PrintTaskOptionDetailsオブジェクト</param>
        public void BuildPrintOption(PrintTaskOptionDetails details)
        {
            ResourceMap map = ResourceManager.Current.MainResourceMap.GetSubtree("FooEditEngnine.UWP/Resources");
            ResourceContext context = ResourceContext.GetForCurrentView();

            var properties = this.PrintDocument.GetType().GetRuntimeProperties();
            foreach(PropertyInfo property in properties)
            {
                DisplayPrintOptionResourceIDAttribute attr = property.GetCustomAttribute<DisplayPrintOptionResourceIDAttribute>();
                if (attr == null)
                    continue;
                string resourceName;
                if(property.PropertyType.GetTypeInfo().IsEnum)
                {
                    PrintCustomItemListOptionDetails iteminfo = details.CreateItemListOption(property.Name, map.GetValue(attr.ResourceID, context).ValueAsString);
                    foreach (var enumvalue in Enum.GetValues(property.PropertyType))
                    {
                        string enumvalueStr = enumvalue.ToString();
                        //リソース内部の"."は"/"に変換される
                        resourceName = property.PropertyType.Name + "/" + enumvalueStr;
                        ResourceCandidate resValue = map.GetValue(resourceName, context);
#if DEBUG
                        if(resValue == null)
                            System.Diagnostics.Debug.WriteLine("{0} is not defined by resource",resourceName.Replace('/','.'));
#endif
                        iteminfo.AddItem(enumvalueStr, resValue != null ? resValue.ValueAsString : enumvalueStr);
                    }
                    iteminfo.TrySetValue(property.GetValue(this.PrintDocument).ToString());
                    details.DisplayedOptions.Add(property.Name);
                }
            }

            details.OptionChanged += details_OptionChanged;
        }

        void details_OptionChanged(PrintTaskOptionDetails sender, PrintTaskOptionChangedEventArgs args)
        {
            string optionID = args.OptionId as string;
            if (optionID == null)
                return;
            string optionValue = sender.Options[optionID].Value as string;
            var property = this.GetRuntimeProperty(this.PrintDocument.GetType(),optionID);
            if (property == null)
                return;
            if (property.PropertyType.GetTypeInfo().IsEnum)
            {
                Enum output = (Enum)Enum.Parse(property.PropertyType, optionValue);
                property.SetMethod.Invoke(this.PrintDocument, new object[] { output });
            }

            this.PrintDocument.InvalidatePreview();
        }

        PropertyInfo GetRuntimeProperty(Type t,string name)
        {
            foreach(PropertyInfo property in t.GetRuntimeProperties())
            {
                if (property.Name == name)
                    return property;
            }
            return null;
        }
    }
}
