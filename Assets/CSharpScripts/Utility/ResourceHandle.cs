using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

public class ResourceHandle : MonoBehaviour {

	public void LoadPrefabByRes(string[] assetNames, string path)
	{
		StartCoroutine(StartLoadAsync(assetNames, path));
	}

	IEnumerator StartLoadAsync(string[] assetNames, string path)
	{
		List<UObject> result = new List<UObject>();
		string assetPath = "Prefabs/";
		if (path != null)
		{
			assetPath += path;
		}
		for (int i = 0; i < assetNames.Length; i++)
		{
			ResourceRequest resourceRequest = Resources.LoadAsync<GameObject>(assetPath + assetNames[i]);
			yield return resourceRequest;
			result.Add(resourceRequest.asset);
		}
		Debug.Log(result);
	}
}
