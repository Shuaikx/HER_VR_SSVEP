using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modality : MonoBehaviour
{
    [System.Serializable]
    public enum ModalityState
    {
        airTap,
        dwell,
        blink,
        SSVEP,
    };

    [System.Serializable]
    public enum VisibilityState
    {
        visible,
        invisible,
    };

    [SerializeField]
    private int PersonID;

    [SerializeField]
    private int TrialCount = 50;

    [SerializeField]
    private ModalityState currentState;

    [SerializeField]
    private VisibilityState visibleState;

    public int GetPersonID()
    {
        return PersonID;
    }

    public int GetTrialCount()
    {
        return TrialCount;
    }

    public ModalityState GetCurrentState()
    {
        return currentState;
    }

    public VisibilityState GetVisibleState()
    {
        return visibleState;
    }
}
