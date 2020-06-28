using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    [SerializeField] private float SpeedMove = 1;
    
    private bool isMove = false;
    private Vector2 MovementDirection = Vector2.zero;

    private void Start()
    {
        InputControls.Instance.OnMove += Move;
        InputControls.Instance.OnStop += Stop;
    }

    private void OnDestroy()
    {
        if (InputControls.Instance != null)
        {
            InputControls.Instance.OnMove -= Move;
            InputControls.Instance.OnStop -= Stop;
        }
    }

    void Update()
    {
        if (isMove)
        {
            transform.Translate(MovementDirection * SpeedMove * Time.deltaTime, Space.World);
        }
    }

    public void Move(Vector2 direction)
    {
        isMove = true;
        MovementDirection = direction;
    }

    public void Stop()
    {
        isMove = false;
    }
}
