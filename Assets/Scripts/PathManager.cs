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
        //�浹�� ���� ��� �̵��� ���� ��ε� ĳ��Ʈ

        //�н��� �ƴϰų�
        //�н����� �������� ���� ���� ���� ��ġ��.
        //�н��� ���� ���� ��ġ��.
    }

    public void Move()
    {
        mbMoving = true;
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, nextPathObject.transform.position, Time.deltaTime*3);
    }
}
