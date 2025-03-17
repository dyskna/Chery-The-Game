using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class Collector : MonoBehaviour
{
    public int cherry = 0 ;
    [SerializeField] Text cherriesText;

   public void CherryCollector()
   {
    cherry = Random.Range(3,7) + cherry;

    Debug.Log("Cherries: "+ cherry);
    cherriesText.text = "Wisnie: " + cherry;
    //var msgObject = Instantiate(_messagePrefab, transform.position, Quaternion.identity);
    //msgObject.GetComponentInChildren<TMP_Text>().SetText("Wisienki "+cherry);
   }
}
