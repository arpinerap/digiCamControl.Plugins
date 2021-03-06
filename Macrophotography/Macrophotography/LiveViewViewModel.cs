﻿using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Translation;
using CameraControl.Core.Wpf;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Timer = System.Timers.Timer;

namespace Macrophotography
{
    public class LiveViewViewModel : ViewModelBase
    {
        private const int DesiredFrameRate = 20;
        private int _totalframes = 0;
        private DateTime _photoCapturedTime = DateTime.MinValue;

        private bool _operInProgress = false;
        private Timer _timer = new Timer(1000 / 25.0);

        private BitmapSource _previewBitmap;
        private bool _previewBitmapVisible;

        private ICameraDevice _cameraDevice;
        private BitmapSource _bitmap;
        private Bitmap _ColorBitmap;
        private BitmapSource _ColorBitmapSource;
        private BitmapImage _ColorBitmapImage;
        private bool _settingArea;
        private CameraProperty _cameraProperty;

        private static LiveViewViewModel _instance;

        public LiveViewData LiveViewData { get; set; }


        private AsyncObservableCollection<string> _grids;
        private int _gridType;

        private PointCollection _luminanceHistogramPoints = null;
        private PointCollection _redColorHistogramPoints;
        private PointCollection _greenColorHistogramPoints;
        private PointCollection _blueColorHistogramPoints;
        private PropertyValue<long> _exposureDelay;
        private PropertyValue<long> _compressionSetting;
        private PropertyValue<long> _rawCompressionType;
        private PropertyValue<long> _RawCompressionBitMode;      
        private PropertyValue<long> _imageSize;
        private PropertyValue<long> _activePicCtrlItem;
        private PropertyValue<long> _noiseReduction;
        private PropertyValue<long> _flashCompensation;
        private PropertyValue<long> _ColorSpace;
        private PropertyValue<long> _WbColorTemp;
        private PropertyValue<long> _WbTuneFluorescentType;       
        private PropertyValue<long> _applicationMode;
        private PropertyValue<long> _initFlash;
        private PropertyValue<long> _fNumber;
        private PropertyValue<long> _captureAreaCrop;
        private int _LevelAngle;
        private string _levelAngleColor;
        private decimal _angleLevelPitching;
        private decimal _angleLevelYawing;
        private PropertyValue<long> _dLighting;
        private PropertyValue<long> _HDRMode;
        private PropertyValue<long> _HDREv;
        private PropertyValue<long> _HDRSmoothing;
        private PropertyValue<long> _FocalLength;
        private PropertyValue<long> _NoiseReductionHiIso;
        private PropertyValue<long> _FlashSyncSpeed;
        private PropertyValue<long> _ActiveSlot;
        private PropertyValue<long> _LensSort;
        private PropertyValue<long> _LensType;



        public ICameraDevice SelectedCameraDevice
        {
            get { return _cameraDevice; }
            set
            {
                _cameraDevice = value;
                RaisePropertyChanged(() => SelectedCameraDevice);
            }
        }

