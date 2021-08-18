
using UnityEngine;

public class Movement : MonoBehaviour
{
    float sizeMin = 2f;
    float sizeMax = 200f;

    Camera cam;

    Vector3 prevWorldPos, curWorldPos, worldPosDelta;

    // Use this for initialization
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        //Handles mouse scroll wheel input to modify the zoom of the camera
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - Input.mouseScrollDelta.y, sizeMin, sizeMax);

        //Handles middle mouse button panning.
        //Sets the previous vectors to what is current because we are restarting the panning chain of events.
        if (Input.GetMouseButtonDown(0))
        {
            prevWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            //Gets the delta of the worldPos and mousePos
            worldPosDelta = cam.ScreenToWorldPoint(Input.mousePosition) - prevWorldPos;

            cam.transform.position = new Vector3(cam.transform.position.x - worldPosDelta.x, cam.transform.position.y - worldPosDelta.y, cam.transform.position.z);

            //Set previous variables to current state for use in next frame
            prevWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        }
    }
}
