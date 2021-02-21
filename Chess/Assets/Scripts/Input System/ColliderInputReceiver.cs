using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderInputReceiver : InputReceiver
{
    private Vector3 clickPosition;
    public override void OnInputReceived()
    {
        //get all the handlers and find the right one to handle board click
        foreach (var handler in inputHandlers)
        {
            handler.ProcessInput(clickPosition, null, null);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if mouse clicked then shoot ray to click position
        if(Input.GetMouseButtonDown(0)) //left mouse button
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log("Click position: " + clickPosition);
                clickPosition = hit.point;
                OnInputReceived(); //send to BoardInputHandler
            }
        }
    }
}
