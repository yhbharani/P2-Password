using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    public Button tab1;
    public Button tab2;
    public Button tab3;
    public Button tab4;
    public GameObject Panel1;
    public GameObject Panel2;
    public GameObject Panel3;
    public GameObject Panel4;

    private void Start()
    {
        // Set default tab
        ShowTab1();

        // Add listeners to the buttons
        tab1.onClick.AddListener(ShowTab1);
        tab2.onClick.AddListener(ShowTab2);
        tab3.onClick.AddListener(ShowTab3);
        tab4.onClick.AddListener(ShowTab4);
    }

    void ShowTab1()
    {
        Panel1.SetActive(true);
        Panel2.SetActive(false);
        Panel3.SetActive(false);
        Panel4.SetActive(false);

        SetTabActive(tab1);
        SetTabInactive(tab2);
        SetTabInactive(tab3);
        SetTabInactive(tab4);
    }

    void ShowTab2()
    {
        Panel1.SetActive(false);
        Panel2.SetActive(true);
        Panel3.SetActive(false);
        Panel4.SetActive(false);

        SetTabInactive(tab1);
        SetTabActive(tab2);
        SetTabInactive(tab3);
        SetTabInactive(tab4);
    }

    void ShowTab3()
    {
        Panel1.SetActive(false);
        Panel2.SetActive(false);
        Panel3.SetActive(true);
        Panel4.SetActive(false);

        SetTabInactive(tab1);
        SetTabInactive(tab2);
        SetTabActive(tab3);
        SetTabInactive(tab4);
    }

    void ShowTab4()
    {
        Panel1.SetActive(false);
        Panel2.SetActive(false);
        Panel3.SetActive(false);
        Panel4.SetActive(true);

        SetTabInactive(tab1);
        SetTabInactive(tab2);
        SetTabInactive(tab3);
        SetTabActive(tab4);
    }

    void SetTabActive(Button tab)
    {
        TextMeshProUGUI text = tab.GetComponentInChildren<TextMeshProUGUI>();
        Image underline = tab.transform.GetChild(1).GetComponent<Image>(); // Assuming the underline is the second child

        if (text) text.alpha = 1f; // 100% alpha
        if (underline) underline.enabled = true;
    }

    void SetTabInactive(Button tab)
    {
        TextMeshProUGUI text = tab.GetComponentInChildren<TextMeshProUGUI>();
        Image underline = tab.transform.GetChild(1).GetComponent<Image>(); // Assuming the underline is the second child

        if (text) text.alpha = 0.5f; // 50% alpha
        if (underline) underline.enabled = false;
    }
}
