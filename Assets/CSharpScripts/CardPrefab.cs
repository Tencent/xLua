using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardPrefab : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	// (GameObject)Instantiate(Resources.Load("Prefab/UI/CardPrefab"));
	public GameObject view;
	public int index;
	private Button button;
	private Vector3 originPosition;
	public delegate void Callback(int index);
	public Callback OnEndDragCallback;

	void Start()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(
			delegate{
				OnButtonClick();		
			}
		);
	}

	public void SetNativeSize()
	{
		RectTransform rt = view.GetComponent<RectTransform>();
		float whRate = rt.rect.width / rt.rect.height;
		RectTransform parentRt = view.transform.parent.GetComponent<RectTransform>();
		view.GetComponent<RectTransform>().sizeDelta = new Vector2(parentRt.rect.height * whRate, parentRt.rect.height);
	}

	public void SavePosition()
	{
		originPosition = transform.position;
	}

	public void SavePosition(Vector3 position)
	{
		originPosition = position;
	}


	private void OnButtonClick()
	{
		Debug.Log("按钮被点击了");
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		var canvas = Utility.FindInParents<Canvas>(gameObject);
		if (canvas == null)
			return;
		SetDraggedPosition(eventData);
	}
 
	public void OnDrag(PointerEventData eventData)
	{
		SetDraggedPosition(eventData);
	}
 
	private void SetDraggedPosition(PointerEventData eventData)
	{
		Vector3 movePosition;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(view.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out movePosition))
		{
			DragAnimation(movePosition);
		}		
	}
 
	public void OnEndDrag(PointerEventData eventData)
	{
		Vector3 endPosition;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(view.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out endPosition))
		{
			Debug.Log(endPosition.y);
			Debug.Log(originPosition.y);
			if (endPosition.y - originPosition.y > 2f)
			{
				gameObject.SetActive(false);
				if (OnEndDragCallback != null)
				{
					OnEndDragCallback(index);
				}
			} else {
				DragAnimation(originPosition);
			}
		}	
	}

	private void DragAnimation(Vector3 position)
	{
		if (position != null)
		{
			var rt = view.GetComponent<RectTransform>();
			Tweener moveTweener = rt.DOMove(position, 0.1f);
			moveTweener.onComplete = delegate() {
			};
		}
	}
}
