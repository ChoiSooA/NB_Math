/*
 * Copyright 2024 (c) Leia Inc.  All rights reserved.
 *
 * NOTICE:  All information contained herein is, and remains
 * the property of Leia Inc. and its suppliers, if any.  The
 * intellectual and technical concepts contained herein are
 * proprietary to Leia Inc. and its suppliers and may be covered
 * by U.S. and Foreign Patents, patents in process, and are
 * protected by trade secret or copyright law.  Dissemination of
 * this information or reproduction of this materials strictly
 * forbidden unless prior written permission is obtained from
 * Leia Inc.
 */
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static LeiaUnity.RenderTrackingDevice;

#if LEIA_URP_DETECTED
using UnityEngine.Rendering.Universal;
#endif

namespace LeiaUnity
{
    [ExecuteInEditMode]
    public class LeiaDisplay : MonoBehaviour
    {
#region Private_Variables

        private int _numViewsX = 2;
        private readonly int _numViewsY = 1;

        private Vector3 viewerPositionNonPredicted = new Vector3(0, 0, 535.964f);
        private Vector3 viewerPositionPredicted = new Vector3(0, 0, 535.964f);
        private Vector2[] _initialViewOffsets;

        private RenderTexture _interlacedTexture;
        private Material _editorPreviewMaterial;
        private string _editorPreviewShaderName { get { return "EditorPreview"; } }

        private ScreenOrientation _previousOrientation;

        private float calculatedVirtualHeight;

        private bool drawCameraBounds;
        [SerializeField] private int antiAliasingLevel;
        private bool divideByPerspectiveFactor = true;
        private EditorPreviewMode desiredPreviewMode = EditorPreviewMode.Interlaced;
        private bool useCameraClippingPlanes;
        [Range(.1f, 20)]
        private float maxDisparity = 10f; //10% of screen width
        private float fovFactor = 1.0f;
        private bool cameraShiftEnabled;
        private int scriptableRendererIndex;
#if UNITY_ANDROID && !UNITY_EDITOR
        private bool supportAllAxis;
#endif
        private bool _initialized;

        [SerializeField] private float _desiredDepthFactor = 1.0f;
        [SerializeField] private float _depthFactor = 1.0f;

        [SerializeField] private float _desiredLookAroundFactor = 1.0f;
        [SerializeField] private float _lookAroundFactor = 1.0f;

#endregion
#region Public_Variables

        [SerializeField]
        public enum EditorPreviewMode { Interlaced, SideBySide };
        public EditorPreviewMode DesiredPreviewMode
        {
            get { return desiredPreviewMode; }
            set { desiredPreviewMode = value; }
        }

        public bool CameraShiftEnabled
        {
            get { return cameraShiftEnabled; }
            set { cameraShiftEnabled = value; }
        }
        public static event System.Action StateChanged = delegate { };

        public bool DivideByPerspectiveFactor
        {
            get { return divideByPerspectiveFactor; }
            set { divideByPerspectiveFactor = value; }
        }

        public int ScriptableRendererIndex
        {
            get { return scriptableRendererIndex; }
            set { scriptableRendererIndex = value; }
        }

        public float VirtualHeight = 10;

        public float FOVFactor
        {
            get { return fovFactor; }
            set { fovFactor = value; }
        }

        public float LookAroundFactor
        {
            get
            {
                return _lookAroundFactor;
            }
            set
            {
                _desiredLookAroundFactor = value;
                if (_numViewsX == 2)
                {
                    _lookAroundFactor = value;
                }
            }
        }

        public float DepthFactor
        {
            get
            {
                return _depthFactor;
            }
            set
            {
                _desiredDepthFactor = value;
                if (_numViewsX == 2)
                {
                    _depthFactor = value;
                }
            }
        }

        public bool DrawCameraBounds
        {
            get { return drawCameraBounds; }
            set { drawCameraBounds = value; }
        }

        //Real Display Dimensions
        [HideInInspector]
        public float WidthMM
        {
            get
            {
                if (Application.isEditor)
                {
                    return 266; // default values for LP2
                }
                else
                {
                    return RenderTrackingDevice.Instance.GetDisplaySizeInMM().x;
                }
            }
        }
        [HideInInspector]
        public float HeightMM
        {
            get
            {
                if (Application.isEditor)
                {
                    return 168; // default values for LP2
                }
                else
                {
                    return RenderTrackingDevice.Instance.GetDisplaySizeInMM().y;
                }
            }
        }
        [HideInInspector]
        public float ViewingDistanceMM //= 450; //this will be pulled from config
        {
            get
            {
                if (Application.isEditor)
                {
                    return 450.0f; // default values for LP2
                }
                else
                {
                    return RenderTrackingDevice.Instance.GetViewingDistanceInMM();
                }
            }
        }
        [HideInInspector]
        public float IPDMM
        {
            get
            {
                if (Application.isEditor)
                {
                    return 63.0f; // default values for LP2
                }
                else
                {
                    return RenderTrackingDevice.Instance.IPDInMM;
                }
            }
            set
            {
                RenderTrackingDevice.Instance.IPDInMM = value;
            }
        }
        public float MaxDisparity
        {
            get { return maxDisparity; }
            set { maxDisparity = value; }
        }//10% of screen width

