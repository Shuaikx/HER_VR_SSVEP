using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Don't listen in update, directly write the logic to jump to next target into a function, trigger after completing logic.
/// </summary>

public class aimTarget_Gaze_t2 : aimTarget_Gaze
{
    public T2_SceneManager sceneManager;

    enum GazeStatus
    {
        Practice,
        Formal,
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        //base.cursorDistance = 70f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void clabiratedAndStart()
    {
        base.clabiratedAndStart();
        sceneManager.ShuffleChildPositions();

    }

    public override void updateScene()
    {
        if (base.updating)
        {
            Debug.Log("T2 Updating：" + updating);
            sceneManager.ShuffleChildPositions();
            updating = false;
        }
        //sceneManager.ShuffleChildPositions();
    }
}
