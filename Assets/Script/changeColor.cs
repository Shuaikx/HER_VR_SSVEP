using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class changeColor : MonoBehaviour {
    Image image;

    public void changeToRed()
    {
        image = transform.GetComponent<Image>();
        image.color = Color.red;
    }

    public void changeToGreen()
    {
        image = transform.GetComponent<Image>();
        image.color = Color.green;
    }

    public void changeToWhite()
    {
        image = transform.GetComponent<Image>();
        image.color = Color.white;
    }

}
