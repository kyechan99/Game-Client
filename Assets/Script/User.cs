using UnityEngine;
using System.Collections;

public class User : MonoBehaviour
{
    public TextMesh nickName;
    public TextMesh chatText;

    public bool isPlayer = false;
    public Vector3 pos;

    void Update()
    {

    }


    /**
     * @brief 채팅한 내용을 보여줌
     * @param text 내용 
     */
    public void SetChatText(string text)
    {
        CancelInvoke("HideChat");
        chatText.text = text;
        Invoke("HideChat", Mathf.Max(5f, text.Length / 10));
    }

    /**
     * @brief 채팅 숨기기 
     */
    void HideChat()
    {
        chatText.text = "";
    }
}
