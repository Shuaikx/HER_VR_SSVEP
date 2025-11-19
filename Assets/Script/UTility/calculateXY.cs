using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class calculateXY : MonoBehaviour
{
    /**
     *  For 3D target modeling angular distance calculation.
     *
     *  Caution!!! This script assumes that the forward looking direction of the user is (0,0,1)
     *      according to the Unity Global Axises. Morover, this could be only used for the Fitt's
     *      Circle experiment. Extra codes are needed when the target appeares at the back of the user.
     */
    public static calculateXY single;

    public calculateXY()
    {
        single = this;
    }

    /*
     *  Transfer the 3D position of the target to the angluar position
     *  Note that for target, the` 'pos' should be 'real position of the target' - 'calibrated eye position'
     *            for camera, it is the forward direction (camera.transform.forward)
     */
    public Vector2 posToVec2(Vector3 pos)
    {
        Vector3 posXZ = new Vector3(pos.x, 0, pos.z);
        float angX = Vector3.Angle(posXZ, new Vector3(0, 0, 1)); //unsigned
        if (pos.x < 0)
        {
            angX = -angX;
        }

        float angY = Vector3.Angle(pos, posXZ); //unsigned
        if (pos.y < 0)
        {
            angY = -angY;
        }
        return new Vector2(angX, angY);
    }

    /*
     *  @Transform start: the angle position (relative to the eye position) of the starting target
     *  @Transform end: the angle position of the end target
     *  @Transform camDir: the angle position of the camera
     *
     *  @Returned Vector2: return the value of X and Y in eularAngle
     */
    public Vector2 cal(Vector2 start, Vector2 end, Vector2 camDir)
    {
        Vector2 endToCam = camDir - end;
        Vector2 newXAxis = end - start;
        Vector2 newYAxis = Vector2.Perpendicular(newXAxis);
        float ansX = Vector2.Dot(endToCam, newXAxis) / newXAxis.magnitude; //unsigned
        float ansY = Vector2.Dot(endToCam, newYAxis) / newYAxis.magnitude; //unsigned
        return new Vector2(ansX, ansY);
    }
}
