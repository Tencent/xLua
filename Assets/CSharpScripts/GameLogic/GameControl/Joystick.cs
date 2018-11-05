using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Joystick : ScrollRect
{   
    // 外部调用的方向
    public Vector2 direction;
    // 半径
    private float _mRadius = 0f;
    // 距离
    private const float Dis = 0.8f;

    protected override void Start()
    {
        base.Start();
        // 能移动的半径 = 摇杆的宽 * Dis
		if (content)
		{
        	_mRadius = content.sizeDelta.x * Dis;
		}
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        // 获取摇杆，根据锚点的位置。
        var contentPosition = content.anchoredPosition;

        // 判断摇杆的位置 是否大于 半径
		// 获取Vector2向量的长度 (x*x+y*y+z*z)的平方根
        if (contentPosition.magnitude > _mRadius)
        {   
            // 设置摇杆最远的位置
            contentPosition = contentPosition.normalized * _mRadius;
            SetContentAnchoredPosition(contentPosition);
        }
        // 最后 v2.x/y 就跟 Input中的 Horizontal Vertical 获取的值一样 
        var v2 = content.anchoredPosition.normalized;
		Debug.Log(v2);
        direction = content.anchoredPosition.normalized;
    }

	public override void OnEndDrag(PointerEventData eventData)
	{
		base.OnEndDrag(eventData);
        direction = Vector3.zero;
	}
}