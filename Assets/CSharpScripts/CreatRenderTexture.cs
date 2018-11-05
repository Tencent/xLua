using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CreatRenderTexture : MonoBehaviour {

	public Camera camera;
	private RenderTexture renderTexture;
	private RawImage rawImage;

	// Use this for initialization
	void Start () {
		rawImage = GetComponent<RawImage>();
		
		// renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
		renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
		camera.targetTexture = renderTexture;
		rawImage.texture = renderTexture;
	}
	
	// Update is called once per frame
	void Update () {
		// if (Input.GetMouseButtonDown(0)) {
        //     Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        //     RaycastHit hitInfo;
        //     if (Physics.Raycast(ray, out hitInfo)) {
        //         Debug.Log(hitInfo.transform.name);
        //     }
        // }
	}
}
