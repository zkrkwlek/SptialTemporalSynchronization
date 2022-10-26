using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ObjectState
{ None, Manipulation, Moving, OnPath} //moving�� �ִϸ��̼� Ʈ���ſ� ���� �����̴� ��, onpath�� ��� ���� �ִ� ����. update�� ����ڰ� �����ϴ� ����

public class PathManager : MonoBehaviour
{
    public int contentID;
    //public int startMarkerID;
    public int endContentID;
    public int currPathID;
    public bool mbPath; //�н��� �����Ǿ�����, �ش� ��ü�� ���� ��ü�� �ǰ�, ������ ��ġ�� ��ü�� ����Ǿ�� ��.
    public ObjectState mObjState;
    //public bool mbMoving; //�����̴���
    //public bool mbOnPath; //�н� ���� �ִ���. �±׷� �н� Ȯ��.
    public ContentManager mContentManager;
    public DataSender mSender;
    public GameObject endPathObject; //�̰� ����� �� ó���� �� Ŭ������ ������ ������Ʈ��. ���� ������ ������Ʈ�� �˸� ��.
    public string log;
    float speed;

    string userName;
    string[] logString = new string[4];

    /// <summary>
    /// �н��� �ö��� �� �����Ǵ� ��.
    /// �ڵ����� ���� �н��� Ȱ��ȭ �� �н��̸� ���ŵǾ�� ��.
    /// �̰����� �н��� ���۰� ���� ���������� ����.
    /// �ֳ��ϸ�, �н��� ��� ��� ������ ������ ������
    /// ���� ��ü�� �������� �ʴµ��ȿ��� ��� ��ġ�� �����. �����̴� ���ȿ��� �ϴ� ���ֺ���.
    /// </summary>
    float startTime;
    public GameObject startObject;
    public GameObject endObject;
    LineRenderer lineRenderer;

    float journeyLength;

    [HideInInspector]
    Text mText;

    // Start is called before the first frame update
    void Awake()
    {
        speed = 0.5f;
        log = "";
        mObjState = ObjectState.None;
        mbPath = false;
        var UVR = GameObject.Find("UVR");
        mContentManager = UVR.GetComponent<ContentManager>();
        mSender = UVR.GetComponent<DataSender>();
        userName = UVR.GetComponent<SystemManager>().User.UserName;
        mText = GameObject.Find("StatusText").GetComponent<Text>();

        logString[0] = "NONE";
        logString[1] = "Manipulation";
        logString[2] = "Moving";
        logString[3] = "OnPath";
    }

    // Update is called once per frame
    void Update()
    {
        if (mbPath)
        {
            lineRenderer.SetPosition(0, gameObject.transform.position);
            lineRenderer.SetPosition(1, endPathObject.transform.position);
        }

        Move();

        //�����̸�

        //�浹�� ���� ��� �̵��� ���� ��ε� ĳ��Ʈ

        //�н��� �ƴϰų�
        //�н����� �������� ���� ���� ���� ��ġ��.
        //�н��� ���� ���� ��ġ��.

        //���� ��ü �浹��, ���� �н��� �Ѿ����. ���� �н� ������ �н� ����.
    }
    public bool CheckPath(bool flag)
    {
        return gameObject.tag == "Path" && mbPath == flag;
    }
    
    public bool isOnPath() {
        return gameObject.tag == "VO" && mObjState == ObjectState.OnPath;
    }

    public void CreatePath(GameObject _next)
    {
        mbPath = true;
        endPathObject = _next;
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material.color = new Color(0f, 1f, 1f, 0.2f);
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth   = 0.01f;
    }
    
    public void MoveStart()
    {
        //move�� ������ ����
        mObjState = ObjectState.Moving;
        startTime = Time.time;
    }
    public void MoveEnd()
    {
        mObjState = ObjectState.None;
    }

