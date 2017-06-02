using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PrefabDefine
{
    PongkyBreak,
    StarBreak,
    BIGPongky,
    Point,
    Pongky,
    PongkyLine,
    Star_4,
    Star_10,
    ComboText,
    BOOM,
    SmilePongky_Effect,
    NEW_STARPONGKY_BIG,
    STAREFFECT_RED,
    STAREFFECT_BLUE,
    STAREFFECT_YELLOW,
    STAREFFECT_GREEN,
    STAREFFECT_PURPLE,
    STAREFFECT_SILVER,
    BIG_BOOM,
    BIG_BOOM_EFFECT_RED,
    BIG_BOOM_EFFECT_YELLOW,
    BIG_BOOM_EFFECT_BLUE,
    BIG_BOOM_EFFECT_GREEN,
    BIG_BOOM_EFFECT_SILVER
}
[System.Serializable]
public class PoolData
{
    public string name;
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
}

/// <summary>
/// 새로 작성한 오브젝트 풀 클래스[싱글턴].
/// </summary>
public sealed class C_ObjectPool : MonoBehaviour
{
    private static C_ObjectPool _instance;
    public static C_ObjectPool instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<C_ObjectPool>();

            return _instance;
        }
    }
    public List<PoolData> pList = new List<PoolData>();
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
        List<GameObject> objects = new List<GameObject>();
        foreach (PoolData p in pList)
        {
            p.name = p.Prefab.name;
            for (int i = 0; i < p.Size; i++)
                objects.Add(Create(p.Prefab));
        }


        foreach (GameObject go in objects)
            go.SetActive(false);


        foreach (var m in createList)
        {
            m.name = m.Define.ToString();
            if (m.Prefab != null)
            {
                if( createMap.ContainsKey(m.Define) ==false)
                    createMap.Add(m.Define, m.Prefab);
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
        foreach (GameObject go in list)
        {
            if (go.activeInHierarchy)
                continue;

            hideObject = go;
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
        foreach (PoolData pd in pList)
            if (pd.Prefab == Prefab) 
            { 
                Debug.LogError("Warning! this prefab already added list."); 
                return; 
            }

        PoolData poolData = new PoolData();
        poolData.Prefab = Prefab;
        poolData.Size = Size;
        pList.Add(poolData);
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
                //오브젝트가 하나도 없을경우 자가적으로 오브젝트 풀을 늘린다.
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

/// <summary>
/// 타입에 확장함수 추가
/// </summary>
public static class ObjectPoolExtention
{
    public static void AddPool(this GameObject obj, int size = 0)
    {
        C_ObjectPool.instance.AddPool(obj);
    }

    public static void Create(this GameObject obj)
    {
        C_ObjectPool.instance.Create(obj);
    }
    public static void Create(this PrefabDefine def)
    {
        C_ObjectPool.instance.Create(def);
    }
}
