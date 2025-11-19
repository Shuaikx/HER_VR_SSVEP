using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum OutlineColor
{
    yellow,
    green,
    red
}

public class Ball : MonoBehaviour {
    public int Index;
    private Material material;
    private Transform trans;
    private Vector3 primitiveScale;
    private Vector3 smallerScale;
    [SerializeField] private Outline outline;
    [SerializeField] private Color colorOrigin = Color.white;
    [SerializeField] private Color colorNotify = Color.blue;


    private bool triggered;
	// Use this for initialization
	void Awake() {
        material = gameObject.GetComponent<Renderer>().material;
        trans = gameObject.GetComponent<Transform>();
        smallerScale = new Vector3(1,1,1);

        triggered = false;
	}

    void Start()
    {
        primitiveScale = trans.lossyScale;
        if(outline == null)
            outline = GetComponent<Outline>();
    }


    public void changeColorToBlue()
    {
        if (material.color != Color.blue)
            material.color = Color.blue;
    }

    public void changeColorToWhite()
    {
        if (material.color != Color.white)
        {
            material.color = Color.white;
        }
    }

    public void changeColorToGreen()
    {
        if (material.color != Color.green && material.color == Color.yellow)
        {
            material.color = Color.green;
            triggered = false;
        }
    }

    public void changeColorToYellow()
    {
        if (material.color != Color.yellow && material.color != Color.yellow)
        {
            material.color = Color.yellow;
            triggered = true;
        }
    }

    public void alterToSmallSize()
    {
        if(trans.localScale != smallerScale)
        {
            trans.localScale = smallerScale;
        }
    }

    public void alterToPrimitiveSize()
    {

        if (trans.localScale != primitiveScale)
        {
            trans.localScale = primitiveScale;
        }
    }

    public bool beTrigger()
    {
        return triggered;
    }

    public void changeOutlineColor(string outlineColor)
    {
        if(outlineColor == OutlineColor.yellow.ToString())
        {
            outline.OutlineColor = Color.yellow;
        }
        else if (outlineColor == OutlineColor.green.ToString())
        {
            outline.OutlineColor = Color.green;
        }
        else if (outlineColor == OutlineColor.red.ToString())
        {
            outline.OutlineColor = Color.red;
        }
        else
        {
            Debug.LogWarning("the color is not listed");
        }
    }

    public void enableOutline(bool state)
    {
        outline.enabled = state;
    }

    public void resetOutline()
    {
        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 5.0f;
        outline.enabled = false;
    }

}
