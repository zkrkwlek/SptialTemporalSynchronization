using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using System;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CameraImageManager : MonoBehaviour
{

    //public void RunEvent()
    //{
    //    if (eventImage != null)
    //    {
    //        eventImage();
    //    }
    //}


    //public DataSender sender;
    //public SystemManager mSystemManager;
    //public ScaleAdjuster mScaleAdjuster;
    public ARCameraManager cameraManager;

    //public Text mText;
    //[HideInInspector]
    //public Pose mPose = new Pose();
    
    [HideInInspector]
    public Texture2D m_Texture;
    [HideInInspector]
    public int mnBufferSize = 0;
    [HideInInspector]
    public int mnFrame = 1;
    int mnSkipFrame = 4;
    public Mat rgbaMat;

    XRCpuImage.ConversionParams conversionParams;

    bool WantsToQuit()
    {
        //delete m_Texture;
        return true;
    }

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }
    
    unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)  
    {
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) {
            mnBufferSize = 0;
            return;
        }
        //var conversionParams = new XRCpuImage.ConversionParams
        //{
        //    // Get the entire image.
        //    inputRect = new RectInt(0, 0, image.width, image.height),

        //    // Downsample by 2.
        //    //outputDimensions = new Vector2Int(image.width / 2, image.height / 2),
        //    outputDimensions = new Vector2Int(image.width, image.height),
        //    // Choose RGBA format.
        //    outputFormat = TextureFormat.RGBA32,

        //    // Flip across the vertical axis (mirror image).
        //    transformation = XRCpuImage.Transformation.MirrorY

        //};

        // See how many bytes you need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image.
        //var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        var length = (int)rgbaMat.total() * (int)rgbaMat.elemSize();
        IntPtr addr = (IntPtr)rgbaMat.dataAddr();
        image.Convert(conversionParams, addr, length);
        
        // The image was converted to RGBA32 format and written into the provided buffer
        // so you can dispose of the XRCpuImage. You must do this or it will leak resources.
        image.Dispose();

        // At this point, you can process the image, pass it to a computer vision algorithm, etc.
        // In this example, you apply it to a texture to visualize it.

        // You've got the data; let's put it into a texture so you can visualize it.

        //m_Texture = new Texture2D(
        //    conversionParams.outputDimensions.x,
        //    conversionParams.outputDimensions.y,
        //    conversionParams.outputFormat,
        //    false);
        //m_Texture.LoadRawTextureData(addr, length);
        //m_Texture.Apply();
        Utils.fastMatToTexture2D(rgbaMat, m_Texture);
        mnBufferSize = length;
        //m_Texture.LoadRawTextureData(buffer);
        //m_Texture.Apply();
        //mnBufferSize = buffer.Length;
        //buffer.Dispose();

        ////포즈 업데이트
        //float angle = 0.0f;
        //Vector3 _axis = Vector3.zero;
        //Camera.main.transform.rotation.ToAngleAxis(out angle, out _axis);
        //angle = angle * Mathf.Deg2Rad;
        //_axis = angle * _axis;
        //Vector3 _c = Camera.main.transform.position;
        //mPose.Update(ref _axis, ref _c);
        ////포즈 업데이트

        //tryb 
        //{
        //    if (mnFrame % mnSkipFrame == 0)
        //    {
        //        { 
        //            float angle = 0.0f;
        //            Vector3 _axis = Vector3.zero;
        //            Camera.main.transform.rotation.ToAngleAxis(out angle, out _axis);
        //            angle = angle * Mathf.Deg2Rad;
        //            _axis = angle * _axis;
        //            Vector3 _c = Camera.main.transform.position;

        //            if(mScaleAdjuster.mbScaleAdjustment)
        //                mScaleAdjuster.SetDevicePose(mnFrame, ref _axis, ref _c);

        //            //Matrix3x3 R = Matrix3x3.EXP(_axis).Transpose();
        //            //Vector3 t = -(R * _c);
        //            //mText.text = Camera.main.transform.worldToLocalMatrix.ToString()+"\n"+R.ToString()+"\n"+t.x+" "+t.y+" "+t.z;
        //        }
        //        byte[] bdata = m_Texture.EncodeToJPG(mSystemManager.AppData.JpegQuality);
        //        var timeSpan = DateTime.UtcNow - mSystemManager.StartTime;
        //        double ts = timeSpan.TotalMilliseconds;

        //        ////서버로 전송
        //        UdpData idata = new UdpData("Image", mSystemManager.User.UserName, mnFrame, bdata, ts);

        //        ////테스트 이미지 전송
        //        //StartCoroutine(sender.SendData(idata));
        //        StartCoroutine(sender.SendData("http://143.248.6.143:50001/predict", idata));
        //        //mText.text = SystemManager.Instance.AppData.Address+ " " + bdata.Length + " " + ts;
        //    }
        //}
        //catch(Exception e)
        //{
        //    mText.text = e.ToString();
        //}

        ////save data
        //byte[] bytes = m_Texture.EncodeToPNG();
        ////var 
        ///
        /// 
        /// 
        /// = Application.persistentDataPath + "/../../../../Download/ARFoundation/save";
        //var dirPath = Application.persistentDataPath + "/save";
        //if (!Directory.Exists(dirPath))
        //{
        //    Directory.CreateDirectory(dirPath);
        //}
        //File.WriteAllBytes(dirPath + "/c_" + (mnFrame) + ".png", bytes);

        //giver.RunEvent();
        
        ImageCatchEvent.RunEvent(new ImageCatchEventArgs(rgbaMat, mnFrame++));
    }

    void Start()
    {
        var height = 360;
        var width = 640;
        Application.wantsToQuit += WantsToQuit;
        conversionParams = new XRCpuImage.ConversionParams
        {
            // Get the entire image.
            inputRect = new RectInt(0, 0, width, height),

            // Downsample by 2.
            //outputDimensions = new Vector2Int(image.width / 2, image.height / 2),
            outputDimensions = new Vector2Int(width, height),
            // Choose RGBA format.
            outputFormat = TextureFormat.RGBA32,
             
            // Flip across the vertical axis (mirror image).
            transformation = XRCpuImage.Transformation.None //mirrorx

        };
        rgbaMat = new Mat(height, width, CvType.CV_8UC4);
        m_Texture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false);

        //giver = new ImageCatchEvent();
    }
}
