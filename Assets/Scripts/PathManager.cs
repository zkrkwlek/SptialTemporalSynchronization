using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    public int contentID;
    //public int startMarkerID;
    public int endContentID;
    public int currPathID;
    public bool mbPath;
    public bool mbMoving;
    public ContentManager mContentManager;
    public GameObject nextPathObject;

    // Start is called before the first frame update
    void Start()
    {
        mbMoving = false;
        mbPath = false;
        var UVR = GameObject.Find("UVR");
        mContentManager = UVR.GetComponent<ContentManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //충돌시 다음 경로 이동을 위해 브로드 캐스트

        //패스가 아니거나
        //패스여도 움직이지 않을 때는 현재 위치로.
        //패스인 경우는 다음 위치로.
    }

    public void Move()
    {
        mbMoving = true;
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, nextPathObject.transform.position, Time.deltaTime*3);
    }
}
