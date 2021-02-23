﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LineTweener : MonoBehaviour, IObjectTweener
{
    [SerializeField] private float speed;

    public void MoveTo(Transform transform, Vector3 targetPosition)
    {
        float distance = Vector3.Distance(targetPosition, transform.position);
        transform.DOMove(targetPosition, distance / speed);
    }
}
