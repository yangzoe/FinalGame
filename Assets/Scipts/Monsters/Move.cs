using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Move : MonoBehaviour
{
    public Vector2 moveDir;

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveDir = new Vector2(horizontal, vertical).normalized;

        if (horizontal != 0 || vertical != 0)
        {
            this.gameObject.transform.position += new Vector3(moveDir.x, moveDir.y, 0) * Time.deltaTime * 5;
        }
    }
}
