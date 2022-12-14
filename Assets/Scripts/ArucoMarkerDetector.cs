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

public class MarkerDetectEventArgs2 : EventArgs
{
    public MarkerDetectEventArgs2(int _frameid,float[] data, Vector3 _pos)
    {
        mnFrameID = _frameid;
        marker_data = data;
        pos = _pos;
    }
    public int mnFrameID { get; set; }
    public float[] marker_data { get; set; }
    public Vector3 pos { get; set; }
}

class MarkerDetectEvent2
{
    public static event EventHandler<MarkerDetectEventArgs2> markerDetected;
    public static void RunEvent(MarkerDetectEventArgs2 e)
    {
        if (markerDetected != null)
        {
            markerDetected(null, e);
        }
    }
}

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
    public int frameId; //?????? ???????? ???? ???????? ??????
    public List<Vector2> corners; //???? ?????????? ???? ????
    public Vector3 origin; //arcore ????.
    public Vector3 origin2;//?? ???????? ????
    public ARGameObject gameobject; //????????. ?????? azi, ele?? ???? ?????? ?? ????
    public Matrix4x4 ARM;
    public bool mbCreate;
    public int nUpdated;
    public ArucoMarker(){ 
        corners = new List<Vector2>();
        mbCreate = false;
        nUpdated = 0;
    }
    public ArucoMarker(int _id)
    {
        nUpdated = 0;
        id = _id;
        corners = new List<Vector2>();
        mbCreate = false;
    }
    public float Calculate(Matrix4x4 P, Mat K, Vector3 pos, Vector2 corner, bool bFlip)
    {
        //?????? ???????????? y?? ?????? ?????????? ???????? ??.
        //opencv ???????????? ???????? ????.
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
        nUpdated++;
    }

    public void CreateOrigin(float markerLength, Matrix4x4 fitARFoundationBackgroundMatrix, Matrix4x4 fitHelpersFlipMatrix, GameObject cam)
    {
        //Matrix4x4 obj = Matrix4x4.identity;
        //obj.SetColumn(3, new Vector4(-markerLength / 2, -markerLength / 2, 0.0f, 1.0f));
        //ARM = fitARFoundationBackgroundMatrix * ARM * obj;
        //ARM = fitHelpersFlipMatrix * ARM;
        //ARM = cam.transform.localToWorldMatrix * ARM;
        
        //this.origin = new Vector3(ARM.m03, ARM.m13, ARM.m23);
        
        //marker.gameobject.SetMatrix4x4(ARM);
        this.origin = this.gameobject.transform.position;
    }

    public void CalculateAziAndEleAndDist(Vector3 center, out float azi, out float ele, out float dist) {
        //Matrix4x4 obj = Matrix4x4.identity;
        //obj.SetColumn(3, new Vector4(-markerLength / 2, -markerLength / 2, 0.0f, 1.0f));
        //ARM = fitARFoundationBackgroundMatrix * ARM * obj;
        //ARM = fitHelpersFlipMatrix * ARM;
        //ARM = cam.transform.localToWorldMatrix * ARM;
        //gameobject.SetMatrix4x4(ARM);

        Vector3 dir = center - gameobject.transform.position;
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
                float[] fdata = new float[ids.total()*3+1];
                fdata[0] = (float)ids.total();
                Aruco.estimatePoseSingleMarkers(corners, markerLength, camMatrix, distCoeffs, rvecs, tvecs);
                for (int i = 0; i < ids.total(); i++)
                {
                    ////???? ???? ????
                    int id = (int)ids.get(i, 0)[0];
                    ArucoMarker marker;
                    if (!mDictMarkers.ContainsKey(id))
                    {
                        marker = new ArucoMarker(id);
                        marker.gameobject = this.gameObject.AddComponent<ARGameObject>();
                        mDictMarkers.Add(id, marker);
                    }
                    marker = mDictMarkers[id];
                    ////???? ???? ????

                    ///???? ????
                    marker.corners.Clear();
                    for (int j = 0; j < 4; j++)
                    {
                        float x = (float)corners[i].get(0, j)[0];
                        float y = (float)corners[i].get(0, j)[1];
                        //float x = widthScale * (float)corners[i].get(0, j)[0];
                        //float y = Screen.height - heightScale * (float)corners[i].get(0, j)[1];
                        marker.corners.Add(new Vector2(x, y));
                    }
                    //???? ?????? ?????? ????
                    marker.frameId = mnFrameID;

                    ///???????? ????
                    Mat rvec = new Mat(rvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1));
                    Mat tvec = new Mat(tvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1));
                    marker.ARM = UpdateARObjectTransform(rvec, tvec);
                    //???? ?????? ?????? 3???? ?????? ?????????? ??????.
                    marker.UpdateObject(markerLength, fitARFoundationBackgroundMatrix, fitHelpersFlipMatrix, Camera.main.gameObject);
                    MarkerDetectEvent.RunEvent(new MarkerDetectEventArgs(marker));

                    ////???????? ????
                    fdata[i * 3 + 1] = (float)id;
                    fdata[i * 3 + 2] = marker.corners[0].x;
                    fdata[i * 3 + 3] = marker.corners[0].y;

                }//for
                ////?????? ???? ???? ?????? ????
                {
                    MarkerDetectEvent2.RunEvent(new MarkerDetectEventArgs2(mnFrameID,fdata, Camera.main.transform.position));
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

                    //???? ???? ???? ????
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
                    //???? ???? ???? ????

                    //???? 3???? origin ????. ARCore ???? ??????
                    Matrix4x4 obj = Matrix4x4.identity;
                    obj.SetColumn(3, new Vector4(-markerLength / 2, -markerLength / 2, 0.0f, 1.0f));
                    ARM = fitARFoundationBackgroundMatrix * ARM * obj;
                    ARM = fitHelpersFlipMatrix * ARM;
                    ARM = arCamera.transform.localToWorldMatrix * ARM;

                    //marker.gameobject.SetMatrix4x4(ARM);
                    ARUtils.SetTransformFromMatrix(marker.gameobject.transform, ref ARM);

                    //marker.origin = new Vector3(ARM.m03, ARM.m13, ARM.m23);
                    marker.origin = marker.gameobject.transform.position;
                    //???? 3???? origin ????. ARCore ???? ??????
                    //???? ???? ????
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
                    //???? ???? ????
                    //???? ????????
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

    //?????? ?????? ?????? ???? ????
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
