using System.Collections.Generic;
using UnityEngine;

public class T2_SceneManager : MonoBehaviour
{
    [Tooltip("�жϸó����Ƿ���SSVEP")]
    public bool IsSSVEP = true;

    [Header("ԭʼ������")]
    [Tooltip("����'Group'ϵ�е�ԭʼGameObject���������ơ�")]
    public GameObject GroupOriginal; // ԭʼ��GameObject������'Group'ϵ��

    const int targetNumber = 11;

    [Header("������������")]
    [Tooltip("����'OtherBall'ϵ�е�GameObject���������ơ�")]
    public GameObject OtherBall; // ����'OtherBall'ϵ�е�GameObject

    [Tooltip("Ҫ������'OtherBall'����������")]
    public int OtherBallNumber = 5; // ��Ҫ������'OtherBall'��������

    [Header("�ֲ�����")]
    [Tooltip("������XYƽ���Ϸֲ�������������Ĵ�С�����Ⱥ͸߶ȣ���")]
    public float DistributionAreaSize = 1400f; // ɢ������Ĵ�С (14m x 14m)

    [Header("�������")]
    [Tooltip("�����������Ƶ�������֮�����С������롣")]
    public float MinSeparationDistance = 2f; // ��С�������

    [Tooltip("�ڷ���֮ǰ��Ϊÿ�������ҵ���Чλ�õ�����Դ�����")]
    public int MaxPlacementAttempts = 100; // ��ֹ����ѭ���ĳ��Դ�������

    [Header("���'Group'������ڵ�����")]
    public float OcclusionIncreaseDistance = 0.1f; // ����GroupX�����ϵ�Task3_Occlusion

    private Vector3 _initialCenterPosition; // ���ڴ洢ϴ��ʱ�����ĵ�
    private bool _hasGenerated = false; // �����ȷ����ʼ���ĵ�ֻ����һ��

    public GameObject eye;

    //the distance from eyes to the certer
    float depthDistance = 100; //���
    float targetVisualWidth_angle = 2f;
    ExpList expList;

    private void Awake()
    {
        expList = new ExpList();
        /*expList.initSceneSetting();
        expList.initCycleHZ(targetNumber);*/
    }

    private void Start()
    {
        GenerateGroups(); // �ڻ���ʱ���ɶ�����
    }

    public static int GetTargetNumber()
    {
        return targetNumber;
    }

