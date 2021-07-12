using System.Collections.Generic;
using UnityEngine;

public static class PoolManager
{
    const int DEFAULT_POOL_SIZE = 10;

    public class Pool
    {
        private int m_iNextId = 1;

        private Stack<GameObject> m_cInactive;

        private GameObject m_cPrefab;
        private Transform m_cParentPool;


        public Pool(GameObject prefab, int initialQty, Transform parentPool = null)
        {
            m_cPrefab = prefab;
            m_cParentPool = parentPool;
            m_cInactive = new Stack<GameObject>(initialQty);
        }

        public GameObject Spawn(Vector3 pos, Quaternion rot, Transform parent, bool inWorldPos)
        {
            GameObject obj;
            if (m_cInactive.Count == 0)
            {
                obj = (GameObject)GameObject.Instantiate(m_cPrefab, pos, rot,parent);
                obj.name = m_cPrefab.name + " (" + (m_iNextId++) + ")";
                obj.AddComponent<PoolMember>().m_cPool = this;
            }
            else
            {
                obj = m_cInactive.Pop();

                if (obj == null)
                {
                    return Spawn(pos, rot, parent, inWorldPos);
                }
            }

            Transform objTransform = obj.transform; 
            objTransform.parent = parent;

			if (inWorldPos)
			{
                objTransform.position = pos;
                objTransform.rotation = rot;
			}
			else
			{
                objTransform.localPosition = pos;
                objTransform.localRotation = rot;
			}

            obj.SetActive(true);

            return obj;
        }

        public void Despawn(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.parent = m_cParentPool;
            m_cInactive.Push(obj);
        }

		public int Size()
		{
			return m_cInactive.Count;
		}

    }


    public class PoolMember : MonoBehaviour
    {
        public Pool m_cPool;
    }

	static int m_iID = -1;
    static Dictionary<int, Pool> m_dPools;


    private static void Init(int ID, GameObject prefab = null, Transform parentPool = null, int qty = DEFAULT_POOL_SIZE)
    {
        if (m_dPools == null)
        {
            m_dPools = new Dictionary<int, Pool>();
        }
        if (prefab != null && m_dPools.ContainsKey(ID) == false)
        {
            if (parentPool)
                m_dPools[ID] = new Pool(prefab, qty, parentPool);
            else
                m_dPools[ID] = new Pool(prefab, qty);
        }
    }

    static public int Preload(GameObject prefab, Transform parent, int qty = 1)
    {
        //System.Guid s = System.Guid.NewGuid();

        //int ID = 1;
        //foreach (byte b in System.Guid.NewGuid().ToByteArray())
        //    ID *= ((int)b + 1);

        Init(++m_iID, prefab, parent, qty);

        GameObject[] obs = new GameObject[qty];
        for (int i = 0; i < qty; i++)
            obs[i] = Spawn(m_iID, parent.position, Quaternion.identity, parent);

        for (int i = 0; i < qty; i++)
            Despawn(obs[i]);

        return m_iID;
    }

    static public GameObject Spawn(int ID, Vector3 pos, Quaternion rot, Transform parent, bool inWorldPos = false)
    {
        return m_dPools[ID].Spawn(pos, rot, parent, inWorldPos);
    }

    static public GameObject[] Spawns(int ID, Vector3 pos, Quaternion rot, Transform parent, int size, bool inWorldPos = false)
    {
        GameObject[] objects = new GameObject[size];

        for (int idx = 0; idx < size; idx++)
        {
            objects[idx] = m_dPools[ID].Spawn(pos, rot, parent, inWorldPos);
        }

        return objects;
    }

    static public void Despawn(GameObject obj)
    {
        PoolMember pm = obj.GetComponent<PoolMember>();
        if (pm == null)
        {
            Debug.Log("Object '" + obj.name + "' wasn't spawned from a pool. Destroying it instead.");
            GameObject.Destroy(obj);
        }
        else
        {
            pm.m_cPool.Despawn(obj);
        }
    }

    static public void Despawn(GameObject[] objs)
    {
        if (objs != null)
            foreach (GameObject gameObject in objs)
                Despawn(gameObject);
    }

	static public int GetPoolSize(int poolID)
	{
		return m_dPools[poolID].Size();
	}
}
