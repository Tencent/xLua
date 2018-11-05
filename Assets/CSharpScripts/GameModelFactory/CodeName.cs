using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameModel;
public class CodeName : MonoBehaviour {
	[EnumDescription("日期", 1)]  
	public enum Week  
	{  
		[EnumDescription("星期一", 1, "string")]  
		T_1,  
		[EnumDescription("星期二", 7, 1)]  
		T_2,
		[EnumDescription("星期三", 2, 2.0f)]  
		T_3,
		[EnumDescription("星期四", 6, "汉字")]  
		T_4,
		[EnumDescription("星期五", 3, new int[5] { 99,  98, 92, 97, 95})]
		T_5,
		[EnumDescription("星期六", 4, "1", "2")]  
		T_6,
		[EnumDescription("星期七", 5, 1, 2, 2.0f)]  
		T_7,
		[EnumDescription("星期七", 5, 1, 2, 2.0f)]  
		T_8,

	}

	public EquipCard equipCard = new EquipCard(){Name = "方天画戟"};

	public Dictionary<string, object> dict = new Dictionary<string, object>(){{"xxx","xx"}};

	public enum HeroEnum
	{
		[EnumDescription("琼恩·雪诺", 1)]
		JonSnow,
		[GameModelDescription("xxxx")]
		JonSnow_Fake,
	}

	public enum Monster
	{

	}



}
