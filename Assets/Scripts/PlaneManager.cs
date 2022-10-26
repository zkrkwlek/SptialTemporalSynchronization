using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UVR_Plane
{
    public Plane plane;
    public int mnTTL;
    public int mnId;
    public UVR_Plane(int id, float x, float y, float z, float d, int _skip = 4)
    {
        mnId = id;
        Vector3 normal = new Vector3(x, y, z);
        plane = new Plane(normal, d);
        mnTTL = _skip;
    }
    public Vector3 CreatePoint(Vector3 origin, Vector3 dir) {
        float a = Vector3.Dot(plane.normal, -dir);
        float u = (Vector3.Dot(plane.normal, origin)+plane.distance)/a;
        return origin + dir * u;

        //cv::Mat x3D = cv::Mat::ones(1, 3, CV_32FC1);
        //x3D.at<float>(0) = x;
        //x3D.at<float>(1) = y;

        //cv::Mat R, t;
        //pCameraPose->GetPose(R, t);

        //cv::Mat Xw = pCamera->Kinv * x3D.t();
        //Xw.push_back(cv::Mat::ones(1, 1, CV_32FC1)); //3x1->4x1
        //Xw = pCameraPose->GetInversePose() * Xw; // 4x4 x 4 x 1
        //float testaaasdf = Xw.at<float>(3);
        //Xw = Xw.rowRange(0, 3) / Xw.at<float>(3); // 4x1 -> 3x1
        //cv::Mat Ow = pCameraPose->GetCenter(); // 3x1
        //cv::Mat dir = Xw - Ow; //3x1

        //bool bres = false;
        //auto planes = LocalMapPlanes.Get();
        //float min_val = 10000.0;
        //cv::Mat min_param;
        //for (auto iter = planes.begin(), iend = planes.end(); iter != iend; iter++)
        //{
        //    cv::Mat param = iter->second; //4x1
        //    cv::Mat normal = param.rowRange(0, 3); //3x1
        //    float dist = param.at<float>(3);
        //    float a = normal.dot(-dir);
        //    if (std::abs(a) < 0.000001)
        //        continue;
        //    float u = (normal.dot(Ow) + dist) / a;
        //    if (u > 0.0 && u < min_val)
        //    {
        //        min_val = u;
        //        min_param = param;
        //    }

        //}
        //if (min_val < 10000.0)
        //{
        //    _pos = Ow + dir * min_val;
        //    bres = true;
        //}

    }
}

public class PlaneEventArgs : EventArgs
{
    public PlaneEventArgs(Plane p)
    {
        plane = p;
    }
    public Plane plane { get; set; }
}

class PlaneDetectionEvent
{
    public static event EventHandler<PlaneEventArgs> planeDetected;
    public static void RunEvent(PlaneEventArgs e)
    {
        if (planeDetected != null)
        {
            planeDetected(null, e);
        }
    }
}

public class PlaneManager : MonoBehaviour
{
    /// <summary>
    /// 실험용 임시
    /// </summary>
    public Text mText;
    public UVR_Plane Pfloor;
    //Dictionary<int, UVR_Plane> Planes;
    List<UVR_Plane> Planes;

    // Start is called before the first frame update
    void Awake()
    {
        //Planes = new Dictionary<int, UVR_Plane>();
        Planes = new List<UVR_Plane>();
        Pfloor = new UVR_Plane(0, 0f,0f,0f,0f);
    }

    // Update is called once per frame
    void Update()
    {
        //var planes = Planes.Values;
        ////TTL 관리
        foreach(UVR_Plane p in Planes)
        {
            p.mnTTL--;
            if(p.mnTTL <= 0)
            {
                //삭제 및 종료
            }
        }
    }

    public void AddPlane(int id, float x, float y, float z, float d, int _skip = 4) {
        var p = new UVR_Plane(id, x, y, z, d, _skip);
        Planes.Add(p);
    }

    public void UpdatePlane(float x, float y, float z, float d)
    {
        Pfloor.plane.normal = new Vector3(x, y, z);
        Pfloor.plane.distance = d;

        //PlaneEventArgs args = new PlaneEventArgs();
        //args.plane = Pfloor.plane;
        PlaneDetectionEvent.RunEvent(new PlaneEventArgs(Pfloor.plane));
        //mText.text = "plane detection event";
    }

    
    ////가장 작은 애 찾기

    //추후 업데이트 필요함
}
