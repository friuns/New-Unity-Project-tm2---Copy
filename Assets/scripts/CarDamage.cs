//Basedon the work of BTM (http://forum.unity3d.com/members/20246-%95BTM)
//Info: http://forum.unity3d.com/threads/52040-Deform-Script-(early-stage)
// Modified by Michele Di Lena unitycar@unitypackages.net

using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CarDamage : bs
{
    [Serializable]
    public class permaVertsColl
    {
        public Vector3[] permaVerts;
    }
    internal MeshFilter[] meshFilters; // array of the meshes deformed by the script. If left empty the script will use all the meshes of the car
    private MeshFilter[] m_meshFilters;
    public float deformNoise = 0.03f; //noise added to the deformation in order to simulate breaks
    public float deformRadius = 0.5f; //radius of the deformation from the collision point.
    float bounceBackSleepCap = 0.002f; // below this value mesh is considered repaired
    public float bounceBackSpeed = 2f; //speed at which object's mesh go back to it's original state after pressing repair key
    private permaVertsColl[] originalMeshData;
    private bool sleep = true;
    public float maxDeform = 0.5f; //maximum distance from it's original position that a vertex can move. If left to 0 the vertex will move with no limit
    //float minForce = 5f; //below this value collisions are ignored. WARNING: values too low (<5) cause weird car damages
    public float multiplier = 0.1f; //the deformation value is the force of the collision * this value.
    public float YforceDamp = 1f; // damps of the strenght collisions in vertical direction. Values <1 will save the car from severe damages after jumps;. Vaules between 0.0 - 1.0
    [HideInInspector]
    public bool repair = false;
    Vector3 vec;
    Transform myTransform;
    //GameObject body;
    public void Start()
    {
        int i;
        myTransform = transform;
        originalMeshData = new permaVertsColl[meshFilters.Length];
        for (i = 0; i < meshFilters.Length; i++)
        {
            originalMeshData[i]= new permaVertsColl();
            originalMeshData[i].permaVerts = meshFilters[i].mesh.vertices;
        }
        //foreach (Transform child in transform)
        //{
        //    if (child.gameObject.tag == "Body" || child.gameObject.name == "Body")
        //        body = child.gameObject;
        //}
    }
    public void Update()
    {
        if (!sleep && repair && bounceBackSpeed > 0)
        {
            int k;
            sleep = true;
            for (k = 0; k < meshFilters.Length; k++)
            {
                Vector3[] vertices = meshFilters[k].mesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] += (originalMeshData[k].permaVerts[i] - vertices[i]) * (Time.deltaTime * bounceBackSpeed);
                    if ((originalMeshData[k].permaVerts[i] - vertices[i]).magnitude >= bounceBackSleepCap) sleep = false;
                }
                meshFilters[k].mesh.vertices = vertices;
                meshFilters[k].mesh.RecalculateNormals();
                meshFilters[k].mesh.RecalculateBounds();
            }
            if (sleep) repair = false;
        }
    }
    public void OnHit(Collision collision, Vector3 colRelVel)
    {
        if (collision.contacts.Length > 0)
        {
            colRelVel.y *= YforceDamp;            
            //float angle = Vector3.Angle(collision.contacts[0].normal, colRelVel);
            //float cos = Mathf.Abs(Mathf.Cos(angle * Mathf.Deg2Rad));
            //if (colRelVel.magnitude * cos >= minForce)
            {
                sleep = false;
                vec = myTransform.InverseTransformDirection(colRelVel) * multiplier * 0.1f;
                for (int i = 0; i < meshFilters.Length; i++)
                    DeformMesh(meshFilters[i].mesh, originalMeshData[i].permaVerts, collision, 1, meshFilters[i].transform);
            }
        }
    }
    public void DeformMesh(Mesh mesh, Vector3[] originalMesh, Collision collision, float cos, Transform meshTransform)
    {
        Vector3[] vertices = mesh.vertices;
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 point = meshTransform.InverseTransformPoint(contact.point);
            for (int i = 0; i < vertices.Length; i++)
            {
                if ((point - vertices[i]).magnitude < deformRadius)
                {
                    vertices[i] += ((vec * (deformRadius - (point - vertices[i]).magnitude) / deformRadius) * cos + (Random.onUnitSphere * deformNoise)) ;
                    if (maxDeform > 0 && (vertices[i] - originalMesh[i]).magnitude > maxDeform)
                    {
                        vertices[i] = originalMesh[i] + (vertices[i] - originalMesh[i]).normalized * maxDeform;
                    }
                }
            }
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}