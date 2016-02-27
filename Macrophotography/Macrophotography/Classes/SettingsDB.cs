using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using CameraControl.Core.Classes;

namespace Macrophotography.Classes
{
    public static class SettingsDB
    {
        public static SqlConnection GetConnection()
        {
            string SettingsCS =string.Format(@"Data Source=(LocalDB)\v11.0;AttachDbFilename={0}\plugins\Macrophotography\DataBase\Settings.mdf",Settings.ApplicationFolder);
            SqlConnection conn = new SqlConnection(SettingsCS);
            return conn;
        }
        public static void AddLens(string NameLens, double Aperture, double NA, int iManualLens, int iMicrolens)
        {
            string QueryIns = "insert into LensTable (name_lens, aperture, NA, manual_lens, microscopy_lens) values (@nameLens, @aperture, @na, @manualLens, @microLens)";
            SqlConnection conn = GetConnection();
            SqlCommand InsComm = new SqlCommand(QueryIns, conn);
            InsComm.Parameters.AddWithValue("@nameLens", NameLens);
            InsComm.Parameters.AddWithValue("@aperture", Aperture);
            InsComm.Parameters.AddWithValue("@na", NA);
            InsComm.Parameters.AddWithValue("@manualLens", iManualLens);
            InsComm.Parameters.AddWithValue("@microLens", iMicrolens);
            try { conn.Open(); InsComm.ExecuteNonQuery(); }
            catch (SqlException ex) { throw ex; }
            finally { conn.Close(); }
        }
        public static List<Lens> GetLens()
        {
            List<Lens> LensList = new List<Lens>();
            SqlConnection conn = GetConnection();
            string QuerySel = "Select * from LensTable Order by Name_Lens";
            SqlCommand SelComm = new SqlCommand(QuerySel, conn);
            try
            {
                conn.Open();
                SqlDataReader reader = SelComm.ExecuteReader();
                while (reader.Read())
                {
                    Lens lens = new Lens();
                    lens.IdLens = (int)reader["IdLens"];
                    lens.NameLens = reader["NameLens"].ToString();
                    lens.Aperture = (double)reader["Aperture"];
                    lens.NA = (double)reader["NA"];
                    lens.ManualLens = reader["sManualLens"].ToString();
                    lens.MicroLens = reader["sMicroLens"].ToString();
                    LensList.Add(lens);
                }
                reader.Close();
            }
            catch (SqlException ex) { throw ex; }
            finally { conn.Close(); }
            return LensList;
        }
        public static void DeleteLens(string nameLens)
        {
            string QueryDel = "delete from LensTable where name_lens = @nameLens";
            SqlConnection conn = GetConnection();
            SqlCommand DelComm = new SqlCommand(QueryDel, conn);
            DelComm.Parameters.AddWithValue("@nameLens", nameLens);
            try { conn.Open(); DelComm.ExecuteNonQuery(); }
            catch (SqlException ex) { throw ex; }
            finally { conn.Close(); }
        }

        public static void AddRail(string NameRail, string Motor_steps, string Micro_steps, string Ball_screw, string Gear_box)
        {
            string QueryIns = "insert into RailTable (name_rail, motor_steps, micro_steps, ball_screw, gear_box) values (@nameRail, @motor_steps, @micro_steps, @ball_screw, @gear_box)";
            SqlConnection conn = GetConnection();
            SqlCommand InsComm = new SqlCommand(QueryIns, conn);
            InsComm.Parameters.AddWithValue("@nameRail", NameRail);
            InsComm.Parameters.AddWithValue("@motor_steps", Motor_steps);
            InsComm.Parameters.AddWithValue("@micro_steps", Micro_steps);
            InsComm.Parameters.AddWithValue("@ball_screw", Ball_screw);
            InsComm.Parameters.AddWithValue("@gear_box", Gear_box);
            try { conn.Open(); InsComm.ExecuteNonQuery(); }
            catch (SqlException ex) { throw ex; }
            finally { conn.Close(); }
        }
        public static List<Rail> GetRail()
        {
            List<Rail> RailList = new List<Rail>();
            SqlConnection conn = GetConnection();
            string QuerySel = "Select * from RailTable Order by Name_Rail";
            SqlCommand SelComm = new SqlCommand(QuerySel, conn);
            try
            {
                conn.Open();
                SqlDataReader reader = SelComm.ExecuteReader();
                while (reader.Read())
                {
                    Rail rail = new Rail();
                    rail.IdRail = (int)reader["IdRail"];
                    rail.NameRail = reader["NameRail"].ToString();
                    rail.Motor_steps = reader["Motor_steps"].ToString();
                    rail.Micro_steps = reader["Micro_steps"].ToString();
                    rail.Ball_screw = reader["Ball_gear"].ToString();
                    rail.Gear_box = reader["Gear_box"].ToString();
                    RailList.Add(rail);
                }
                reader.Close();
            }
            catch (SqlException ex) { throw ex; }
            finally { conn.Close(); }
            return RailList;
        }

        public static void DeleteRail(string nameRail)
        {
            string QueryDel = "delete from RailTable where name_rail = @nameRail";
            SqlConnection conn = GetConnection();
            SqlCommand DelComm = new SqlCommand(QueryDel, conn);
            DelComm.Parameters.AddWithValue("@nameRail", nameRail);
            try { conn.Open(); DelComm.ExecuteNonQuery(); }
            catch (SqlException ex) { throw ex; }
            finally { conn.Close(); }
        }

    }
}
