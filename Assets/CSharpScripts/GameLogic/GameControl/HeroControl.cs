using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroControl : MonoBehaviour {

	// public ScrollRect controller;

	public Joystick controller;

	// Use this for initialization
	void Start () {
		// heart = 10;
		// force = 10;
	
		// string[] skillNames = new string[3];
		// skillNames[0] = "龙爪手";
		// skillNames[1] = "九阳神功";
		// skillNames[2] = "美利坚盾击";
		// for (int index = 0; index < 3; index++)
		// {
		// 	SkillCard skill = new SkillCard(){cardName = skillNames[Random.Range(0, 3)], physicsDamage = Random.Range(1, 100), cost = Random.Range(1, 10)};
		// 	allCards.Add(skill);
		// 	skillCards.Add(skill);
		// }
		// string[] equipNames = new string[3];
		// equipNames[0] = "夏洛克之花";
		// equipNames[1] = "共和国之灰";
		// equipNames[2] = "美利坚之盾";
		// for (int index = 0; index < 3; index++)
		// {
		// 	EquipCard equip = new EquipCard(){cardName = equipNames[Random.Range(0, 3)], attack = Random.Range(1, 100), defence = Random.Range(1, 10)};
		// 	allCards.Add(equip);
		// 	equipCards.Add(equip);
		// }
		// dump();
	}
	
	// void dump () {

	// 	// CodeName.Week.
	// 	// Debug.Log(EnumDescription.GetFieldText(CodeName.Week.T_1));
	// 	// string value = EnumDescription.GetEnumText(typeof(CodeName.Week));
	// 	// Debug.Log(value);
	// 	EnumDescription[] array = EnumDescription.GetFieldTexts(typeof(CodeName.Week));
	// 	Debug.Log(array);
	// 	for (int index = 0; index < array.Length; index++)
	// 	{
	// 		// Debug.Log(array[index].Value);
	// 		// Debug.Log(array[index].FieldName);
	// 		// Debug.Log(array[index].Text);
	// 		// Debug.Log(array[index].Rank);
	// 		// for (int j = 0; j < array[index].Params.Length; j++)
	// 		// {
	// 		// 	// Debug.Log(array[index].Params[j]);
	// 		// 	Debug.Log(array[index].a);
	// 		// 	Debug.Log(array[index].b);
	// 		// 	Debug.Log(array[index].c);
	// 		// 	Debug.Log(array[index].d);
	// 		// }
	// 	}

	// 	Debug.Log("当前生命值为" + heart);
	// 	Debug.Log("当前体力值为" + force);
	// 	Debug.Log("当前手牌数量" + allCards.Count);
	// 	for (int index = 0; index < allCards.Count; index++)
	// 	{
	// 		CardModel card = allCards[index];
	// 		// Debug.Log(card.name);
	// 		card.dump();
	// 	}
	// }

	public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;
    void Update() {
		// 获取horizontal 和 vertical 的值，其值位遥感的anchoredPosition的的单位向量
        float hor = controller.direction.x;
        float ver = controller.direction.y;

		// Debug.Log(controller.content.anchoredPosition.normalized);

        Vector3 direction = new Vector3(hor, 0, ver);
 
        if(direction!= Vector3.zero) {
            //控制转向
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction),Time.deltaTime*10);
            //向前移动
            transform.Translate(Vector3.forward * Time.deltaTime * 5);
        }
    }
}
