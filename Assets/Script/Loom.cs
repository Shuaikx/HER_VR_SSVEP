using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Loom : MonoBehaviour
{
    //�Ƿ��Ѿ���ʼ��
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

    //��ʼ��
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

    //����ִ�е�Ԫ�����ӳ٣�
    struct NoDelayedQueueItem
    {
        public Action<object> action;
        public object param;
    }

    //ȫ��ִ���б������ӳ٣�
    List<NoDelayedQueueItem> listNoDelayActions = new List<NoDelayedQueueItem>();

    //����ִ�е�Ԫ�����ӳ٣�
    struct DelayedQueueItem
    {
        public Action<object> action;
        public object param;
        public float time;
    }

    //ȫ��ִ���б������ӳ٣�
    List<DelayedQueueItem> listDelayedActions = new List<DelayedQueueItem>();

    //���뵽���߳�ִ�ж��У����ӳ٣�
    public static void QueueOnMainThread(Action<object> taction, object param)
    {
        QueueOnMainThread(taction, param, 0f);
    }

    //���뵽���߳�ִ�ж��У����ӳ٣�
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

    //��ǰִ�е�����ʱ������
    List<NoDelayedQueueItem> currentActions = new List<NoDelayedQueueItem>();

    //��ǰִ�е�����ʱ������
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
