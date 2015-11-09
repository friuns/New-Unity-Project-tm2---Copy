using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
public class DamageText:MonoBehaviour
{
    public TextMesh tmMesh;
    public int damage;
    private float time;
    private Vector3 vel;
    private Camera main;
    public void Start()
    {
        main = Camera.main;
        transform.LookAt(main.transform);
        vel = Random.insideUnitSphere + Vector3.up*3;
    }
    public void Update()
    {
        var deltaTime = Time.deltaTime;

        var sqrt = Mathf.Sqrt((transform.position - main.transform.position).magnitude);
        tmMesh.color = new Color(1, 1, 1, 1 - time);
        transform.position += vel * sqrt * deltaTime;
        vel += Vector3.down * 10 * deltaTime;
        if(time>2)
            Destroy(gameObject);
        tmMesh.transform.localScale = new Vector3(-1, 1, 1) * sqrt * .2f;
        time += deltaTime;
    }
    
}