﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Macrophotography.controls;

namespace Macrophotography
{
    public class StepperManager : ViewModelBase
    {
        #region RaisePropertyChanged Variables

        private SerialPort sp = new SerialPort();
        private static StepperManager _instance;
        private int _speed = 50;
        private int _speed3d = 400;
        private bool _isBusy;
        private bool _IsStacking;
        private bool _IsNearFocusLocked; 
        private bool _IsFarFocusLocked;
        private bool _IsLightON = false;
        private bool _IsLightON2 = false;
        private bool _GoNearToFar = true;
        private int _position = 0;
        private int _lastPosition = 0;
        private int _nearFocus = 0;
        private int _farFocus = 0;
        private int _nearFocus2 = 0;
        private int _farFocus2 = 0;
        
        private int _totalDOF = 0;
        private int _totalDOFFull = 0;
        private double _shotDOF = 0;
        private double _shotDOFFull = 0;
       
        private int _Step;
        private int _shotStep = 0;
        private int _shotStep2 = 0;
        private int _Overlap = 20;
        
        private double _magni = 2;
        private double _magniMax = 40;
        private double _magniMin = 0.25;
        private double _railAccuracy = 0;

        private int _InitStackDelay = 0;
        private int _StabilizationDelay = 0;

        private int _ShotsNumber = 1;
        private int _ShotsNumberFull = 1;
        private int _PlusNearShots = 0;
        private int _PlusFarShots = 0;

        private double _aperture = 0;
        private double _apertureAF = 0;
        private double _nA = 0;

        private double _pitch = 4.7;
        private double _e = 2;
        private int _lambda = 550;
        private double _n = 1;

        private int _LinesNumber = 0;

        private int _LightValue = 20;
        private int _LightValue2 = 20;

        private int _degrees = 20;


        #endregion


        #region RaisePropertyChanged Methods

