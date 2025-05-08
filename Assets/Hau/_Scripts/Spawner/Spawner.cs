using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Spawner<T> : HauMonoBehaviour where T : PoolObj // Day 6 - E40 them <T>
{
    [SerializeField] protected int spawnCount = 0;
    [SerializeField] protected Transform poolHolder; //E49,50 create
    [SerializeField] protected List<T> inPoolObj;

    [SerializeField] protected PoolPrefabs<T> poolPrefabs;
    public PoolPrefabs<T> PoolPrefabs => poolPrefabs;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadPoolHolder(); //E49,50 create
        this.LoadPoolPrefabs();
    }

    protected virtual void LoadPoolPrefabs()
    {
        if (this.poolPrefabs != null) return;
        this.poolPrefabs = GetComponentInChildren<PoolPrefabs<T>>();
        Debug.Log(transform.name + ": LoadPoolPrefabs", gameObject);
    }

    private void LoadPoolHolder() //E49,50 create
    {
        if (this.poolHolder != null) return;
        this.poolHolder = transform.Find("PoolHolder");

        if(this.poolHolder == null )
        {
            this.poolHolder = new GameObject("PoolHolder").transform;
            this.poolHolder.parent = this.transform;
        }


        Debug.LogWarning(transform.name + ": LoadPoolHolder: " + gameObject);
    }

    public virtual T Spawn(T Prefab) // Day 6 - E40
    {

        T newObject = GetObjFromPool(Prefab);

        if(newObject == null)
        {
            newObject = Instantiate(Prefab);
            this.spawnCount++;
            this.UpdateName(Prefab.transform, newObject.transform);
        }

        if (this.poolHolder != null) newObject.transform.parent = this.poolHolder.transform; //E49,50 create

        return newObject;
    }

    public virtual T Spawn(T buletPrefab, Vector3 postion)
    {
        T newBullet = this.Spawn(buletPrefab);
        newBullet.transform.position = postion;
        return newBullet;
    }

    public virtual Transform Spawn(Transform prefab)
    {
        Transform newObject = Instantiate(prefab);
        return newObject;
    }

    public virtual void Despawn(Transform obj) // Day 5 - E39
    {
        Destroy(obj.gameObject);
    }

    public virtual void Despawn(T obj) // Day 6 - E40
    {
        if (obj is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.gameObject.SetActive(false);
            this.AddObjectToPool(obj);
        }
       
    }

    protected virtual void AddObjectToPool(T obj) // Day 6 - E40
    {
       this.inPoolObj.Add(obj);
    }

    protected virtual void RemoveObjectToPool(T obj) // Day 6 - E42
    {
        this.inPoolObj.Remove(obj);
    }

    protected virtual void UpdateName(Transform prefab, Transform newObject) // Day 6 - E40
    {
        newObject.name = prefab.name + "_" + this.spawnCount;
    }

    protected virtual T GetObjFromPool(T prefab) // Day 6 - E42
    {
        foreach (T obj in this.inPoolObj)
        {
            if(prefab.GetName() == obj.GetName())
            {
                this.RemoveObjectToPool(obj);
                return obj;
            }
        }

        return null;
    }
}
