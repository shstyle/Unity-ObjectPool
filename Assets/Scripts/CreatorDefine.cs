using System.Collections.Generic; using UnityEngine;

public enum PrefabDefine
{
	/* Game Creator... (Example)*/
	Game_XXXX,


	/* Cube Creator  (Example)*/
	CubeCreator_XXXX
}
[System.Serializable]
public class PoolData
{
	#if UNITY_EDITOR
	public string name;
	#endif
	public List<GameObject> prefabList = new List<GameObject>(); 
	public GameObject Prefab;
	public int Size;
}
[System.Serializable]
public class CreatePrefabList
{
	[HideInInspector]
	public string name;
	public GameObject Prefab;
	public PrefabDefine Define;
	
	public int cachingSize;
}
