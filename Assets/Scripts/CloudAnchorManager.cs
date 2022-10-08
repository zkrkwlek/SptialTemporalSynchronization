using Google.XR.ARCoreExtensions;
using OpenCVForUnity.CoreModule;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CloudAnchorManager : MonoBehaviour
{
    // The prefab to instantiate on touch.
    [SerializeField]
   // private GameObject anchorPrefab;
    private GameObject anchorGameObject;

    // Cache ARRaycastManager GameObject from ARCoreSession
    public Mode mode = Mode.READY;
    public ARRaycastManager raycastManager;
    public ARAnchorManager anchorManager;
    public ArucoMarkerDetector mMarkerDetector;
    public GameObject AnchoredObjectPrefab;
    
    private ARAnchor localAnchor;
    private ARCloudAnchor cloudAnchor;

    private const string cloudAnchorKey = "CloudAnchorTest";//"AIzaSyAlmi4sdX0DR6x-9L3QYBRf2pH0ABlzb_A";
    private string strCloudAnchorId;

    public enum Mode { READY, HOST, HOST_PENDING, RESOLVE, RESOLVE_PENDING };
    public Button hostButton;
    public Button resolveButton; 
    public Button resetButton;   
    public Text messageText;

    //latency ���
    string filePath;
    StreamWriter writer_latency;
    StreamWriter writer_spatial;
    DateTime startTime;


    // List for raycast hits is re-used by raycast manager
    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void OnEnable()
    {
        MarkerDetectEvent.markerDetected += OnMarkerInteraction;
        CameraInitEvent.camInitialized += OnCameraInitialization;
    }
    void OnDisable()
    {
        MarkerDetectEvent.markerDetected -= OnMarkerInteraction;
        CameraInitEvent.camInitialized -= OnCameraInitialization;
    }
    Mat camMatrix;
    Matrix4x4 fitARFoundationBackgroundMatrix;
    Matrix4x4 fitHelpersFlipMatrix;
    void OnCameraInitialization(object sender, CameraInitEventArgs e)
    {
        camMatrix = e.camMat;
        fitARFoundationBackgroundMatrix = e.fitARFoundationBackgroundMatrix;
        fitHelpersFlipMatrix = e.fitHelpersFlipMatrix;
    }
    void OnMarkerInteraction(object sender, MarkerDetectEventArgs me)
    {
        try
        {
            var marker = me.marker;
            int id = marker.id;
            var position = marker.corners[0];
            

            if (mode == Mode.HOST)
            {
                if (anchorGameObject == null)
                {
                    marker.CreateOrigin(0.18f, fitARFoundationBackgroundMatrix, fitHelpersFlipMatrix, Camera.main.gameObject);
                    Pose pose = new Pose(marker.origin, Quaternion.identity);
                    localAnchor = anchorManager.AddAnchor(pose);
                    anchorGameObject = Instantiate(AnchoredObjectPrefab, localAnchor.transform.position, Quaternion.identity);
                }
                else
                {
                    var pos3D = marker.origin;
                    localAnchor.transform.position = pos3D;
                    anchorGameObject.transform.position = pos3D;
                }

            }
            if (mode == Mode.READY && anchorGameObject != null)
            {
                //var dist = mMarkerDetector.Calculate(pos3D);
                //messageText.text = "marker dist = " + dist;
                float azi = 0f;
                float ele = 0f;
                float dist = 0f;
                marker.CalculateAziAndEleAndDist(Camera.main.gameObject, out azi, out ele, out dist);
                float dist2 = marker.Calculate(Camera.main.transform.worldToLocalMatrix, camMatrix, anchorGameObject.transform.position, marker.corners[0], true);
                if(dist2 < 1000f)
                    writer_spatial.WriteLine(dist + "," + azi + "," + ele + "," + dist2);
            }


            //var ids = mMarkerDetector.mListIDs;
            //var markers = mMarkerDetector.mDictMarkers;
            //if (ids.Count > 0)
            //{
            //    int id = ids[0];
            //    var marker = mMarkerDetector.GetMarker(id);
            //    var pos3D = marker.origin;

            //    if (mode == Mode.HOST)
            //    {


            //        if (anchorGameObject == null)
            //        {
            //            Pose pose = new Pose(pos3D, Quaternion.identity);
            //            localAnchor = anchorManager.AddAnchor(pose);
            //            anchorGameObject = Instantiate(AnchoredObjectPrefab, pos3D, Quaternion.identity);
            //        }
            //        else
            //        {
            //            localAnchor.transform.position = pos3D;
            //            anchorGameObject.transform.position = pos3D;
            //        }

            //        //var position = marker.corners[0];

            //        //List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
            //        //if (raycastManager.Raycast(position, aRRaycastHits) && aRRaycastHits.Count > 0)
            //        //{
            //        //    ARRaycastHit hit = aRRaycastHits[0];
            //        //    if (hit.trackable is ARPlane plane)
            //        //    {


            //        //        messageText.text = pos3D + " " + localAnchor.transform.position;
            //        //    }
            //        //}
            //        //mText.text = "marekr event " + id;
            //    }
            //    if(mode == Mode.READY && anchorGameObject!=null)
            //    {
            //        var dist = mMarkerDetector.Calculate(pos3D);
            //        messageText.text = "marker dist = " + dist;
            //    }
            //}

            //else
            //    mText.text = "adsfasdfasdfasdf";

            //mText.text = "marekr event";
        }
        catch (Exception e)
        {
            messageText.text = e.ToString();
        }
    }

    bool WantsToQuit()
    {
        writer_latency.Close();
        writer_spatial.Close();
        return true;
    }

    void Awake()
    {
        string dirPath = Application.persistentDataPath + "/data";
        filePath = dirPath + "/error_latency_google.csv";
        writer_latency = new StreamWriter(filePath, true);
        filePath = dirPath + "/error_spatial_google.csv";
        writer_spatial = new StreamWriter(filePath, true);
    }

    void Start()
    {
        //_raycastManager = GetComponent<ARRaycastManager>();
        hostButton.onClick.AddListener(() => OnHostClick());
        resolveButton.onClick.AddListener(() => OnResolveClick());
        resetButton.onClick.AddListener(() => OnResetClick());

        strCloudAnchorId = PlayerPrefs.GetString(cloudAnchorKey, "");

        Application.wantsToQuit += WantsToQuit;
    }
    int idx = 0;
    void Update()
    {
        if (mode == Mode.HOST)
        {
            //Hosting();
            HostProcessing();
        }
        if (mode == Mode.HOST_PENDING)
        {
            HostPending();
        }
        if (mode == Mode.RESOLVE)
        {
            Resolving();
        }
        if(mode == Mode.RESOLVE_PENDING)
        {
            ResolvePending();
        }
        if(mode == Mode.READY)
        {
            //if (PlayerPrefs.HasKey(cloudAnchorKey))
            //{
            //    messageText.text = "Already anchor = " + PlayerPrefs.GetString(cloudAnchorKey);
            //}
        }
    }

    void Hosting()
    {
        if (Input.touchCount < 1) return;
        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;
        if (localAnchor == null)
        {
            if (raycastManager.Raycast(touch.position, hits, TrackableType.All))
            {
                if (hits[0].trackable is ARPlane plane)
                {
                    localAnchor = anchorManager.AddAnchor(hits[0].pose);
                    anchorGameObject = Instantiate(AnchoredObjectPrefab, localAnchor.transform);
                }
            }
        }
    }
    void HostProcessing()
    {
        if (localAnchor == null) return;
        FeatureMapQuality quality = anchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
        string mappingText = string.Format("���� ǰ�� = {0}, {1}", quality, localAnchor.transform.position);
        if (quality == FeatureMapQuality.Sufficient || quality == FeatureMapQuality.Good)
        {
            cloudAnchor = anchorManager.HostCloudAnchor(localAnchor, 1);
            
            if (cloudAnchor == null)
            {
                mappingText = "Ŭ���� ��Ŀ ���� ����";
            }
            else
            {
                mappingText = "Ŭ���� ��Ŀ ���� ����";
                mode = Mode.HOST_PENDING;
            }
        }
        messageText.text = mappingText;
    }
    void HostPending()
    {
        string mappingText = "";
        if (cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
        {
            mappingText = $"Ŭ���� ��Ŀ ���� ����, CloudAnchor ID = {cloudAnchor.cloudAnchorId}, {cloudAnchor.transform.position}";
            strCloudAnchorId = cloudAnchor.cloudAnchorId;
            PlayerPrefs.SetString(cloudAnchorKey, strCloudAnchorId);

            cloudAnchor = null;
            Destroy(anchorGameObject);
            mode = Mode.READY;
        }
        else
        {
            mappingText = $"Ŭ���� ��Ŀ ���� ������...{cloudAnchor.cloudAnchorState}";
        }
        messageText.text = mappingText;
    }
    void Resolving()
    {
        if (PlayerPrefs.HasKey(cloudAnchorKey) == false) return;
        strCloudAnchorId = PlayerPrefs.GetString(cloudAnchorKey);
        messageText.text = $"{strCloudAnchorId}";
        ////���� �ð� ���
        startTime = DateTime.Now;

        cloudAnchor = anchorManager.ResolveCloudAnchorId(strCloudAnchorId);
                
        messageText.text = $"{strCloudAnchorId} �׽�Ʈ";
        if (cloudAnchor == null)
        {
            messageText.text = "Ŭ���� ��Ŀ ������ ����";
        }
        else
        {
            messageText.text = $"Ŭ���� ��Ŀ ������ ���� : {cloudAnchor.cloudAnchorId}";
            mode = Mode.RESOLVE_PENDING;
        }
    }
    void ResolvePending()
    {
        if (cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
        {
            messageText.text = "������ ���� = " + cloudAnchor.transform.position;
            anchorGameObject = Instantiate(AnchoredObjectPrefab, cloudAnchor.transform);
            mode = Mode.READY;
            ////���� �ð� ���
            var timeSpan = DateTime.Now - startTime;
            double ts = timeSpan.TotalMilliseconds;
            writer_latency.WriteLine(ts);
        }
        else
        {
            messageText.text = $"������ ���� ��...{cloudAnchor.cloudAnchorState}";
        }
    }

    public Pose GetCameraPose()
    {
        return new Pose(Camera.main.transform.position, Camera.main.transform.rotation);
    }
    private void OnHostClick()
    {
        mode = Mode.HOST;
    }
    private void OnResolveClick()
    {
        mode = Mode.RESOLVE;
    }
    private void OnResetClick()
    {
        if (anchorGameObject != null)
        {
            Destroy(anchorGameObject);
            
        }
        //if(localAnchor != null)
        //{
        //    Destroy(localAnchor);
        //}
        cloudAnchor = null;
        localAnchor = null;
        messageText.text = "�غ�Ϸ�";
        mode = Mode.READY;
    }
       
}