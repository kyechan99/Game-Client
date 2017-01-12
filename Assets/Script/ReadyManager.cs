using UnityEngine;
using System.Collections;

namespace GM
{
    public class ReadyManager : MonoBehaviour
    {
        [SerializeField]
        UnityEngine.UI.Text roomIdx;
        
        void Awake()
        {
            NetworkManager.getInstance.SendMsg(string.Format("LOGIN:{0}", NetworkManager.getInstance.nickName));
        }

        void Start()
        {
            roomIdx.text = NetworkManager.getInstance.myRoom + "";
        }
    }
}