using UnityEngine;
using UnityEngine.UI;

public class UIToggleController : MonoBehaviour
{
    public Toggle toggle;
    public GameObject imageObject;
    public GameObject buttonObject;

    public GameObject shop;

    public GameObject wavetext;

    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    public void OnToggleChanged(bool isOn)
    {
        Debug.Log("토글 변경: " + isOn);

        imageObject.SetActive(isOn);
        buttonObject.SetActive(isOn);
        shop.SetActive(isOn);
        wavetext.SetActive(isOn);
    }
}