using UnityEngine;

[XLua.CSharpCallLua]
public delegate GameObject TD111(object o, out GameObject go, ref MonoBehaviour mb);

[XLua.CSharpCallLua]
public delegate GameObject TD222(Rotate o, out GameObject go, ref MonoBehaviour mb, object o2);


[XLua.Hotfix]
public class Rotate : MonoBehaviour {

	// Use this for initialization
	void Start () {    
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.up * Time.deltaTime * 20);
    }

    GameObject Test(out GameObject go, ref MonoBehaviour mb)
    {
        go = null;
        return go;
    }

    GameObject Test2(out GameObject go, ref MonoBehaviour mb, GameObject go2)
    {
        return Test(out go, ref mb);     
    }
}
