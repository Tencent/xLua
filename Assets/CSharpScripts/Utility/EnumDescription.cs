using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    class EnumDescription : Attribute
    {
        private FieldInfo fieldInfo;
        public EnumDescription(string text, int rank, params object[] numStack)
        {
            this.Text = text;
            this.Rank = rank;
			this.Params = numStack;
        }
 
        /// <summary>
        /// 描述枚举值，默认排序为5
        /// </summary>
        /// <param name="enumDisplayText">描述内容</param>
        public EnumDescription(string text)
            : this(text, 5) { }

        public EnumDescription()
            : this("none", 5) { }
 
        public string Text { get; set; }
        public int Rank { get; set; }
		public object[] Params {get; set;}
    
        public int Value
        {
            get
            {
                return (int)fieldInfo.GetValue(null);
            }
        }
        public string FieldName
        {
            get
            {
                return fieldInfo.Name;
            }
        }
 
        #region 对枚举描述属性的解释相关函数
        /// <summary>
        /// 排序类型
        /// </summary>
        public enum SortType
        {
            /// <summary>
            ///按枚举顺序默认排序
            /// </summary>
            Default,
            /// <summary>
            /// 按描述值排序
            /// </summary>
            DisplayText,
            /// <summary>
            /// 按排序熵
            /// </summary>
            Rank
        }
 
        private static Hashtable cachedEnum = new Hashtable();
 
        /// <summary>
        /// 得到对枚举的描述文本
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <returns></returns>
        public static string GetEnumText(Type enumType)
        {
            EnumDescription[] eds = (EnumDescription[])enumType.GetCustomAttributes(typeof(EnumDescription), false);
            if (eds.Length != 1) return string.Empty;
            return eds[0].Text;
        }
 
        /// <summary>
        /// 获得指定枚举类型中，指定值的描述文本。
        /// </summary>
        /// <param name="enumValue">枚举值，不要作任何类型转换</param>
        /// <returns>描述字符串</returns>
        public static string GetFieldText(object enumValue)
        {
            EnumDescription[] descriptions = GetFieldTexts(enumValue.GetType(), SortType.Default);
            foreach (EnumDescription ed in descriptions)
            {
                if (ed.fieldInfo.Name == enumValue.ToString())
                    return ed.Text;
            }
            return string.Empty;
        }
 
        /// <summary>
        /// 得到枚举类型定义的所有文本，按定义的顺序返回
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        /// <param name="enumType">枚举类型</param>
        /// <returns>所有定义的文本</returns>
        public static EnumDescription[] GetFieldTexts(Type enumType)
        {
            return GetFieldTexts(enumType, SortType.Default);
        }
 
 
        /// <summary>
        /// 得到枚举类型定义的所有文本
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        /// <param name="enumType">枚举类型</param>
        /// <param name="sortType">指定排序类型</param>
        /// <returns>所有定义的文本</returns>
        public static EnumDescription[] GetFieldTexts(Type enumType, SortType sortType)
        {
            EnumDescription[] descriptions = null;
            //缓存中没有找到，通过反射获得字段的描述信息
            if (cachedEnum.Contains(enumType.FullName) == false)
            {
                FieldInfo[] fields = enumType.GetFields();
                ArrayList edAL = new ArrayList();
                foreach (FieldInfo fi in fields)
                {
                    object[] eds = fi.GetCustomAttributes(typeof(EnumDescription), false);
                    if (eds.Length != 1) continue;
                    ((EnumDescription)eds[0]).fieldInfo = fi;
                    edAL.Add(eds[0]);
                }
 
                cachedEnum.Add(enumType.FullName, (EnumDescription[])edAL.ToArray(typeof(EnumDescription)));
            }
            descriptions = (EnumDescription[])cachedEnum[enumType.FullName];
            if (descriptions.Length <= 0) throw new NotSupportedException("枚举类型[" + enumType.Name + "]未定义属性EnumValueDescription");
 
            //按指定的属性冒泡排序
            for (int m = 0; m < descriptions.Length; m++)
            {
                //默认就不排序了
                if (sortType == SortType.Default) break;
 
                for (int n = m; n < descriptions.Length; n++)
                {
                    EnumDescription temp;
                    bool swap = false;
 
                    switch (sortType)
                    {
                        case SortType.Default:
                            break;
                        case SortType.DisplayText:
                            if (string.Compare(descriptions[m].Text, descriptions[n].Text) > 0) swap = true;
                            break;
                        case SortType.Rank:
                            if (descriptions[m].Rank > descriptions[n].Rank) swap = true;
                            break;
                    }
 
                    if (swap)
                    {
                        temp = descriptions[m];
                        descriptions[m] = descriptions[n];
                        descriptions[n] = temp;
                    }
                }
            }
            return descriptions;
        }
 
 
        /// <summary>
        /// 获得枚举类型数据的列表
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <returns>Meta 列表</returns>
        public static List<Meta> GetMeta(Type enumType)
        {
            List<Meta> list = new List<Meta>();
            FieldInfo[] fields = enumType.GetFields();
            foreach (FieldInfo fi in fields)
            {
                object[] eds = fi.GetCustomAttributes(typeof(EnumDescription), false);
                if (eds.Length != 1)
                    continue;
                ((EnumDescription)eds[0]).fieldInfo = fi;
                var item = eds[0] as EnumDescription;
                Meta meta = new Meta
                {
                    MetaName = item.FieldName,
                    MetaRank = item.Rank,
                    MetaText = item.Text,
                    MetaValue = item.Value
                };
                list.Add(meta);
            }
            return list;
        }
 
        /// <summary>
        /// 返回枚举类型数据的哈希表
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <returns>Hashtable</returns>
        public static Hashtable GetMetaTable(Type enumType)
        {
            Hashtable table = new Hashtable();
            FieldInfo[] fields = enumType.GetFields();
            foreach (FieldInfo fi in fields)
            {
                object[] eds = fi.GetCustomAttributes(typeof(EnumDescription), false);
                if (eds.Length != 1)
                    continue;
                ((EnumDescription)eds[0]).fieldInfo = fi;
                var item = eds[0] as EnumDescription;
                table.Add(item.Value, item.Text);
            }
            return table;
        }
 
        /// <summary>
        /// 根据枚举值获得枚举文本
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <param name="key">枚举值</param>
        /// <returns>Text</returns>
        public static object GetMetaValue(Type enumType, int key)
        {
            object value = null;
            Hashtable table = GetMetaTable(enumType);
            if (table.Count > 0)
            {
                value = table[key];
            }
            return value;
        }
 
 
        /// <summary>
        /// 根据枚举文本获得枚举值
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <param name="value">枚举文本</param>
        /// <returns>Value</returns>
        public static object GetMetaKey(Type enumType, string value)
        {
            object key = null;
            Hashtable table = GetMetaTable(enumType);
            if (table.Count > 0)
            {
                foreach (DictionaryEntry de in table)
                {
                    if (de.Value.Equals(value))
                    {
                        key = de.Key;
                    }
                }
            }
            return key;
        }
 
        #endregion
    }
    class Meta
    {
        public string MetaName { get; set; }
        public int MetaValue { get; set; }
        public string MetaText { get; set; }
        public int MetaRank { get; set; }
    }