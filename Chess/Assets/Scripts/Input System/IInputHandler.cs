using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Based on the input will handle the operation in different ways
/// </summary>
public interface IInputHandler
{
    void ProcessInput(Vector3 inputPosition, GameObject selectedObject, Action callback);
}
