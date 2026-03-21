/*
* Door.cs
* * This script goes on any "Door" object.
* * It has a single public function that can be called by
* * other objects (like a pressure plate) to open it.
*/

using UnityEngine;

public class Door : MonoBehaviour
{
    // This is the public function that other scripts will call
    public void OpenDoor()
    {
        // For now, "opening" just means destroying the door.
        // Later, we could replace this with an animation.
        Destroy(gameObject);
    }
}