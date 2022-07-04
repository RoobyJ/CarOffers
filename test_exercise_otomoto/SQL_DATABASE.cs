using MySql.Data.MySqlClient;
using System;

namespace test_exercise_otomoto
{
    public class SQL_DATABASE
    {
        public string query;

        private SQL_DATABASE()
        {
        }

        public string Server { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        private MySqlConnection Connection { get; set; }

        private static SQL_DATABASE? _instance = null;
        public static SQL_DATABASE Instance()
        {
            if (_instance == null)
                _instance = new SQL_DATABASE();
            return _instance;
        }

        //Conecting to the Mysql Server
        public bool IsConnect()
        {
            if (Connection == null)
            {
                if (String.IsNullOrEmpty(DatabaseName))
                    return false;
                string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}", Server, DatabaseName, UserName, Password);
                Connection = new MySqlConnection(connstring);
                Connection.Open();
            }
            return true;
        }

        //Creating a table named by brand name or model of the car
        public bool CreateTable(string brand_model, string id, string brand, string model, string prodyear, string mileage, string fuel, string power)
        {
            query = String.Format("CREATE TABLE if not exists `{0}`({1} TEXT,{2} TEXT, {3} TEXT, {4} TEXT, {5} TEXT, {6} TEXT, {7} TEXT);", brand_model, id, brand, model, prodyear, mileage, fuel, power);
            var cmd = new MySqlCommand(query, Connection);
            var reader = cmd.ExecuteReader();
            reader.Close();
            return true;
        }
        public bool SelectData()
        {
            // to build in future
            return true;
        }

        // Inserting data to table with matching brand name or model
        public bool PostData(string brand_model, string id, string brand, string model, string prodyear, string fuel, string power, string mileage = "???")
        {
            query = String.Format("INSERT INTO `{0}` (offer_id, marka_pojazdu, model_pojazdu, rok_produkcji, przebieg, rodzaj_paliwa, moc) ", brand_model) +
                String.Format("VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}'); ", id, brand, model, prodyear, mileage, fuel, power);
            var cmd = new MySqlCommand(query, Connection);
            var reader = cmd.ExecuteReader();
            reader.Close();
            return true;
        }


        // Close the connection to the server
        public void Close()
        {
            Connection.Close();
        }
    }
}
