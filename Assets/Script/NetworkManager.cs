using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System;
using UnityEngine.UI;

/** ********** 사전 세팅 *****************************************************************
  * Player Settings > Resolution and Presentation > Run in Background > true
  *
  * 
  */

public class NetworkManager : MonoBehaviour
{
    static Socket socket = null;
    string address = "192.168.0.5";   // 주소, 서버 주소와 같게 할 것
    int port = 10000;               // 포트 번호, 서버포트와 같게 할 것
    byte[] buf = new byte[4096];
    int recvLen = 0;

    static NetworkManager _instance;
    public static NetworkManager getInstance
    {
        get
        {
            return _instance;
        }
    }

    public InputField nickName;
    List<User> v_user = new List<User>();

    public GameObject playerPrefs;

    void Start()
    {
        _instance = this;
    }

    /**
     * @brief 서버에 접속 
     */
    public void Login()
    {
        if (checkNetwork())
        {
            Logout();       // 이중 접속 방지

            IPAddress serverIP = IPAddress.Parse(address);
            int serverPort = Convert.ToInt32(port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 10000);      // 송신 제한시간 10초
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);   // 수신 제한시간 10초

            // 서버가 닫혀 있을것을 대비하여 예외처리
            try
            {
                socket.Connect(new IPEndPoint(serverIP, serverPort));
            }
            catch (SocketException err)
            {
                Debug.Log("서버가 닫혀있습니다.");
            }
            catch (Exception ex)
            {
                Debug.Log("ERROR 개반자에게 문의");
            }

            StartCoroutine(PacketProc());
        }
    }

    /**
     * @brief 접속 종료 
     */
    public void Logout()
    {
        if (socket != null && socket.Connected)
            socket.Close();
        StopCoroutine(PacketProc());
    }

    /**
     * @brief 채팅
     * @param input 내용
     */
    public void Chat(InputField input)
    {
        SendMsg(string.Format("CHAT:{0}", input.text));
    }

    /**
     * @brief 접속 종료 
     * @param pos 생성 위치
     * @param nickName 이름
     * @param isPlayer 나 인가 아닌가
     */
    public void CreateUser(Vector3 pos, string nickName, bool isPlayer)
    {
        GameObject obj = Instantiate(playerPrefs, pos, Quaternion.identity) as GameObject;
        User player = obj.GetComponent<User>();

        player.nickName.text = nickName;
        player.isPlayer = isPlayer;

        v_user.Add(player);
    }

    /**
     * @brief 서버에게 패킷 전달
     * @param txt 패킷 내용
     */
    public void SendMsg(string txt)
    {
        try
        {
            if (socket != null && socket.Connected)
            {
                byte[] buf = new byte[4096];

                Buffer.BlockCopy(ShortToByte(Encoding.UTF8.GetBytes(txt).Length + 2), 0, buf, 0, 2);

                Buffer.BlockCopy(Encoding.UTF8.GetBytes(txt), 0, buf, 2, Encoding.UTF8.GetBytes(txt).Length);

                socket.Send(buf, Encoding.UTF8.GetBytes(txt).Length + 2, 0);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    /**
     * @brief 패킷 처리 업데이트
     */
    IEnumerator PacketProc()
    {
        while (true)
        {
            if (socket.Available > 0)
            {
                byte[] buf = new byte[4096];
                int nRead = socket.Receive(buf, socket.Available, 0);

                if (nRead > 0)
                {
                    Buffer.BlockCopy(buf, 0, this.buf, recvLen, nRead);
                    recvLen += nRead;

                    while (true)
                    {
                        int len = BitConverter.ToInt16(this.buf, 0);

                        if (len > 0 && recvLen >= len)
                        {
                            ParsePacket(len);
                            recvLen -= len;
                            Buffer.BlockCopy(this.buf, len, this.buf, 0, recvLen);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            yield return null;
        }
    }

    /**
     * @brief 패킷 분석
     */
    public void ParsePacket(int len)
    {
        string msg = Encoding.UTF8.GetString(this.buf, 2, len - 2);
        string[] txt = msg.Split(':');

        if (txt[0].Equals("CONNECT"))
        {
            Debug.Log("Connected.");
            SendMsg(string.Format("LOGIN:{0}", nickName.text));
        }
        else if (txt[0].Equals("USER"))
        {
            CreateUser(new Vector3(float.Parse(txt[2]), float.Parse(txt[3]), 0), txt[1], false);
        }
        else if (txt[0].Equals("ADDUSER"))
        {
            CreateUser(new Vector3(UnityEngine.Random.RandomRange(-3, 3), UnityEngine.Random.RandomRange(-2, 2), 0), nickName.text, true);
        }
        else if (txt[0].Equals("CHAT"))
        {
            int idx = int.Parse(txt[1]);
            v_user[idx].SetChatText(txt[2]);
        }
        else if (txt[0].Equals("MOVE"))
        {

        }
        else if (txt[0].Equals("LOGOUT"))
        {
            int idx = int.Parse(txt[1]);

            Destroy(v_user[idx].gameObject);
            v_user.RemoveAt(idx);
        }
    }

    /**
     * @brief 기기에서 접속을 끊었을때 
     */
    void OnDestroy()
    {
        if (socket != null && socket.Connected)
        {
            SendMsg("DISCONNECT");
            Thread.Sleep(500);
            socket.Close();
        }
        StopCoroutine(PacketProc());
    }

    /**
     * @brief 인터넷 연결되어 있는지 확인
     */
    public bool checkNetwork()
    {
        string HtmlText = GetHtmlFromUri("http://google.com");
        if (HtmlText == "")
        {
            // 연결 실패
            Debug.Log("인터넷 연결 실패");
        }
        else if (!HtmlText.Contains("schema.org/WebPage"))
        {
            // 비정상적인 루트일때
            Debug.Log("인터넷 연결 실패");
        }
        else
        {
            // 성공적인 연결
            Debug.Log("인터넷 연결 성공");
            return true;
        }

        return false;
    }

    /**
     * @brief html 받아오기
     * @param resource url
     */
    public string GetHtmlFromUri(string resource)
    {
        string html = string.Empty;
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(resource);
        try
        {
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                bool isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
                if (isSuccess)
                {
                    using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                    {
                        char[] cs = new char[80];
                        reader.Read(cs, 0, cs.Length);
                        foreach (char ch in cs)
                        {
                            html += ch;
                        }
                    }
                }
            }
        }
        catch
        {
            return "";
        }
        return html;
    }

    /**
     * @brief int 를 2바이트 데이터로 변환
     * @param val 변경할 변수
     */
    public static byte[] ShortToByte(int val)
    {
        byte[] temp = new byte[2];
        temp[1] = (byte)((val & 0x0000ff00) >> 8);
        temp[0] = (byte)((val & 0x000000ff));
        return temp;
    }
}
