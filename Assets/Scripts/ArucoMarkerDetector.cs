using ARFoundationWithOpenCVForUnity.UnityUtils.Helper;
using ARFoundationWithOpenCVForUnityExample;
using OpenCVForUnity.ArucoModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MarkerDetectEventArgs : EventArgs
{
    public MarkerDetectEventArgs(ArucoMarker m) 
    {
        marker = m;
    }
    public ArucoMarker marker { get; set; }
}

//delegate void eventMarkerDetect();
class MarkerDetectEvent
{
    public static event EventHandler<MarkerDetectEventArgs> markerDetected;
    public static void RunEvent(MarkerDetectEventArgs e)
    {
        if (markerDetected != null)
        {
            markerDetected(null, e);
        }
    }
}
public enum ArUcoDictionary
{
    DICT_4X4_50 = Aruco.DICT_4X4_50,
    DICT_4X4_100 = Aruco.DICT_4X4_100,
    DICT_4X4_250 = Aruco.DICT_4X4_250,
    DICT_4X4_1000 = Aruco.DICT_4X4_1000,
    DICT_5X5_50 = Aruco.DICT_5X5_50,
    DICT_5X5_100 = Aruco.DICT_5X5_100,
    DICT_5X5_250 = Aruco.DICT_5X5_250,
    DICT_5X5_1000 = Aruco.DICT_5X5_1000,
    DICT_6X6_50 = Aruco.DICT_6X6_50,
    DICT_6X6_100 = Aruco.DICT_6X6_100,
    DICT_6X6_250 = Aruco.DICT_6X6_250,
    DICT_6X6_1000 = Aruco.DICT_6X6_1000,
    DICT_7X7_50 = Aruco.DICT_7X7_50,
    DICT_7X7_100 = Aruco.DICT_7X7_100,
    DICT_7X7_250 = Aruco.DICT_7X7_250,
    DICT_7X7_1000 = Aruco.DICT_7X7_1000,
    DICT_ARUCO_ORIGINAL = Aruco.DICT_ARUCO_ORIGINAL,
}

public class ArucoMarker{
    public int id;
    public int frameId; //마커를 디텍션한 최신 프레임의 아이디
    public List<Vector2> corners; //최신 프레임에서 코너 위치
    public Vector3 origin; //arcore 기준.
    public Vector3 origin2;//내 알고리즘 기준
    public ARGameObject gameobject; //필터링용. 이것은 azi, ele로 위치 측정할 때 이용
    public Matrix4x4 ARM;
    public bool mbCreate;
    public ArucoMarker(){ 
        corners = new List<Vector2>();
        mbCreate = false;
    }
    public ArucoMarker(int _id)
    {
        id = _id;
        corners = new List<Vector2>();
        mbCreate = false;
    }
    public float Calculate(Matrix4x4 P, Mat K, Vector3 pos, Vector2 corner, bool bFlip)
    {
        //유니티 좌표계에서는 y를 카메라 좌표계에서 플립해야 함.
        //opencv 좌표계에서는 해당사항 없음.
        float sign= bFlip ? -1f : 1f;
        var pt = P.MultiplyPoint(pos);
        Mat proj = new Mat(3, 1, CvType.CV_64FC1);
        proj.put(0, 0, pt.x);
        proj.put(1, 0, sign*pt.y);
        proj.put(2, 0, pt.z);
        proj = K * proj;
        double depth = proj.get(2, 0)[0];
        float px = (float)(proj.get(0, 0)[0] / depth);
        float py = (float)(proj.get(1, 0)[0] / depth);
        Vector2 proj2D = new Vector2(px, py) - corner;
        return proj2D.sqrMagnitude;
    }

    public void UpdateObject(float markerLength, Matrix4x4 fitARFoundationBackgroundMatrix, Matrix4x4 fitHelpersFlipMatrix, GameObject cam)
    {
        Matrix4x4 obj = Matrix4x4.identity;
        obj.SetColumn(3, new Vector4(-markerLength / 2, -markerLength / 2, 0.0f, 1.0f));
        Matrix4x4 tempMat = fitARFoundationBackgroundMatrix * ARM * obj;
        tempMat = fitHelpersFlipMatrix * tempMat;
        tempMat = cam.transform.localToWorldMatrix * tempMat;
        gameobject.SetMatrix4x4(tempMat);
    }

