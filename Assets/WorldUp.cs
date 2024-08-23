using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUp : MonoBehaviour
{
    private Quaternion _originalRotation;
    // Start is called before the first frame update
    void Start()
    {
        _originalRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = _originalRotation;
    }
}
