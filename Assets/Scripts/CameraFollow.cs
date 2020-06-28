using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform Target;
    [SerializeField] private float SpeedMove = 2;

    private void Start()
    {
        if (Target == null)
            Target = PlayerController.Instance.transform;
    }

    void Update()
    {
        if (Target != null)
        {
            Vector2 _position = Vector2.Lerp(transform.position, Target.position, SpeedMove * Time.deltaTime);
            transform.position = new Vector3(_position.x, _position.y, transform.position.z);
        }
    }
}
