using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChessUIManager : MonoBehaviour
{
    [SerializeField] private GameObject UIParent;
    [SerializeField] private GameObject UIUndo;
    [SerializeField] private Text resultText;

    public void HideUI()
    {
        UIParent.SetActive(false);
        UIUndo.SetActive(false);
    }

    public void onNewMove()
    {
        UIUndo.SetActive(true);
    }

    public void OnGameFinished(string winner)
    {
        UIParent.SetActive(true);
        resultText.text = string.Format("{0} won", winner);
    }
}
