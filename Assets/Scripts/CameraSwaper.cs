using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraSwaper : MonoBehaviour
{
    public static event Action<CameraControl.CameraType> OnSwapCamera;
    [SerializeField] private CameraControl.CameraType _from, _to;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var desiredType = CameraControl.CurrentType == _from ? _to : _from;
            OnSwapCamera?.Invoke(desiredType);
        }
    }
}
