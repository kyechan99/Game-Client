using UnityEngine;
using System.Collections;

public class MainManager : MonoBehaviour
{
    [SerializeField]
    private GameObject nowLoadingWindow;

    void Start()
    {
        GM.NetworkManager.getInstance.nowLoadingWindow = nowLoadingWindow;
    }
}
