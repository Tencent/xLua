using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayView : MonoBehaviour {

	public GameObject view;
	public List<CardPrefab> cardPrefabs;
	
	private static int CardCount = 21;

	// Use this for initialization
	void Start () {
		for (int index = 0; index < CardCount; index++)
		{
			GameObject gameObject = (GameObject)Instantiate(Resources.Load("Prefab/UI/CardPrefab"));
			CardPrefab prefab = gameObject.GetComponent<CardPrefab>();
			cardPrefabs.Add(prefab);
			prefab.view.transform.SetParent(view.transform);
			prefab.view.transform.localScale = Vector3.one;
			prefab.index = index;
			prefab.SetNativeSize();
			//添加点击事件回调
			prefab.OnEndDragCallback += new CardPrefab.Callback(OnEndDragCallback);
		}
		RelignCardPosition();
	}
	
	void RelignCardPosition()
	{
		int count = cardPrefabs.Count;
		RectTransform viewRT = view.GetComponent<RectTransform>();
		float midIndex = count * 0.5f;
		float margin = 0;
		// 角度就是 -30° 到 30° 总计 60° //手牌自身旋转角度
		float maxAngle = 45f;
		float handAngle = 15f;


		for (int index = 0; index < count; index++)
		{
			CardPrefab prefab = cardPrefabs[index];
			RectTransform cardRT = prefab.view.GetComponent<RectTransform>();
			// 每次校准 他的下标都是会变化的
			prefab.index = index;

			float angleZ = (index - midIndex + 0.5f) * (maxAngle / count) * -1;
			// prefab.view.transform.localEulerAngles = new Vector3(0, 0, angleZ);

			// 获取每张牌的位置
			// 每张牌的间隔是 (父控件的总宽度 减去 一张牌的宽度) 之后 除以 (牌总数减一) 的结果
			if (margin == 0 && count != 1)
			{
				if (cardRT.rect.width * count > viewRT.rect.width)
				{
					// margin = (viewRT.rect.width - cardRT.rect.width) / (count - 1);
					margin = (viewRT.rect.width - cardRT.rect.width - calculateOffset(angleZ, cardRT.rect.width, cardRT.rect.height) * 2) / (count - 1);
				} else {
					margin = cardRT.rect.width;
					angleZ = 0;
				}
				// 第一张牌的时候 初始化这个间隔
			}
			float x = (index - midIndex + 0.5f) * margin;
			float handAngleZ = (index - midIndex + 0.5f) * (handAngle / count) * -1;
			float y = calculateOffset(handAngleZ, cardRT.rect.width, cardRT.rect.height);
			y = -Mathf.Abs(y);
			Debug.Log(y);
			Vector3 pos = new Vector3(x, y, 0);
			// prefab.view.transform.localPosition = pos;
			prefab.view.transform.DOLocalMove(pos, 0.1f);
			prefab.view.transform.DOLocalRotate(new Vector3(0, 0, angleZ), 0.1f);
			prefab.SavePosition();
		}
	}

	void OnEndDragCallback(int index)
	{
		cardPrefabs.Remove(cardPrefabs[index]);
		RelignCardPosition();
		Debug.Log(string.Format("PlayView响应到了 {0}", index));
	}

	// Update is called once per frame
	void Update () {
		
	}

	float calculateOffset(float angle, float w, float h){
		/*
					a-----------b-----------|	
				  /	|	 w/2 	|			|	oa == oix == om == r 这都是半径
				 /	|			|			|	xy ⊥ ayin	垂直
				x---y			| h/2		|	△xyi ∽ △oni	三角形相识
			   /  \	|			|			|	∠θ = ∠xioa == angel
			  /		i			|			|	∠α = ∠aob
			 /		|			|			|	∠β = ∠aom
			m-------n-----------o-----------|	∠Δ = ∠ion == ∠yxi
		 */
		float r = Mathf.Sqrt(Mathf.Pow(w, 2) + Mathf.Pow(h, 2)) * 0.5f;
		float xioa = angle;
		float aob = Mathf.Acos(h * 0.5f / r) * Mathf.Rad2Deg;
		float aom = 90 - aob;
		float ion = 90 - aob - xioa;
		float yxi = ion;
		float ma = 2 * r * Mathf.Sin((aom * 0.5f) * Mathf.Deg2Rad);
		float mn = Mathf.Sqrt(Mathf.Pow(ma, 2) - Mathf.Pow(h * 0.5f, 2));
		float no = r - mn;
		float oi = no / Mathf.Cos(ion * Mathf.Deg2Rad);
		float ix = r - oi;
		float xy = Mathf.Cos(ion * Mathf.Deg2Rad) * ix;
		return xy;
	}
}
