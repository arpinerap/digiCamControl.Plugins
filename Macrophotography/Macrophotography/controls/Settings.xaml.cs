using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using Macrophotography.Classes;
using System.IO.Ports;
using System.Management;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Devices;

namespace Macrophotography.controls
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        int ManuLensInt;
        int MicroLensInt;


        public Settings()
        {
            InitializeComponent();
            Fill_ComboName();
            Fill_ComboNameRail();
            ArduinoPorts.Instance.DetectArduino();
            NameRail_Combo.SelectedIndex = 0;
            NameLens_Combo.SelectedIndex = 0;
            Fill_LensData();
            Fill_RailData();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LensParameters_grpbx.IsEnabled = false;
            //LensPresets_grpbx.IsEnabled = false;
        }


        # region RadioButtons
        public void ChkAFLens()
        {
            LensPresets_grpbx.IsEnabled = false;
            LensParameters_grpbx.IsEnabled = true;
            ShotDOF_nud.IsEnabled = false;
            ShotDOF_nud.BorderThickness = new Thickness(0);
            aperture_nud.BorderThickness = new Thickness(0);
            apertureAF_nud.BorderThickness = new Thickness(0);

            AFLensRdb.IsChecked = true;
            MicroLensRdb.IsChecked = false;
            ManuLensRdb.IsChecked = false;
            ManuPropRdb.IsChecked = false;

            aperture_nud.IsEnabled = false;
            apertureAF_nud.Visibility = System.Windows.Visibility.Visible;
            aperture_nud.Visibility = System.Windows.Visibility.Hidden;
            Aperture_label.Visibility = System.Windows.Visibility.Visible;
            NA_nud.Visibility = System.Windows.Visibility.Hidden;
            NA_label.Visibility = System.Windows.Visibility.Hidden;

            
        }
        public void ChkManualLens()
        {
            LensPresets_grpbx.IsEnabled = true;
            LensParameters_grpbx.IsEnabled = true;
            ShotDOF_nud.IsEnabled = false;
            ShotDOF_nud.BorderThickness = new Thickness(0);
            aperture_nud.BorderThickness = new Thickness(1);

            ManuLensInt = 1;
            MicroLensInt = 0;

            AFLensRdb.IsChecked = false;
            MicroLensRdb.IsChecked = false;
            ManuLensRdb.IsChecked = true;
            ManuPropRdb.IsChecked = false;

            aperture_nud.IsEnabled = true;
            aperture_nud.Visibility = System.Windows.Visibility.Visible;
            apertureAF_nud.Visibility = System.Windows.Visibility.Hidden;
            Aperture_label.Visibility = System.Windows.Visibility.Visible;
            NA_nud.IsEnabled = false;
            NA_nud.Visibility = System.Windows.Visibility.Hidden;
            NA_label.Visibility = System.Windows.Visibility.Hidden;
        }
        public void ChkMicroLens()
        {
            LensPresets_grpbx.IsEnabled = true;
            LensParameters_grpbx.IsEnabled = true;
            ShotDOF_nud.IsEnabled = false;
            ShotDOF_nud.BorderThickness = new Thickness(0);

            ManuLensInt = 0;
            MicroLensInt = 1;

            AFLensRdb.IsChecked = false;
            ManuLensRdb.IsChecked = false;
            MicroLensRdb.IsChecked = true;
            ManuPropRdb.IsChecked = false;

            aperture_nud.IsEnabled = false;
            aperture_nud.Visibility = System.Windows.Visibility.Hidden;
            apertureAF_nud.Visibility = System.Windows.Visibility.Hidden;
            Aperture_label.Visibility = System.Windows.Visibility.Hidden;
            NA_nud.IsEnabled = true;
            NA_nud.Visibility = System.Windows.Visibility.Visible;
            NA_label.Visibility = System.Windows.Visibility.Visible;
        }
        public void ChkManuProp()
        {
            LensPresets_grpbx.IsEnabled = false;
            LensParameters_grpbx.IsEnabled = true;
            ShotDOF_nud.IsEnabled = true;
            ShotDOF_nud.BorderThickness = new Thickness(1);
            
            AFLensRdb.IsChecked = false;
            MicroLensRdb.IsChecked = false;
            ManuLensRdb.IsChecked = false;
            ManuPropRdb.IsChecked = true;

            aperture_nud.IsEnabled = false;
            aperture_nud.Visibility = System.Windows.Visibility.Hidden;
            apertureAF_nud.Visibility = System.Windows.Visibility.Hidden;
            Aperture_label.Visibility = System.Windows.Visibility.Hidden;
            NA_nud.IsEnabled = false;
            NA_nud.Visibility = System.Windows.Visibility.Hidden;
            NA_label.Visibility = System.Windows.Visibility.Hidden;
        }

        private void AFLensRdb_Checked(object sender, RoutedEventArgs e)
        {
            try 
            {
                ChkAFLens();
                sDoFCalc();
            }
            catch (Exception)
            {
            }
            
        }
        public void ManuLensRdb_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ChkManualLens();
                sDoFCalc();
            }
            catch (Exception)
            {
            }
        }
        public void MicroLensRdb_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ChkMicroLens();
                sDoFCalc();
            }
            catch (Exception)
            {
            }
        }
        private void ManuPropRdb_Checked(object sender, RoutedEventArgs e)
        {
            try 
            { 
                ChkManuProp();
                sDoFCalc();
            }
            catch (Exception)
            {
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButtonClick();
        }
        public void RadioButtonClick()
        {
            if (AFLensRdb.IsChecked == true)
            {
                ChkAFLens();
                Clear_LensData();
                //aperture_nud.IsEnabled =    SelectedCameraDevice.FNumber.IsEnabled;
            }
            if (ManuLensRdb.IsChecked == true)
            {
                ChkManualLens();
                Clear_LensData();
                aperture_nud.Value = 0;
            }
            if (MicroLensRdb.IsChecked == true)
            {
                ChkMicroLens();
                Clear_LensData();
                NA_nud.Value = 0;
            }
            if (ManuPropRdb.IsChecked == true)
            {
                ChkManuProp();
                Clear_LensData();
                
            }
        }
        # endregion

        # region Lens DataBase
        private void savelens_button_Click(object sender, RoutedEventArgs e)
        {
            if (ManuLensRdb.IsChecked == true)
            {
                NA_nud.Value = 0;
            }
            else
            {
                aperture_nud.Value = 0;
            }

            SettingsDB.AddLens(Lens_txt.Text, (double)aperture_nud.Value, (double)NA_nud.Value, ManuLensInt, MicroLensInt, (double)NumUD_Magni.Value);
            MessageBox.Show("Lens Added");
            Lens_txt.Text = "";
            aperture_nud.Value = 0;
            NA_nud.Value = 0;
            ManuLensInt = 0;
            MicroLensInt = 0;
            StepperManager.Instance.Magni = 0;
            Fill_ComboName();
        }
        void Fill_ComboName()
        {
            string QueryName = "select * from LensTable";       
            try
            {
                SqlConnection conn = SettingsDB.GetConnection();
                conn.Open();
                SqlCommand NameComm = new SqlCommand(QueryName, conn);
                SqlDataReader dr = NameComm.ExecuteReader();
                NameLens_Combo.Items.Clear();
                while (dr.Read())
                {                   
                    string name = dr.GetString(1);
                    NameLens_Combo.Items.Add(name);
                    
                }
                conn.Close();
            }
            catch (SqlException ex) { throw ex; }
        }
        void Fill_LensData()
        {
            string QueryNameFill = "select * from LensTable where name_lens = '" + NameLens_Combo.Text + "'";

            try
            {
                SqlConnection conn = SettingsDB.GetConnection();
                conn.Open();
                SqlCommand NameFillComm = new SqlCommand(QueryNameFill, conn);
                SqlDataReader drfill = NameFillComm.ExecuteReader();
                while (drfill.Read())
                {
                    string sname = drfill.GetString(1);
                    double aperture = (double)drfill.GetDecimal(2);
                    double NA = (double)drfill.GetDecimal(3);
                    bool bManuLens = drfill.GetBoolean(4);
                    bool bMicroLens = drfill.GetBoolean(5);
                    double augmentation = (double)drfill.GetDecimal(6);

                    Lens_txt.Text = sname;
                    aperture_nud.Value = aperture;
                    NA_nud.Value = NA;
                    NumUD_Magni.Value = augmentation;


                    if (bManuLens == true)
                    { ChkManualLens(); }

                    if (bMicroLens == true)
                    { ChkMicroLens(); }

                    sDoFCalc();

                }
                conn.Close();
            }
            catch (SqlException ex) { throw ex; }
        }
        void Clear_LensData() 
        {
            NameLens_Combo.SelectedIndex = -1;
            Lens_txt.Text = "";
            ShotDOF_nud.Value = 0;
            ShotStep_nud.Value = 0;
        }
        private void NameLens_Combo_DropDownClosed(object sender, EventArgs e)
        {
            Fill_LensData();
        }
        private void Deletelens_button_Click(object sender, RoutedEventArgs e)
        {
            SettingsDB.DeleteLens(NameLens_Combo.SelectedItem.ToString());
            MessageBox.Show("Lens Deleted");
            Fill_ComboName();                      
        }
        #endregion

        # region Rail DataBase
        private void saverail_button_Click(object sender, RoutedEventArgs e)
        {
            SettingsDB.AddRail(Rail_txt.Text, MotorSteps_nud.Value.ToString(), MicroSteps_nud.Value.ToString(), BallScrew_nud.Value.ToString(), GearBox_nud.Value.ToString());
            MessageBox.Show("Rail Added");
            Rail_txt.Text = "";
            MotorSteps_nud.Value = 0;
            MicroSteps_nud.Value = 0;
            BallScrew_nud.Value = 0;
            GearBox_nud.Value = 0;
            Fill_ComboNameRail();
        }
        void Fill_ComboNameRail()
        {
            string QueryName = "select * from RailTable";

            try
            {
                SqlConnection conn = SettingsDB.GetConnection();
                conn.Open();
                SqlCommand NameComm = new SqlCommand(QueryName, conn);
                SqlDataReader dr = NameComm.ExecuteReader();
                NameRail_Combo.Items.Clear();
                while (dr.Read())
                {                   
                    string name = dr.GetString(1);
                    NameRail_Combo.Items.Add(name);
                }
                conn.Close();
            }
            catch (SqlException ex) { throw ex; }
        }
        void Fill_RailData()
        {
            string QueryNameFill = "select * from RailTable where name_rail = '" + NameRail_Combo.Text + "'";

            try
            {
                SqlConnection conn = SettingsDB.GetConnection();
                conn.Open();
                SqlCommand NameFillComm = new SqlCommand(QueryNameFill, conn);
                SqlDataReader drfill = NameFillComm.ExecuteReader();
                while (drfill.Read())
                {
                    string sname = drfill.GetString(1);
                    int motor_steps = drfill.GetInt16(2);
                    int micro_steps = drfill.GetInt16(3);
                    int ball_screw = drfill.GetInt16(4);
                    int gear_box = drfill.GetInt16(5);

                    MotorSteps_nud.Value = motor_steps;
                    MicroSteps_nud.Value = micro_steps;
                    BallScrew_nud.Value = ball_screw;
                    GearBox_nud.Value = gear_box;
                    Rail_txt.Text = sname;
                    NameRail_Combo.SelectedItem = sname;

                    RailCalc();
                    sDoFCalc();
                }
                conn.Close();
            }
            catch (SqlException ex) { throw ex; }
        }
        private void NameRail_Combo_DropDownClosed(object sender, EventArgs e)
        {
            Fill_RailData();
        }
        private void Deleterail_button_Click(object sender, RoutedEventArgs e)
        {
            SettingsDB.DeleteRail(NameRail_Combo.SelectedItem.ToString());
            Rail_txt.Text = "";
            MessageBox.Show("Rail Deleted");
            Fill_ComboNameRail();
        }

        public void RailCalc()
        {
            if (BallScrew_nud.Value != null && MotorSteps_nud.Value != null && GearBox_nud.Value != null && MicroSteps_nud.Value != null)
            {
                RailAccuracy_nud.Value = Convert.ToDouble(1 * (int)BallScrew_nud.Value) / ((int)MotorSteps_nud.Value * (int)GearBox_nud.Value * (int)MicroSteps_nud.Value);
                StepperManager.Instance.RailAccuracy = Convert.ToDouble(RailAccuracy_nud.Value);
            }
        }

        private void RailCalcUpdate(object sender, RoutedEventArgs e)
        {
            RailCalc();
        }

        private void RailCalc_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            NameRail_Combo.SelectedIndex = -1;
            Rail_txt.Text = "";
            RailCalc();
        }

        # endregion

        #region Sensor DataBase
        
        private void savesensor_button_Click(object sender, RoutedEventArgs e)
        {
            SettingsDB.AddSensor(Sensor_txt.Text, (double)Pitch_nud.Value, (double)E_nud.Value, (int)Lambda_nud.Value, (double)N_nud.Value);
            MessageBox.Show("Sensor Added");
            Sensor_txt.Text = "";
            Pitch_nud.Value = 0;
            E_nud.Value = 0;
            Lambda_nud.Value = 0;
            N_nud.Value = 0;
            Fill_ComboNameSensor();
            NameSensor_Update();
        }
        void Fill_ComboNameSensor()
        {
            string QueryName = "select * from SensorTable";

            try
            {
                SqlConnection conn = SettingsDB.GetConnection();
                conn.Open();
                SqlCommand NameComm = new SqlCommand(QueryName, conn);
                SqlDataReader dr = NameComm.ExecuteReader();
                NameSensor_Combo.Items.Clear();
                while (dr.Read())
                {
                    string name = dr.GetString(1);
                    NameSensor_Combo.Items.Add(name);
                }
                conn.Close();
            }
            catch (SqlException ex) { throw ex; }
        }
        private void NameSensor_Update()
        {
            string QueryNameFill = "select * from SensorTable where name_sensor = '" + NameSensor_Combo.Text + "'";

            try
            {
                SqlConnection conn = SettingsDB.GetConnection();
                conn.Open();
                SqlCommand NameFillComm = new SqlCommand(QueryNameFill, conn);
                SqlDataReader drfill = NameFillComm.ExecuteReader();
                while (drfill.Read())
                {
                    string sname = drfill.GetString(1);
                    double pitch = (double)drfill.GetDecimal(2);
                    double ee = (double)drfill.GetDecimal(3);
                    int lambda = drfill.GetInt16(4);
                    double n = (double)drfill.GetDecimal(5);

                    Pitch_nud.Value = pitch;
                    E_nud.Value = ee;
                    Lambda_nud.Value = lambda;
                    N_nud.Value = n;
                    Sensor_txt.Text = sname;
                    NameSensor_Combo.SelectedItem = sname;

                    RailCalc();
                    sDoFCalc();
                }
                conn.Close();
            }
            catch (SqlException ex) { throw ex; }
        }
        private void NameSensor_Combo_DropDownClosed(object sender, EventArgs e)
        {
            NameSensor_Update();
        }
        private void Deletesensor_button_Click(object sender, RoutedEventArgs e)
        {
            SettingsDB.DeleteSensor(NameSensor_Combo.SelectedItem.ToString());
            Rail_txt.Text = "";
            MessageBox.Show("Sensor Deleted");
            Fill_ComboNameSensor();
        }

        #endregion

        # region DOF Calcs

        public void sDoFCalcAperture()
        {
            if (aperture_nud.Value != null && E_nud.Value != null && Pitch_nud.Value != null && Sld_Magni.Value != 0)
            {
                double mShotDOF;
                mShotDOF = Convert.ToDouble(2 * E_nud.Value * Pitch_nud.Value * aperture_nud.Value * (Sld_Magni.Value + 1) / (Sld_Magni.Value * Sld_Magni.Value));
                ShotDOF_nud.Value = mShotDOF * 0.001;
            }
            else
            {
                ShotDOF_nud.Value = 0;
            }
        }

        public void sDoFCalcApertureAF()
        {
            if (apertureAF_nud.Value != null && E_nud.Value != null && Pitch_nud.Value != null && Sld_Magni.Value != 0)
            {
                double mShotDOF;
                mShotDOF = Convert.ToDouble(2 * E_nud.Value * Pitch_nud.Value * apertureAF_nud.Value * (Sld_Magni.Value + 1) / (Sld_Magni.Value * Sld_Magni.Value));
                ShotDOF_nud.Value = mShotDOF * 0.001;
            }
            else
            {
                ShotDOF_nud.Value = 0;
            }
        }

        public void sDoFCalcNA()
        {
            if (NA_nud.Value != null && Lambda_nud.Value != null && E_nud.Value != null && Pitch_nud.Value != null && Sld_Magni.Value != 0)
            {
                double mShotDOF;
                mShotDOF = Convert.ToDouble(10000000/1*((Lambda_nud.Value* N_nud.Value*0.0000000001/(NA_nud.Value * NA_nud.Value)+(E_nud.Value*Pitch_nud.Value*0.0000001/(NA_nud.Value * Sld_Magni.Value)))));
                ShotDOF_nud.Value = mShotDOF * 0.001;
            }
            else
            {
                ShotDOF_nud.Value = 0;
            }
        }

        public void sDoFCalc()
        {
            if (AFLensRdb.IsChecked == true)
            { sDoFCalcApertureAF(); }
            if (ManuLensRdb.IsChecked == true)
            { sDoFCalcAperture(); }
            if (MicroLensRdb.IsChecked == true)
            { sDoFCalcNA(); }
            if (ManuPropRdb.IsChecked == true)
            { StepperManager.Instance.ShotStep = Convert.ToInt16(ShotStep_nud.Value); }

            StepperManager.Instance.UpdateShotStep();
            
        }

#endregion

        private void sDoFCalcUpdate(object sender, RoutedEventArgs e)
        {
            sDoFCalc();
        }

        private void sDoFCalcUpdate(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            sDoFCalc();
        }

        private void ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            sDoFCalc();
        }

        private void SearchPort_btn_Click(object sender, RoutedEventArgs e)
        {
            ArduinoPorts.Instance.DetectArduino();
        }

     




    }
}
