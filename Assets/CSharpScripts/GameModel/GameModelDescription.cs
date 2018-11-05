using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    class GameModelDescription : EnumDescription
    {
        // private FieldInfo fieldInfo;
        public GameModelDescription(string text, int rank, params object[] numStack)
        {
            this.Text = text;
            this.Rank = rank;
			this.Params = numStack;
        }
 
        public GameModelDescription(string text)
            : this(text, 5) { }

        public GameModelDescription()
            : this("none", 5) { }
 
        public Dictionary<string, object> xxxx { get; set; }
        public int yyyy { get; set; }
		// public object[] Params {get; set;}
    
        // public int Value
        // {
        //     get
        //     {
        //         return (int)fieldInfo.GetValue(null);
        //     }
        // }
        // public string FieldName
        // {
        //     get
        //     {
        //         return fieldInfo.Name;
        //     }
        // }
    }