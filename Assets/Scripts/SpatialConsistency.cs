using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SpatialEventArgs : EventArgs
{
    public SpatialEventArgs(Vector3 _pos, float _err)
    {
        pos = _pos;
        err = _err;
    }
    public Vector3 pos { get; set; }
    public float err { get; set; }
}

class SpatialEvent
{
    public static event EventHandler<SpatialEventArgs> spatialEvent;
    public static void RunEvent(SpatialEventArgs e)
    {
        if (spatialEvent != null)
        {
            spatialEvent(null, e);
        }
    }
}

public class SpatialConsistency : MonoBehaviour
{

    StreamWriter writer_uvr;
    StreamWriter writer_google;
    
    public Text mText;
    //public ArucoMarkerDetector mMarkerDetector;
    //public CameraManager mCamManager; //K를 얻기 위함.
    public PlaneManager mPlaneManager;//평면 정보
    
    //public ParticleSystem mParticleSystem;
    //ParticleSystem.Particle[] mParticles;

    public GameObject prefabObj;
    public GameObject UVR; //카메라 트랜스폼 기록용
    bool mbCamInit = false;
    Plane p;
    //Vector3 pos3D;
    // Start is called before the first frame update

    Mat camMatrix;
    Mat invCamMatrix;
    Matrix4x4 fitARFoundationBackgroundMatrix;
    Matrix4x4 fitHelpersFlipMatrix;

    bool WantsToQuit()
    {
        writer_uvr.Close();
        writer_google.Close();
        return true;
    }

    void OnEnable()
    {
        MarkerDetectEvent.markerDetected += OnMarkerInteraction;
        CameraInitEvent.camInitialized += OnCameraInitialization;
        ImageCatchEvent.frameReceived += OnFrameReceived;
        PlaneDetectionEvent.planeDetected += OnPlaneDetection;
        SpatialEvent.spatialEvent += OnSpatialEvent;
        //ContentRegistrationEvent.contentRegisted += OnContentRegistration;
    }
    void OnDisable()
    {
        MarkerDetectEvent.markerDetected -= OnMarkerInteraction;
        CameraInitEvent.camInitialized -= OnCameraInitialization;
        ImageCatchEvent.frameReceived -= OnFrameReceived;
        PlaneDetectionEvent.planeDetected -= OnPlaneDetection;
        SpatialEvent.spatialEvent -= OnSpatialEvent;
        //ContentRegistrationEvent.contentRegisted -= OnContentRegistration;
    }
    void Awake()
    {
        p = new Plane(Vector3.zero,0f);
        
        ////실험 파일
        var dirPath = Application.persistentDataPath + "/data";
        var filePath = dirPath + "/spatial_uvr.csv";
        writer_uvr = new StreamWriter(filePath, true);
        var filePath2 = dirPath + "/spatial_google.csv";
        writer_google = new StreamWriter(filePath2, true);
        Application.wantsToQuit += WantsToQuit;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnSpatialEvent(object sender, SpatialEventArgs e)
    {
        //서버 포즈 계산
        if (marker == null)
            return;

        //float dist2 = marker.Calculate(e.pos, camMatrix, testPos, position, true);
        float azi = 0f;
        float ele = 0f;
        float dist = 0f;
        marker.CalculateAziAndEleAndDist(e.pos, out azi, out ele, out dist);
        writer_uvr.WriteLine(dist + "," + azi + "," + ele + "," + e.err);
        //mText.text = "test spatial = " + e.err;
    }

    void OnFrameReceived(object sender, ImageCatchEventArgs e)
    {

    }

    void OnContentRegistration(object sender, ContentEventArgs e)
    {
        //var pos = e.content.position;
        //pos = UVR.transform.worldToLocalMatrix.MultiplyPoint(pos);
        //Mat proj = new Mat(3, 1, CvType.CV_64FC1);
        //proj.put(0, 0, pos.x);
        //proj.put(1, 0, pos.y);
        //proj.put(2, 0, pos.z);
        //proj = camMatrix * proj;
        //double depth = proj.get(2, 0)[0];
        //float px = (float)(proj.get(0, 0)[0] / depth);
        //float py = (float)(proj.get(1, 0)[0] / depth);
        //Mat rgbMat = e.rgbMat;
        //Imgproc.circle(rgbMat, new Point(px, py), 5, new Scalar(255, 0, 255), -1);
        //Imgcodecs.imwrite(Application.persistentDataPath + "/save/i.jpg", mCamManager.rgbMat);
        //mText.text = "update content";
    }

    void OnPlaneDetection(object sender, PlaneEventArgs e)
    {
        //mText.text = "plane event test~~ "+e.plane.ToString();
        p = e.plane;
    }

    void OnCameraInitialization(object sender, CameraInitEventArgs e)
    {
        mbCamInit = true;
        camMatrix = e.camMat;
        invCamMatrix = e.invCamMat;
        fitARFoundationBackgroundMatrix = e.fitARFoundationBackgroundMatrix;
        fitHelpersFlipMatrix = e.fitHelpersFlipMatrix;
        //카메라 정보 받기
        //Kinv = new Mat(3, 3, CvType.CV_64FC1);
        //mCamManager.invCamMatrix.copyTo(Kinv);
    }

    //bool bCreate = true;

    //float Calculate(Vector3 p3D, Vector2 p2D, Matrix4x4 matWtoL, bool bFlip)
    //{

    //}
    public Vector3 CreatePoint(Vector3 origin, Vector3 dir, Plane plane)
    {
        float a = Vector3.Dot(plane.normal, -dir);
        float u = (Vector3.Dot(plane.normal, origin) + plane.distance) / a;
        return origin + dir * u;
    }

    ArucoMarker marker = null;
    //ARFOUNDATION 결과만 저장하도록 변경
    void OnMarkerInteraction(object sender, MarkerDetectEventArgs me)
    {
        try
        {
            marker = me.marker;
            int id = marker.id;
            var position = marker.corners[0];

            

            if (!marker.mbCreate && marker.nUpdated > 10)
            {
                marker.mbCreate = true;
                marker.CreateOrigin(0.18f, fitARFoundationBackgroundMatrix, fitHelpersFlipMatrix, Camera.main.gameObject);

                //Instantiate(prefabObj, marker.gameobject.transform.position, Quaternion.identity);
                Instantiate(prefabObj, marker.origin, Quaternion.identity);
            }
            else {
                float dist2 = marker.Calculate(Camera.main.transform.worldToLocalMatrix, camMatrix, marker.origin, position, true);
                float azi = 0f;
                float ele = 0f;
                float dist = 0f;
                marker.CalculateAziAndEleAndDist(Camera.main.transform.position, out azi, out ele, out dist);
                writer_google.WriteLine(dist + "," + azi + "," + ele + "," + dist2);//거리 방위 고도 에러
            }
            
        }
        catch (Exception e)
        {
            mText.text = e.ToString();
        }
    }

}
