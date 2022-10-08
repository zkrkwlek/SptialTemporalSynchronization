using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Path
{
    public int startID; //컨텐트 아이디를 기록
    public int endID;
    public GameObject pathObj;
    
    public Path(int sid, int eid)
    {
        //GameObject.Instantiate(AnchoredObjectPrefab, worldPos, worldRot)
        startID = sid;
        endID = eid;
    }
}

public class Content
{
    public int mnContentID;
    public Vector3 position;
    public GameObject obj;
    public Material material;
    public int nTTL;
    public bool visible;

    public Content(int id, float x, float y, float z, int _TTL)
    {
        mnContentID = id;
        position = new Vector3(x, y, z);
        obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        obj.transform.position = position;
        material = obj.GetComponent<Renderer>().material;
        material.color = Color.blue;

        //float fAngle = rot.magnitude * Mathf.Rad2Deg;
        //Quaternion q = Quaternion.AngleAxis(fAngle, rot.normalized);
        nTTL = _TTL;
        visible = true;
    }
}

public class ContentEventArgs : EventArgs
{
    public ContentEventArgs(Content c)
    {
        content = c;
    }
    public Content content { get; set; }
}

class ContentRegistrationEvent
{
    public static event EventHandler<ContentEventArgs> contentRegisted;
    public static void RunEvent(ContentEventArgs e)
    {
        if (contentRegisted != null)
        {
            contentRegisted(null, e);
        }
    }
}

public class ContentManager : MonoBehaviour
{
    public GameObject UVR;
    public GameObject pathPrefab;
    public Text mText;
    public Dictionary<int, Content> ContentDictionary;
    public Dictionary<int, int> MapContentMarker; //마커와 컨텐트 아이디 저장, 마커 아이디, 컨텐츠 아이디
    public Dictionary<int, Path> PathDictionary; //마커의 시작 아이디로 패스.

    public void UpdateContents(ref float[] tdata, Pose NewPose)
    {
        try
        {
            //int N = (int)tdata[0];
            //int dataIdx = 1;
            //for (int i = 0; i < N; i++)
            //{
            //    int id = (int)tdata[dataIdx++];
            //    int nid = (int)tdata[dataIdx++];
            //    int attr = (int)tdata[dataIdx++];
            //    bool battr = false;
            //    if (attr > 0)
            //        battr = true;
            //    Vector3 spos = new Vector3(tdata[dataIdx++], tdata[dataIdx++], tdata[dataIdx++]);
            //    Vector3 epos = new Vector3(tdata[dataIdx++], tdata[dataIdx++], tdata[dataIdx++]);

            //    string text = "\nContent = "+spos + " ";
            //    spos = NewPose.R * spos + NewPose.t;
            //    epos = NewPose.R * epos + NewPose.t;
            //    text = text + spos;
            //    mText.text = mText.text + text;

            //    if (ContentDictionary.ContainsKey(id))
            //    {
            //        //update
            //        ContentDictionary[id].nTTL += 5;
            //        ContentDictionary[id].position = spos;
            //        ContentDictionary[id].obj.transform.position = spos;
            //    }
            //    else
            //    {
            //        //생성
            //        var newContent = new Content(id, spos.x, spos.y, spos.z, 5);
            //        ContentDictionary.Add(id, newContent);
            //    }
            //}
            ////mText.text = "Content = " + N;
        }
        catch(Exception e)
        {
            mText.text = e.ToString();
        }
        
    }


    // Start is called before the first frame update
    void Start()
    {
        ContentDictionary = new Dictionary<int, Content>();
        MapContentMarker = new Dictionary<int, int>();
        PathDictionary = new Dictionary<int, Path>();
        //enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (int id in ContentDictionary.Keys)
        {

            ContentDictionary[id].nTTL--;
            if(ContentDictionary[id].nTTL <= 0)
            {
                ContentDictionary[id].obj.SetActive(false);
            }
        }
    }

    public bool CheckContent(int id)
    {
        return ContentDictionary.ContainsKey(id);
    }
    int ttl = 6;
    void RegistContent(int id, float x, float y, float z)
    {
        var newContent = new Content(id, x, y, z, ttl);
        newContent.obj.AddComponent<PathManager>();
        ContentDictionary.Add(id, newContent);
        
    }
    void UpdateContent(int id, Vector3 pos)
    {
        //딕셔너리에서 빼와서 수정해도 딕셔너리 안에 있는 요소와 연결되어 있음.
        var c = ContentDictionary[id];
        var pathManager = c.obj.GetComponent<PathManager>();
        c.nTTL = ttl;
        if(!pathManager.mbPath || !pathManager.mbMoving)
            c.obj.transform.position = Vector3.Lerp(c.obj.transform.position, pos, Time.deltaTime);
        c.obj.SetActive(true);

        //mText.text = "update test = " + c.obj.transform.position.x + " " + ContentDictionary[id].obj.transform.position.x;
        //ContentDictionary[id] = c;
    }

