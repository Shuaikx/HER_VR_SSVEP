using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class IOManager : MonoBehaviour
{
    public Modality modalitySelection;
    private static int personID;
    private static int trialCount;
    private static string path = @"E:/VR_SSVEP/VR_SSVEP_Data";
    private static string pathPeople = path + "ModelingNum.txt";
    private static string pathOutput;
    private static string pathOutput_dwell;
    private static string pathOutput_SSVEP;
    public static Modality.ModalityState stateCM;
    public static Modality.VisibilityState stateVisible;

    private void Awake()
    {
        personID = modalitySelection.GetPersonID();
        stateCM = modalitySelection.GetCurrentState();
        stateVisible = modalitySelection.GetVisibleState();
        trialCount = modalitySelection.GetTrialCount();
    }

    void Start()
    {
        Debug.Log("Address: " + path);
        Debug.Log("Person ID: " + personID.ToString());
        Debug.Log("Current State: " + stateCM);
        Debug.Log("Visible State: " + stateVisible);
    }

    public static int GetPersonID()
    {
        return personID;
    }

    public static int GetTrialCount()
    {
        return trialCount;
    }

    public static void createUser()
    {
        /* Automatically Output the Results in Counting Order*/
        pathOutput =
            path
            + "/Recording"
            + "-"
            + personID.ToString()
            + "-"
            + stateCM
            + "-"
            + stateVisible
            + ".txt";
        using (StreamWriter sw = File.CreateText(pathOutput))
        {
            sw.WriteLine(
                "PersonID"
                    + "\t"
                    + "x"
                    + "\t"
                    + "y"
                    + "\t"
                    + "Distance"
                    + "\t"
                    + "Time"
                    + "\t"
                    + "Width"
                    + "\t"
                    + "Amplitude"
                    + "\t"
                    + "preName"
                    + "\t"
                    + "CurrentName"
                    + "\t"
                    + "ImputMethods"
                    + "\t"
                    + "pointerState"
                    + "\t"
                    + "reRenterTimes"
            );
        }
    }

    public static void createUser_dwell()
    {
        /* Automatically Output the Results in Counting Order*/
        pathOutput_dwell =
            path + "Recording" + "-" + personID + "-" + stateCM + "-" + stateVisible + ".txt";
        using (StreamWriter sw = File.CreateText(pathOutput_dwell))
        {
            sw.WriteLine(
                "PersonID"
                    + "\t"
                    + "ave_Distance"
                    + "\t"
                    + "std_Distance"
                    + "\t"
                    + "Time"
                    + "\t"
                    + "Width"
                    + "\t"
                    + "Amplitude"
                    + "\t"
                    + "preName"
                    + "\t"
                    + "CurrentName"
                    + "\t"
                    + "ImputMethods"
                    + "\t"
                    + "pointerState"
                    + "\t"
                    + "reRenterTimes"
            );
        }
    }

    public static void createUser_SSVEP() //还需要确定记录格式
    {
        /* Automatically Output the Results in Counting Order*/
        pathOutput_SSVEP =
            path + "Recording" + "-" + personID + "-" + stateCM + "-" + stateVisible + ".txt";
        using (StreamWriter sw = File.CreateText(pathOutput_SSVEP))
        {
            sw.WriteLine(
                "PersonID"
                    + "\t"
                    + "ave_Distance"
                    + "\t"
                    + "std_Distance"
                    + "\t"
                    + "Time"
                    + "\t"
                    + "Width"
                    + "\t"
                    + "Amplitude"
                    + "\t"
                    + "preName"
                    + "\t"
                    + "CurrentName"
                    + "\t"
                    + "ImputMethods"
                    + "\t"
                    + "pointerState"
                    + "\t"
                    + "reRenterTimes"
            );
        }
    }

    /*In endScene*/
    public static void finishExperiment() { }

    /*WirteResult*/
    public static void writeResult(
        Vector2 xy,
        float time,
        float distance,
        float width,
        float Amplitude,
        string preName,
        string currentName,
        int reRenterTime
    )
    {
        if (preName.Length > 4)
            preName = preName.Remove(0, 4);
        currentName = currentName.Remove(0, 4);
        using (StreamWriter sw = File.AppendText(pathOutput))
        {
            // Debug.Log(getPersonID().ToString() + '\t' + xy.x.ToString() + '\t' + xy.y.ToString() + '\t' +  distance.ToString() + '\t' +  time.ToString() + '\t' + width.ToString() + '\t'
            //          + Amplitude + '\t' + preName + '\t' + currentName  + '\t' +  stateCM + '\t' +  stateVisible);
            sw.WriteLine(
                personID.ToString()
                    + '\t'
                    + xy.x.ToString()
                    + '\t'
                    + xy.y.ToString()
                    + '\t'
                    + distance.ToString()
                    + '\t'
                    + time.ToString()
                    + '\t'
                    + width.ToString()
                    + '\t'
                    + Amplitude
                    + '\t'
                    + preName
                    + '\t'
                    + currentName
                    + '\t'
                    + stateCM
                    + '\t'
                    + stateVisible
                    + '\t'
                    + reRenterTime
            );
        }
    }

    public static void writeResult_dwell(
        float time,
        float ave_distance,
        float std_distance,
        float width,
        float Amplitude,
        string preName,
        string currentName,
        int reRenterTime
    )
    {
        if (preName.Length > 4)
            preName = preName.Remove(0, 4);
        currentName = currentName.Remove(0, 4);
        using (StreamWriter sw = File.AppendText(pathOutput_dwell))
        {
            // Debug.Log(getPersonID().ToString() + '\t' + xy.x.ToString() + '\t' + xy.y.ToString() + '\t' +  distance.ToString() + '\t' +  time.ToString() + '\t' + width.ToString() + '\t'
            //          + Amplitude + '\t' + preName + '\t' + currentName  + '\t' +  stateCM + '\t' +  stateVisible);
            sw.WriteLine(
                personID.ToString()
                    + '\t'
                    + ave_distance.ToString()
                    + '\t'
                    + std_distance.ToString()
                    + '\t'
                    + time.ToString()
                    + '\t'
                    + width.ToString()
                    + '\t'
                    + Amplitude
                    + '\t'
                    + preName
                    + '\t'
                    + currentName
                    + '\t'
                    + stateCM
                    + '\t'
                    + stateVisible
                    + '\t'
                    + reRenterTime
            );
        }
    }

    public static void writeResult_SSVEP(
        float time,
        float ave_distance,
        float std_distance,
        float width,
        float Amplitude,
        string preName,
        string currentName,
        int reRenterTime
    )
    {
        if (preName.Length > 4)
            preName = preName.Remove(0, 4);
        currentName = currentName.Remove(0, 4);
        using (StreamWriter sw = File.AppendText(pathOutput_SSVEP))
        {
            // Debug.Log(getPersonID().ToString() + '\t' + xy.x.ToString() + '\t' + xy.y.ToString() + '\t' +  distance.ToString() + '\t' +  time.ToString() + '\t' + width.ToString() + '\t'
            //          + Amplitude + '\t' + preName + '\t' + currentName  + '\t' +  stateCM + '\t' +  stateVisible);
            sw.WriteLine(
                personID.ToString()
                    + '\t'
                    + ave_distance.ToString()
                    + '\t'
                    + std_distance.ToString()
                    + '\t'
                    + time.ToString()
                    + '\t'
                    + width.ToString()
                    + '\t'
                    + Amplitude
                    + '\t'
                    + preName
                    + '\t'
                    + currentName
                    + '\t'
                    + stateCM
                    + '\t'
                    + stateVisible
                    + '\t'
                    + reRenterTime
            );
        }
    }
}
