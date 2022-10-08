using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARFoundationWithOpenCVForUnityExample
{
    public class ARFoundationCameraToMat : MonoBehaviour
    {
        public ARCameraManager cameraManager;
        [SerializeField, TooltipAttribute("The ARCamera.")]
        public Camera arCamera;
        [SerializeField]
        public Camera mainCamera = default;
        [HideInInspector]
        public Texture2D texture;
        public Mat rgbaMat;
        Mat rotatedFrameMat;

        bool hasInitDone = false;
        bool isPlaying = true;
        ScreenOrientation screenOrientation;
        int displayRotationAngle = 0;
        bool displayFlipVertical = false;
        bool displayFlipHorizontal = false;
        FpsMonitor fpsMonitor; //상태 봐서 삭제

        public Text mText;

        bool WantsToQuit()
        {
            //delete m_Texture;
            Dispose();
            return true;
        }

        void OnEnable()
        {
            if (cameraManager != null)
            {
                cameraManager.frameReceived += OnCameraFrameReceived;
            }
        }

        void OnDisable()
        {
            if (cameraManager != null)
            {
                cameraManager.frameReceived -= OnCameraFrameReceived;
            }
        }

        unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            if ((cameraManager == null) || (cameraManager.subsystem == null) || !cameraManager.subsystem.running)
                return;
            if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            {
                return;
            }
            int width = image.width;
            int height = image.height;
            
            try {
                if (!hasInitDone || rgbaMat == null || rgbaMat.cols() != width || rgbaMat.rows() != height || screenOrientation != Screen.orientation)
                {
                    Dispose();

                    screenOrientation = Screen.orientation;

                    XRCameraConfiguration config = (XRCameraConfiguration)cameraManager.currentConfiguration;
                    int framerate = config.framerate.HasValue ? config.framerate.Value : -1;
                    if (eventArgs.displayMatrix.HasValue)
                    {
                        Matrix4x4 cameraMatrix = eventArgs.displayMatrix ?? Matrix4x4.identity;

                        Vector2 affineBasisX = new Vector2(1.0f, 0.0f);
                        Vector2 affineBasisY = new Vector2(0.0f, 1.0f);
                        Vector2 affineTranslation = new Vector2(0.0f, 0.0f);

#if UNITY_IOS
                        affineBasisX = new Vector2(cameraMatrix[0, 0], cameraMatrix[1, 0]);
                        affineBasisY = new Vector2(cameraMatrix[0, 1], cameraMatrix[1, 1]);
                        affineTranslation = new Vector2(cameraMatrix[2, 0], cameraMatrix[2, 1]);
#endif // UNITY_IOS
#if UNITY_ANDROID
                        affineBasisX = new Vector2(cameraMatrix[0, 0], cameraMatrix[0, 1]);
                        affineBasisY = new Vector2(cameraMatrix[1, 0], cameraMatrix[1, 1]);
                        affineTranslation = new Vector2(cameraMatrix[0, 2], cameraMatrix[1, 2]);
#endif // UNITY_ANDROID

                        affineBasisX = affineBasisX.normalized;
                        affineBasisY = affineBasisY.normalized;
                        Matrix4x4 m_DisplayRotationMatrix = Matrix4x4.identity;
                        m_DisplayRotationMatrix = Matrix4x4.identity;
                        m_DisplayRotationMatrix[0, 0] = affineBasisX.x;
                        m_DisplayRotationMatrix[0, 1] = affineBasisY.x;
                        m_DisplayRotationMatrix[1, 0] = affineBasisX.y;
                        m_DisplayRotationMatrix[1, 1] = affineBasisY.y;

#if UNITY_IOS
                        Matrix4x4 FlipYMatrix = Matrix4x4.Scale(new Vector3(1, -1, 1));
                        m_DisplayRotationMatrix = FlipYMatrix.inverse * m_DisplayRotationMatrix;
#endif // UNITY_IOS

                        displayRotationAngle = (int)ARUtils.ExtractRotationFromMatrix(ref m_DisplayRotationMatrix).eulerAngles.z;
                        Vector3 localScale = ARUtils.ExtractScaleFromMatrix(ref m_DisplayRotationMatrix);
                        displayFlipVertical = Mathf.Sign(localScale.y) == -1;
                        displayFlipHorizontal = Mathf.Sign(localScale.x) == -1;


                        if (fpsMonitor != null)
                        {
                            fpsMonitor.Add("displayMatrix", "\n" + eventArgs.displayMatrix.ToString());
                            fpsMonitor.Add("displayRotationAngle", displayRotationAngle.ToString());
                            fpsMonitor.Add("displayFlipVertical", displayFlipVertical.ToString());
                            fpsMonitor.Add("displayFlipHorizontal", displayFlipHorizontal.ToString());
                        }
                    }
                    rgbaMat = new Mat(height, width, CvType.CV_8UC4);

                    if (displayRotationAngle == 90 || displayRotationAngle == 270)
                    {
                        width = image.height;
                        height = image.width;

                        rotatedFrameMat = new Mat(height, width, CvType.CV_8UC4);
                    }

                    texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

                    gameObject.GetComponent<Renderer>().material.mainTexture = texture;

                    gameObject.transform.localScale = new Vector3(width, height, 1);
                    Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

                    float widthScale = (float)Screen.width / width;
                    float heightScale = (float)Screen.height / height;
                    if (widthScale < heightScale)
                    {
                        mainCamera.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
                    }
                    else
                    {
                        mainCamera.orthographicSize = height / 2;
                    }

                    hasInitDone = true;


                    if (fpsMonitor != null)
                    {
                        fpsMonitor.Add("width", image.width.ToString());
                        fpsMonitor.Add("height", image.height.ToString());
                        fpsMonitor.Add("framerate", framerate.ToString());
                        fpsMonitor.Add("format", image.format.ToString());
                        fpsMonitor.Add("orientation", Screen.orientation.ToString());
                    }

                }
                if (hasInitDone && isPlaying)
                {
                    XRCpuImage.ConversionParams conversionParams = new XRCpuImage.ConversionParams(image, TextureFormat.RGBA32, XRCpuImage.Transformation.None);
                    image.Convert(conversionParams, (IntPtr)rgbaMat.dataAddr(), (int)rgbaMat.total() * (int)rgbaMat.elemSize());

                    DisplayImage();

                    if (fpsMonitor != null)
                    {
                        fpsMonitor.Add("currentFacingDirection", cameraManager.currentFacingDirection.ToString());
                        fpsMonitor.Add("autoFocusEnabled", cameraManager.autoFocusEnabled.ToString());
                        fpsMonitor.Add("currentLightEstimation", cameraManager.currentLightEstimation.ToString());
                    }

                    if (cameraManager.TryGetIntrinsics(out var cameraIntrinsics))
                    {
                        var focalLength = cameraIntrinsics.focalLength;
                        var principalPoint = cameraIntrinsics.principalPoint;

                        if (fpsMonitor != null)
                        {
                            fpsMonitor.Add("cameraIntrinsics", "\n" + "FL: " + focalLength.x + "x" + focalLength.y + "\n" + "PP: " + principalPoint.x + "x" + principalPoint.y);
                        }
                        mText.text = focalLength.ToString() + " " + principalPoint.ToString();
                    }

                    if (eventArgs.projectionMatrix.HasValue)
                    {
                        if (fpsMonitor != null)
                        {
                            fpsMonitor.Add("projectionMatrix", "\n" + eventArgs.projectionMatrix.ToString());
                        }
                    }

                    if (eventArgs.timestampNs.HasValue)
                    {
                        if (fpsMonitor != null)
                        {
                            fpsMonitor.Add("timestampNs", eventArgs.timestampNs.ToString());
                        }
                    }
                }
                mText.text = mText.text+" " + rgbaMat.size() + arCamera.transform.position.ToString();
            }
            catch(Exception e)
            {
                mText.text = e.ToString();
            }
            
            image.Dispose();
        }

        protected void DisplayImage()
        {
            if (displayFlipVertical && displayFlipHorizontal)
            {
                Core.flip(rgbaMat, rgbaMat, -1);
            }
            else if (displayFlipVertical)
            {
                Core.flip(rgbaMat, rgbaMat, 0);
            }
            else if (displayFlipHorizontal)
            {
                Core.flip(rgbaMat, rgbaMat, 1);
            }

            if (rotatedFrameMat != null)
            {
                if (displayRotationAngle == 90)
                {
                    Core.rotate(rgbaMat, rotatedFrameMat, Core.ROTATE_90_CLOCKWISE);
                }
                else if (displayRotationAngle == 270)
                {
                    Core.rotate(rgbaMat, rotatedFrameMat, Core.ROTATE_90_COUNTERCLOCKWISE);
                }

                Utils.fastMatToTexture2D(rotatedFrameMat, texture);
            }
            else
            {
                if (displayRotationAngle == 180)
                {
                    Core.rotate(rgbaMat, rgbaMat, Core.ROTATE_180);
                }

                Utils.fastMatToTexture2D(rgbaMat, texture);
            }
        }

        private void Dispose()
        {
            hasInitDone = false;

            if (rgbaMat != null)
            {
                rgbaMat.Dispose();
                rgbaMat = null;
            }
            if (rotatedFrameMat != null)
            {
                rotatedFrameMat.Dispose();
                rotatedFrameMat = null;
            }
            if (texture != null)
            {
                Texture2D.Destroy(texture);
                texture = null;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Application.wantsToQuit += WantsToQuit;

            Debug.Assert(cameraManager != null, "camera manager cannot be null");

            fpsMonitor = GetComponent<FpsMonitor>();

            // Checks camera permission state.
            if (fpsMonitor != null && !cameraManager.permissionGranted)
            {
                fpsMonitor.consoleText = "Camera permission has not been granted.";
            }
           
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


