using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System;

[System.Serializable]
public class Done_Boundary
{
    public float xMin, xMax, zMin, zMax;
}

[Serializable]
public class Point
{
    public float X;

    public float Y;

    public float Z;
}

public class Done_PlayerController : MonoBehaviour
{
    public float speed;
    public float tilt;
    public Done_Boundary boundary;

    public GameObject shot;
    public Transform shotSpawn;
    public float fireRate;

    private float nextFire;
    private string serverUrl = "http://192.168.1.119:9999/";

    private Vector3 position;
    private bool positionInit = false;
    private float moveRate = 0.1f;
    void Update()
    {
        if (GetFire() && Time.time > nextFire)// Input.GetButton("Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            GetComponent<AudioSource>().Play();
        }
    }

    void FixedUpdate1()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        GetComponent<Rigidbody>().velocity = movement * speed;

        GetComponent<Rigidbody>().position = new Vector3
        (
            Mathf.Clamp(GetComponent<Rigidbody>().position.x, boundary.xMin, boundary.xMax),
            0.0f,
            Mathf.Clamp(GetComponent<Rigidbody>().position.z, boundary.zMin, boundary.zMax)
        );

        GetComponent<Rigidbody>().rotation = Quaternion.Euler(0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -tilt);
    }

    void FixedUpdate()
    {
        if (!this.positionInit)
        {
            this.position = this.GetPosition();
            this.positionInit = true;
        }
        else
        {
            var newPosition = this.GetPosition();
            var offset = (newPosition - this.position) * this.moveRate;
            var rigidBody = GetComponent<Rigidbody>();
            var avatarPosition = rigidBody.position + offset;
            var x = rigidBody.position.x + offset.x;
            var z = rigidBody.position.z - offset.z;
            rigidBody.position = new Vector3(
                Mathf.Clamp(x, this.boundary.xMin, this.boundary.xMax),
                0.0f,
                0.0f); // Mathf.Clamp(z, this.boundary.zMin, this.boundary.zMax));

            this.position = newPosition;
        }
    }

    private Vector3 GetPosition()
    {
        WebRequest request = HttpWebRequest.Create(serverUrl + "getposition");
        Debug.Log(request);
        request.Method = "GET";
        request.ContentType = "application/json";
        var response = request.GetResponse();
        Debug.Log(response);
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            string content = reader.ReadToEnd();
            Debug.Log(content);
            var point = JsonUtility.FromJson<Point>(content);

            Vector3 result = new Vector3(point.X, point.Y, point.Z);

            return result;
        }
    }

    private bool GetFire()
    {
        WebRequest request = HttpWebRequest.Create(serverUrl + "getfire");
        request.Method = "GET";
        request.ContentType = "application/json";
        var response = request.GetResponse();
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            string content = reader.ReadToEnd();
            //Debug.Log("4444444444"+content);
            return content == "true";
        }
    }
}
