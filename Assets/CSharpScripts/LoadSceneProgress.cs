using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.IO;

public class LoadSceneProgress : MonoBehaviour {

	public Slider progressSlider;

	// Use this for initialization
	void Start () {
		OnLoadScene();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void SetLoadingPercentage(float v)
	{
		progressSlider.value = v / 100;
	}


	public void OnLoadScene()
	{
		StartCoroutine(StartLoading(2));
	}

	private IEnumerator StartLoading(int scene)
	{
		int displayProgress = 0;
		int toProgress = 0;
		AsyncOperation op = SceneManager.LoadSceneAsync(scene);
		op.allowSceneActivation = false;
		while(op.progress < 0.9f) 
		{
			toProgress = (int)op.progress * 100;
			while(displayProgress < toProgress)
			{
				++displayProgress;
				SetLoadingPercentage(displayProgress);
				// 等待帧结束
				yield return new WaitForEndOfFrame();
			}
		}

		toProgress = 100;
		while (displayProgress < toProgress) 
		{
			++displayProgress;
			SetLoadingPercentage(displayProgress);
			// 等待帧结束
			yield return new WaitForEndOfFrame();
		}

		op.allowSceneActivation = true;
	}
}
