using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class FarmerAI : MonoBehaviour, INPC
{

    public void ProcessTag(string tag)
    {
        switch (tag)
        {
            case "Funny":
                Funny();
                break;
            
            case "CherryToBox":
                CherryToBox();
                break;

            default:
                Debug.Log("Unknown tag for farmer: " + tag);
                break;
        }
    }

    private void Funny()
    {
        Debug.Log("in the future");
        
    }

    private void CherryToBox()
    {
        Debug.Log("works");
    }
    
}




