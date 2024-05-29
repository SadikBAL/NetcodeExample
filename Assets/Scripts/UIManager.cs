using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject GameMenu;
    public Button StartHostButton;
    public Button JoinHostButton;
    private void Awake()
    {
        GameMenu.SetActive(true);
        StartHostButton.onClick.AddListener(() =>
        {
            GameMenu.SetActive(false);
            NetworkManager.Singleton.StartHost();
        });
        JoinHostButton.onClick.AddListener(() => 
        {
            GameMenu.SetActive(false);
            NetworkManager.Singleton.StartClient();
        });
    }
}