    public void CreateOrigin(float markerLength, Matrix4x4 fitARFoundationBackgroundMatrix, Matrix4x4 fitHelpersFlipMatrix, GameObject cam)
    {
        Matrix4x4 obj = Matrix4x4.identity;
        obj.SetColumn(3, new Vector4(-markerLength / 2, -markerLength / 2, 0.0f, 1.0f));
        ARM = fitARFoundationBackgroundMatrix * ARM * obj;
        ARM = fitHelpersFlipMatrix * ARM;
        ARM = cam.transform.localToWorldMatrix * ARM;
        
        this.origin = new Vector3(ARM.m03, ARM.m13, ARM.m23);
        //marker.gameobject.SetMatrix4x4(ARM);
        //this.origin = this.gameobject.transform.position;
    }

    public void CalculateAziAndEleAndDist(GameObject cam, out float azi, out float ele, out float dist) {
        //Matrix4x4 obj = Matrix4x4.identity;
        //obj.SetColumn(3, new Vector4(-markerLength / 2, -markerLength / 2, 0.0f, 1.0f));
        //ARM = fitARFoundationBackgroundMatrix * ARM * obj;
        //ARM = fitHelpersFlipMatrix * ARM;
        //ARM = cam.transform.localToWorldMatrix * ARM;
        //gameobject.SetMatrix4x4(ARM);

        Vector3 dir = cam.transform.position - gameobject.transform.position;
        dir.z *= -1f;
        
        azi = Mathf.Rad2Deg*Mathf.Atan2(dir.z, dir.x);
        if (azi < 0f)
            azi += 360f;
        if (azi > 360f)
            azi -= 360f;

        Vector3 dir2 = dir; dir2.y = 0f;
        ele = Mathf.Rad2Deg * Mathf.Atan2(dir.sqrMagnitude, dir2.sqrMagnitude);
        if (ele < 0f)
            ele += 360f;
        if (ele > 360f)
            ele -= 360f;

        dist = dir.sqrMagnitude;
    }
}

public class ArucoMarkerDetector : MonoBehaviour
{
    public Camera arCamera;
    //public CameraManager mCamManager;
    public Text mText;
    [HideInInspector]
    public Dictionary<int, ArucoMarker> mDictMarkers;

    ArUcoDictionary dictionaryId = ArUcoDictionary.DICT_6X6_250;
    
    public float markerLength;
    Mat rgbMat;
    Mat ids;
    [HideInInspector]
    public List<Mat> corners;
    
    List<Mat> rejectedCorners;
    Mat rvecs;
    Mat tvecs;
    Mat rotMat;
    DetectorParameters detectorParams;
    Dictionary dictionary;

    Mat camMatrix;
    Mat distCoeffs;
    Matrix4x4 fitARFoundationBackgroundMatrix;
    Matrix4x4 fitHelpersFlipMatrix;
    int width;
    int height;
    float widthScale;
    float heightScale;
    int mnFrameID;

    public ArucoMarker GetMarker(int id)
    {
        return mDictMarkers[id];
    }

    public float Calculate(Vector3 pos)
    {
        //float diff = Mathf.Infinity;
        //if (corners.Count > 0)
        //{
        //    try
        //    {
        //        var a = arCamera.transform.worldToLocalMatrix.MultiplyPoint3x4(pos);
        //        a = mCamManager.fitHelpersFlipMatrix.inverse.MultiplyPoint3x4(a);
        //        a = mCamManager.fitARFoundationBackgroundMatrix.inverse.MultiplyPoint3x4(a);
        //        Mat aMat = new Mat(3, 1, CvType.CV_64FC1);
        //        aMat.put(0, 0, a.x);
        //        aMat.put(1, 0, -a.y);
        //        aMat.put(2, 0, a.z);
        //        Mat proj = mCamManager.camMatrix * aMat;

        //        double depth = proj.get(2, 0)[0];
        //        float x = ((float)(proj.get(0, 0)[0] / depth));
        //        float y = ((float)(proj.get(1, 0)[0] / depth));
        //        Vector2 pt1 = new Vector2(x, y);
        //        Vector2 pt2 = new Vector2((float)corners[0].get(0, 0)[0], (float)corners[0].get(0, 0)[1]);
        //        var diffPt = pt1 - pt2;
        //        diff = diffPt.magnitude;

        //    }
        //    catch (Exception e)
        //    {
        //        mText.text = e.ToString();
        //    }
        //}

        //return diff;
        return 0f;


        //    Vector3 pt = corner3Ds[0];
        //    //pt.y *= -1f;

        //Imgproc.circle(rgbMat, new Point(x, y), 5, new Scalar(255, 0, 255), -1);
        //    mText.text = imgPt + ", " + corners[0].get(0, 0)[0] + " " + corners[0].get(0, 0)[1]+ " = " +(imgPt.y+ corners[0].get(0, 0)[1]);
    }

