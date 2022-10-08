using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//public class Pose
//{
//    public Pose(int id = 0) {
//        R = new Matrix3x3();
//        t = Vector3.zero;
//    }
//    //기기의 자세로부터 생성시 : axis = Rwc, _t는 center임.
//    public Pose(int id, ref Vector3 axis, ref Vector3 _c) {
//        mnID = id;
//        R = Matrix3x3.EXP(axis).Transpose();
//        c = new Vector3(_c.x, _c.y, _c.z);
//        t = -(R * c);
//    }

//    //서버에서 전송받을 때 : Rs, ts임.
//    public Pose(int id, ref float[] fdata)
//    {
//        mnID = id;
//        R = new Matrix3x3(fdata[0], fdata[1], fdata[2], fdata[3], fdata[4], fdata[5], fdata[6], fdata[7], fdata[8]);
//        t = new Vector3(fdata[9], fdata[10], fdata[11]);
//        c = -(R.Transpose() * t);
//    }
//    public void Update(ref Vector3 axis, ref Vector3 _c)
//    {
//        R = Matrix3x3.EXP(axis).Transpose();
//        c = new Vector3(_c.x, _c.y, _c.z);
//        t = -(R * c);
//    }

//    public Pose Copy() {
//        Pose P = new Pose();
//        P.mnID = this.mnID;
//        P.R = new Matrix3x3(this.R.m00, this.R.m01, this.R.m02, this.R.m10, this.R.m11, this.R.m12, this.R.m20, this.R.m21, this.R.m22);
//        P.t = new Vector3(this.t.x, this.t.y, this.t.z);
//        P.c  = new Vector3(this.c.x, this.c.y, this.c.z);
//        return P;
//    }
//    public string ToString() {
//        return "ID = "+mnID+" "+R.ToString() + t.ToString();
//    }
//    public int mnID;
//    public Matrix3x3 R; //rotation
//    public Vector3 t; //translation
//    public Vector3 c; //center
//}

//public class ScaleAdjuster : MonoBehaviour
//{

//    public Text mText;

//    ////기기에서 추정한 자세
//    //Pose prevDevicePose = new Pose();
//    //Pose currDevicePose = new Pose();

//    ////서버로부터 전송받은 자세
//    //Pose prevServerPose = new Pose();
//    //Pose currServerPose = new Pose();

//    int currID, prevID;
//    Dictionary<int, Pose> ServerPoseDictionary = new Dictionary<int, Pose>();
//    Dictionary<int, Pose> DevicePoseDictionary = new Dictionary<int, Pose>();

//    Matrix3x3 Ty = new Matrix3x3(1f, 0f, 0f, 0f, -1f, 0f, 0f, 0f, 1f);
//    public Pose Tlg = new Pose();


//    //스케일
//    KalmanFilter filter = new KalmanFilter();
//    public float mfScale = 1.0f;
//    public bool mbScaleAdjustment = false;

//    // Start is called before the first frame update
//    void Start()
//    {
//        enabled = false;
//        currID = 0;
//        prevID = 0;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//    }

//    ///axis = normalized*angle
//    public void SetDevicePose(int id, ref Vector3 _axis, ref Vector3 _t)
//    {
//        try
//        {
            
//            var pose = new Pose(id, ref _axis, ref _t);
//            DevicePoseDictionary.Add(id, pose);
//            //if (!mbScaleAdjustment)
//            //{
//            ////prevDevicePose = currDevicePose.Copy();
//            ////currDevicePose = new Pose(id, ref _axis, ref _t);    
//            //}
//        }
//        catch (Exception e)
//        {
//            mText.text = e.ToString();
//        }
//    }

//    public void SetServerPose(int id, ref float[] fdata)
//    {
//        try
//        {
//            var pose = new Pose(id, ref fdata);
//            ServerPoseDictionary.Add(id, pose);
//            currID = id;

//            //if (!mbScaleAdjustment)
//            //{
//            //    //prevServerPose = currServerPose.Copy();
//            //    //currServerPose = new Pose(id, ref fdata);
//            //}
//        }
//        catch (Exception e) 
//        {
//            mText.text = e.ToString();
//        }
        
//    }
//    public void CalculateScale(int id)
//    {
//        Pose currDevicePose = DevicePoseDictionary[id];
//        Pose currServerPose = ServerPoseDictionary[id];
//        if (mbScaleAdjustment) {

//            Matrix3x3 R = mfScale * (currServerPose.R);
//            Vector3 t = mfScale * currServerPose.t;
//            //Matrix3x3 R = (currServerPose.R);
//            //Vector3 t = currServerPose.t;

//            ////y축 변환
//            R = Ty * R;
//            t.y = -t.y;