        public bool UseCameraClippingPlanes
        {
            get { return useCameraClippingPlanes; }
            set { useCameraClippingPlanes = value; }
        }

        float comfortZoneNearClipPlane;
        float comfortZoneFarClipPlane;

        //Virtual Display Dimensions
        [HideInInspector]
        public float VirtualWidth
        {
            get
            {
                return GetGameViewAspectRatio() * calculatedVirtualHeight;
            }
        }

        //[HideInInspector]
        public Camera DriverCamera;

        public float MMToVirtual
        {
            get
            {
                return calculatedVirtualHeight / HeightMM;
            }
        }

        public float VirtualToMM
        {
            get
            {
                return HeightMM / VirtualHeight;
            }
        }

        public enum ControlMode { DisplayDriven, CameraDriven };
        [HideInInspector]
        public ControlMode mode;

        public Head ViewersHead;
        public Camera HeadCamera
        {
            get
            {
                return ViewersHead.headcamera;
            }
        }

        public static float MinFocalDistance = .0001f;
        public float FocalDistance
        {
            get
            {
                if (mode == ControlMode.CameraDriven)
                {
                    return Mathf.Max(MinFocalDistance, transform.localPosition.z);
                }
                else
                {
                    return 0; //Display Driven mode FocalDistance is always zero
                }
            }
            set
            {
                if (mode == ControlMode.CameraDriven)
                {
                    transform.localPosition = new Vector3(
                        transform.localPosition.x,
                        transform.localPosition.y,
                        value
                    );
                }
                else
                {
                    //changing focal distance does nothing in display driven mode
                    Debug.LogError("Error: attempted to set focal distance in Display Driven mode");
                }
            }
        }

        void ValidateFocalDistance()
        {
            if (mode == ControlMode.CameraDriven)
            {
                FocalDistance = Mathf.Max(MinFocalDistance, FocalDistance);
            }
        }

        public int AntiAliasingLevel
        {
            get { return antiAliasingLevel; }
            set { antiAliasingLevel = value; }
        }

        [SerializeField]
        public bool LateLatchingEnabled;

        #endregion
        #region Unity_Functions

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            RenderTrackingDevice.Instance = FindObjectOfType<RenderTrackingDevice>();
            if (RenderTrackingDevice.Instance == null)
            {
                GameObject renderDeviceObj = new GameObject("RenderTrackingDevice");
                RenderTrackingDevice.Instance = renderDeviceObj.AddComponent<RenderTrackingDevice>();
                DontDestroyOnLoad(renderDeviceObj);
            }

            RenderTrackingDevice.Instance.Initialize();
            RenderTrackingDevice.Instance.LightfieldModeChanged += HandleLightfieldModeChanged;

            if (!_initialized)
            {
                UpdateInitialViewOffsets();
                _interlacedTexture = new RenderTexture(RenderTrackingDevice.Instance.GetDevicePanelResolution().x, RenderTrackingDevice.Instance.GetDevicePanelResolution().y, 0);
                _interlacedTexture.Create();
                _initialized = true;
            }



#if UNITY_EDITOR
            EnsureEditorPreivewMaterialInitialized();
#endif

#if LEIA_URP_DETECTED
            if (mode == ControlMode.CameraDriven)
            {
                scriptableRendererIndex = DriverCamera.GetUniversalAdditionalCameraData().GetRendererIndex();
            }
            else
            {
                scriptableRendererIndex = HeadCamera.GetUniversalAdditionalCameraData().GetRendererIndex();
            }
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
            supportAllAxis = ValidateSupportAllAxis();
#endif
        }

        void OnDisable()
        {
            LogUtil.Log(LogLevel.Debug, "OnDisable in leiaDisplay");

            if (RenderTrackingDevice.Instance)
            {
                RenderTrackingDevice.Instance.LightfieldModeChanged -= HandleLightfieldModeChanged;
            }

        }
#region Unity_Functions