    bool WantsToQuit()
    {
        rgbMat.Dispose();
        ids.Dispose();
        rvecs.Dispose();
        tvecs.Dispose();
        rotMat.Dispose();
        foreach (var item in corners)
        {
            item.Dispose();
        }
        corners.Clear();
        mDictMarkers.Clear();
        return true;
    }

    void OnEnable()
    {
        ImageCatchEvent.frameReceived += OnCameraFrameReceived;
        CameraInitEvent.camInitialized += OnCameraInitialization;
    }

    void OnDisable()
    {
        ImageCatchEvent.frameReceived -= OnCameraFrameReceived;
        CameraInitEvent.camInitialized -= OnCameraInitialization;
    }

    void OnCameraInitialization(object Sender, CameraInitEventArgs e)
    {
        try {
            camMatrix = e.camMat;
            distCoeffs = e.distCoeffs;
            fitARFoundationBackgroundMatrix = e.fitARFoundationBackgroundMatrix;
            fitHelpersFlipMatrix = e.fitHelpersFlipMatrix;
            width = e.width;
            height = e.height;
            widthScale = e.widthScale;
            heightScale = e.heightScale;

            rgbMat = new Mat(height, width, CvType.CV_8UC3);
            ids = new Mat();
            corners = new List<Mat>();
            rejectedCorners = new List<Mat>();

            rvecs = new Mat();
            tvecs = new Mat();
            rotMat = new Mat(3, 3, CvType.CV_64FC1);

            detectorParams = DetectorParameters.create();
            dictionary = Aruco.getPredefinedDictionary((int)dictionaryId);
            mDictMarkers = new Dictionary<int, ArucoMarker>();
        }
        catch (Exception ex)
        {
            ex.ToString();
        }
    }