    public void GenerateGroups()
    {
        if (GroupOriginal == null)
        {
            Debug.LogError("GroupOriginal δ���䡣�޷����� 'Group' ������");
            // ��ѡ�����GroupOriginalΪ�գ������Ƿ���Ҫ����OtherBalls
            // Ŀǰ�������Ҫģ��ȱʧ�����ǽ����ء�
            return;
        }
        if (OtherBall == null && OtherBallNumber > 0)
        {
            Debug.LogWarning("OtherBall δ���䣬�� OtherBallNumber > 0�����ᴴ�� 'OtherBall' ������");
        }

        if (!_hasGenerated) // ���ڵ�һ������ʱ�洢��ʼ���ĵ�
        {
            T2_Occlusion occulusion = GroupOriginal.GetComponent<T2_Occlusion>();
            if (occulusion)
            {
                Debug.Log("Reloacte the depth");
                float targetActualWidth = sceneUtility.targetActualWidth(
                    targetVisualWidth_angle,
                    occulusion.Ball2.transform,
                    eye.transform
                );
                occulusion.Ball2.transform.localScale *= targetActualWidth;
                occulusion.Ball1.transform.localScale *= targetActualWidth;
                OtherBall.transform.localScale *= targetActualWidth;
                _initialCenterPosition = GroupOriginal.transform.position;
                _hasGenerated = true;
            }
        }

        float halfSize = DistributionAreaSize / 2f; // ɢ�������߳�
        List<Vector3> placedPositions = new List<Vector3>(); // �洢�ѷ��ö����λ��
        int successfullyPlacedCountTotal = 0; // �ɹ����õĶ�������

        if (IsSSVEP)
        {
            // ��ʼ��SSVEP���ã�
            initSSVEP();
        }

        // --- 1. ���� 'Group' ���� ---
        if (GroupOriginal != null && targetNumber > 0)
        {
            for (int i = 0; i < targetNumber; i++)
            {
                // ���Է��ö��󣬲���ȡʵ�����Ķ���
                if (
                    TryPlaceObject(
                        GroupOriginal,
                        "Group" + i,
                        _initialCenterPosition,
                        halfSize,
                        placedPositions,
                        out GameObject newGroupInstance,
                        i
                    )
                )
                {
                    successfullyPlacedCountTotal++;
                    // �ر�Ϊ Group ����Ӧ���ڵ��߼�
                    T2_Occlusion occlusion = newGroupInstance.GetComponent<T2_Occlusion>();
                    if (occlusion != null)
                    {
                        // ����� 'i' �� GroupNumber ��ѭ���������������� 0 �� GroupNumber-1
                        occlusion.TargetOcclusion = OcclusionIncreaseDistance * (i); // ԭʼ�߼�: i+1
                        occlusion.Ball1.name = "Occlude" + i;
                        occlusion.Ball2.name = "Ball" + i;
                        Ball_SSVEP SSVEP = occlusion.Ball2.GetComponent<Ball_SSVEP>();
                        if (SSVEP != null)
                        {
                            SSVEP.Index = i + 1;
                            SSVEP.CycleHz = expList.targetCycleHz[i];
                            SSVEP.PhaseDelay = expList.targetCyclePhasedelay[i];
                        }
                        Ball ball = occlusion.Ball2.GetComponent<Ball>();
                        int index = i + 1;
                        if (ball != null)
                        {
                            ball.Index = index + 1;
                        }
                    }
                    newGroupInstance.transform.LookAt(eye.transform);
                }
                else
                {
                    Debug.LogWarning($"�޷����� Group{(i + 1)}���ܷ�������������������������");
                }
            }
        }

        // --- 2. ���� 'OtherBall' ���� ---
        if (OtherBall != null && OtherBallNumber > 0)
        {
            for (int i = 0; i < OtherBallNumber; i++)
            {
                // ���� OtherBall��TryPlaceObject �����һ������ (objectIndexForLogic) �����䵱ǰ�߼������ϸ���Ҫ
                // ��Ϊ�˱���һ���Ի�δ��ʹ�ã����� 'i'��
                if (
                    TryPlaceObject(
                        OtherBall,
                        "OtherBall" + (i + 1),
                        _initialCenterPosition,
                        halfSize,
                        placedPositions,
                        out GameObject _,
                        i
                    )
                )
                {
                    successfullyPlacedCountTotal++;
                }
                else
                {
                    Debug.LogWarning($"�޷����� OtherBall{i + 1}���ܷ�������������������������");
                }
            }
        }

        // Debug.Log($"������ɡ��ܹ��ɹ����� {successfullyPlacedCountTotal} ������");

        // ���ԭʼģ�岻����Ҫ����ͣ�û�������
        if (GroupOriginal != null && GroupOriginal.scene.IsValid()) // ������Ƿ��ǳ�������
        {
            // GroupOriginal.SetActive(false); // ͣ��ԭʼ����
            Destroy(GroupOriginal); // �����������������ȡ�����е�ע��
        }

        // ���ԭʼģ�岻����Ҫ����ͣ�û�������
        if (OtherBall != null && GroupOriginal.scene.IsValid()) // ������Ƿ��ǳ�������
        {
            // GroupOriginal.SetActive(false); // ͣ��ԭʼ����
            Destroy(GroupOriginal); // �����������������ȡ�����е�ע��
        }
    }

    /// <summary>
    /// ����Ϊ�����ҵ�һ����Чλ�ò�ʵ��������
    /// </summary>
    /// <returns>����ɹ��򷵻�true�����򷵻�false��</returns>
    private bool TryPlaceObject(
        GameObject prefabToInstantiate,
        string objectName,
        Vector3 center,
        float areaHalfSize,
        List<Vector3> existingPositions,
        out GameObject instantiatedObject,
        int objectIndexForLogic
    )
    {
        instantiatedObject = null; // ��ʼ���������
        Vector3 potentialPosition = Vector3.zero; // Ǳ��λ��
        bool positionFound = false; // �Ƿ��ҵ�λ�õı��
        int currentAttempts = 0; // ��ǰ���Դ���

        while (!positionFound && currentAttempts < MaxPlacementAttempts)
        {
            currentAttempts++;
            // ���������X��Y����
            float randomX = Random.Range(center.x - areaHalfSize, center.x + areaHalfSize);
            float randomY = Random.Range(center.y - areaHalfSize, center.y + areaHalfSize);
            float zPosition = center.z; // Z��λ�������ĵ㱣��һ��
            potentialPosition = new Vector3(randomX, randomY, zPosition);

            bool isValidPosition = true; // ��ǰλ���Ƿ���Ч�ı��
            if (MinSeparationDistance > 0) // ������С�������0ʱ�ż��
            {
                foreach (Vector3 placedPos in existingPositions) // ���������ѷ��õ�λ��
                {
                    if (Vector3.Distance(potentialPosition, placedPos) < MinSeparationDistance) // �������С����С���
                    {
                        isValidPosition = false; // ���λ����Ч
                        break; // ����ѭ����������һ����ѡλ��
                    }
                }
            }

            if (isValidPosition) // ���λ����Ч
            {
                positionFound = true; // ���Ϊ���ҵ�
            }
        }

        if (positionFound) // ����ҵ�����Чλ��
        {
            // ��ʵ�����Ķ�����Ϊ��GameObject���Ӷ��󣬷����ڼ��������������λ��
            instantiatedObject = Instantiate(
                prefabToInstantiate,
                potentialPosition,
                prefabToInstantiate.transform.rotation,
                this.transform
            );
            instantiatedObject.name = objectName; // ���ö�������
            existingPositions.Add(potentialPosition); // ����λ�����ӵ��ѷ���λ���б�

            // Debug.Log($"�� {currentAttempts} �γ��Ժ��� {instantiatedObject.transform.position} ������ {instantiatedObject.name}��");
            return true; // ���سɹ�
        }
        else
        {
            // Debug.LogWarning($"�� {MaxPlacementAttempts} �γ��Ժ�δ��Ϊ {objectName} �ҵ���Чλ�á�");
            return false; // ����ʧ��
        }
    }

