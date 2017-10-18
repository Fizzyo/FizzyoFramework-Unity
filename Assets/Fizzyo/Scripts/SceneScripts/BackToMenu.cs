using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class BackToMenu : MonoBehaviour {

    public Button back;

    void Start()
    {
        Button backBtn = back.GetComponent<Button>();
        backBtn.onClick.AddListener(BackClick);

    }

    void BackClick()
    {
        SceneManager.LoadScene("Menu");
    }
}