//            ////현재 기기의 좌표계로
//            Tlg.R = currDevicePose.R.Transpose() * R;
//            Tlg.t = currDevicePose.R.Transpose() * t + currDevicePose.c;
//            Tlg.c = -(Tlg.R.Transpose() * Tlg.t);
//            //mText.text = "Cetner = " +id +" = "+ Tlg.c + " " + currDevicePose.c + " " + currServerPose.c + "\n" + "Scale = " + string.Format("{0:0.000} ", mfScale);
//            return;
//        }

        
//        float max_dist = 0f;
//        foreach (int tid in DevicePoseDictionary.Keys)
//        {
//            if (tid == id)
//                continue;
//            Pose prevDevicePose = DevicePoseDictionary[tid];
//            Pose prevServerPose = ServerPoseDictionary[tid];

//            float dist_device = (currDevicePose.c - prevDevicePose.c).magnitude;
//            float dist_server = (currServerPose.c - prevServerPose.c).magnitude;
            
//            if(dist_device > max_dist)
//            {
//                max_dist = dist_device;
//            }

//            if (dist_device > 0.2)
//            {
//                mfScale = dist_device / dist_server;
//                ////스케일 보정
//                Matrix3x3 R = mfScale * (currServerPose.R);
//                Vector3 t = mfScale * currServerPose.t;
//                //Matrix3x3 R = (currServerPose.R);
//                //Vector3 t = currServerPose.t;

//                ////y축 변환
//                R = Ty * R;
//                t.y = -t.y;

//                ////현재 기기의 좌표계로
//                Tlg.R = currDevicePose.R.Transpose() * R;
//                Tlg.t = currDevicePose.R.Transpose() * t + currDevicePose.c;
//                Tlg.c = -(Tlg.R.Transpose() * Tlg.t);
//                //mText.text = "Tlg = " + Tlg.t + " Td = " + currDevicePose.t + " Ts = " + currServerPose.t + "\n" + "Cetner = " + Tlg.c + " " + currDevicePose.c + " " + currServerPose.c + "\n" + "Scale = " + string.Format("{0:0.000} ", mfScale) + " || " + string.Format("{0:0.000} ", dist_device) + " " + string.Format("{0:0.000} ", dist_server);
//                //mText.text = "Scale = " + mfScale;
//                mbScaleAdjustment = true;
//            }
//            if (mbScaleAdjustment)
//                break;
//        }

//        if (!mbScaleAdjustment)
//            mText.text = "Scale Test = " + max_dist;
//    }
//    public void CalculateScale() {
//        //smText.text = "ScaleAdjuster::Calculate="+prevDevicePose.mnID + " " + currDevicePose.mnID + " " + prevServerPose.mnID + " " + currServerPose.mnID;

//        //if(prevDevicePose.mnID > 0)
//        //{
//        //    float d_device = (currDevicePose.c - prevDevicePose.c).magnitude;
//        //    float d_server = (currServerPose.c - prevServerPose.c).magnitude;
//        //    mfScale = filter.Update(d_device / d_server);
//        //    //mfScale = currDevicePose.t.magnitude / currServerPose.t.magnitude;
//        //    mbScaleAdjustment = true;
            
//        //    //////스케일 보정
//        //    //Matrix3x3 R = mfScale * (currServerPose.R.Transpose());
//        //    //Vector3 t = mfScale*currServerPose.c;

//        //    //////y축 변환
//        //    //R = Ty * R;
//        //    //t.y = -t.y;

//        //    //////현재 기기의 좌표계로
//        //    //Tlg.R = currDevicePose.R * R;
//        //    //Tlg.t = currDevicePose.R * t + currDevicePose.t;
//        //    //Tlg.c = -(Tlg.R.Transpose() * Tlg.t);
//        //    //mText.text = "Tlg = " + Tlg.t + " Td = " + currDevicePose.t+" Ts = "+currServerPose.t +"\n"+"Cetner = "+Tlg.c+" "+currDevicePose.c+" "+currServerPose.c;
//        //    ////mText.text = "Scale = " + mfScale;

//        //    ////스케일 보정
//        //    //Matrix3x3 R = mfScale * (currServerPose.R);
//        //    //Vector3 t = mfScale * currServerPose.t;
//        //    Matrix3x3 R = (currServerPose.R);
//        //    Vector3 t = currServerPose.t;

//        //    ////y축 변환
//        //    R = Ty * R;
//        //    t.y = -t.y;

//        //    ////현재 기기의 좌표계로
//        //    Tlg.R = currDevicePose.R.Transpose() * R;
//        //    Tlg.t = currDevicePose.R.Transpose() * t + currDevicePose.c;
//        //    Tlg.c = -(Tlg.R.Transpose() * Tlg.t);
//        //    mText.text = "Tlg = " + Tlg.t + " Td = " + currDevicePose.t + " Ts = " + currServerPose.t + "\n" + "Cetner = " + Tlg.c + " " + currDevicePose.c + " " + currServerPose.c+"\n"+"Scale = "+ string.Format("{0:0.000} ", mfScale) + " || "+ string.Format("{0:0.000} ", d_device) + " "+ string.Format("{0:0.000} ", d_server);
//        //    //mText.text = "Scale = " + mfScale;
//        //}
//    }



//}

