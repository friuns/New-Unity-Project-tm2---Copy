using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Pool :bs
{
    public ModelHolder curent;
    public Dictionary<ModelHolder, string> list = new Dictionary<ModelHolder, string>(new Cmp());
    //public List<KeyValuePair<string, ModelHolder>> list = new List<KeyValuePair<string, ModelHolder>>();
    //public Dictionary<int, Component[]> cache = new Dictionary<int, Component[]>();
    public override void Awake()
    {
        _Pool = this;
        transform.parent = null;
        base.Awake();
    }
    //public Transform LoadAndParent(string path, Transform parent)
    //{
    //    var g2 = (GameObject)Resources.Load(path);
    //    var g = Load(path);
    //    Transform t1 = g.transform;
    //    Transform t2 = g2.transform;
    //    t1.parent = parent;
    //    t1.localPosition = t2.position;
    //    t1.localRotation = t2.rotation;
    //    return t1;
    //}
    public GameObject Load(GameObject path, Vector3 pos, Quaternion rot)
    {
        if (!setting.disablePool)
        {
            var name = Path.GetFileName(path.name) + "(Clone)";
            var keyvalue = list.FirstOrDefault(a => a.Value == name);
            ModelHolder mh = keyvalue.Key;
            if (mh != null && mh.model == null)
            {
                Debug.LogError("Pool Model Removed");
                list.Remove(mh);
                mh = null;
            }
            if (mh != null)
            {
                mh.model.position = pos;
                mh.model.rotation = rot;
                mh.UnHide();
                list.Remove(mh);
                return mh.model.gameObject;
            }
        }
        return (GameObject)Instantiate(path,pos,rot);
    }
    public void Save(Transform model, bool hideCompnents = false)
    {
        if (setting.disablePool)
        {
            Destroy(model.gameObject);
            return;
        }

        var mh = new ModelHolder();
        mh.model = model;
        if (isDebug || !list.ContainsKey(mh))
        {
            list.Add(mh, model.name);
            mh._Pool = this;
            mh.HideCompnents = hideCompnents;
            mh.Hide();
            model.parent = transform;
        }
    }
}
public class Cmp : IEqualityComparer<ModelHolder>
{
    public bool Equals(ModelHolder x, ModelHolder y)
    {
        return x.model == y.model;
    }
    public int GetHashCode(ModelHolder obj)
    {
        return obj.model.GetHashCode();
    }
}
public class ModelHolder
{
    public Pool _Pool;
    public Transform model;
    Vector3 oldModelPos;
    Quaternion oldModelRot;
    private Component[] components;
    private bool[] oldComponents;
    internal bool HideCompnents;
    //public Vector3 position;
    public void Hide()
    {
        if (!HideCompnents)
            model.gameObject.SetActive(false);
        else
        {
            //components = model.GetComponentsInChildren<Renderer>();
            components = model.GetComponentsInChildren<Component>();
            oldComponents = new bool[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                {
                    var r = components[i] as Renderer;
                    if (r != null)
                    {
                        oldComponents[i] = r.enabled;
                        r.enabled = false;
                    }
                }
                {
                    var r = components[i] as Collider;
                    if (r != null)
                    {
                        oldComponents[i] = r.enabled;
                        r.enabled = false;
                    }
                }
                {
                    var r = components[i] as Behaviour;
                    if (r != null)
                    {
                        oldComponents[i] = r.enabled;
                        r.enabled = false;
                    }
                }
                {
                    var r = components[i] as Rigidbody;
                    if (r != null)
                    {
                        oldComponents[i] = r.isKinematic;
                        r.isKinematic = true;
                    }
                }
            }
        }
    }
    public void UnHide()
    {
        if (!HideCompnents)
            model.gameObject.SetActive(true);
        else
        {
            for (int i = 0; i < components.Length; i++)
            {
                {
                    var c = components[i] as Collider;
                    if (c != null) c.enabled = oldComponents[i];
                }
                {
                    var c = components[i] as Renderer;
                    if (c != null) c.enabled = oldComponents[i];
                }
                {
                    var c = components[i] as Behaviour;
                    if (c != null) c.enabled = oldComponents[i];
                }
                {
                    var c = components[i] as Rigidbody;
                    if (c != null) c.isKinematic = oldComponents[i];
                }
            }
        }
    }
}