        void ValidateScale()
        {
            transform.localScale = Vector3.one;
        }

        public void Update()
        {
            ValidateFocalDistance();
            ValidateScale();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            calculatedVirtualHeight = VirtualHeight;
#endif
            if (Application.isPlaying)
            {
                UpdateViews();
#if UNITY_ANDROID && !UNITY_EDITOR
                if(supportAllAxis)
                {
                    if (IsLandscape(Screen.orientation) && RenderTrackingDevice.Instance.deviceConfig.DeviceNaturalOrientation == Leia.Orientation.Portrait)
                    {
                        calculatedVirtualHeight = VirtualHeight;
                    }
                    else if (IsPortrait(Screen.orientation) && RenderTrackingDevice.Instance.deviceConfig.DeviceNaturalOrientation == Leia.Orientation.Portrait)
                    {
                        calculatedVirtualHeight = VirtualHeight * ((float)Screen.height / (float)Screen.width);
                    }
                    else if (IsLandscape(Screen.orientation) && RenderTrackingDevice.Instance.deviceConfig.DeviceNaturalOrientation == Leia.Orientation.Landscape)
                    {
                        calculatedVirtualHeight = VirtualHeight;
                    }
                    else if (IsPortrait(Screen.orientation) && RenderTrackingDevice.Instance.deviceConfig.DeviceNaturalOrientation == Leia.Orientation.Landscape)
                    {
                        calculatedVirtualHeight = VirtualHeight * ((float)Screen.height / (float)Screen.width);
                    }
                }
                else
                {
                    calculatedVirtualHeight = VirtualHeight;
                }
#endif
            }
#if UNITY_EDITOR
            if (!GetComponentInChildren<Head>())
            {
                OnComponentAddedToGameObject();
                return;
            }
#endif
            if (mode == ControlMode.CameraDriven)
            {
                UpdateVirtualDisplayFromCamera();
            }
            else
            {
                UpdateCameraFromVirtualDisplay();
            }
        }

        bool ValidateSupportAllAxis()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Leia.SDK.GetLegalOrientations(out Leia.LegalOrientations legalOrientations))
            {
                return legalOrientations.portrait == 1 && legalOrientations.reversePortrait == 1 &&
                       legalOrientations.landscape == 1 && legalOrientations.reverseLandscape == 1;
            }
            else
            {
                return false;
            }
#else
            return false;
#endif
        }

        void UpdateComfortZoneClippingPlanes()
        {
            if (ViewersHead == null)
            {
                return;
            }
            comfortZoneFarClipPlane = 1 / ((HeightMM / calculatedVirtualHeight) * fovFactor * ((1 / ViewersHead.HeadPositionMM.z) - (1 / (6 * DepthFactor)) * (5 / 1000 + 1 / ViewersHead.HeadPositionMM.z)));
            comfortZoneNearClipPlane = 1 / ((HeightMM / calculatedVirtualHeight) * fovFactor * ((1 / ViewersHead.HeadPositionMM.z) + (1 / (5 * DepthFactor)) * (4 / 1000 + 1 / ViewersHead.HeadPositionMM.z)));

            if (comfortZoneFarClipPlane < 0)
            {
                comfortZoneFarClipPlane = 1000;
            }
        }
        public static float GetGameViewAspectRatio()
        {
#if UNITY_EDITOR
            Vector2 wh = UnityEditor.Handles.GetMainGameViewSize();
            return wh.x / Mathf.Max(1.0f, wh.y);
#else
            return Screen.width / Mathf.Max(1.0f, Screen.height);
#endif
        }

        private void OnDrawGizmos()
        {
            LogUtil.Log(LogLevel.Debug, "OnDrawGizmos");
            if (ViewersHead == null)
            {
                return; //Wait until initilized before attempting to draw gizmos
            }

            float ratio = GetGameViewAspectRatio();
            float Width = calculatedVirtualHeight * ratio;

            Vector3 TopLeftCorner = new Vector3(
                    Width / 2f,
                    calculatedVirtualHeight / 2f,
                    0
                    );

            Vector3 TopRightCorner = new Vector3(
                    -Width / 2f,
                    calculatedVirtualHeight / 2f,
                    0
                    );

            Vector3 BottomLeftCorner = new Vector3(
                    Width / 2f,
                    -calculatedVirtualHeight / 2f,
                    0
                    );

            Vector3 BottomRightCorner = new Vector3(
                    -Width / 2f,
                    -calculatedVirtualHeight / 2f,
                    0
                    );

            TopLeftCorner = transform.position + transform.rotation * TopLeftCorner;
            TopRightCorner = transform.position + transform.rotation * TopRightCorner;
            BottomLeftCorner = transform.position + transform.rotation * BottomLeftCorner;
            BottomRightCorner = transform.position + transform.rotation * BottomRightCorner;
            if (enabled)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(TopRightCorner, TopLeftCorner);
                Gizmos.DrawLine(BottomLeftCorner, BottomRightCorner);
                Gizmos.DrawLine(TopRightCorner, BottomRightCorner);
                Gizmos.DrawLine(TopLeftCorner, BottomLeftCorner);
            }
        }

        private void OnDestroy()
        {
            LogUtil.Log(LogLevel.Debug, "OnDestroy");
            if (ViewersHead != null)
            {
                DestroyImmediate(ViewersHead.gameObject);
            }
        }
