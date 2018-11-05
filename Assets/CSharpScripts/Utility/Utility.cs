using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Utility : MonoBehaviour {
	/*
		寻找父控件内的组件
	 */
	static public T FindInParents<T>(GameObject gameObject) where T : Component
	{
		if (gameObject == null) 
		{
			return null;
		}

		var component = gameObject.GetComponent<T>();
 
		if (component != null) 
		{
			return component;
		}
		
		var temp = gameObject.transform.parent;
		while (temp != null && component == null)
		{
			component = temp.gameObject.GetComponent<T>();
			temp = temp.parent;
		}
		return component;
	}

	/*
		这两个是用作枚举字符串的设定
	 */
	// [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	// public sealed class EnumDescription : Attribute
	// {
	// 	private string description;
	// 	public string Description { get { return description; } }
	// 	public EnumDescription(string description)
	// 		:base()
	// 	{
	// 		this.description = description;
	// 	}
	// }

	// public static class EnumHelper
	// {
	// 	public static string GetDescription(Enum value)
	// 	{
	// 		if (value == null)
	// 		{
	// 			throw new ArgumentException("value");
	// 		}
	// 		string description = value.ToString();
	// 		var fieldInfo = value.GetType().GetField(description);
	// 		var attributes =
	// 			(EnumDescriptionAttribute[]) fieldInfo.GetCustomAttributes(typeof (EnumDescriptionAttribute), false);
	// 		if (attributes != null && attributes.Length > 0)
	// 		{
	// 			description = attributes[0].Description;
	// 		}
	// 		return description;
	// 	}
	// }

}
