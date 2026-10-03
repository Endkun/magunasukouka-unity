using System;
using JetBrains.Annotations;
using UnityEngine;

public class move : MonoBehaviour
{
    public Vector2 velocity = new Vector2(0.2f, 0.05f); //pvx,pvyでx,y方向速度
    public float ay = 0.002f; //重力加速度
    public float ka = 0.001f; //回転力(正でトップスピン、負でバックスピン)
    public float kt = 0.05f; //(空気抵抗の係数)
    public float ko = 0.1f; //(空気の密度)
    public float angle = 0f; //回転の速度(回転力に掛ける)
    public Transform Square;
    public 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = new Vector3(-8f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        float dragX = 0.5f * ko * kt * velocity.x * Mathf.Abs(velocity.x);
        float dragY = 0.5f * ko * kt * velocity.y * Mathf.Abs(velocity.y);
        velocity.x -= dragX;
        velocity.y -= dragY;
        float pay = ay + ka;
        float vy = -velocity.y;
        velocity.y += pay * Time.deltaTime * 60f;
        transform.position += new Vector3(velocity.x / 60f,vy/60f,0f);
        angle -= ka * 50f;
        float rad = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad);
        float y = Mathf.Sin(rad);
        Square.localPosition = new Vector3(x/3,y/3,-1f);
        //transform.rotation = Quaternion.Euler(0f, 0f, angle);

    }
}
