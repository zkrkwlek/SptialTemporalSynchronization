using ARFoundationWithOpenCVForUnityExample;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class Path
{
    public int startID; //����Ʈ ���̵� ���
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
    //public Vector3 position;
    public GameObject obj;
    public ARGameObject aobj;
    public Material material;
    public int nTTL;
    public bool visible;
    public PathManager pathManager;


    public Content(int id, int mid, Vector3 _pos, int _TTL)
    {
        mnContentID = id;
        //position = new Vector3(x, y, z);
        obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.transform.position = _pos;
        nTTL = _TTL;
        visible = true;

        material = obj.GetComponent<Renderer>().material;
        aobj = obj.AddComponent<ARGameObject>();
        pathManager = obj.AddComponent<PathManager>();
        pathManager.contentID = id;
        
        //pathManager.gameObject.AddComponent<CapsuleCollider>();
        //obj.GetComponent<CapsuleCollider>().isTrigger = true;

        ////�浹�� �����ϱ� ���ؼ��� �ּ� �����̴� ��ü���� ������ �ٵ�� �ݶ��̴��� �߰��Ǿ����.
        ////Ű�׸�ƽ�� �����ִ� ���¿��� ��.

        var col = obj.GetComponent<SphereCollider>();
        col.isTrigger = false; //�浹�� �������� �ʵ��� ����.

        if (mid > 0 && mid < 101)
        {
            obj.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            pathManager.currPathID = mid;
            obj.tag = "Path";
            col.isTrigger = true;
            material.color = Color.green;
        }
        else if (mid > 100) {
            obj.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            obj.tag = "VO";
            material.color = Color.black;
        }
        else
        {
            obj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            obj.tag = "VO";
            material.color = Color.blue;

            var body = obj.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        //float fAngle = rot.magnitude * Mathf.Rad2Deg;
        //Quaternion q = Quaternion.AngleAxis(fAngle, rot.normalized);
        
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
    public Dictionary<int, int> MapContentMarker; //��Ŀ�� ����Ʈ ���̵� ����, ��Ŀ ���̵�, ������ ���̵�
    public Dictionary<int, Path> PathDictionary; //��Ŀ�� ���� ���̵�� �н�.

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
            //        //����
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
    void Awake()
    {
        ContentDictionary = new Dictionary<int, Content>();
        MapContentMarker = new Dictionary<int, int>();
        PathDictionary = new Dictionary<int, Path>();
        //enabled = false;

        //arSession = FindObjectOfType<ARSession>(); // todo cache or assign reference via Inspector
        //XRSessionSubsystem xrSessionSubsystem = arSession.subsystem;
        //if (xrSessionSubsystem != null)
        //{
        //    trackingState = xrSessionSubsystem.trackingState;
        //}
    }

    //ARSession arSession;
    //XRSessionSubsystem xrSessionSubsystem;
    //TrackingState trackingState;

    // Update is called once per frame
    void Update()
    {
        //arSession = FindObjectOfType<ARSession>(); // todo cache or assign reference via Inspector
        //xrSessionSubsystem = arSession.subsystem;
        //if (xrSessionSubsystem != null)
        //{
        //    trackingState = xrSessionSubsystem.trackingState;
        //    //if (trackingState == TrackingState.Limited)
        //    //    mText.text = "Tracking.Limited";
        //    //if (trackingState == TrackingState.Tracking) {
        //    //    mText.text = "Tracking.Tracking";
        //    //}
        //}

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
    void RegistContent(int id, int mid, Vector3 pos)
    {
        var newContent = new Content(id, mid, pos, ttl);
        ContentDictionary.Add(id, newContent);
    }
    void UpdateContent(int id, Vector3 pos)
    {
        //��ųʸ����� ���ͼ� �����ص� ��ųʸ� �ȿ� �ִ� ��ҿ� ����Ǿ� ����.
        var c = ContentDictionary[id];
        var pathManager = c.obj.GetComponent<PathManager>();
        c.nTTL = ttl;
        if(pathManager.mObjState == ObjectState.None)
            c.obj.transform.position = Vector3.Lerp(c.obj.transform.position, pos, Time.deltaTime);
        c.obj.SetActive(true);

        //mText.text = "update test = " + c.obj.transform.position.x + " " + ContentDictionary[id].obj.transform.position.x;
        //ContentDictionary[id] = c;
    }

    void UpdateContent2(int id, Vector3 pos)
    {
        //��ųʸ����� ���ͼ� �����ص� ��ųʸ� �ȿ� �ִ� ��ҿ� ����Ǿ� ����.
        var c = ContentDictionary[id];
        var pathManager = c.obj.GetComponent<PathManager>();
        c.nTTL = ttl;
        c.obj.SetActive(true);
        if (pathManager.mObjState == ObjectState.None)
        {
            pos = UVR.transform.worldToLocalMatrix.MultiplyPoint(pos);
            pos.y *= -1f;
            ////���͸��ϸ� �ڱ� �ڽſ��Ը� ���� ��. �̰��� �ٸ� �ֵ鿡�� �����ҷ��� ���õ� ���� �˷��� ��.
            //Matrix4x4 obj = Matrix4x4.identity;
            //obj.SetColumn(3, new Vector4(pos.x, pos.y, pos.z, 1.0f));
            //Matrix4x4 mat = Camera.main.transform.localToWorldMatrix * obj;
            //c.aobj.SetMatrix4x4(mat);
            //c.obj.transform.localRotation = c.aobj.transform.localRotation;
            //c.obj.transform.localPosition = c.aobj.transform.localPosition;

            pos = Camera.main.transform.localToWorldMatrix.MultiplyPoint3x4(pos);
            c.obj.transform.position = pos;
        }
        else if (pathManager.mObjState == ObjectState.Manipulation) {
            pos = UVR.transform.worldToLocalMatrix.MultiplyPoint(pos);
            pos.y *= -1f;
            pos = Camera.main.transform.localToWorldMatrix.MultiplyPoint3x4(pos);
            c.obj.transform.position = pos;
        }
        else if(pathManager.mObjState == ObjectState.OnPath)
        {
            c.obj.transform.position = pathManager.startObject.transform.position;
        }
        
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
            //if (trackingState != TrackingState.Tracking)
            //{
            //    return;
            //}

            //�������� ������ 3���� ��� ���� �ý����� ��ǥ��� ��ȯ��.
            var pos = new Vector3(x, y, z);
            
            //pos = Camera.main.transform.localToWorldMatrix.MultiplyPoint(pos);

            ////���� ��ü ��� üũ.
            ////��Ŀ���� �ƴ����� �˷���

            if (CheckContent(contentid))
            {
                UpdateContent2(contentid, pos);
            }
            else
            {
                DateTime s = DateTime.Now;
                RegistContent(contentid, markerid, pos);

                ////���Ⱑ �н��Ŵ����� ��ŸƮ���� ������ ������ awake�� �ʿ��� ��.
                if(markerid > 0 && markerid < 101)
                {
                    MapContentMarker[markerid] = contentid;
                    var currContent = ContentDictionary[contentid];
                    int prevMarkerId = markerid - 1;
                    int nextMarkerId = markerid + 1;
                    
                    if (CheckPath(prevMarkerId)) {
                        int prevContentID = MapContentMarker[prevMarkerId];
                        var prevContent = ContentDictionary[prevContentID];

                        prevContent.material.color = Color.cyan;
                        currContent.material.color = Color.red;

                        //prevContent.pathManager.mbPath = true;
                        prevContent.pathManager.CreatePath(currContent.obj);

                    }
                    if (CheckPath(nextMarkerId)) {
                        int nextContentID = MapContentMarker[nextMarkerId];
                        var nextContent = ContentDictionary[nextContentID];

                        currContent.material.color = Color.black;
                        nextContent.material.color = Color.yellow;

                        //currContent.pathManager.mbPath = true;
                        currContent.pathManager.CreatePath(nextContent.obj);

                    }
                    
                }
                var timeSpan = DateTime.Now - s;
                double ts = timeSpan.TotalMilliseconds;
                mText.text = "Content generation time = " + ts;
            }
            
            //mText.text = "marker cotent = " + markerid + " " + id;
            //ContentDictionary[contentid].position = new Vector3(x, y, z);
            ContentRegistrationEvent.RunEvent(new ContentEventArgs(ContentDictionary[contentid]));
        }
        catch(Exception e)
        {
            mText.text = "Content Process " + e.ToString();
        }
    }

    public void Move(int id)
    {
        if (ContentDictionary.ContainsKey(id))
        {
            var content = ContentDictionary[id];
            content.pathManager.MoveStart();
        }
    }

    bool CheckPath(int mid)
    {
        return MapContentMarker.ContainsKey(mid);
    }
}
