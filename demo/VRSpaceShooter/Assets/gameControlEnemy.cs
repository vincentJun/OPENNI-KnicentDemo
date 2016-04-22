using UnityEngine;
using System.Collections;

public class gameControlEnemy : MonoBehaviour {

    public GameObject[] Enemy;

    public GameObject parent;

    public Vector3[] points;

    public float speed = 1.0f; 

    Quaternion myquaternion = Quaternion.identity;

    public float time;

    public float fixtime = 2;

	// Use this for initialization
	void Start () {
        parent = GameObject.Find("pointEnemy");
        points = new Vector3[parent.transform.childCount];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = parent.transform.GetChild(i).transform.position;
        }
        time = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.time > time)
        {
            time = Time.time + fixtime;
            StartCoroutine(SelectEnemy());
        }
      
	}
    //
    IEnumerator SelectEnemy() {
        yield return new WaitForSeconds(speed);
        int randomPoints = Random.Range(0,points.Length);
        Vector3 clonePoints = points[randomPoints];
        int randomEnemy = Random.Range(0,Enemy.Length);
        GameObject cloneEnemy = Enemy[randomEnemy];
        GameObject cloneobj = GameObject.Instantiate(cloneEnemy,clonePoints,cloneEnemy.transform.rotation)as GameObject;
    }
}
