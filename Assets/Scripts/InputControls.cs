using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputControls : Singleton<InputControls>
{
    private const float MOVEMENT_OFFSET = 0.2f;
    
    private Vector2 previousMoveDirection = Vector2.zero;

    public Action<Vector2> OnMove;
    public Action OnStop;
    
    void Update()
    {
        Vector2 _moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (!EqualVectors(_moveDirection, previousMoveDirection))
        {
            previousMoveDirection = _moveDirection;
            
            if (_moveDirection.sqrMagnitude > MOVEMENT_OFFSET)
                OnMove?.Invoke(_moveDirection);
            else 
                OnStop?.Invoke();
        }
    }

    private bool EqualVectors(Vector2 a, Vector2 b)
    {
        float d = Vector3.SqrMagnitude(a - b);
        return !(d > MOVEMENT_OFFSET);
    }
}
