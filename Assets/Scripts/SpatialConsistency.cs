using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ExData
{
    public int mnFrameID;
    public Mat rgbMat;
    public ArucoMarker marker;
    public Matrix4x4 matWorldToLocal; //슬램에서 R,t. 3차원 포인트를 카메라에 프로젝션할 때 이용
    public Matrix4x4 matLocalToWorld; //슬램에서 inverse pose. 카메라 좌표계에서 월드 좌표계로 변경.
}

public class SpatialConsistency : MonoBehaviour
{

    StreamWriter writer_uvr;
    StreamWriter writer_google;

    public Dictionary<int, ExData> mExDatas;
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
        //ContentRegistrationEvent.contentRegisted += OnContentRegistration;
    }
    void OnDisable()
    {
        MarkerDetectEvent.markerDetected -= OnMarkerInteraction;
        CameraInitEvent.camInitialized -= OnCameraInitialization;
        ImageCatchEvent.frameReceived -= OnFrameReceived;
        PlaneDetectionEvent.planeDetected -= OnPlaneDetection;
        //ContentRegistrationEvent.contentRegisted -= OnContentRegistration;
    }
    void Awake()
    {
        p = new Plane(Vector3.zero,0f);
        mExDatas = new Dictionary<int, ExData>();
        //pos3D = Vector3.zero;
        //mParticles = new ParticleSystem.Particle[];

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

    void OnFrameReceived(object sender, ImageCatchEventArgs e)
    {
        ExData data = new ExData();
        data.mnFrameID = e.mnFrameID;
        data.rgbMat = e.rgbMat;
        mExDatas.Add(e.mnFrameID,data);
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

    void OnMarkerInteraction(object sender, MarkerDetectEventArgs me)
    {
        try
        {
            var marker = me.marker;
            int id = marker.id;
            var position = marker.corners[0];

            mExDatas[marker.frameId].marker = marker;
            
            if (!marker.mbCreate)
            {
                Mat pos = new Mat(3, 1, CvType.CV_64FC1);
                pos.put(0, 0, position.x);
                pos.put(1, 0, position.y);
                pos.put(2, 0, 1f);
                Mat temp = invCamMatrix * pos;
                var ptCam = new Vector3((float)temp.get(0, 0)[0], (float)temp.get(1, 0)[0], (float)temp.get(2, 0)[0]);

                var toPoint = UVR.transform.localToWorldMatrix.MultiplyPoint(ptCam);
                var dir = toPoint - UVR.transform.position;
                dir = dir.normalized;
                Ray ray = new Ray(UVR.transform.position, dir);
                //var ray = temp3 - UVR.transform.position;
                float dist = 0f;
                bool bRay = p.Raycast(ray, out dist);

                if (bRay)
                {
                    marker.mbCreate = true;
                    marker.origin2 = ray.origin + ray.direction * dist;

                    Vector3 ptUnity = ptCam; ptUnity.y *= -1f;
                    ptUnity = Camera.main.transform.localToWorldMatrix.MultiplyPoint(ptUnity);

                    //float aaa = p.GetDistanceToPoint(pos3D);
                    //mText.text = "dist  " + aaa;
                    //var pos3D2 = CreatePoint(UVR.transform.position, dir, p);
                    Instantiate(prefabObj, ptUnity, Quaternion.identity);
                    Instantiate(prefabObj, marker.origin, Quaternion.identity);
                    //mText.text = pos3D.ToString() + testAA.ToString();
                    //mText.text = ptUnity2.ToString() + " " + ptUnity.ToString();
                }

                marker.CreateOrigin(0.18f, fitARFoundationBackgroundMatrix, fitHelpersFlipMatrix, Camera.main.gameObject);
            }
            //var pos3D = marker.origin;

            if (marker.mbCreate)
            {
                float dist1 = marker.Calculate(UVR.transform.worldToLocalMatrix, camMatrix, marker.origin2, marker.corners[0], false);
                float dist2 = marker.Calculate(Camera.main.transform.worldToLocalMatrix, camMatrix, marker.origin, marker.corners[0], true);
                //var pt = UVR.transform.worldToLocalMatrix.MultiplyPoint(pos3D);
                //Mat proj = new Mat(3, 1, CvType.CV_64FC1);
                //proj.put(0, 0, pt.x);
                //proj.put(1, 0, pt.y);
                //proj.put(2, 0, pt.z);
                //proj = camMatrix * proj;
                //double depth = proj.get(2, 0)[0];
                //float px = (float)(proj.get(0, 0)[0] / depth);
                //float py = (float)(proj.get(1, 0)[0] / depth);
                //Vector2 proj2D = new Vector2(px, py) - position;

                float azi = 0f;
                float ele = 0f;
                float dist = 0f;
                marker.CalculateAziAndEleAndDist(Camera.main.gameObject, out azi, out ele, out dist);
                //writer_uvr.WriteLine(dist + "," + azi + "," + ele + "," + dist1);
                //writer_google.WriteLine(dist + "," + azi + "," + ele + "," + dist2);
                //마커의 불도 변화는지 확인
                //mText.text = "azi = " + azi + " ele = " + ele+" dist = "+dist;
                //mText.text += "\n dist = " + dist1 + " " + dist2 + " " + marker.mbCreate;
                    
                //mText.text = "ray = " + ", " + proj2D.magnitude;

                //Mat rgbMat = mCamManager.rgbMat;
                //Imgproc.circle(rgbMat, new Point(px, py), 5, new Scalar(255, 0, 255), -1);
                //Imgproc.circle(rgbMat, new Point(position.x, position.y), 5, new Scalar(255, 0, 255), -1);
                //Imgcodecs.imwrite(Application.persistentDataPath + "/save/a.jpg", mCamManager.rgbMat);
            }

            //var ids = mMarkerDetector.mListIDs;
            //var markers = mMarkerDetector.mDictMarkers;

            //if (ids.Count > 0)
            //{
            //    int id = ids[0];
            //    var marker = mMarkerDetector.GetMarker(id);
            //    var position = marker.corners[0];

            //    if (bCreate)
            //    {
            //        Mat pos = new Mat(3, 1, CvType.CV_64FC1);
            //        pos.put(0, 0, position.x);
            //        pos.put(1, 0, position.y);
            //        pos.put(2, 0, 1f);
            //        Mat temp = invCamMatrix * pos;
            //        var ptCam = new Vector3((float)temp.get(0, 0)[0], (float)temp.get(1, 0)[0], (float)temp.get(2, 0)[0]);

            //        var toPoint = UVR.transform.localToWorldMatrix.MultiplyPoint(ptCam);
            //        var dir = toPoint - UVR.transform.position;
            //        dir = dir.normalized;
            //        Ray ray = new Ray(UVR.transform.position, dir);
            //        //var ray = temp3 - UVR.transform.position;
            //        float dist = 0f;
            //        bool bRay = p.Raycast(ray, out dist);

            //        if (bRay)
            //        {
            //            bCreate = false;
            //            pos3D = ray.origin + ray.direction * dist;

            //            Vector3 ptUnity = ptCam; ptUnity.y *= -1f;
            //            ptUnity = Camera.main.transform.localToWorldMatrix.MultiplyPoint(ptUnity);

            //            float aaa= p.GetDistanceToPoint(pos3D);
            //            //mText.text = "dist  " + aaa;
            //            //var pos3D2 = CreatePoint(UVR.transform.position, dir, p);
            //            Instantiate(prefabObj, ptUnity, Quaternion.identity);
            //            Instantiate(prefabObj, marker.origin, Quaternion.identity);
            //            //mText.text = pos3D.ToString() + testAA.ToString();
            //            //mText.text = ptUnity2.ToString() + " " + ptUnity.ToString();
            //        }
            //    }
            //    //var pos3D = marker.origin;

            //    if (!bCreate)
            //    {
            //        var pt = UVR.transform.worldToLocalMatrix.MultiplyPoint(pos3D);
            //        Mat proj = new Mat(3, 1, CvType.CV_64FC1);
            //        proj.put(0, 0, pt.x);
            //        proj.put(1, 0, pt.y);
            //        proj.put(2, 0, pt.z);
            //        proj = camMatrix * proj;
            //        double depth = proj.get(2, 0)[0];
            //        float px = (float)(proj.get(0, 0)[0] / depth);
            //        float py = (float)(proj.get(1, 0)[0] / depth);
            //        Vector2 proj2D = new Vector2(px, py) - position;
            //        //mText.text = "ray = " + ", " + proj2D.magnitude;

            //        //Mat rgbMat = mCamManager.rgbMat;
            //        //Imgproc.circle(rgbMat, new Point(px, py), 5, new Scalar(255, 0, 255), -1);
            //        //Imgproc.circle(rgbMat, new Point(position.x, position.y), 5, new Scalar(255, 0, 255), -1);
            //        //Imgcodecs.imwrite(Application.persistentDataPath + "/save/a.jpg", mCamManager.rgbMat);
            //    }
               
            //    //if (bRay)
            //    //{
                    

            //    //    if (bCreate)
            //    //    {
                        
                        
            //    //    }

            //    //    if (!bCreate)
            //    //    {
                        
            //    //    }

            //    //}
            //    //else
            //    //    mText.text = "ray fail";


            //    //mText.text = "temp3 = " + temp3.ToString() + " "+ UVR.transform.position();

            //    //List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
            //    //if (arRaycastManager.Raycast(position, aRRaycastHits) && aRRaycastHits.Count > 0)
            //    //{
            //    //    ARRaycastHit hit = aRRaycastHits[0];
            //    //    //mText.text = position + " "+mMarkerDetector.corner3Ds[0].ToString();
            //    //    if (hit.trackable is ARPlane plane)
            //    //    {
            //    //        //localAnchor = anchorManager.AddAnchor(hits[0].pose);
            //    //        var anchorGameObject = Instantiate(AnchoredObjectPrefab, pos3D, hit.pose.rotation);
            //    //        //mText.text = "created " + pos3D;
            //    //    }

            //    //}
            //    //mText.text = "marekr event " + id + " " + pos3D;
            //}
            //else
            //    mText.text = "123123adsfasdfasdfasdf";

            //mText.text = "marekr event";
        }
        catch (Exception e)
        {
            mText.text = e.ToString();
        }
    }

}
