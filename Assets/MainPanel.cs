using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    public TMPro.TMP_InputField COM;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        Debug.LogWarning(COM.text);
    }
}
