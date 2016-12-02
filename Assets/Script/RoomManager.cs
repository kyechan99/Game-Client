using UnityEngine;
using System.Collections;

namespace GM
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField]
        UnityEngine.UI.Text roomNameTxt;        // 방 생성시 이름
        [SerializeField]
        UnityEngine.UI.Text roomPWTxt;          // 방 비밀번호 이름

        [SerializeField]
        GameObject roomPrefabBT;                // 방 찾기시 생성될 방 버튼들
        [SerializeField]
        GameObject foundRoomList;               // 발견된 방 리스트들
        [SerializeField]
        GameObject checkPWpop;                  // 비밀번호 체크 팝업

        //[HideInInspector]
        public string roomPW;
        //[HideInInspector]
        public string inputPW;

        void Start()
        {
            roomPW = "";
            inputPW = "";
            NetworkManager.getInstance._roomGM = this;
        }

        /**
         * @brief 방 생성
         */
        public void createRoom()
        {
            NetworkManager.getInstance.SendMsg(string.Format("CREATE_ROOM:{0}:{1}", roomNameTxt.text, roomPWTxt.text));
        }

        /**
         * @brief 방 입장
         */
        public void intoRoom()
        {
            Debug.Log("DDD");
            NetworkManager.getInstance.SendMsg("INTO_ROOM");
        }

        /**
         * @brief 방 찾기
         */
        public void foundRoom()
        {
            NetworkManager.getInstance.SendMsg("FOUND_ROOM");
        }

        /**
         * @brief 방 입장 게이트 생성
         */
        public void makeGate(string roomIdx, string roomName, string roomPW)
        {
            GameObject go = Instantiate(roomPrefabBT) as GameObject;
            go.GetComponentInChildren<UnityEngine.UI.Text>().text = string.Format("{0}:{1}:{2}", roomIdx, roomName, roomPW);

            AddListener(go.GetComponent<UnityEngine.UI.Button>(), roomPW);
            go.transform.SetParent(foundRoomList.transform);
            go.transform.localScale = Vector2.one;
        }

        void AddListener(UnityEngine.UI.Button b, string roomPW)
        {
            b.onClick.AddListener(() => checkRoomPW(roomPW));
        }

        /**
         * @brief 방 암호 확인
         * @param roomPW 방 암호
         */
        public void checkRoomPW(string roomPW)
        {
            // 암호가 존재하는 방
            if (!roomPW.Equals(""))
            {
                checkPWpop.SetActive(true);
                this.roomPW = roomPW;
            }
            else
            {
                intoRoom();
            }
        }

        /**
         * @brief 입력한 비밀번호 확인
         * @param inputPW 입력한 비밀번호
         */
        public void checkPWCorrect(UnityEngine.UI.Text inputPW)
        {
            // 방 암호와 내가 입력한 암호가 일치 할 때
            if (this.roomPW.Equals(inputPW.text))
            {
                this.inputPW = "";
                this.roomPW = "";
                intoRoom();
            }
        }

        /**
         * @brief 입력하는 비밀번호 변경
         * @param inputPW 입력한 비밀번호
         */
        public void setPassword(string inputPW)
        {
            this.inputPW = inputPW;
        }

        /**
         * @brief 방 게이트 새로고침
         */
        public void refreshRoomList()
        {
            for (int i = 0; i < foundRoomList.transform.childCount; i++)
            {
                Destroy(foundRoomList.transform.GetChild(i).gameObject);
            }
        }
    }
}