    public void Move()
    {
        if (mObjState == ObjectState.Moving)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(startObject.transform.position, endObject.transform.position, fractionOfJourney);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        var obj = other.gameObject;
        var path = obj.GetComponent<PathManager>();
        if (path.CheckPath(true))
        {
            mObjState = ObjectState.OnPath;
            //�н� ����.

            startObject = obj;
            endObject = path.endPathObject;

            journeyLength = Vector3.Distance(startObject.transform.position, endObject.transform.position);

            log = "on path object =" + contentID + " is on path " + path.currPathID + logString[(int)mObjState];
            //log = "OnTriggerEnter = col = " + other.gameObject.tag + ", this = " + gameObject.tag;
            
            transform.position = startObject.transform.position;
            gameObject.GetComponent<Renderer>().material.color = Color.magenta;
        }
        else if (path.CheckPath(false))
        {
            mObjState = ObjectState.None;
            gameObject.GetComponent<Renderer>().material.color = Color.blue;
            log = "on end path object =" + contentID + " is on path " + path.currPathID + "= dist = " + journeyLength;
        }
        else
            log = obj.tag + " " + path.contentID + "/ " + contentID;

        //log = "OnTriggerEnter2 = col = " + other.gameObject.tag + ", this = " + gameObject.tag;
        mText.text = log;

        //{
        //    string keyword = "VO.SELECTION";
        //    float[] fdata = new float[5];
        //    fdata[0] = 0f;
        //    fdata[1] = 0f;
        //    fdata[2] = transform.position.x;
        //    fdata[3] = transform.position.y;
        //    fdata[4] = transform.position.z;
        //    byte[] bdata = new byte[fdata.Length * 4];
        //    Buffer.BlockCopy(fdata, 0, bdata, 0, bdata.Length); //��ü �Ǽ��� ������ ��
        //    UdpData mdata = new UdpData(keyword, userName, contentID, bdata, 1.0);
        //    StartCoroutine(mSender.SendData(mdata));
        //}

    }
    private void OnTriggerStay(Collider other)
    {
        var obj = other.gameObject;
        var path = obj.GetComponent<PathManager>();
        log = "OnTriggerStay " + contentID +", "+ logString[(int)mObjState] + " = " + obj.tag+" "+path.currPathID;
        mText.text = log;
    }

    void OnTriggerExit(Collider other)
    {
        var obj = other.gameObject;
        var path = obj.GetComponent<PathManager>();
        //if (path.CheckPath(true))
        //{
        //    mObjState = ObjectState.OnPath;
        //    //�н� ����.

        //    startObject = obj;
        //    endObject = path.endPathObject;

        //    journeyLength = Vector3.Distance(startObject.transform.position, endObject.transform.position);

        //    log = "on path object =" + contentID + " is on path " + path.currPathID + "= dist = " + journeyLength;
        //    //log = "OnTriggerEnter = col = " + other.gameObject.tag + ", this = " + gameObject.tag;

        //    transform.position = startObject.transform.position;
        //    gameObject.GetComponent<Renderer>().material.color = Color.magenta;
        //}
        //else if (path.CheckPath(false))
        //{
        //    mObjState = ObjectState.None;
        //    gameObject.GetComponent<Renderer>().material.color = Color.blue;
        //    log = "on end path object =" + contentID + " is on path " + path.currPathID + "= dist = " + journeyLength;
        //}
        //else
        //    log = obj.tag + " " + path.contentID + "/ " + contentID;

        //�浹�� VO
        //other�� Path
        log = "OnTriggerExit " + contentID + ", " + logString[(int)mObjState]+" = "+other.gameObject.tag+" "+path.currPathID;
        mText.text = log;
    }
    void OnCollisionEnter(Collision collision)
    {
        //var obj = collision.gameObject;
        //var path = obj.GetComponent<PathManager>(); 
        //if (path.CheckPath(true))
        //{
        //    mObjState = ObjectState.OnPath;
        //    //�н� ����.
        //    startObject = obj;
        //    endObject = path.endPathObject;

        //    journeyLength = Vector3.Distance(startObject.transform.position, endObject.transform.position);

        //    log = "object =" + contentID + " is on path " + path.currPathID + "= dist = "+journeyLength;

        //    transform.position = startObject.transform.position;
        //    gameObject.GetComponent<Renderer>().material.color = Color.magenta;
        //}else if (path.CheckPath(false))
        //{
        //    mObjState = ObjectState.None;
        //    gameObject.GetComponent<Renderer>().material.color = Color.blue;
        //}
        //else 
        //    log = obj.tag+" "+path.contentID + "/ "+contentID;

        //if(obj == endPathObject)
        //{
        //    log = "check end path objecct";
        //}

        //��ΰ� ������ ��
        //1)���� ��ΰ� �����ϴ� ���
        //���� ��� ���� ��û.
        //��� ����
        //2) ���� ��ΰ� �������� �ʴ� ���
        //������ ����.
        //�н� ����


        //�� �˰��򿡼� ���� ���� �� ��ü�� ��� �浹 �ߴ����� �˷���.
        //��� ��ü�� ������ �н��Ŵ����� �߰���.
        //�н��� �������� 1) �浹�� ��ü�� �±װ� path�̰�, 2) �ش� ��ü�� �н��� �����Ǿ���� ��.


        ////Check for a match with the specified name on any GameObject that collides with your GameObject
        //if (collision.gameObject.name == "MyGameObjectName")
        //{
        //    //If the GameObject's name matches the one you suggest, output this message in the console
        //    Debug.Log("Do something here");
        //}

        ////Check for a match with the specific tag on any GameObject that collides with your GameObject
        //if (collision.gameObject.tag == "MyGameObjectTag")
        //{
        //    //If the GameObject has the same tag as specified, output this message in the console
        //    Debug.Log("Do something else here");
        //}
        
    }

}
