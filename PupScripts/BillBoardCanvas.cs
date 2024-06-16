using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoardCanvas : MonoBehaviour
{
    void Start()
    {

    }
    void Update()
    {
        transform.LookAt(Camera.main.transform);
    }
}