#endregion
#endregion
#region Init_Functions

        public void InitLeiaDisplay(List<Vector2> ViewConfig)
        {
            LogUtil.Log(LogLevel.Debug, "InitLeiaDisplay");
            GameObject newHeadGameObject = new GameObject("Head");
            newHeadGameObject.transform.parent = transform;
            newHeadGameObject.transform.localPosition = new Vector3(
                0,
                0,
                ViewingDistanceMM * calculatedVirtualHeight / (HeightMM * fovFactor)
                );
            newHeadGameObject.transform.localPosition = Vector3.zero;
            newHeadGameObject.transform.localRotation = Quaternion.identity;
            if (mode == ControlMode.CameraDriven)
            {
                UpdateVirtualDisplayFromCamera();
            }
            ViewersHead = newHeadGameObject.AddComponent<Head>();
            ViewersHead.InitHead(ViewConfig, this);
            ViewersHead.HeadUpdate();
        }

        void OnComponentAddedToGameObject()
        {
#if UNITY_EDITOR
            Selection.activeGameObject = this.gameObject;
#endif

            //If LeiaDisplay component was just added to a camera game object do this
            DriverCamera = GetComponent<Camera>();

            Camera camParent = null;
            if (transform.parent != null)
            {
                camParent = transform.parent.GetComponent<Camera>();
            }

            if (DriverCamera != null)
            {
                DriverCamera.enabled = false;
                mode = ControlMode.CameraDriven;
                fovFactor = (ViewingDistanceMM * calculatedVirtualHeight) / (HeightMM * transform.localPosition.z);
                GameObject leiaDisplayGameObject = new GameObject("LeiaDisplay");
                leiaDisplayGameObject.transform.parent = transform;
                LeiaDisplay newLeiaDisplay = leiaDisplayGameObject.AddComponent<LeiaDisplay>();
                newLeiaDisplay.useCameraClippingPlanes = true;
                leiaDisplayGameObject.transform.localPosition = new Vector3(
                    0,
                    0,
                    2f / (1f / DriverCamera.nearClipPlane + 1f / DriverCamera.farClipPlane)
                );
                leiaDisplayGameObject.transform.localRotation = Quaternion.identity;

                DestroyImmediate(this);
            }
            else
            {
                if (camParent != null)
                {
                    DriverCamera = camParent;
                    List<Vector2> ViewConfig = new List<Vector2>
                {
                    //new Vector2(-1.5f, 0),
                    new Vector2(-.5f, 0),
                    new Vector2(.5f, 0)//,
                    //new Vector2(1.5f, 0)
                };
                    mode = ControlMode.CameraDriven;
                    InitLeiaDisplay(ViewConfig);
                }
                else //If leiadisplay added to a blank object do this
                {
                    MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();

                    if (meshRenderer != null)
                    {
                        // Get the bounds of the object
                        Bounds bounds = meshRenderer.bounds;

                        // Calculate and print the height of the object
                        float height = bounds.size.y;

                        this.calculatedVirtualHeight = height * 4f;
                    }

                    List<Vector2> ViewConfig = new List<Vector2>
                {
                    //new Vector2(-1.5f, 0),
                    new Vector2(-.5f, 0),
                    new Vector2(.5f, 0) //,
                    //new Vector2(1.5f, 0)
                };
                    mode = ControlMode.DisplayDriven;
                    InitLeiaDisplay(ViewConfig);
                }
            }
        }

