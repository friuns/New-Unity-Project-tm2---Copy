using UnityEngine;

public class Example : MonoBehaviour
{
    public LODGroup group;
    void Start() {
        group = gameObject.AddComponent<LODGroup>();
        LOD[] lods = new LOD[4];
        int i = 0;
        while (i < 4)
        {
            PrimitiveType primType = PrimitiveType.Cube;
            if (i == 1) primType = PrimitiveType.Capsule;
            if (i == 2) primType = PrimitiveType.Sphere;
            if (i == 3) primType = PrimitiveType.Cylinder;
            GameObject go = GameObject.CreatePrimitive(primType);
            go.transform.parent = gameObject.transform;
            Renderer[] renderers = new Renderer[1];
            renderers[0] = go.renderer;
            lods[i] = new LOD(1.0F / (i + 1), renderers);
            i++;
        }
        group.SetLODS(lods);
        group.RecalculateBounds();
    }
    void OnGUI()
    {
        if (GUILayout.Button("Enable / Disable"))
            group.enabled = !group.enabled;

        if (GUILayout.Button("Default"))
            group.ForceLOD(-1);

        if (GUILayout.Button("Force 0"))
            group.ForceLOD(0);

        if (GUILayout.Button("Force 1"))
            group.ForceLOD(1);

        if (GUILayout.Button("Force 2"))
            group.ForceLOD(2);

        if (GUILayout.Button("Force 3"))
            group.ForceLOD(3);

        if (GUILayout.Button("Force 4"))
            group.ForceLOD(4);

        if (GUILayout.Button("Force 5"))
            group.ForceLOD(5);

        if (GUILayout.Button("Force 6"))
            group.ForceLOD(6);

    }
}