using System;
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

        private bool _operInProgress = false;
        private Timer _timer = new Timer(1000 / 25.0);

        private ICameraDevice _cameraDevice;
        private BitmapSource _bitmap;
        private bool _settingArea;
        private CameraProperty _cameraProperty;

        private AsyncObservableCollection<string> _grids;
        private int _gridType;

        private PointCollection _luminanceHistogramPoints = null;
        private PointCollection _redColorHistogramPoints;
        private PointCollection _greenColorHistogramPoints;
        private PointCollection _blueColorHistogramPoints;

        public ICameraDevice SelectedCameraDevice
        {
            get { return _cameraDevice; }
            set
            {
                _cameraDevice = value;
                RaisePropertyChanged(() => SelectedCameraDevice);
            }
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

        public CameraProperty CameraProperty
        {
            get { return _cameraProperty; }
            set
            {
                _cameraProperty = value;
                RaisePropertyChanged(() => CameraProperty);
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

        public BitmapSource Bitmap
        {
            get { return _bitmap; }
            set
            {
                _bitmap = value;
                RaisePropertyChanged(() => Bitmap);
            }
        }
        


        public bool IsActive { get; set; }
        public LiveViewData LiveViewData { get; set; }

        public RelayCommand StartLiveViewCommand { get; set; }
        public RelayCommand StopLiveViewCommand { get; set; }

        public RelayCommand SetAreaCommand { get; set; }
        public RelayCommand DoneSetAreaCommand { get; set; }

        public LiveViewViewModel()
        {
            StartLiveViewCommand = new RelayCommand(StartLiveView);
            StopLiveViewCommand = new RelayCommand(StopLiveView);
            SetAreaCommand = new RelayCommand(() => SettingArea = true);
            DoneSetAreaCommand = new RelayCommand(() => SettingArea = false);

            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            if (!IsInDesignMode)
            {
                SelectedCameraDevice = ServiceProvider.DeviceManager.SelectedCameraDevice;
                CameraProperty = SelectedCameraDevice.LoadProperties();
                InitCommands();
                ShowHistogram = true;
                Init();
            }
        }

        public LiveViewViewModel(ICameraDevice device)
        {
            SelectedCameraDevice = device;
            CameraProperty = device.LoadProperties();
            InitCommands();
            ShowHistogram = true;
            Init();
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
           // CameraDevice.PhotoCaptured += CameraDevicePhotoCaptured;
            StartLiveView();
            //_freezeTimer.Interval = ServiceProvider.Settings.LiveViewFreezeTimeOut * 1000;
            //_freezeTimer.Elapsed += _freezeTimer_Elapsed;
            _timer.Elapsed += _timer_Elapsed;
            //ServiceProvider.WindowsManager.Event += WindowsManager_Event;
            //_focusStackingTimer.AutoReset = true;
            //_focusStackingTimer.Elapsed += _focusStackingTimer_Elapsed;
            //_restartTimer.AutoReset = true;
            //_restartTimer.Elapsed += _restartTimer_Elapsed;
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
                if (LiveViewData != null && LiveViewData.ImageData != null)
                {
                    MemoryStream stream = new MemoryStream(LiveViewData.ImageData,
                        LiveViewData.
                            ImageDataPosition,
                        LiveViewData.ImageData.
                            Length -
                        LiveViewData.
                            ImageDataPosition);


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
