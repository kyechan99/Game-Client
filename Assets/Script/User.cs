using UnityEngine;
using System.Collections;

public enum MOVE_CONTROL
{
    STOP,
    DANCE,
    UP,
    DOWN,
    LEFT,
    RIGHT,
}

public class User : MonoBehaviour
{
    public TextMesh nickName;
    public TextMesh chatText;

    public bool isPlayer = false;
    public Vector3 pos;

    public MOVE_CONTROL myMove = MOVE_CONTROL.STOP;
    public MOVE_CONTROL beforeMove = MOVE_CONTROL.STOP;

    void Update()
    {
        if (isPlayer)
        {
            if (Input.GetKey(KeyCode.UpArrow)) myMove = MOVE_CONTROL.UP;
            else if (Input.GetKey(KeyCode.LeftArrow)) myMove = MOVE_CONTROL.LEFT;
            else if (Input.GetKey(KeyCode.RightArrow)) myMove = MOVE_CONTROL.RIGHT;
            else if (Input.GetKey(KeyCode.DownArrow)) myMove = MOVE_CONTROL.DOWN;
            else myMove = MOVE_CONTROL.STOP;

            if (myMove != beforeMove)
            {
                NetworkManager.getInstance.SendMsg(string.Format("MOVE:{0}:{1}:{2}", transform.position.x, transform.position.y, (int)myMove));
                beforeMove = myMove;
            }
        }

        if (myMove == MOVE_CONTROL.UP)
        {
            //anim.Play("UP");
            transform.Translate(Vector3.up * 4f * Time.deltaTime);
        }
        else if (myMove == MOVE_CONTROL.DOWN)
        {
            //anim.Play("DOWN");
            transform.Translate(Vector3.down * 4f * Time.deltaTime);
        }
        else if (myMove == MOVE_CONTROL.LEFT)
        {
            //anim.Play("LEFT");
            transform.Translate(Vector3.left * 4f * Time.deltaTime);
        }
        else if (myMove == MOVE_CONTROL.RIGHT)
        {
            //anim.Play("RIGHT");
            transform.Translate(Vector3.right * 4f * Time.deltaTime);
        }
    }

    /**
     * @brief 바라볼 방향 설정
     * @param direction 방향 
     */
    public void setDirection(MOVE_CONTROL direction)
    {
        //
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