    public void GeneratePath(int markerID)
    {
        var path = new Path(markerID, markerID+1);
        PathDictionary.Add(markerID, path);
    }

    public void Process(int contentid, int markerid, float x, float y, float z)
    {
        try
        {
            var pos = new Vector3(x, y, z);
            pos = UVR.transform.worldToLocalMatrix.MultiplyPoint(pos);
            pos.y *= -1f;
            pos = Camera.main.transform.localToWorldMatrix.MultiplyPoint(pos);

            if (CheckContent(contentid))
            {
                UpdateContent(contentid, pos);
            }
            else
            {
                RegistContent(contentid, pos.x, pos.y, pos.z);
                if (markerid > 0)
                {
                    MapContentMarker[markerid] = contentid; //무조건 마커랑 컨텐츠를 연결하게 되어 있음.
                    int prevMarkerId = markerid - 1;
                    int nextMarkerId = markerid + 1;
                    var currContent = ContentDictionary[contentid];
                    //var currPathManager = ContentDictionary[contentid].obj.GetComponent<PathManager>();

                    if (CheckPath(prevMarkerId))
                    {
                        int prevContentID = MapContentMarker[prevMarkerId];
                        var prevContent = ContentDictionary[prevContentID];
                        prevContent.obj.AddComponent<CapsuleCollider>();
                        prevContent.material.color = Color.red;
                        prevContent.obj.tag = "Path";
                        prevContent.obj.name = "Pathprev";
                        var pathManager = prevContent.obj.GetComponent<PathManager>();
                        pathManager.mbPath = true;
                        pathManager.nextPathObject = currContent.obj;
                        //pathManager.Init(prevContentID, prevMarkerId, contentid);
                    }
                    if (CheckPath(nextMarkerId))
                    {
                        int nextContentID = MapContentMarker[nextMarkerId];
                        var nextContent = ContentDictionary[nextContentID];
                        var col = currContent.obj.AddComponent<CapsuleCollider>();
                        currContent.material.color = Color.red;
                        currContent.obj.tag = "Path";
                        currContent.obj.name = "Pathcurr";
                        var pathManager = currContent.obj.GetComponent<PathManager>();
                        pathManager.mbPath = true;
                        pathManager.nextPathObject = nextContent.obj;
                        //pathManager.Init(contentid, markerid, nextContentID);
                    }
                }
                //if (markerid > 0 && !MapContentMarker.ContainsKey(markerid))
                //{
                //    MapContentMarker[markerid] = contentid;
                //    int previd = markerid - 1;
                //    int nextid = markerid + 1;
                //    if (CheckPath(previd))
                //    {
                //        mText.text = "generate prev path";
                //        GeneratePath(previd);
                //        int prevContentID = MapContentMarker[previd];
                //        var prevContent = ContentDictionary[prevContentID];
                //        prevContent.obj.AddComponent<CapsuleCollider>();
                //        prevContent.material.color = Color.red;
                //        prevContent.obj.tag = "Path";
                //        prevContent.obj.name = "Pathprev";
                //        //패스 생성
                //        //패스객체에는 충돌 추가
                //        var pathManager = prevContent.obj.AddComponent<PathManager>();
                //        pathManager.Init(prevContentID, previd, contentid);
                        
                //    }
                //    if (CheckPath(nextid))
                //    {
                //        mText.text = "generate curr path";
                //        //패스 생성
                //        GeneratePath(markerid);
                //        var content = ContentDictionary[contentid];
                //        var col = content.obj.AddComponent<CapsuleCollider>();
                //        content.material.color = Color.red;
                //        content.obj.tag = "Path";
                //        content.obj.name = "Pathcurr";
                //        var pathManager = content.obj.AddComponent<PathManager>();
                //        pathManager.Init(contentid, markerid, nextid);
                //    }
                //}
            }
            //mText.text = "marker cotent = " + markerid + " " + id;
            ContentDictionary[contentid].position = new Vector3(x, y, z);
            ContentRegistrationEvent.RunEvent(new ContentEventArgs(ContentDictionary[contentid]));
        }
        catch(Exception e)
        {
            mText.text = "Content Process " + e.ToString();
        }
    }
    
    bool CheckPath(int mid)
    {
        return MapContentMarker.ContainsKey(mid);
    }
}
