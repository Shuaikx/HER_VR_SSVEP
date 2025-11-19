using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeBallsController : MonoBehaviour
{
    public void resetBallsOutlineColor()
    {
        gameObject.BroadcastMessage("resetOutline");
    }

    public void disableBallsOutline()
    {
        gameObject.BroadcastMessage("enableOutline", false);
    }
}