    void OnCameraFrameReceived(object sender, ImageCatchEventArgs e) {

        try
        {
            mnFrameID = e.mnFrameID;
            Imgproc.cvtColor(e.rgbMat, rgbMat, Imgproc.COLOR_RGBA2RGB);
            Aruco.detectMarkers(rgbMat, dictionary, corners, ids, detectorParams, rejectedCorners);

            if(ids.total() > 0)
            {
                Aruco.estimatePoseSingleMarkers(corners, markerLength, camMatrix, distCoeffs, rvecs, tvecs);
                for (int i = 0; i < ids.total(); i++)
                {
                    ////마커 객체 생성
                    int id = (int)ids.get(i, 0)[0];
                    ArucoMarker marker;
                    if (!mDictMarkers.ContainsKey(id))
                    {
                        marker = new ArucoMarker(id);
                        marker.gameobject = this.gameObject.AddComponent<ARGameObject>();
                        mDictMarkers.Add(id, marker);
                    }
                    marker = mDictMarkers[id];
                    ////마커 객체 생성

                    ///코너 갱신
                    marker.corners.Clear();
                    for (int j = 0; j < 4; j++)
                    {
                        float x = (float)corners[i].get(0, j)[0];
                        float y = (float)corners[i].get(0, j)[1];
                        //float x = widthScale * (float)corners[i].get(0, j)[0];
                        //float y = Screen.height - heightScale * (float)corners[i].get(0, j)[1];
                        marker.corners.Add(new Vector2(x, y));
                    }
                    //마커 프레임 아이디 갱신
                    marker.frameId = mnFrameID;

                    ///마커에서 포즈
                    Mat rvec = new Mat(rvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1));
                    Mat tvec = new Mat(tvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1));
                    marker.ARM = UpdateARObjectTransform(rvec, tvec);
                    marker.UpdateObject(0.18f, fitARFoundationBackgroundMatrix, fitHelpersFlipMatrix, Camera.main.gameObject);
                    MarkerDetectEvent.RunEvent(new MarkerDetectEventArgs(marker));
                }
            }
        }
        catch (Exception ex)
        {
            ex.ToString();
        }
    }

    private void EstimatePoseCanonicalMarker(Mat rgbMat)
    {
        try
        {
            Aruco.estimatePoseSingleMarkers(corners, markerLength, camMatrix, distCoeffs, rvecs, tvecs);

            for (int i = 0; i < ids.total(); i++)
            {
                
                using (Mat rvec = new Mat(rvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1)))
                using (Mat tvec = new Mat(tvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1)))
                {
                    // In this example we are processing with RGB color image, so Axis-color correspondences are X: blue, Y: green, Z: red. (Usually X: red, Y: green, Z: blue)
                    // Calib3d.drawFrameAxes(rgbMat, camMatrix, distCoeffs, rvec, tvec, markerLength * 0.5f);

                    if (i == 0)
                    {
                        //UpdateARObjectTransform(rvec, tvec, id, ref marker, corners[i]);
                    }
                    var ARM = UpdateARObjectTransform(rvec, tvec);

                    //마커 생성 또는 탐색
                    int id = (int)ids.get(i, 0)[0];
                    //mListIDs.Add(id);
                    ArucoMarker marker;
                    if (!mDictMarkers.ContainsKey(id))
                    {
                        marker = new ArucoMarker(id);
                        marker.gameobject = this.gameObject.AddComponent<ARGameObject>();
                        mDictMarkers.Add(id, marker);
                    }
                    marker = mDictMarkers[id];
                    marker.ARM = ARM;
                    //마커 생성 또는 탐색

                    //마커 3차원 origin 복원. ARCore 성능 검증용
                    Matrix4x4 obj = Matrix4x4.identity;
                    obj.SetColumn(3, new Vector4(-markerLength / 2, -markerLength / 2, 0.0f, 1.0f));
                    ARM = fitARFoundationBackgroundMatrix * ARM * obj;
                    ARM = fitHelpersFlipMatrix * ARM;
                    ARM = arCamera.transform.localToWorldMatrix * ARM;

                    //marker.gameobject.SetMatrix4x4(ARM);
                    ARUtils.SetTransformFromMatrix(marker.gameobject.transform, ref ARM);

                    //marker.origin = new Vector3(ARM.m03, ARM.m13, ARM.m23);
                    marker.origin = marker.gameobject.transform.position;
                    //마커 3차원 origin 복원. ARCore 성능 검증용
                    //마커 코너 저장
                    marker.corners.Clear();
                    for (int j = 0; j < 4; j++)
                    {
                        float x = (float)corners[i].get(0, j)[0];
                        float y = (float)corners[i].get(0, j)[1];
                        //float x = widthScale * (float)corners[i].get(0, j)[0];
                        //float y = Screen.height - heightScale * (float)corners[i].get(0, j)[1];
                        marker.corners.Add(new Vector2(x, y));
                    }
                    marker.frameId = mnFrameID;
                    //마커 코너 저장
                    //마커 이벰ㄴ트
                    MarkerDetectEvent.RunEvent(new MarkerDetectEventArgs(marker));
                }
                
               
                //mDictMarkers[id] = marker;
                //mText.text = marker.origin.ToString()+marker.gameobject.transform.position.ToString();
            }
        }
        catch (Exception e)
        {
            mText.text = e.ToString();
        }

    }

    //마커와 카메라 사이의 포즈 계산
    private Matrix4x4 UpdateARObjectTransform(Mat rvec, Mat tvec)
    {
        Matrix4x4 ARM;
        // Convert to unity pose data.
        double[] rvecArr = new double[3];
        rvec.get(0, 0, rvecArr);
        double[] tvecArr = new double[3];
        tvec.get(0, 0, tvecArr);
        PoseData poseData = ARUtils.ConvertRvecTvecToPoseData(rvecArr, tvecArr);
        ARM = ARUtils.ConvertPoseDataToMatrix(ref poseData, true);
        
        //// Convert to transform matrix.
        //try
        //{
            
            
        //    //// Apply the effect (flipping factors) of the projection matrix applied to the ARCamera by the ARFoundationBackground component to the ARM.
        //    //ARM = fitARFoundationBackgroundMatrix * ARM * obj;

        //    //// When detecting the AR marker from a horizontal inverted image (front facing camera),
        //    //// will need to apply an inverted X matrix to the transform matrix to match the ARFoundationBackground component display.
        //    //ARM = fitHelpersFlipMatrix * ARM;

        //    //ARM = arCamera.transform.localToWorldMatrix * ARM;

        //    //marker.gameobject.SetMatrix4x4(ARM);


        //    //mText.text = fitARFoundationBackgroundMatrix.ToString()+"\n"+fitHelpersFlipMatrix.ToString();

        //    //if (enableLerpFilter)
        //    //{
        //    //    arGameObject.SetMatrix4x4(ARM);
        //    //}
        //    //else
        //    //{
        //    //    ARUtils.SetTransformFromMatrix(arGameObject.transform, ref ARM);
        //    //}

        //    //mText.text = poseData.pos.ToString();
        //    mText.text = "markder detection~~";
        //}
        //catch (Exception e)
        //{
        //    mText.text = e.ToString();
        //}
        return ARM;
    }

    void Awake()
    {
        try {
            
        } catch (Exception e)
        {
            mText.text = e.ToString();
        }
        

    }
    // Start is called before the first frame update
    void Start()
    {
        Application.wantsToQuit += WantsToQuit;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
