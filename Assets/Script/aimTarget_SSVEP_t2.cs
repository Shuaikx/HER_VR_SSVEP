using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aimTarget_SSVEP_t2 : aimTarget_SSEVEP
{
    public T2_SceneManager sM_2;
    /*protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }*/

    protected override void UpdateScene()
    {
        sM_2.ShuffleChildPositions();
    }
}
