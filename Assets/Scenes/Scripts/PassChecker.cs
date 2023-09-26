using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class PassChecker : MonoBehaviour
{
    public DotGrid dotGridScript; // Reference to the DotGrid script
    public GameObject statusPanel; // Reference to the panel
    private TextMeshProUGUI statusText; // Reference to the TextMeshProUGUI component
    private string originalMessage = "Draw your pattern";
    private string presetPassword = "0_2,1_1,1_2,1_1,1_0,2_0,1_1,2_2,1_2"; // Stored password pattern

    private void Start()
    {
        // Subscribe to the OnDrawingComplete event
        dotGridScript.OnDrawingComplete.AddListener(CheckPassword);

        // Get the TextMeshProUGUI component from the child of the panel
        statusText = statusPanel.GetComponentInChildren<TextMeshProUGUI>();
        statusText.text = originalMessage;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to prevent memory leaks
        dotGridScript.OnDrawingComplete.RemoveListener(CheckPassword);
    }

    private void CheckPassword()
    {
        string inputPassword = string.Join(",", dotGridScript.visitedDotNames);
        if (inputPassword == presetPassword)
        {
            Debug.Log("Password is correct! HOORAYYY!!!");
            StartCoroutine(ShowMessage("Correct Password!", "#5EFFB1", "#0054A9"));
        }
        else
        {
            Debug.Log("Password is incorrect.:( Try Again");
            StartCoroutine(ShowMessage("Incorrect Password", "#FF7575", "#FFFFFF"));
        }
    }

    private IEnumerator ShowMessage(string message, string panelHexColor, string textHexColor)
    {
        statusText.text = message;

        Color panelColor;
        if (ColorUtility.TryParseHtmlString(panelHexColor, out panelColor))
        {
            if (statusPanel != null && statusPanel.GetComponent<Image>() != null)
            {
                statusPanel.GetComponent<Image>().color = panelColor;
            }
            else
            {
                Debug.LogWarning("statusPanel or Image component on statusPanel is null!");
            }
        }
        else
        {
            Debug.LogError("Invalid hex color for panel: " + panelHexColor);
        }

        if (!string.IsNullOrEmpty(textHexColor))
        {
            Color textColor;
            if (ColorUtility.TryParseHtmlString(textHexColor, out textColor))
            {
                statusText.color = textColor;
            }
            else
            {
                Debug.LogError("Invalid hex color for text: " + textHexColor);
            }
        }

        yield return new WaitForSeconds(2f);

        statusText.text = originalMessage;
        if (statusPanel != null && statusPanel.GetComponent<Image>() != null)
        {
            statusPanel.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f); // Reset panel color to transparent white
        }
        statusText.color = Color.white; // Reset text color to white
    }
}