#endregion
#region Head_Functions

        public int GetViewCount()
        {
            LogUtil.Log(LogLevel.Debug, "GetViewCount");
            return ViewersHead.ViewConfig.Count;
        }

        public Camera GetEyeCamera(int index)
        {
            LogUtil.Log(LogLevel.Debug, "GetEyeCamera");
            return ViewersHead.eyes[index].Eyecamera;
        }

        public Eye GetEye(int index)
        {
            return ViewersHead.eyes[index];
        }

        void UpdateCameraFromVirtualDisplay()
        {
            this.ViewersHead.HeadUpdate();
        }

        void UpdateVirtualDisplayFromCamera()
        {
            VirtualHeight = 2f * Mathf.Tan(Mathf.Deg2Rad * (DriverCamera.fieldOfView / 2f)) * transform.localPosition.z;
            fovFactor = (ViewingDistanceMM * VirtualHeight) / (HeightMM * transform.localPosition.z);
            if (ViewersHead != null)
            {
                this.ViewersHead.HeadUpdate();
            }
        }

        public Vector3 RealToVirtualCenterFacePosition(Vector3 FacePositionMM)
        {
            LogUtil.Log(LogLevel.Debug, "RealToVirtualCenterFacePosition");
            float newHeadX = FacePositionMM.x;
            float newHeadY = FacePositionMM.y;
            float newHeadZ = 0;

            if (fovFactor < 1)
            {
                newHeadZ = -(this.ViewingDistanceMM / fovFactor + (FacePositionMM.z - ViewingDistanceMM)); // Vd / Beta + (Z - Vd)
            }
            else
            {
                newHeadZ = -(this.ViewingDistanceMM + (FacePositionMM.z - ViewingDistanceMM)) / fovFactor;
            }

            return new Vector3(newHeadX, newHeadY, newHeadZ) * VirtualHeight / HeightMM;
        }

        #endregion
