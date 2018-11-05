using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour {

	public string sceneName;

	private Button button;

	// Use this for initialization
	void Start () {
		button = GetComponent<Button>();
		button.onClick.AddListener(OnChangeScene);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// 切换场景
	private void OnChangeScene()
	{
		if (sceneName != null)
		{
			SceneManager.LoadScene(sceneName);
		}
	}
}
