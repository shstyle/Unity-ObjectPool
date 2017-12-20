using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 새로 작성한 오브젝트 풀 클래스[싱글턴].
/// </summary>
public class Creator : MonoBehaviour
{
	public List<PoolData> PooledList = new List<PoolData>();
	public List<CreatePrefabList> createList = new List<CreatePrefabList>();
	public Dictionary<PrefabDefine, GameObject> createMap = new Dictionary<PrefabDefine, GameObject>();
	void Awake()
	{
		Setup();
		
	}
	
	/// <summary>
	/// 미리 풀 갯수만큼 오브젝트를 생성해둡니다.
	/// </summary>
	public void Setup()
	{

		/* pList를 구성합니다 */
		for(int i = 0 ; i < createList.Count; i++)
		{
			PoolData pdata = new PoolData();
			#if UNITY_EDITOR
			pdata.name = createList[i].name;
			#endif
			pdata.Prefab = createList[i].Prefab;
			pdata.Size = createList[i].cachingSize;
			PooledList.Add (pdata);
		}


		/* 사전 생성 */
		List<GameObject> objects = new List<GameObject>();
		for(int v = 0 ;  v < PooledList.Count; v++)
		{
			PooledList[v].name = PooledList[v].Prefab.name;
			for (int i = 0; i < PooledList[v].Size; i++)
				objects.Add(Create(PooledList[v].Prefab));
		}

		
		for (int i = 0 ; i < objects.Count; i++)
			objects[i].SetActive(false);
		
		
		for (int i = 0 ; i < createList.Count; i++)
		{
			createList[i].name = createList[i].Define.ToString();
			if (createList[i].Prefab != null)
			{
				if( createMap.ContainsKey(createList[i].Define) ==false)
					createMap.Add(createList[i].Define, createList[i].Prefab);
			}
			
		}
	}
	/// <summary>
	/// 리스트 중 액티브가 꺼져있는 오브젝트를 찾아서 반환합니다.
	/// </summary>
	/// <param name="list"></param>
	/// <returns></returns>
	public GameObject GetHierachyHideObject(List<GameObject> list)
	{
		GameObject hideObject = null;

		for(int i =  0 ;  i <  list.Count; i ++)
		{
			if (list[i].activeInHierarchy)
				continue;
			
			hideObject = list[i];
			break;
		}
		return hideObject;
	}
	
	public void Pool(GameObject obj)
	{
		if (obj.GetComponent<Rigidbody>()   != null)  obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
		if (obj.GetComponent<Rigidbody2D>() != null)  obj.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
		
		obj.SetActive(false);
	}
	
	
	/// <summary>
	/// 새로운 풀을 만듭니다.
	/// </summary>
	/// <param name="Prefab"></param>
	/// <param name="Size"></param>
	public void AddPool(GameObject Prefab, int Size = 0)
	{
		if (Prefab == null) return;

		for(int i  = 0 ; i < PooledList.Count; i++)
		{
			if (PooledList[i].Prefab == Prefab) 
			{ 
				Debug.LogError("Warning! this prefab already added list."); 
				return; 
			}

		}
		
		PoolData poolData = new PoolData();
		poolData.Prefab = Prefab;
		poolData.Size = Size;
		PooledList.Add(poolData);
	}
	/// <summary>
	/// 타입으로생성
	/// </summary>
	public GameObject Create(PrefabDefine Define)
	{
		bool status = createMap.ContainsKey(Define);
		if (status) { return Create(createMap[Define].gameObject, new Vector3(0, 0, 0), createMap[Define].gameObject.transform.rotation); }
		return null;
	}
	public GameObject Create(PrefabDefine Define, Vector3 pos)
	{
		bool status = createMap.ContainsKey(Define);
		if (status) { return Create(createMap[Define].gameObject, pos, Quaternion.identity); }
		return null;
	}
	public GameObject Create(PrefabDefine Define, Vector3 pos, Quaternion rot)
	{
		bool status = createMap.ContainsKey(Define);
		if (status) { return Create(createMap[Define].gameObject, pos, rot); }
		return null;
	}
	/// <summary>
	/// 프리팹으로 생성
	/// </summary>
	public GameObject Create(GameObject Prefab)
	{
		return Create(Prefab, new Vector3(0, 0, 0), Prefab.transform.rotation);
	}
	public GameObject Create(GameObject Prefab, Vector3 pos)
	{
		return Create(Prefab, pos, Prefab.transform.rotation);
	}
	/// <summary>
	/// 오브젝트 생성 풀
	/// </summary>
	/// <returns></returns>
	public GameObject Create(GameObject Prefab, Vector3 pos, Quaternion quaternion)
	{
		GameObject temp_go = null;
		string parent_name = Prefab.name + " Pool";
		GameObject parent = transform.Find(parent_name) ? transform.Find(parent_name).gameObject : null;
		
		
		if (!transform.Find(parent_name))
		{
			GameObject temp_parent = new GameObject();
			temp_parent.name = parent_name;
			temp_parent.transform.parent = gameObject.transform;
			parent = temp_parent;
		}
		
		for(int i = 0 ; i < PooledList.Count; i++)
		{
			//이 프리팹이 존재하는가?
			if (PooledList[i].Prefab.Equals(Prefab))
			{
				bool findedNonActive = false;
				//오브젝트가 하나도 없을경우 자가적으로 오브젝트 풀을 늘린다.
				if (PooledList[i].prefabList.Count == 0)
				{
					if (!findedNonActive)
					{
						temp_go = Instantiate(Prefab, pos, quaternion) as GameObject;
						PooledList[i].prefabList.Add(temp_go);
					}
					break;
				}
				
				GameObject hideObj = GetHierachyHideObject(PooledList[i].prefabList); // 하이라키에서 꺼져있는 오브젝트를 가져온다.
				//비활성된 오브젝트를 재사용한다. (재사용되는 오브젝트로 반환)
				if (hideObj != null)
				{
					hideObj.SetActive(true);
					hideObj.transform.position = pos;
					hideObj.transform.rotation = quaternion;
					findedNonActive = true;
					return hideObj;
				}
				
				
				//비활성 오브젝트가 없을시. (새로만든 오브젝트로 반환)
				if (!findedNonActive)
				{
					temp_go = Instantiate(Prefab, pos, quaternion) as GameObject;
					PooledList[i].prefabList.Add(temp_go);
				}
				
				break;
			}
		}
		
		temp_go.transform.parent = parent.transform;
		return temp_go;
	}
	
}