#region Render_Functions

        public void RenderImage()
        {
#if UNITY_EDITOR
            if (RenderTrackingDevice.Instance.DesiredLightfieldMode == RenderTrackingDevice.LightfieldMode.On && GetViewCount() == 2)
            {
                EnsureEditorPreivewMaterialInitialized();
                SetEditorPreviewProperties();
                Graphics.Blit(Texture2D.whiteTexture, _interlacedTexture, _editorPreviewMaterial);
            }
            else
            {
                Graphics.Blit(GetEyeCamera(0).targetTexture, _interlacedTexture);
            }
#elif UNITY_ANDROID && !UNITY_EDITOR
            RenderTrackingDevice.Instance.Render(this, ref _interlacedTexture);
#endif
            Graphics.Blit(_interlacedTexture, Camera.current.activeTexture);
        }

        public void UpdateViews()
        {
            int viewResolutionX = RenderTrackingDevice.Instance.GetDeviceViewResolution().x;
            int viewResolutionY = RenderTrackingDevice.Instance.GetDeviceViewResolution().y;

#if UNITY_ANDROID && !UNITY_EDITOR
            if(supportAllAxis)
            {
                ScreenOrientation currentOrientation = Screen.orientation;
                Leia.Orientation deviceNaturalOrientation = RenderTrackingDevice.Instance.deviceConfig.DeviceNaturalOrientation;

                if (currentOrientation != _previousOrientation)
                {
                    var deviceViewResolution = RenderTrackingDevice.Instance.GetDeviceViewResolution();
                    var devicePanelResolution = RenderTrackingDevice.Instance.GetDevicePanelResolution();

                    if ((IsPortrait(Screen.orientation) && deviceNaturalOrientation == Leia.Orientation.Landscape) ||
                        (IsLandscape(Screen.orientation) && deviceNaturalOrientation == Leia.Orientation.Portrait))
                    {
                        viewResolutionX = deviceViewResolution.y;
                        viewResolutionY = deviceViewResolution.x;
                        Debug.Log($"Update Views - Landscape viewResolutionX: {viewResolutionX} , viewResolutionY: {viewResolutionY}");
                        UpdateInterlacedTexture(devicePanelResolution.y, devicePanelResolution.x);
                    }
                    else if ((IsLandscape(Screen.orientation) && deviceNaturalOrientation == Leia.Orientation.Landscape) ||
                             (IsPortrait(Screen.orientation) && deviceNaturalOrientation == Leia.Orientation.Portrait))
                    {
                        viewResolutionX = deviceViewResolution.x;
                        viewResolutionY = deviceViewResolution.y;
                        Debug.Log($"Update Views - Portrait viewResolutionX: {viewResolutionX} , viewResolutionY: {viewResolutionY}");
                        UpdateInterlacedTexture(devicePanelResolution.x, devicePanelResolution.y);
                    }
                    _previousOrientation = Screen.orientation;
                }
            }
#endif
            for (int ix = 0; ix < GetViewCount(); ix++)
            {
                Eye view = GetEye(ix);
                view.SetTextureParams(viewResolutionX, viewResolutionY);
            }
        }

        private bool IsLandscape(ScreenOrientation orientation)
        {
            return orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeRight;
        }

        private bool IsPortrait(ScreenOrientation orientation)
        {
            return orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown;
        }

        private void UpdateInterlacedTexture(int width, int height)
        {
            if (_interlacedTexture.width != width || _interlacedTexture.height != height)
            {
                _interlacedTexture?.Release();
                _interlacedTexture = new RenderTexture(width, height, 0);
                _interlacedTexture.Create();
            }
        }

        private void UpdateInitialViewOffsets()
        {
            _initialViewOffsets = new Vector2[_numViewsX * _numViewsY];

            // Calculate initial offsets to center the view grid
            float baseOffsetX = -0.5f * (_numViewsX - 1.0f);
            float baseOffsetY = -0.5f * (_numViewsY - 1.0f);

            for (int viewY = 0; viewY < _numViewsY; viewY++)
            {
                for (int viewX = 0; viewX < _numViewsX; viewX++)
                {
                    // Calculate view offset based on base offsets and view indices
                    float viewOffsetX = baseOffsetX + viewX;
                    float viewOffsetY = baseOffsetY + viewY;

                    _initialViewOffsets[viewX + viewY * _numViewsX] = new Vector2(viewOffsetX, viewOffsetY);
                }
            }
        }

        private float GetInitialViewOffsetX(int viewX, int viewY)
        {
            return _initialViewOffsets[viewX + viewY * _numViewsX].x;
        }

        private float GetInitialViewOffsetY(int viewX, int viewY)
        {
            return _initialViewOffsets[viewX + viewY * _numViewsX].y;
        }
        public static float GetViewportAspectFor(Camera renderingCamera)
        {
            return renderingCamera.pixelRect.width * 1.0f / renderingCamera.pixelRect.height;
        }

        public static Matrix4x4 GetConvergedProjectionMatrixForPosition(Camera Camera, Vector3 convergencePoint)
        {
            LogUtil.Log(LogLevel.Debug, "GetConvergedProjectionMatrixForPosition");
            Matrix4x4 m = Matrix4x4.zero;

            Vector3 cameraToConvergencePoint = convergencePoint - Camera.transform.position;

            float far = Camera.farClipPlane;
            float near = Camera.nearClipPlane;

            // posX and posY are the camera-axis-aligned translations off of "root camera" position
            float posX = -1 * Vector3.Dot(cameraToConvergencePoint, Camera.transform.right);
            float posY = -1 * Vector3.Dot(cameraToConvergencePoint, Camera.transform.up);

            // this is really posZ. it is better if posZ is positive-signed
            float ConvergenceDistance = Mathf.Max(Vector3.Dot(cameraToConvergencePoint, Camera.transform.forward), 1E-5f);

            if (Camera.orthographic)
            {
                // calculate the halfSizeX and halfSizeY values that we need for orthographic cameras

                float halfSizeX = Camera.orthographicSize * GetViewportAspectFor(Camera);
                float halfSizeY = Camera.orthographicSize;

                // orthographic

                // row 0
                m[0, 0] = 1.0f / halfSizeX;
                m[0, 1] = 0.0f;
                m[0, 2] = -posX / (halfSizeX * ConvergenceDistance);
                m[0, 3] = 0.0f;

                // row 1
                m[1, 0] = 0.0f;
                m[1, 1] = 1.0f / halfSizeY;
                m[1, 2] = -posY / (halfSizeY * ConvergenceDistance);
                m[1, 3] = 0.0f;

                // row 2
                m[2, 0] = 0.0f;
                m[2, 1] = 0.0f;
                m[2, 2] = -2.0f / (far - near);
                m[2, 3] = -(far + near) / (far - near);

                // row 3
                m[3, 0] = 0.0f;
                m[3, 1] = 0.0f;
                m[3, 2] = 0.0f;
                m[3, 3] = 1.0f;
            }
            else
            {
                // calculate the halfSizeX and halfSizeY values for perspective DriverCamera that we would have gotten if we had used new CameraCalculatedParams.
                // we don't need "f" (disparity per camera vertical pixel count) or EmissionRescalingFactor
                const float minAspect = 1E-5f;
                float aspect = Mathf.Max(GetViewportAspectFor(Camera), minAspect);
                float halfSizeY = ConvergenceDistance * Mathf.Tan(Camera.fieldOfView * Mathf.PI / 360.0f);
                float halfSizeX = aspect * halfSizeY;

                // perspective

                // row 0
                m[0, 0] = ConvergenceDistance / halfSizeX;
                m[0, 1] = 0.0f;
                m[0, 2] = -posX / halfSizeX;
                m[0, 3] = 0.0f;

                // row 1
                m[1, 0] = 0.0f;
                m[1, 1] = ConvergenceDistance / halfSizeY;
                m[1, 2] = -posY / halfSizeY;
                m[1, 3] = 0.0f;

                // row 2
                m[2, 0] = 0.0f;
                m[2, 1] = 0.0f;
                m[2, 2] = -(far + near) / (far - near);
                m[2, 3] = -2.0f * far * near / (far - near);

                // row 3
                m[3, 0] = 0.0f;
                m[3, 1] = 0.0f;
                m[3, 2] = -1.0f;
                m[3, 3] = 0.0f;
            }
            return m;
        }

        public Matrix4x4 GetProjectionMatrixForCamera(Camera camera, Vector3 offset, bool isEye)
        {
            LogUtil.Log(LogLevel.Debug, "GetProjectionMatrixForCamera");
            float W = VirtualWidth;
            float H = calculatedVirtualHeight;

            Vector3 cameraPositionRelativeToDisplay = camera.transform.InverseTransformPoint(transform.position);

            Vector3 cameraRelative = -cameraPositionRelativeToDisplay;

            float xc = cameraRelative.x;
            float yc = cameraRelative.y;
            float zc = cameraRelative.z;

            float r = -(W / 2 - xc) / zc; //minus sign is to make zc positive
            float l = -(-W / 2 - xc) / zc;
            float t = -(H / 2 - yc) / zc;
            float b = -(-H / 2 - yc) / zc;

            float far = camera.farClipPlane;
            float near = camera.nearClipPlane;

            Matrix4x4 p = new Matrix4x4();

            p.m00 = 2 / (r - l);
            p.m11 = 2 / (t - b);
            p.m02 = (r + l) / (r - l);
            p.m12 = (t + b) / (t - b);
            p.m22 = -(far + near) / (far - near);
            p.m32 = -1;
            p.m23 = -(2 * far * near) / (far - near);

            // row 1
            p.m10 = 0.0f;
            p.m13 = 0.0f;

            // row 2
            p.m20 = 0.0f;
            p.m21 = 0.0f;

            // row 3
            p.m30 = 0.0f;
            p.m31 = 0.0f;
            p.m33 = 0.0f;

            return p;
        }

        public void DrawFrustum(Camera camera)
        {
            LogUtil.Log(LogLevel.Debug, "DrawFrustum");
            if (camera == null)
            {
                return;
            }


            Vector3[] nearCorners = new Vector3[4];
            Vector3[] farCorners = new Vector3[4];

            UpdateComfortZoneClippingPlanes();

            camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), comfortZoneNearClipPlane, Camera.MonoOrStereoscopicEye.Mono, nearCorners);
            camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), comfortZoneFarClipPlane, Camera.MonoOrStereoscopicEye.Mono, farCorners);

            for (int i = 0; i < 4; i++)
            {
                nearCorners[i] = camera.transform.TransformPoint(nearCorners[i]);
                farCorners[i] = camera.transform.TransformPoint(farCorners[i]);
            }

            Gizmos.color = Color.magenta;
            for (int i = 0; i < 4; i++)
            {
                int nextIndex = (i + 1) % 4;

                // Draw the near clip plane
                Gizmos.DrawLine(nearCorners[i], nearCorners[nextIndex]);

                // Draw the far clip plane
                Gizmos.DrawLine(farCorners[i], farCorners[nextIndex]);

                // Connect the near and far clip planes
                Gizmos.DrawLine(nearCorners[i], farCorners[i]);
            }
        }