        public static StepperManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StepperManager();
                return _instance;
            }
            set { _instance = value; }
        }


        public int TotalDOF
        {
            get { return _totalDOF; }
            set
            {
                _totalDOF = value;
                RaisePropertyChanged(() => TotalDOF);
            }
        }

        public int TotalDOFFull
        {
            get { return _totalDOFFull; }
            set
            {
                _totalDOFFull = value;
                RaisePropertyChanged(() => TotalDOFFull);
            }
        }
        
        public int FarFocus
        {
            get { return _farFocus; }
            set
            {
                _farFocus = value;
                RaisePropertyChanged(() => FarFocus);
            }
        }

        public int FarFocus2
        {
            get { return _farFocus2; }
            set
            {
                _farFocus2 = value;
                RaisePropertyChanged(() => FarFocus2);
            }
        }

        public int NearFocus
        {
            get { return _nearFocus; }
            set
            {
                _nearFocus = value;
                RaisePropertyChanged(() => NearFocus);
            }
        }

        public int NearFocus2
        {
            get { return _nearFocus2; }
            set
            {
                _nearFocus2 = value;
                RaisePropertyChanged(() => NearFocus2);
            }
        }

        public int Position
        {
            get { return _position; }
            set
            {
                _position = value;
                RaisePropertyChanged(() => Position);
            }
        }

        public int LastPosition
        {
            get { return _lastPosition; }
            set
            {
                _lastPosition = value;
                RaisePropertyChanged(() => LastPosition);
            }
        }
             
        public bool IsNearFocusLocked
        {
            get { return _IsNearFocusLocked; }
            set
            {
                _IsNearFocusLocked = value;
                RaisePropertyChanged(() => IsNearFocusLocked);
                RaisePropertyChanged(() => IsNearFocusUnlocked);
            }
        }

        public bool IsNearFocusUnlocked
        {
            get { return !IsNearFocusLocked; }
        }

        public bool IsFarFocusLocked
        {
            get { return _IsFarFocusLocked; }
            set
            {
                _IsFarFocusLocked = value;
                RaisePropertyChanged(() => IsFarFocusLocked);
                RaisePropertyChanged(() => IsFarFocusUnlocked);
            }
        }

        public bool IsFarFocusUnlocked
        {
            get { return !IsFarFocusLocked; }
        }

        public bool IsLightON
        {
            get { return _IsLightON; }
            set
            {
                _IsLightON = value;
                RaisePropertyChanged(() => IsLightON);
            }
        }

        public bool IsLightON2
        {
            get { return _IsLightON2; }
            set
            {
                _IsLightON2 = value;
                RaisePropertyChanged(() => IsLightON2);
            }
        }

        public bool IsStacking
        {
            get { return _IsStacking; }
            set
            {
                _IsStacking = value;
                RaisePropertyChanged(() => IsStacking);
                RaisePropertyChanged(() => IsNotStacking);
            }
        }

        public bool IsNotStacking
        {
            get { return !IsStacking; }
            set
            {
                IsNotStacking = value;
                RaisePropertyChanged(() => IsNotStacking);
                RaisePropertyChanged(() => IsStacking);
            }
        }


        public bool GoNearToFar
        {
            get { return _GoNearToFar; }
            set
            {
                _GoNearToFar = value;
                RaisePropertyChanged(() => GoNearToFar);
                RaisePropertyChanged(() => GoFarToNear);
            }
        }

        public bool GoFarToNear
        {
            get { return !GoNearToFar; }
        }

        public int Speed
        {
            get { return _speed; }
            set
            {
                _speed = value;
                RaisePropertyChanged(() => Speed);
            }
        }

        public int Speed3d
        {
            get { return _speed3d; }
            set
            {
                _speed3d = value;
                RaisePropertyChanged(() => Speed3d);
            }
        }

        public int Step
        {
            get { return _Step; }
            set
            {
                _Step = value;
                RaisePropertyChanged(() => Step);
            }
        }

        public double Magni
        {
            get { return _magni; }
            set
            {
                _magni = value;
                RaisePropertyChanged(() => Magni);
            }
        }

        public double MagniMin
        {
            get { return _magniMin; }
            set
            {
                _magniMin = value;
                RaisePropertyChanged(() => MagniMin);
            }
        }

        public double MagniMax
        {
            get { return _magniMax; }
            set
            {
                _magniMax = value;
                RaisePropertyChanged(() => MagniMax);
            }
        }

        public double RailAccuracy
        {
            get { return _railAccuracy; }
            set
            {
                _railAccuracy = value;
                RaisePropertyChanged(() => RailAccuracy);
            }
        }

        public int InitStackDelay
        {
            get { return _InitStackDelay; }
            set
            {
                _InitStackDelay = value;
                RaisePropertyChanged(() => InitStackDelay);
            }
        }

        public int StabilizationDelay
        {
            get { return _StabilizationDelay; }
            set
            {
                _StabilizationDelay = value;
                RaisePropertyChanged(() => StabilizationDelay);
            }
        }

        public double ShotDOF
        {
            get { return _shotDOF; }
            set
            {
                _shotDOF = value;
                RaisePropertyChanged(() => ShotDOF);
            }
        }

        public double ShotDOFFull
        {
            get { return _shotDOFFull; }
            set
            {
                _shotDOFFull = value;
                RaisePropertyChanged(() => ShotDOFFull);
            }
        }


        public int ShotStep
        {
            get { return _shotStep; }
            set
            {
                _shotStep = value;
                RaisePropertyChanged(() => ShotStep);
            }
        }

        public int ShotStepFull
        {
            get { return _shotStep2; }
            set
            {
                _shotStep2 = value;
                RaisePropertyChanged(() => ShotStepFull);
            }
        }


        public int Overlap
        {
            get { return _Overlap; }
            set
            {
                _Overlap = value;
                RaisePropertyChanged(() => Overlap);
            }
        }


        public int ShotsNumber
        {
            get { return _ShotsNumber; }
            set
            {
                _ShotsNumber = value;
                RaisePropertyChanged(() => ShotsNumber);
            }
        }

        public int ShotsNumberFull
        {
            get { return _ShotsNumberFull; }
            set
            {
                _ShotsNumberFull = value;
                RaisePropertyChanged(() => ShotsNumberFull);
            }
        }


        public int PlusNearShots
        {
            get { return _PlusNearShots; }
            set
            {
                _PlusNearShots = value;
                RaisePropertyChanged(() => PlusNearShots);
            }
        }

        public int PlusFarShots
        {
            get { return _PlusFarShots; }
            set
            {
                _PlusFarShots = value;
                RaisePropertyChanged(() => PlusFarShots);
            }
        }


        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
                RaisePropertyChanged(() => IsFree);
            }
        }

        public bool IsFree
        {
            get { return !IsBusy; }
        }


        public double Aperture
        {
            get { return _aperture; }
            set
            {
                _aperture = value;
                RaisePropertyChanged(() => Aperture);
            }
        }

        public double ApertureAF
        {
            get { return _apertureAF; }
            set
            {
                _apertureAF = value;
                RaisePropertyChanged(() => ApertureAF);
            }
        }

        public double NA
        {
            get { return _nA; }
            set
            {
                _nA = value;
                RaisePropertyChanged(() => NA);
            }
        }
      
        public double Pitch
        {
            get { return _pitch; }
            set
            {
                _pitch = value;
                RaisePropertyChanged(() => Pitch);
            }
        }

        public double E
        {
            get { return _e; }
            set
            {
                _e = value;
                RaisePropertyChanged(() => E);
            }
        }

        public int Lambda
        {
            get { return _lambda; }
            set
            {
                _lambda = value;
                RaisePropertyChanged(() => Lambda);
            }
        }

        public double N
        {
            get { return _n; }
            set
            {
                _n = value;
                RaisePropertyChanged(() => N);
            }
        }

        public int LinesNumber
        {
            get { return _LinesNumber; }
            set
            {
                _LinesNumber = value;
                RaisePropertyChanged(() => LinesNumber);
            }
        }

        public int LightValue
        {
            get { return _LightValue; }
            set
            {
                _LightValue = value;
                RaisePropertyChanged(() => LightValue);
            }
        }

        public int LightValue2
        {
            get { return _LightValue2; }
            set
            {
                _LightValue2 = value;
                RaisePropertyChanged(() => LightValue2);
            }
        }

        public int Degrees
        {
            get { return _degrees; }
            set
            {
                _degrees = value;
                RaisePropertyChanged(() => Degrees);
            }
        }


        /*public static Stopwatch stopwatch
        {
            get { return _stopwatch; }
            set
            {
                _stopwatch = value;
                RaisePropertyChanged(() => stopwatch);
            }
        }*/

        #endregion


        #region Name Strings

        private string _NameLens;
        private string _NameRail;
        private string _NameSensor;

        public string NameLens
        {
            get { return _NameLens; }
            set
            {
                _NameLens = value;
                RaisePropertyChanged(() => NameLens);
            }
        }

        public string NameRail
        {
            get { return _NameRail; }
            set
            {
                _NameRail = value;
                RaisePropertyChanged(() => NameRail);
            }
        }

        public string NameSensor
        {
            get { return _NameSensor; }
            set
            {
                _NameSensor = value;
                RaisePropertyChanged(() => NameSensor);
            }
        }

        #endregion

     
        #region Position

        public void UpDatePosition()
        {
            Position = Position + Step;          
            UpDateTotalDOF();
        }

        public void UpDateShotsNumber()
        {
            if (ShotStep != 0)
            {
                ShotsNumber = 1 + (TotalDOF / ShotStepFull);
                ShotsNumberFull = 1 + (TotalDOFFull / ShotStepFull);
            }
        }

        public void UpDateTotalDOF()
        {           
            TotalDOF = FarFocus - NearFocus;
            UpDateTotalDOFFull();
        }

        public void UpDateTotalDOFFull()
        {
            FarFocus2 = FarFocus + (PlusFarShots * ShotStepFull);
            NearFocus2 = NearFocus - (PlusNearShots * ShotStepFull);
            TotalDOFFull = FarFocus2 - NearFocus2;
            UpDateShotsNumber();
        }

        public void SetNearFocus()
        {
            if (IsFarFocusLocked == true)
            { FarFocus = FarFocus - Position; }
            NearFocus = 0;
            Position = 0;
            UpDateTotalDOF();
            IsNearFocusLocked = true;
        }

        public void SetFarFocus()
        {
            FarFocus = Position;
            UpDateTotalDOF();
            IsFarFocusLocked = true;
        }

        #endregion


        #region DoF Calc

        public void UpdateShotStep()
        {            
            if (ShotDOF != 0 && RailAccuracy != 0)
            {
                ShotDOFFull = ShotDOF - (ShotDOF * Overlap / 100);
                double mShotStep;
                mShotStep = ShotDOF / RailAccuracy;
                ShotStep = Convert.ToInt16(mShotStep);
                ShotStepFull = ShotStep - (ShotStep * Overlap / 100);
            }
        }

        #endregion

        #region AF Lens

        /*private bool _AFLensConnected = false;

        public bool AFLensConnected
        {
            get { return _AFLensConnected; }
            set
            {
                _AFLensConnected = value;
                RaisePropertyChanged(() => AFLensConnected);
                RaisePropertyChanged(() => NotAFLensConnected);
            }
        }

        public bool NotAFLensConnected
        {
            get { return !AFLensConnected; }
        }*/

        #endregion
    }
}
