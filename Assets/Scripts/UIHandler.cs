using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIHandler : MonoBehaviour
{
    [SerializeField] private Button buttonRollDices;

    public static UIHandler Instance { get; private set; }

    private void Awake() => Instance = this;

    public void ShowButtonRollDices()
    {
        this.buttonRollDices.interactable = true;
        this.buttonRollDices.gameObject.SetActive(true);
    }

    public void HideButtonRollDices()
    {
        this.buttonRollDices.interactable = false;
        this.buttonRollDices.gameObject.SetActive(false);
    }
}