    /// <summary>
    /// ���Ҵ˽ű�����GameObject������������GameObject��λ�á�
    /// ��������ԭʼ��Z��λ�á�
    /// </summary>
    public void ShuffleChildPositions()
    {
        if (!_hasGenerated) // ����Ƿ������ɹ�������
        {
            // Debug.LogWarning("�������ٵ���һ�� GenerateGroups ������ϴ��ǰ�����ĵ㡣");
            return;
        }

        List<Transform> childrenToShuffle = new List<Transform>(); // ��Ҫ����λ�õ��Ӷ����б�
        foreach (Transform child in transform) // 'transform' ָ���Ǵ˽ű�����GameObject��transform���
        {
            childrenToShuffle.Add(child); // �����Ӷ����б�
        }

        if (childrenToShuffle.Count == 0) // ���û���Ӷ���
        {
            Debug.Log("û���Ӷ�����Դ���λ�á�");
            return;
        }

        Debug.Log($"��ʼ���� {childrenToShuffle.Count} ���Ӷ����λ��...");

        List<Vector3> newPositions = new List<Vector3>(); // �洢����ϴ�Ʋ������ѷ������λ��
        float halfSize = DistributionAreaSize / 2f; // ɢ�������߳�
        int successfullyShuffledCount = 0; // �ɹ�����λ�õ��Ӷ�������

        foreach (Transform childToMove in childrenToShuffle) // ����ÿ����Ҫ�ƶ����Ӷ���
        {
            Vector3 potentialPosition = Vector3.zero; // Ǳ�ڵ���λ��
            bool positionFound = false; // �Ƿ��ҵ���λ�õı��
            int currentAttempts = 0; // ��ǰ���Դ���

            // �����Ӷ���ԭʼ��Z��λ��������λ�õļ���
            float originalZ = childToMove.position.z;

            while (!positionFound && currentAttempts < MaxPlacementAttempts)
            {
                currentAttempts++;
                // �ڶ���������ڼ��������X��Y����
                float randomX = Random.Range(
                    _initialCenterPosition.x - halfSize,
                    _initialCenterPosition.x + halfSize
                );
                float randomY = Random.Range(
                    _initialCenterPosition.y - halfSize,
                    _initialCenterPosition.y + halfSize
                );
                potentialPosition = new Vector3(randomX, randomY, originalZ); // ʹ��ԭʼ��Z��λ��

                bool isValidPosition = true; // ��ǰλ���Ƿ���Ч�ı��
                if (MinSeparationDistance > 0) // ������С�������0ʱ�ż��
                {
                    foreach (Vector3 placedPos in newPositions) // ����뱾��ϴ���������·��õĶ���֮��ľ���
                    {
                        if (Vector3.Distance(potentialPosition, placedPos) < MinSeparationDistance)
                        {
                            isValidPosition = false; // ��λ����Ч
                            break; // ����ѭ��
                        }
                    }
                    // ��ѡ�������ܻ�������Щ��δ�ƶ����Ӷ����ԭʼλ���Ƿ���뵱ǰ�Ӷ������λ�ó�ͻ��
                    // Ϊ��������˰汾����ϴ���ڼ���*�·����*λ�á�
                }

                if (isValidPosition) // ���λ����Ч
                {
                    positionFound = true; // ���Ϊ���ҵ�
                }
            }

            if (positionFound) // ����ҵ�����Ч����λ��
            {
                childToMove.position = potentialPosition; // �����Ӷ����λ��
                newPositions.Add(potentialPosition); // ������λ�����ӵ��б��У����������
                childToMove.LookAt(eye.transform);
                successfullyShuffledCount++; // �ɹ���������
                // Debug.Log($"�� {currentAttempts} �γ��Ժ󣬽� {childToMove.name} �ƶ��� {potentialPosition}��");
            }
            else
            {
                Debug.LogWarning(
                    $"��ϴ�ƹ����У�{MaxPlacementAttempts} �γ��Ժ�δ��Ϊ {childToMove.name} �ҵ��µ���Чλ�á����������ڵ�ǰλ�á�"
                );
            }
        }
        // Debug.Log($"ϴ����ɡ��� {childrenToShuffle.Count} ���Ӷ����У��ɹ����¶�λ�� {successfullyShuffledCount} ����");
    }

    private void initSSVEP()
    {
        expList.initCycleHZ(targetNumber);
    }
}
