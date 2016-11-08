using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[System.Serializable]
public class PoolData
{
    public List<GameObject> prefabList = new List<GameObject>(); //this is pool prefab List
    public GameObject Prefab; // this is identity GameObject
    public string Name; // this is identity Key
    public int Size;
}
public sealed class ObjectPool : MonoBehaviour
{
    private static ObjectPool _instance;
    public static ObjectPool instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.Find("ObjectPool").GetComponent<ObjectPool>();

            return _instance;
        }
    }
    public List<PoolData> pList = new List<PoolData>();


    void Awake()
    {
        AddPool(Resources.Load("Dong") as GameObject, 15); // 테스트
        Setup();


    }

    /// <summary>
    /// 미리 풀 갯수만큼 오브젝트를 생성해둡니다.
    /// </summary>
    public void Setup()
    {
        List<GameObject> objects = new List<GameObject>();
        foreach (PoolData p in pList)
        {
            for (int i = 0; i < p.Size; i++)
                objects.Add(Create(p.Prefab));
        }


        foreach (GameObject go in objects)
            go.SetActive(false);
    }
    /// <summary>
    /// 리스트 중 액티브가 꺼져있는 오브젝트를 찾아서 반환합니다.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public GameObject GetHierachyHideObject(List<GameObject> list)
    {
        GameObject hideObject = null;
        foreach (GameObject go in list)
        {
            if (go.activeInHierarchy)
                continue;

            hideObject = go;
            break;
        }
        return hideObject;
    }
    /// <summary>
    /// 새로운 풀을 만듭니다.
    /// </summary>
    /// <param name="Prefab"></param>
    /// <param name="Size"></param>
    public void AddPool(GameObject Prefab, int Size = 0)
    {
        foreach (PoolData pd in pList)
            if (pd.Prefab == Prefab) { Debug.LogError("Warning! this prefab already added list."); return; };

        PoolData poolData = new PoolData();
        poolData.Prefab = Prefab;
        poolData.Name = Prefab.ToString();
        poolData.Size = Size;
        pList.Add(poolData);
    }

    public GameObject Create(GameObject Prefab)
    {
        return Create(Prefab, new Vector3(0, 0, 0), Quaternion.identity);
    }
    public GameObject Create(GameObject Prefab, Vector3 pos)
    {
        return Create(Prefab, pos, Quaternion.identity);
    }
    /// <summary>
    /// 오브젝트 생성 풀
    /// </summary>
    /// <param name="Prefab"></param>
    /// <param name="pos"></param>
    /// <param name="quaternion"></param>
    /// <returns></returns>
    public GameObject Create(GameObject Prefab, Vector3 pos, Quaternion quaternion)
    {
        GameObject temp_go = null;
        string parent_name = Prefab.name + " Pool";
        GameObject parent = transform.FindChild(parent_name) ? transform.FindChild(parent_name).gameObject : null;


        if (!transform.FindChild(parent_name))
        {
            GameObject temp_parent = new GameObject();
            temp_parent.name = parent_name;
            temp_parent.transform.parent = gameObject.transform;
            parent = temp_parent;
        }

        foreach (PoolData p in pList)
        {

            //이 프리팹이 존재하는가?
            if (p.Prefab.Equals(Prefab))
            {
                bool findedNonActive = false;
                //오브젝트가 하나도 없을경우.

                if (p.prefabList.Count == 0)
                {
                    if (!findedNonActive)
                    {
                        temp_go = Instantiate(Prefab, pos, quaternion) as GameObject;
                        p.prefabList.Add(temp_go);
                    }
                    break;
                }

                GameObject hideObj = GetHierachyHideObject(p.prefabList); // 하이라키에서 꺼져있는 오브젝트를 가져온다.
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
                    p.prefabList.Add(temp_go);
                }

                break;
            }
        }

        temp_go.transform.parent = parent.transform;
        return temp_go;
    }

}


public static class ObjectPoolExtention
{
    public static void AddPool(this GameObject obj, int size = 0)
    {
        ObjectPool.instance.AddPool(obj);
    }

    public static void Create(this GameObject obj)
    {
        ObjectPool.instance.Create(obj);
    }
}