        public static LiveViewViewModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LiveViewViewModel(ServiceProvider.DeviceManager.SelectedCameraDevice);
                return _instance;
            }
            set { _instance = value; }
        }

       
        

        #region Commands

        public RelayCommand AutoFocusCommand { get; set; }
        public RelayCommand RecordMovieCommand { get; set; }
        public RelayCommand CaptureCommand { get; set; }
        public RelayCommand CancelCaptureCommand { get; set; }

        public RelayCommand FocusPCommand { get; set; }
        public RelayCommand FocusPPCommand { get; set; }
        public RelayCommand FocusPPPCommand { get; set; }
        public RelayCommand FocusMCommand { get; set; }
        public RelayCommand FocusMMCommand { get; set; }
        public RelayCommand FocusMMMCommand { get; set; }
        public RelayCommand MoveACommand { get; set; }
        public RelayCommand MoveBCommand { get; set; }
        public RelayCommand StartFocusStackingCommand { get; set; }
        public RelayCommand PreviewFocusStackingCommand { get; set; }
        public RelayCommand StopFocusStackingCommand { get; set; }

        public RelayCommand StartSimpleFocusStackingCommand { get; set; }
        public RelayCommand PreviewSimpleFocusStackingCommand { get; set; }
        public RelayCommand StopSimpleFocusStackingCommand { get; set; }

        //public RelayCommand StartLiveViewCommand { get; set; }
        //public RelayCommand StopLiveViewCommand { get; set; }

        public RelayCommand ResetBrigthnessCommand { get; set; }
        public RelayCommand BrowseOverlayCommand { get; set; }
        public RelayCommand HelpFocusStackingCommand { get; set; }

        public RelayCommand ResetOverlayCommand { get; set; }

        public RelayCommand ZoomOutCommand { get; set; }
        public RelayCommand ZoomInCommand { get; set; }
        public RelayCommand ZoomIn100 { get; set; }
        public RelayCommand ToggleGridCommand { get; set; }

        //public RelayCommand SetAreaCommand { get; set; }
        //public RelayCommand DoneSetAreaCommand { get; set; }

        public RelayCommand LockCurrentNearCommand { get; set; }
        public RelayCommand LockCurrentFarCommand { get; set; }

        public bool ShowHistogram { get; set; }

        #endregion

        #region RaisePropertyChanged

        public bool CaptureInProgress
        {
            get { return _captureInProgress; }
            set
            {
                _captureInProgress = value;
                RaisePropertyChanged(() => CaptureInProgress);
            }
        }

        private int _rotation;

        public bool ShowFocusRect
        {
            get { return CameraProperty.LiveviewSettings.ShowFocusRect; }
            set
            {
                CameraProperty.LiveviewSettings.ShowFocusRect = value;
                RaisePropertyChanged(() => ShowFocusRect);
            }
        }

        public int PreviewTime
        {
            get { return CameraProperty.LiveviewSettings.PreviewTime; }
            set
            {
                CameraProperty.LiveviewSettings.PreviewTime = value;
                RaisePropertyChanged(() => PreviewTime);
            }
        }

        public bool ShowRuler
        {
            get { return CameraProperty.LiveviewSettings.ShowRuler; }
            set
            {
                CameraProperty.LiveviewSettings.ShowRuler = value;
                RaisePropertyChanged(() => ShowRuler);
            }
        }

        public bool SettingArea
        {
            get { return _settingArea; }
            set
            {
                _settingArea = value;
                RaisePropertyChanged(() => SettingArea);
                RaisePropertyChanged(() => NoSettingArea);
            }
        }

        public bool NoSettingArea
        {
            get { return !SettingArea; }
        }

        public Rect RullerRect
        {
            get { return new Rect(HorizontalMin, VerticalMin, HorizontalMax, VerticalMax); }
        }

        public int HorizontalMin
        {
            get { return CameraProperty.LiveviewSettings.HorizontalMin; }
            set
            {
                CameraProperty.LiveviewSettings.HorizontalMin = value;
                RaisePropertyChanged(() => RullerRect);
            }
        }

        public int HorizontalMax
        {
            get { return CameraProperty.LiveviewSettings.HorizontalMax; }
            set
            {
                CameraProperty.LiveviewSettings.HorizontalMax = value;
                RaisePropertyChanged(() => RullerRect);
            }
        }

        public int VerticalMin
        {
            get { return CameraProperty.LiveviewSettings.VerticalMin; }
            set
            {
                CameraProperty.LiveviewSettings.VerticalMin = value;
                RaisePropertyChanged(() => RullerRect);
            }
        }

        public int VerticalMax
        {
            get { return CameraProperty.LiveviewSettings.VerticalMax; }
            set
            {
                CameraProperty.LiveviewSettings.VerticalMax = value;
                RaisePropertyChanged(() => RullerRect);
            }
        }

        public int Brightness
        {
            get { return CameraProperty.LiveviewSettings.Brightness; }
            set
            {
                CameraProperty.LiveviewSettings.Brightness = value;
                RaisePropertyChanged(() => Brightness);
            }
        }

        public bool BlackAndWhite
        {
            get { return CameraProperty.LiveviewSettings.BlackAndWhite; }
            set
            {
                CameraProperty.LiveviewSettings.BlackAndWhite = value;
                RaisePropertyChanged(() => BlackAndWhite);
            }
        }

        public bool EdgeDetection
        {
            get { return CameraProperty.LiveviewSettings.EdgeDetection; }
            set
            {
                CameraProperty.LiveviewSettings.EdgeDetection = value;
                RaisePropertyChanged(() => EdgeDetection);
            }
        }

        public BitmapSource PreviewBitmap
        {
            get { return _previewBitmap; }
            set
            {
                _previewBitmap = value;
                RaisePropertyChanged(() => PreviewBitmap);
                PreviewBitmapVisible = true;
            }
        }

        public bool PreviewBitmapVisible
        {
            get { return _previewBitmapVisible; }
            set
            {
                _previewBitmapVisible = value;
                RaisePropertyChanged(() => PreviewBitmapVisible);
            }
        }
        public BitmapSource Bitmap
        {
            get { return _bitmap; }
            set
            {
                _bitmap = value;
                RaisePropertyChanged(() => Bitmap);
            }
        }


        public Bitmap ColorBitmap
        {
            get { return _ColorBitmap; }
            set
            {
                _ColorBitmap = value;
                RaisePropertyChanged(() => ColorBitmap);
            }
        }

        public BitmapSource ColorBitmapSource
        {
            get { return _ColorBitmapSource; }
            set
            {
                _ColorBitmapSource = value;
                RaisePropertyChanged(() => ColorBitmapSource);
            }
        }

        public BitmapImage ColorBitmapImage
        {
            get { return _ColorBitmapImage; }
            set
            {
                _ColorBitmapImage = value;
                RaisePropertyChanged(() => ColorBitmapImage);
            }
        }

        public int RotationIndex
        {
            get { return CameraProperty.LiveviewSettings.RotationIndex; }
            set
            {
                CameraProperty.LiveviewSettings.RotationIndex = value;
                RaisePropertyChanged(() => RotationIndex);
            }
        }

        public int Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                RaisePropertyChanged(() => Rotation);
            }
        }

        public bool LiveViewIsRunning
        {
            get { return _liveViewIsRunning; }
            set
            {
                _liveViewIsRunning = value;
                RaisePropertyChanged(() => LiveViewIsRunning);
                RaisePropertyChanged(() => LiveViewNotRunning);
            }
        }

        public bool LiveViewNotRunning
        {
            get { return !LiveViewIsRunning; }
        }


        public bool IsActive { get; set; }

        public RelayCommand StartLiveViewCommand { get; set; }
        public RelayCommand StopLiveViewCommand { get; set; }

        public RelayCommand SetAreaCommand { get; set; }
        public RelayCommand DoneSetAreaCommand { get; set; }

        #endregion

        #region Camera Properties

        

        public CameraProperty CameraProperty
        {
            get { return _cameraProperty; }
            set
            {
                _cameraProperty = value;
                RaisePropertyChanged(() => CameraProperty);
            }
        }

        public PropertyValue<long> ExposureDelay
        {
            get { return _exposureDelay; }
            set
            {
                _exposureDelay = value;
                RaisePropertyChanged(() => ExposureDelay);
            }
        }

        public PropertyValue<long> CompressionSetting
        {
            get { return _compressionSetting; }
            set
            {
                _compressionSetting = value;
                RaisePropertyChanged(() => CompressionSetting);
            }
        }

        public PropertyValue<long> RawCompressionType
        {
            get { return _rawCompressionType; }
            set
            {
                _rawCompressionType = value;
                RaisePropertyChanged(() => RawCompressionType);
            }
        }

        public PropertyValue<long> RawCompressionBitMode
        {
            get { return _RawCompressionBitMode; }
            set
            {
                _RawCompressionBitMode = value;
                RaisePropertyChanged(() => RawCompressionBitMode);
            }
        }

        public PropertyValue<long> ImageSize
        {
            get { return _imageSize; }
            set
            {
                _imageSize = value;
                RaisePropertyChanged(() => ImageSize);
            }
        }

        public PropertyValue<long> ActivePicCtrlItem
        {
            get { return _activePicCtrlItem; }
            set
            {
                _activePicCtrlItem = value;
                RaisePropertyChanged(() => ActivePicCtrlItem);
            }
        }

        public PropertyValue<long> NoiseReduction
        {
            get { return _noiseReduction; }
            set
            {
                _noiseReduction = value;
                RaisePropertyChanged(() => NoiseReduction);
            }
        }

        public PropertyValue<long> FlashCompensation
        {
            get { return _flashCompensation; }
            set
            {
                _flashCompensation = value;
                RaisePropertyChanged(() => FlashCompensation);
            }
        }

        public PropertyValue<long> ColorSpace
        {
            get { return _ColorSpace; }
            set
            {
                _ColorSpace = value;
                RaisePropertyChanged(() => ColorSpace);
            }
        }
        
        public PropertyValue<long> WbColorTemp
        {
            get { return _WbColorTemp; }
            set
            {
                _WbColorTemp = value;
                RaisePropertyChanged(() => WbColorTemp);
            }
        }

        public PropertyValue<long> WbTuneFluorescentType
        {
            get { return _WbTuneFluorescentType; }
            set
            {
                _WbTuneFluorescentType = value;
                RaisePropertyChanged(() => WbTuneFluorescentType);
            }
        }

        public PropertyValue<long> ApplicationMode
        {
            get { return _applicationMode; }
            set
            {
                _applicationMode = value;
                RaisePropertyChanged(() => ApplicationMode);
            }
        }

        public PropertyValue<long> InitFlash
        {
            get { return _initFlash; }
            set
            {
                _initFlash = value;
                RaisePropertyChanged(() => InitFlash);
            }
        }

        public PropertyValue<long> FNumber
        {
            get { return _fNumber; }
            set
            {
                _fNumber = value;
                RaisePropertyChanged(() => FNumber);
            }
        }

        public int LevelAngle
        {
            get { return _LevelAngle; }
            set
            {
                _LevelAngle = value;
                RaisePropertyChanged(() => LevelAngle);
                LevelAngleColor = _LevelAngle % 90 <= 1 || _LevelAngle % 90 >= 89 ? "Green" : "Red";
            }
        }

        public string LevelAngleColor
        {
            get { return _levelAngleColor; }
            set
            {
                _levelAngleColor = value;
                RaisePropertyChanged(() => LevelAngleColor);
            }
        }

        public decimal AngleLevelPitching
        {
            get { return _angleLevelPitching; }
            set
            {
                _angleLevelPitching = value;
                RaisePropertyChanged(() => AngleLevelPitching);
            }
        }
        
        public decimal AngleLevelYawing
        {
            get { return _angleLevelYawing; }
            set
            {
                _angleLevelYawing = value;
                RaisePropertyChanged(() => AngleLevelYawing);
            }
        }

        public PropertyValue<long> CaptureAreaCrop
        {
            get { return _captureAreaCrop; }
            set
            {
                _captureAreaCrop = value;
                RaisePropertyChanged(() => CaptureAreaCrop);
            }
        }

        public PropertyValue<long> DLighting
        {
            get { return _dLighting; }
            set
            {
                _dLighting = value;
                RaisePropertyChanged(() => DLighting);
            }
        }

        public PropertyValue<long> HDRMode
        {
            get { return _HDRMode; }
            set
            {
                _HDRMode = value;
                RaisePropertyChanged(() => HDRMode);
            }
        }

        public PropertyValue<long> HDREv
        {
            get { return _HDREv; }
            set
            {
                _HDREv = value;
                RaisePropertyChanged(() => HDREv);
            }
        }

        public PropertyValue<long> HDRSmoothing
        {
            get { return _HDRSmoothing; }
            set
            {
                _HDRSmoothing = value;
                RaisePropertyChanged(() => HDRSmoothing);
            }
        }

        public PropertyValue<long> FocalLength
        {
            get { return _FocalLength; }
            set
            {
                _FocalLength = value;
                RaisePropertyChanged(() => FocalLength);
            }
        }

        public PropertyValue<long> NoiseReductionHiIso
        {
            get { return _NoiseReductionHiIso; }
            set
            {
                _NoiseReductionHiIso = value;
                RaisePropertyChanged(() => NoiseReductionHiIso);
            }
        }

        public PropertyValue<long> FlashSyncSpeed
        {
            get { return _FlashSyncSpeed; }
            set
            {
                _FlashSyncSpeed = value;
                RaisePropertyChanged(() => FlashSyncSpeed);
            }
        }

        public PropertyValue<long> ActiveSlot
        {
            get { return _ActiveSlot; }
            set
            {
                _ActiveSlot = value;
                RaisePropertyChanged(() => ActiveSlot);
            }
        }

        public PropertyValue<long> LensType
        {
            get { return _LensType; }
            set
            {
                _LensType = value;
                RaisePropertyChanged(() => LensType);
            }
        }

        public PropertyValue<long> LensSort
        {
            get { return _LensSort; }
            set
            {
                if (_LensSort != null)
                    _LensSort.PropertyChanged -= _LensSort_PropertyChanged;
                _LensSort = value;

                if (_LensSort != null)
                    _LensSort.PropertyChanged += _LensSort_PropertyChanged;
                SetAFLensConnected();

                RaisePropertyChanged(() => LensSort);
            }
        }


        public void SetAFLensConnected()
        {
            if (_LensSort != null) AFLensConnected = _LensSort.NumericValue == 1;
            else AFLensConnected = false;
           
        }
        void _LensSort_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetAFLensConnected();
        }

        private bool _AFLensConnected = false;
        private bool _captureInProgress;
        private bool _liveViewIsRunning;


        public bool AFLensConnected
        {
            //get { return StepperManager.Instance.AFLensConnected; }
            get { return _AFLensConnected; }
            set
            {
                //StepperManager.Instance.AFLensConnected = value;
                _AFLensConnected = value;
                RaisePropertyChanged(() => AFLensConnected);
                RaisePropertyChanged(() => NotAFLensConnected);
            }
        }

        public bool NotAFLensConnected
        {
            get { return !AFLensConnected; }
        }


        

        
        






        #endregion

        #region Auto Magnification

        public void MagiCalc(double sensor)
        {
            // get image from LiveView
            LiveViewData = SelectedCameraDevice.GetLiveViewImage();

            MemoryStream stream = new MemoryStream(LiveViewData.ImageData,
                        LiveViewData.
                            ImageDataPosition,
                        LiveViewData.ImageData.
                            Length -
                        LiveViewData.
                            ImageDataPosition);

            using (var tempImage = new Bitmap(stream))
            {
                Bitmap bmp = tempImage;
        

                var preview = BitmapFactory.ConvertToPbgra32Format(
                    BitmapSourceConvert.ToBitmapSource(bmp));



                Bitmap binaryimage = bmp;

                // binarize the image to BinaryImage
                
                short[,] se = new short[,] {
                {  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1 },
                {  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1 },
                {  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1 },
                {  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1 },
                {  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1 },
                {  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1 },
                {  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1 },
                {  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1 },
                {  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1 },
                {  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1 },
                {  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1 }
                };


                var filter = new FiltersSequence(
                                Grayscale.CommonAlgorithms.BT709,
                                new Threshold(100),
                                new Closing(se),
                                new Opening(se),
                                new Opening(se),
                                new Opening(se),
                                new Invert()
                                );
                binaryimage = filter.Apply(bmp);

                // Pass ConnectedComponentsLabeling Filter
                WriteableBitmap writeableBitmap;

                // Pass ConnectedComponentsLabeling Filter
                ConnectedComponentsLabeling filter2 = new ConnectedComponentsLabeling();
                writeableBitmap = BitmapFactory.ConvertToPbgra32Format(BitmapSourceConvert.ToBitmapSource(filter2.Apply(binaryimage)));

                // Check objects count
                int objectCount = filter2.ObjectCount;
                StepperManager.Instance.LinesNumber = objectCount;

                writeableBitmap.Freeze();
                ColorBitmapSource = writeableBitmap;


                // Calc Magnification
                double one_one;
                one_one = 59 * sensor;

                double _magni;
                _magni = one_one / (objectCount);

                double _Magni = Math.Round(_magni, 1, MidpointRounding.AwayFromZero); //Rounds"up"

                StepperManager.Instance.Magni = _Magni;
            }
            return;
        }


        public BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap wbm)
        {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }

        #endregion


        public LiveViewViewModel()
        {

        }

        void DeviceManager_CameraSelected(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice)
        {
            SelectedCameraDevice = ServiceProvider.DeviceManager.SelectedCameraDevice;
            CameraProperty = SelectedCameraDevice.LoadProperties();
            SelectedCameraDevice.CameraInitDone += SelectedCameraDevice_CameraInitDone;           
        }

        void DeviceManager_CameraDisconnected(ICameraDevice cameraDevice)
        {
            SelectedCameraDevice = ServiceProvider.DeviceManager.SelectedCameraDevice;
            CameraProperty = SelectedCameraDevice.LoadProperties();
            InitAdvancedProperties();
        }


        void InitAdvancedProperties()
        {
            ExposureDelay = SelectedCameraDevice.GetProperty(0xD06A);
            InitFlash = SelectedCameraDevice.GetProperty(0x500C);
            ApplicationMode = SelectedCameraDevice.GetProperty(0xD1F0);
            ColorSpace = SelectedCameraDevice.GetProperty(0xD032);
            WbColorTemp = SelectedCameraDevice.GetProperty(0xD01E);
            WbTuneFluorescentType = SelectedCameraDevice.GetProperty(0xD14F);
            FlashCompensation = SelectedCameraDevice.GetProperty(0xD124);
            NoiseReduction = SelectedCameraDevice.GetProperty(0xD06B);
            ActivePicCtrlItem = SelectedCameraDevice.GetProperty(0xD200);
            ImageSize = SelectedCameraDevice.GetProperty(0x5003);
            RawCompressionType = SelectedCameraDevice.GetProperty(0xD016);
            RawCompressionBitMode = SelectedCameraDevice.GetProperty(0xD149);
            FNumber = SelectedCameraDevice.GetProperty(0x5007);
            //LevelAngle = SelectedCameraDevice.GetProperty(0xD067);
            //AngleLevelPitching = SelectedCameraDevice.GetProperty(0xD07D);
            //AngleLevelYawing = SelectedCameraDevice.GetProperty(0xD07E);
            CaptureAreaCrop = SelectedCameraDevice.GetProperty(0xD030);
            DLighting = SelectedCameraDevice.GetProperty(0xD14E);
            HDRMode = SelectedCameraDevice.GetProperty(0xD130);
            HDREv = SelectedCameraDevice.GetProperty(0xD131);
            HDRSmoothing = SelectedCameraDevice.GetProperty(0xD132);
            FocalLength = SelectedCameraDevice.GetProperty(0x5008);
            NoiseReductionHiIso = SelectedCameraDevice.GetProperty(0xD070);
            FlashSyncSpeed = SelectedCameraDevice.GetProperty(0xD074);
            ActiveSlot = SelectedCameraDevice.GetProperty(0xD1F2);
            LensSort = SelectedCameraDevice.GetProperty(0xD0E1);
            LensType = SelectedCameraDevice.GetProperty(0xD0E2);
            
        }

        void SelectedCameraDevice_CameraInitDone(ICameraDevice cameraDevice)
        {
            InitAdvancedProperties();
            SelectedCameraDevice.CameraInitDone -= SelectedCameraDevice_CameraInitDone;
        }

        public LiveViewViewModel(ICameraDevice device)
        {
            StartLiveViewCommand = new RelayCommand(StartLiveView);
            StopLiveViewCommand = new RelayCommand(StopLiveView);
            SetAreaCommand = new RelayCommand(() => SettingArea = true);
            DoneSetAreaCommand = new RelayCommand(() => SettingArea = false);
            ServiceProvider.DeviceManager.CameraSelected += DeviceManager_CameraSelected;
            ServiceProvider.DeviceManager.CameraDisconnected += DeviceManager_CameraDisconnected;
            ServiceProvider.FileTransfered += ServiceProvider_FileTransfered;
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            if (!IsInDesignMode)
            {
                SelectedCameraDevice = ServiceProvider.DeviceManager.SelectedCameraDevice;
                CameraProperty = SelectedCameraDevice.LoadProperties();
                InitAdvancedProperties();
                InitCommands();
                ShowHistogram = true;
                Init();
            }
        }

        void ServiceProvider_FileTransfered(object sender, FileItem fileItem)
        {
            if (LiveViewIsRunning)
            {
                _timer.Start();
                StartLiveView();
            }
            _photoCapturedTime = DateTime.Now;
        }


        private void Init()
        {
            IsActive = true;
            //WaitTime = 2;
            //PhotoNo = 2;
           // FocusStep = 2;
            //PhotoCount = 5;
            //CaptureCount = 1;
            //DelayedStart = false;

            _timer.Stop();
            _timer.AutoReset = true;
            //CameraDevice.CameraDisconnected += CameraDeviceCameraDisconnected;
            //_photoCapturedTime = DateTime.MinValue;
            SelectedCameraDevice.PhotoCaptured += CameraDevicePhotoCaptured;
            //StartLiveView();
            //_freezeTimer.Interval = ServiceProvider.Settings.LiveViewFreezeTimeOut * 1000;
            //_freezeTimer.Elapsed += _freezeTimer_Elapsed;
            _timer.Elapsed += _timer_Elapsed;
            //ServiceProvider.WindowsManager.Event += WindowsManager_Event;
            //_focusStackingTimer.AutoReset = true;
            //_focusStackingTimer.Elapsed += _focusStackingTimer_Elapsed;
            //_restartTimer.AutoReset = true;
            //_restartTimer.Elapsed += _restartTimer_Elapsed;
        }

        private void CameraDevicePhotoCaptured(object sender, PhotoCapturedEventArgs eventargs)
        {
            _photoCapturedTime = DateTime.Now;
            StartLiveView();
        }

        private void InitCommands()
        {
           //AutoFocusCommand = new RelayCommand(AutoFocus);
            //RecordMovieCommand = new RelayCommand(delegate
            //{
             //   if (Recording)
            //        StopRecordMovie();
            //    else
            //        RecordMovie();
           // },
            //     () => CameraDevice.GetCapability(CapabilityEnum.RecordMovie));
            //CaptureCommand = new RelayCommand(CaptureInThread);
            //FocusMCommand = new RelayCommand(() => SetFocus(SimpleManualFocus ? -ServiceProvider.Settings.SmallFocusStepCanon : -ServiceProvider.Settings.SmalFocusStep));
           // FocusMMCommand = new RelayCommand(() => SetFocus(SimpleManualFocus ? -ServiceProvider.Settings.MediumFocusStepCanon : -ServiceProvider.Settings.MediumFocusStep));
           // FocusMMMCommand = new RelayCommand(() => SetFocus(SimpleManualFocus ? -ServiceProvider.Settings.LargeFocusStepCanon : -ServiceProvider.Settings.LargeFocusStep));
            //FocusPCommand = new RelayCommand(() => SetFocus(SimpleManualFocus ? ServiceProvider.Settings.SmallFocusStepCanon : ServiceProvider.Settings.SmalFocusStep));
            //FocusPPCommand = new RelayCommand(() => SetFocus(SimpleManualFocus ? ServiceProvider.Settings.MediumFocusStepCanon : ServiceProvider.Settings.MediumFocusStep));
            //FocusPPPCommand = new RelayCommand(() => SetFocus(SimpleManualFocus ? ServiceProvider.Settings.LargeFocusStepCanon : ServiceProvider.Settings.LargeFocusStep));
            //MoveACommand = new RelayCommand(() => SetFocus(-FocusCounter));
            //MoveBCommand = new RelayCommand(() => SetFocus(FocusValue));
            //LockCurrentNearCommand = new RelayCommand(() =>
            //{
                //if (LockB)
               // {
               //     FocusValue = FocusValue - FocusCounter;
               //     FocusCounter = 0;
               // }
              //  LockA = true;
            //});

            //LockCurrentFarCommand = new RelayCommand(() =>
            //{
            //    if (LockB || LockA)
            //    {
             //       FocusValue = FocusCounter;
             //   }
             //   LockB = true;
            //});

            //StartFocusStackingCommand = new RelayCommand(StartFocusStacking, () => LockB);
            //PreviewFocusStackingCommand = new RelayCommand(PreviewFocusStacking, () => LockB);
            //StopFocusStackingCommand = new RelayCommand(StopFocusStacking);
            //StartLiveViewCommand = new RelayCommand(StartLiveView);
            //StopLiveViewCommand = new RelayCommand(StopLiveView);
            

            //StartSimpleFocusStackingCommand = new RelayCommand(StartSimpleFocusStacking);
            //PreviewSimpleFocusStackingCommand = new RelayCommand(PreviewSimpleFocusStacking);
            //StopSimpleFocusStackingCommand = new RelayCommand(StopFocusStacking);
            //HelpFocusStackingCommand = new RelayCommand(() => HelpProvider.Run(HelpSections.FocusStacking));

            //BrowseOverlayCommand = new RelayCommand(BrowseOverlay);
            //ResetOverlayCommand = new RelayCommand(ResetOverlay);
            
            ResetBrigthnessCommand = new RelayCommand(() => Brightness = 0);
            ZoomInCommand = new RelayCommand(() => SelectedCameraDevice.LiveViewImageZoomRatio.NextValue());
            ZoomOutCommand = new RelayCommand(() => SelectedCameraDevice.LiveViewImageZoomRatio.PrevValue());
            ZoomIn100 = new RelayCommand(ToggleZoom);
            ToggleGridCommand = new RelayCommand(ToggleGrid);
            //FocuseDone += LiveViewViewModel_FocuseDone;

            SetAreaCommand = new RelayCommand(() => SettingArea = true);
            DoneSetAreaCommand = new RelayCommand(() => SettingArea = false);

            RaisePropertyChanged(()=>ZoomIn100);

            //CancelCaptureCommand = new RelayCommand(() => CaptureCancelRequested = true);

        }

        public void CaptureInThread()
        {
            var thread = new Thread(Capture);
            thread.Start();
            //thread.Join();
        }

        private void Capture()
        {
            if (SelectedCameraDevice.ShutterSpeed != null && SelectedCameraDevice.ShutterSpeed.Value == "Bulb")
            {
                StaticHelper.Instance.SystemMessage = TranslationStrings.MsgBulbModeNotSupported;
                ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.LiveViewWnd_Message,
                    TranslationStrings.MsgBulbModeNotSupported);
                CaptureInProgress = false;
                return;
            }
            CaptureInProgress = true;
            Log.Debug("LiveView: Capture started");
            _timer.Stop();
            Thread.Sleep(300);
            try
            {
                SelectedCameraDevice.CapturePhotoNoAf();
                Log.Debug("LiveView: Capture Initialization Done");
            }
            catch (Exception exception)
            {
                StaticHelper.Instance.SystemMessage = exception.Message;
                Log.Error("Unable to take picture with no af", exception);
            }
            CaptureInProgress = false;
        }

        private void ToggleZoom()
        {
            try
            {
                if (SelectedCameraDevice.LiveViewImageZoomRatio == null || SelectedCameraDevice.LiveViewImageZoomRatio.Values == null)
                    return;
                // for canon cameras 
                if (SelectedCameraDevice.LiveViewImageZoomRatio.Values.Count == 2)
                {
                    SelectedCameraDevice.LiveViewImageZoomRatio.Value = SelectedCameraDevice.LiveViewImageZoomRatio.Value ==
                                                                SelectedCameraDevice.LiveViewImageZoomRatio.Values[0]
                        ? SelectedCameraDevice.LiveViewImageZoomRatio.Values[1]
                        : SelectedCameraDevice.LiveViewImageZoomRatio.Values[0];
                }
                else
                {
                    SelectedCameraDevice.LiveViewImageZoomRatio.Value = SelectedCameraDevice.LiveViewImageZoomRatio.Value ==
                                                                SelectedCameraDevice.LiveViewImageZoomRatio.Values[0]
                        ? SelectedCameraDevice.LiveViewImageZoomRatio.Values[SelectedCameraDevice.LiveViewImageZoomRatio.Values.Count - 2]
                        : SelectedCameraDevice.LiveViewImageZoomRatio.Values[0];
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to set zoom", ex);
            }
        }

        public AsyncObservableCollection<string> Grids
        {
            get { return _grids; }
            set
            {
                _grids = value;
                RaisePropertyChanged(() => Grids);
            }
        }

        public int GridType
        {
            get { return _gridType; }
            set
            {
                _gridType = value;
                RaisePropertyChanged(() => GridType);
            }
        }

        private void ToggleGrid()
        {
            var i = GridType;
            i++;
            if (i >= Grids.Count)
                i = 0;
            GridType = i;
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Task.Factory.StartNew(GetLiveImage);
        }

        private void GetLiveImage()
        {
            if (_operInProgress)
                return;

            try
            {
                LiveViewData = SelectedCameraDevice.GetLiveViewImage();
            }
            catch (Exception ex)
            {
                Log.Error("Error geting lv", ex);
                _operInProgress = false;
                return;
            }

            if (LiveViewData == null)
            {
                _operInProgress = false;
                return;
            }

            try
            {
                if (PreviewTime > 0 && (DateTime.Now - _photoCapturedTime).TotalSeconds <= PreviewTime)
                {
                    Bitmap = ServiceProvider.Settings.SelectedBitmap.DisplayImage;
                    _operInProgress = false;
                    Console.WriteLine("Previeving");
                    return;
                }

                if (LiveViewData != null && LiveViewData.ImageData != null)
                {
                    MemoryStream stream = new MemoryStream(LiveViewData.ImageData,
                        LiveViewData.
                            ImageDataPosition,
                        LiveViewData.ImageData.
                            Length -
                        LiveViewData.
                            ImageDataPosition);

                    LevelAngle = (int)LiveViewData.LevelAngleRolling;
                    AngleLevelPitching = LiveViewData.LevelAnglePitching;
                    AngleLevelYawing = LiveViewData.LevelAngleYawing;


                    using (var res = new Bitmap(stream))
                    {
                        Bitmap bmp = res;
                        if (_totalframes%DesiredFrameRate == 0 && ShowHistogram)
                        {
                            ImageStatisticsHSL hslStatistics =
                                new ImageStatisticsHSL(bmp);
                            LuminanceHistogramPoints =
                                ConvertToPointCollection(
                                    hslStatistics.Luminance.Values);
                            ImageStatistics statistics = new ImageStatistics(bmp);
                            RedColorHistogramPoints = ConvertToPointCollection(
                                statistics.Red.Values);
                            GreenColorHistogramPoints = ConvertToPointCollection(
                                statistics.Green.Values);
                            BlueColorHistogramPoints = ConvertToPointCollection(
                                statistics.Blue.Values);
                        }

                        if (HighlightUnderExp)
                        {
                            ColorFiltering filtering = new ColorFiltering();
                            filtering.Blue = new IntRange(0, 5);
                            filtering.Red = new IntRange(0, 5);
                            filtering.Green = new IntRange(0, 5);
                            filtering.FillOutsideRange = false;
                            filtering.FillColor = new RGB(System.Drawing.Color.Blue);
                            filtering.ApplyInPlace(bmp);
                        }

                        if (HighlightOverExp)
                        {
                            ColorFiltering filtering = new ColorFiltering();
                            filtering.Blue = new IntRange(250, 255);
                            filtering.Red = new IntRange(250, 255);
                            filtering.Green = new IntRange(250, 255);
                            filtering.FillOutsideRange = false;
                            filtering.FillColor = new RGB(System.Drawing.Color.Red);
                            filtering.ApplyInPlace(bmp);
                        }

                        var preview = BitmapFactory.ConvertToPbgra32Format(
                            BitmapSourceConvert.ToBitmapSource(bmp));

                        if (Brightness != 0)
                        {
                            BrightnessCorrection filter = new BrightnessCorrection(Brightness);
                            bmp = filter.Apply(bmp);
                        }


                        Bitmap newbmp = bmp;
                        if (EdgeDetection)
                        {
                            var filter = new FiltersSequence(
                                Grayscale.CommonAlgorithms.BT709,
                                new HomogenityEdgeDetector()
                                );
                            newbmp = filter.Apply(bmp);
                        }

                        WriteableBitmap writeableBitmap;

                        if (BlackAndWhite)
                        {
                            Grayscale filter = new Grayscale(0.299, 0.587, 0.114);
                            writeableBitmap =
                                BitmapFactory.ConvertToPbgra32Format(
                                    BitmapSourceConvert.ToBitmapSource(
                                        filter.Apply(newbmp)));
                        }
                        else
                        {
                            writeableBitmap =
                                BitmapFactory.ConvertToPbgra32Format(
                                    BitmapSourceConvert.ToBitmapSource(newbmp));
                        }
                        DrawGrid(writeableBitmap);
                        DrawFocusPoint(writeableBitmap);
                        writeableBitmap.Freeze();
                        Bitmap = writeableBitmap;
                        _operInProgress = false;
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error geting lv", ex);
                _operInProgress = false;
                return;
            }
        }

        public virtual void SetFocusPos(int x, int y)
        {
            try
            {
                SelectedCameraDevice.Focus(x, y);
            }
            catch (Exception exception)
            {
                Log.Error("Error set focus pos :", exception);
                StaticHelper.Instance.SystemMessage = TranslationStrings.LabelErrorSetFocusPos;
            }
        }

        public void SetFocusPos(Point initialPoint, double refWidth, double refHeight)
        {
            if (LiveViewData != null)
            {
                var CropRatio = 0;
                //CropOffsetX = (writeableBitmap.PixelWidth / 2) * CropRatio / 100;
                double offsetX = (((refWidth * 100) / (100 - CropRatio)) - refWidth) / 2;
                double offsety = (((refHeight * 100) / (100 - CropRatio)) - refHeight) / 2; ; ;
                double xt = (LiveViewData.ImageWidth) / (refWidth + (offsetX * 2));
                double yt = (LiveViewData.ImageHeight) / (refHeight + (offsety * 2));
                int posx = (int)((offsetX + initialPoint.X) * xt);
                //if (FlipImage)
                //    posx = (int)(((refWidth) - initialPoint.X + offsetX) * xt);
                int posy = (int)((initialPoint.Y + offsety) * yt);
                Task.Factory.StartNew(() => SetFocusPos(posx, posy));
            }
        }

        private void DrawFocusPoint(WriteableBitmap bitmap, bool fill = false)
        {
            try
            {
                if (LiveViewData == null)
                    return;
                double xt = bitmap.Width / LiveViewData.ImageWidth;
                double yt = bitmap.Height / LiveViewData.ImageHeight;

                if (fill)
                {
                    bitmap.FillRectangle2((int)(LiveViewData.FocusX * xt - (LiveViewData.FocusFrameXSize * xt / 2)),
                        (int)(LiveViewData.FocusY * yt - (LiveViewData.FocusFrameYSize * yt / 2)),
                        (int)(LiveViewData.FocusX * xt + (LiveViewData.FocusFrameXSize * xt / 2)),
                        (int)(LiveViewData.FocusY * yt + (LiveViewData.FocusFrameYSize * yt / 2)),
                        LiveViewData.HaveFocusData
                            ? System.Windows.Media.Color.FromArgb(0x60, 0, 0xFF, 0)
                            : System.Windows.Media.Color.FromArgb(0x60, 0xFF, 0, 0));
                }
                else
                {
                    bitmap.DrawRectangle((int)(LiveViewData.FocusX * xt - (LiveViewData.FocusFrameXSize * xt / 2)),
                        (int)(LiveViewData.FocusY * yt - (LiveViewData.FocusFrameYSize * yt / 2)),
                        (int)(LiveViewData.FocusX * xt + (LiveViewData.FocusFrameXSize * xt / 2)),
                        (int)(LiveViewData.FocusY * yt + (LiveViewData.FocusFrameYSize * yt / 2)),
                        LiveViewData.HaveFocusData ? Colors.Green : Colors.Red);
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error draw helper lines", exception);
            }
        }

        private void DrawGrid(WriteableBitmap writeableBitmap)
        {
            Color color = Colors.White;
            color.A = 50;

            if (ShowRuler && NoSettingArea)
            {
                int x1 = writeableBitmap.PixelWidth * (HorizontalMin) / 1000;
                int x2 = writeableBitmap.PixelWidth * (HorizontalMin + HorizontalMax) / 1000;
                int y2 = writeableBitmap.PixelHeight * (VerticalMin + VerticalMax) / 1000;
                int y1 = writeableBitmap.PixelHeight * VerticalMin / 1000;

                writeableBitmap.FillRectangle2(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight, Color.FromArgb(128, 128, 128, 128));
                writeableBitmap.FillRectangleDeBlend(x1, y1, x2, y2, Color.FromArgb(128, 128, 128, 128));
                writeableBitmap.DrawRectangle(x1, y1, x2, y2, color);

            }

        }

        private void StartLiveView()
        {
            //if (!IsActive)
            //    return;

            string resp = SelectedCameraDevice.GetProhibitionCondition(OperationEnum.LiveView);
            if (string.IsNullOrEmpty(resp))
            {
                Thread thread = new Thread(StartLiveViewThread);
                thread.Start();
                thread.Join();
                LiveViewIsRunning = true;
            }
            else
            {
                Log.Error("Error starting live view " + resp);
                // in nikon case no show error message
                // if the images not transferd yet from SDRam
                if (resp != "LabelImageInRAM" && resp != "LabelCommandProcesingError")
                    ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.LiveViewWnd_Message,
                        TranslationStrings.LabelLiveViewError + "\n" +
                        TranslationManager.GetTranslation(resp));
            }
        }

        private void StartLiveViewThread()
        {
            try
            {
                bool retry = false;
                int retryNum = 0;
                Log.Debug("LiveView: Liveview started");
                do
                {
                    try
                    {
                        SelectedCameraDevice.StartLiveView();
                    }
                    catch (DeviceException deviceException)
                    {
                        if (deviceException.ErrorCode == ErrorCodes.ERROR_BUSY ||
                            deviceException.ErrorCode == ErrorCodes.MTP_Device_Busy)
                        {
                            Thread.Sleep(100);
                            Log.Debug("Retry live view :" + deviceException.ErrorCode.ToString("X"));
                            retry = true;
                            retryNum++;
                        }
                        else
                        {
                            throw;
                        }
                    }
                } while (retry && retryNum < 35);
                _timer.Start();
                _operInProgress = false;
                Log.Debug("LiveView: Liveview start done");
            }
            catch (Exception exception)
            {
                Log.Error("Unable to start liveview !", exception);
                StaticHelper.Instance.SystemMessage = "Unable to start liveview ! " + exception.Message;
            }
        }

        private void StopLiveView()
        {
            if (!SelectedCameraDevice.IsChecked)
                return;
            Thread thread = new Thread(StopLiveViewThread);
            thread.Start();
        }

        private void StopLiveViewThread()
        {
            try
            {
                bool retry = false;
                int retryNum = 0;
                Log.Debug("LiveView: Liveview stopping");
                do
                {
                    try
                    {
                        SelectedCameraDevice.StopLiveView();
                    }
                    catch (DeviceException deviceException)
                    {
                        if (deviceException.ErrorCode == ErrorCodes.ERROR_BUSY ||
                            deviceException.ErrorCode == ErrorCodes.MTP_Device_Busy)
                        {
                            Thread.Sleep(500);
                            Log.Debug("Retry live view stop:" + deviceException.ErrorCode.ToString("X"));
                            retry = true;
                            retryNum++;
                        }
                        else
                        {
                            throw;
                        }
                    }
                } while (retry && retryNum < 35);
                LiveViewIsRunning = false;
            }
            catch (Exception exception)
            {
                Log.Error("Unable to stop liveview !", exception);
                StaticHelper.Instance.SystemMessage = "Unable to stop liveview ! " + exception.Message;
            }
        }

        #region histogram

        public PointCollection LuminanceHistogramPoints
        {
            get { return _luminanceHistogramPoints; }
            set
            {
                if (_luminanceHistogramPoints != value)
                {
                    _luminanceHistogramPoints = value;
                    RaisePropertyChanged(() => LuminanceHistogramPoints);
                }
            }
        }

        public PointCollection RedColorHistogramPoints
        {
            get { return _redColorHistogramPoints; }
            set
            {
                _redColorHistogramPoints = value;
                RaisePropertyChanged(() => RedColorHistogramPoints);
            }
        }

        public PointCollection GreenColorHistogramPoints
        {
            get { return _greenColorHistogramPoints; }
            set
            {
                _greenColorHistogramPoints = value;
                RaisePropertyChanged(() => GreenColorHistogramPoints);
            }
        }

        public PointCollection BlueColorHistogramPoints
        {
            get { return _blueColorHistogramPoints; }
            set
            {
                _blueColorHistogramPoints = value;
                RaisePropertyChanged(() => BlueColorHistogramPoints);
            }
        }

        public bool HighlightOverExp
        {
            get { return CameraProperty.LiveviewSettings.HighlightOverExp; }
            set
            {
                CameraProperty.LiveviewSettings.HighlightOverExp = value;
                RaisePropertyChanged(() => HighlightOverExp);
            }
        }

        public bool HighlightUnderExp
        {
            get { return CameraProperty.LiveviewSettings.HighlightUnderExp; }
            set
            {
                CameraProperty.LiveviewSettings.HighlightUnderExp = value;
                RaisePropertyChanged(() => HighlightUnderExp);
            }
        }

        private PointCollection ConvertToPointCollection(int[] values)
        {
            int max = values.Max();

            PointCollection points = new PointCollection();
            // first point (lower-left corner)
            points.Add(new Point(0, max));
            // middle points
            for (int i = 0; i < values.Length; i++)
            {
                points.Add(new Point(i, max - values[i]));
            }
            // last point (lower-right corner)
            points.Add(new Point(values.Length - 1, max));
            points.Freeze();
            return points;
        }

        #endregion


    }
}