#endregion
#region 2D_3D

        public void Set3DMode(bool toggle)
        {
            Debug.Log("Set3DMode: "+ toggle);

            if(toggle)
            {
                RenderTrackingDevice.Instance.DesiredLightfieldMode = RenderTrackingDevice.LightfieldMode.On;
            }
            else
            {
                RenderTrackingDevice.Instance.DesiredLightfieldMode = RenderTrackingDevice.LightfieldMode.Off;
            }
        }

        void HandleLightfieldModeChanged(RenderTrackingDevice.LightfieldMode lightfieldMode)
        {
            if (lightfieldMode == RenderTrackingDevice.LightfieldMode.On)
            {
                _numViewsX = 2;
                _depthFactor = _desiredDepthFactor;
                _lookAroundFactor = _desiredLookAroundFactor;
            }
            else
            {
                _numViewsX = 1;
                _depthFactor = 0;
                _lookAroundFactor = 0;
            }
            StateChanged.Invoke();
        }

#endregion
#region Device_Values

        public Vector2Int GetDeviceViewResolution()
        {
            if (RenderTrackingDevice.Instance != null)
            {
                Vector2Int resolution = RenderTrackingDevice.Instance.GetDeviceViewResolution();
                return resolution;
            }
            return new Vector2Int(1280, 800);
        }
        public float GetDeviceSystemDisparityPixels()
        {
            if (RenderTrackingDevice.Instance != null)
            {
                float systemDisparity = RenderTrackingDevice.Instance.GetDeviceSystemDisparityPixels();
                return systemDisparity;
            }
            return 4.0f;
        }

