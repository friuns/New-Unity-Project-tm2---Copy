using UnityEngine;


[ExecuteInEditMode]
public class LoaderEditor : bs
{
    public void OnEnable()
    {

        foreach (Loader a in GetComponentsInChildren<Loader>())
            a.OnInit();
    }
}
