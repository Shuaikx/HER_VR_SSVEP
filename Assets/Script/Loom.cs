using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Loom : MonoBehaviour
{
    // Whether already initialized
    static bool isInitialized;

    private static Loom _ins;
    public static Loom ins
    {
        get
        {
            Initialize();
            return _ins;
        }
    }

    void Awake()
    {
        _ins = this;
        isInitialized = true;
    }

    // Initialize
    public static void Initialize()
    {
        if (!isInitialized)
        {
            if (!Application.isPlaying)
                return;

            isInitialized = true;
            var obj = new GameObject("Loom");
            _ins = obj.AddComponent<Loom>();

            DontDestroyOnLoad(obj);
        }
    }

    // Struct for elements to execute (no delay)
    struct NoDelayedQueueItem
    {
        public Action<object> action;
        public object param;
    }

    // Global execution list (no delay)
    List<NoDelayedQueueItem> listNoDelayActions = new List<NoDelayedQueueItem>();

    // Struct for elements to execute (with delay)
    struct DelayedQueueItem
    {
        public Action<object> action;
        public object param;
        public float time;
    }

    // Global execution list (with delay)
    List<DelayedQueueItem> listDelayedActions = new List<DelayedQueueItem>();

    // Queue to main thread execution queue (no delay)
    public static void QueueOnMainThread(Action<object> taction, object param)
    {
        QueueOnMainThread(taction, param, 0f);
    }

    // Queue to main thread execution queue (with delay)
    public static void QueueOnMainThread(Action<object> action, object param, float time)
    {
        if (time != 0)
        {
            lock (ins.listDelayedActions)
            {
                ins.listDelayedActions.Add(
                    new DelayedQueueItem
                    {
                        time = Time.time + time,
                        action = action,
                        param = param,
                    }
                );
            }
        }
        else
        {
            lock (ins.listNoDelayActions)
            {
                ins.listNoDelayActions.Add(
                    new NoDelayedQueueItem { action = action, param = param }
                );
            }
        }
    }

    // Currently executing non-delayed list
    List<NoDelayedQueueItem> currentActions = new List<NoDelayedQueueItem>();

    // Currently executing delayed list
    List<DelayedQueueItem> currentDelayed = new List<DelayedQueueItem>();

    void Update()
    {
        if (listNoDelayActions.Count > 0)
        {
            lock (listNoDelayActions)
            {
                currentActions.Clear();
                currentActions.AddRange(listNoDelayActions);
                listNoDelayActions.Clear();
            }
            for (int i = 0; i < currentActions.Count; i++)
            {
                currentActions[i].action(currentActions[i].param);
            }
        }

        if (listDelayedActions.Count > 0)
        {
            lock (listDelayedActions)
            {
                currentDelayed.Clear();
                currentDelayed.AddRange(listDelayedActions.Where(d => Time.time >= d.time));
                for (int i = 0; i < currentDelayed.Count; i++)
                {
                    listDelayedActions.Remove(currentDelayed[i]);
                }
            }

            for (int i = 0; i < currentDelayed.Count; i++)
            {
                currentDelayed[i].action(currentDelayed[i].param);
            }
        }
    }

    void OnDisable()
    {
        if (_ins == this)
        {
            _ins = null;
        }
    }
}