#endregion
#region Editor_Preview

        private void EnsureEditorPreivewMaterialInitialized()
        {
            LogUtil.Log(LogLevel.Debug, "LeiaDisplay - EnsureEditorPreivewMaterialInitialized");
            if (_editorPreviewMaterial == null)
            {
                _editorPreviewMaterial = new Material(Resources.Load<Shader>(_editorPreviewShaderName));
            }
        }

        private void SetEditorPreviewProperties()
        {
            LogUtil.Log(LogLevel.Debug, "LeiaDisplay - SetEditorPreviewProperties");
            // default values from LumePad 2
            const int defaultPanelResolutionX = 2560;
            const int defaultPanelResolutionY = 1600;
            const float defaultNumViews = 8;
            const float defaultActSingleTapCoef = 0.12f;
            const float defaultPixelPitch = 0.10389f;
            const float defaultN = 1.6f;
            const float defaultDOverN = 0.6926f;
            const float defaultS = 10.687498f;
            const float defaultAnglePx = 0.1759291824068146f; // theta
            const float defaultNo = 4.629999965429306f; // center view number
            const float defaultPOverDu = 3.0f;
            const float defaultPOverDv = 1.0f;
            const float defaultGamma = 1.99f;
            const float defaultSmooth = 0.05f;
            const float defaultOePitchX = defaultNumViews / defaultPOverDu;
            const float defaultTanSlantAngle = defaultPOverDv / defaultPOverDu;
            Vector3 defaultSubpixelCentersX = new Vector3(-0.333f, 0.0f, 0.333f);
            Vector3 defaultSubpixelCentersY = new Vector3(0.0f, 0.0f, 0.0f);

            if (GetViewCount() == 2)
            {
                _editorPreviewMaterial.SetTexture("_texture_0", GetEyeCamera(0).targetTexture);
                _editorPreviewMaterial.SetTexture("_texture_1", GetEyeCamera(1).targetTexture);
            }

            if (desiredPreviewMode == EditorPreviewMode.SideBySide)
            {
                _editorPreviewMaterial.EnableKeyword("SideBySide");
            }
            else if (desiredPreviewMode == EditorPreviewMode.Interlaced)
            {
                _editorPreviewMaterial.DisableKeyword("SideBySide");

                _editorPreviewMaterial.SetInt("_width", defaultPanelResolutionX);
                _editorPreviewMaterial.SetInt("_height", defaultPanelResolutionY);
                _editorPreviewMaterial.SetFloat("_actSingleTapCoef", defaultActSingleTapCoef);
                _editorPreviewMaterial.SetFloat("_pixelPitch", defaultPixelPitch);
                _editorPreviewMaterial.SetFloat("_n", defaultN);
                _editorPreviewMaterial.SetFloat("_d_over_n", defaultDOverN);
                _editorPreviewMaterial.SetFloat("_s", defaultS);
                _editorPreviewMaterial.SetFloat("_anglePx", defaultAnglePx);
                _editorPreviewMaterial.SetFloat("_no", defaultNo);
                _editorPreviewMaterial.SetFloat("_gamma", defaultGamma);
                _editorPreviewMaterial.SetFloat("_smooth", defaultSmooth);
                _editorPreviewMaterial.SetFloat("_oePitchX", defaultOePitchX);
                _editorPreviewMaterial.SetFloat("_tanSlantAngle", defaultTanSlantAngle);
                _editorPreviewMaterial.SetFloat("_faceX", viewerPositionPredicted.x);
                _editorPreviewMaterial.SetFloat("_faceY", viewerPositionPredicted.y);
                _editorPreviewMaterial.SetFloat("_faceZ", viewerPositionPredicted.z);
                _editorPreviewMaterial.SetVector("_subpixelCentersX", defaultSubpixelCentersX);
                _editorPreviewMaterial.SetVector("_subpixelCentersY", defaultSubpixelCentersY);
            }
            _editorPreviewMaterial.SetFloat("_numViews", defaultNumViews);
        }

#endregion
    }
}
