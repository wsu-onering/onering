using System;
using System.Diagnostics;
using System.Data.SqlClient;

namespace onering.Database{
    public class Database {
        /// <summary>
        /// EnsureTablesCreated statically attempts to ensure that all the necessary database tables exist within the Database. If the tables do not exist, then the necessary SQL commands are run to create those tables. If the tables do exist, then EnsureTablesCreated does nothing.
        /// </summary>
        /// <param name="connectionString">The full connection string used to connect to the database. No attempts are made to derive the connection string from implicit sources (e.g. environment variables), meaning this parameter is required. </param>
        public static void EnsureTablesCreated(string connectionString) {
            // SqlConnection conn = new SqlConnection(connectionString);
            using (SqlConnection conn = new SqlConnection(connectionString)) {
                conn.Open();

                Debug.WriteLine("Creating table for Portlet if it doesn't exist already.");
                SqlCommand createPortlet = new SqlCommand(@"
                CREATE TABLE IF NOT EXISTS Portlet(
                    PortletID INTEGER NOT NULL AUTO_INCREMENT,
                    PortletName NVARCHAR(4096),
                    PortletDescription NVARCHAR(8192),
                    CONSTRAINT PortletPK PRIMARY KEY (PortletID)
                );");
                createPortlet.ExecuteNonQuery();

                Debug.WriteLine("Creating table for User if it doesn't exist already.");
                SqlCommand createUser = new SqlCommand(@"
                CREATE TABLE IF NOT EXISTS User(
                    UserID INTEGER NOT NULL AUTO_INCREMENT,
                    GraphID NVARCHAR(4096),
                    CONSTRAINT UserPK PRIMARY KEY (UserID)
                );");
                createPortlet.ExecuteNonQuery();

                Debug.WriteLine("Creating table for ConfigField if it doesn't exist already.");
                SqlCommand createConfigField = new SqlCommand(@"
                CREATE TABLE IF NOT EXISTS ConfigField(
                    ConfigFieldID INTEGER NOT NULL AUTO_INCREMENT,
                    ConfigFieldName NVARCHAR(4096),
                    ConfigFieldDescription NVARCHAR(8192),
                    PortletID INTEGER NOT NULL,
                    CONSTRAINT ConfigFieldPortletID FOREIGN KEY (PortletID) REFERENCES Portlet (PortletID),
                    CONSTRAINT ConfigFieldPK PRIMARY KEY (ConfigFieldID)
                );");
                createConfigField.ExecuteNonQuery();

                Debug.WriteLine("Creating table for ConfigFieldOption if it doesn't exist already.");
                SqlCommand createConfigFieldOption = new SqlCommand(@"
                CREATE TABLE IF NOT EXISTS ConfigFieldOption(
                    ConfigFieldOptionID INTEGER NOT NULL AUTO_INCREMENT,
                    ConfigFieldOptionValue NVARCHAR(8192),
                    ConfigFieldID INTEGER NOT NULL,
                    CONSTRAINT ConfigFieldOptionConfigFieldID FOREIGN KEY (ConfigFieldID) REFERENCES ConfigField (ConfigFieldID),
                    CONSTRAINT ConfigFieldOptionPK PRIMARY KEY (ConfigFieldOptionID)
                );");
                createConfigFieldOption.ExecuteNonQuery();

                Debug.WriteLine("Creating table for ConfigFieldInstance if it doesn't exist already.");
                SqlCommand createConfigFieldInstance = new SqlCommand(@"
                CREATE TABLE IF NOT EXISTS ConfigFieldInstance(
                    ConfigFieldInstanceID INTEGER NOT NULL AUTO_INCREMENT,
                    ConfigFieldOptionValue NVARCHAR(8192),
                    ConfigFieldID INTEGER,
                    ConfigFieldOptionID INTEGER,
                    CONSTRAINT ConfigFieldInstanceConfigFieldID FOREIGN KEY (ConfigFieldID) REFERENCES ConfigField (ConfigFieldID),
                    CONSTRAINT ConfigFieldInstanceConfigFieldOptionID FOREIGN KEY (ConfigFieldOptionID) REFERENCES ConfigFieldOption (ConfigFieldOptionID),
                    CONSTRAINT ConfigFieldInstancePK PRIMARY KEY (ConfigFieldInstanceID)
                );");
                createConfigFieldInstance.ExecuteNonQuery();

                Debug.WriteLine("Creating table for PortletInstance if it doesn't exist already.");
                SqlCommand createPortletInstance = new SqlCommand(@"
                CREATE TABLE IF NOT EXISTS PortletInstance(
                    PortletInstanceID INTEGER NOT NULL AUTO_INCREMENT,
                    PortletID INTEGER NOT NULL,
                    UserID INTEGER NOT NULL,

                    Height INTEGER NOT NULL,
                    Width INTEGER NOT NULL,
                    XPos INTEGER NOT NULL,
                    YPos INTEGER NOT NULL,

                    CONSTRAINT PortletInstancePortletID FOREIGN KEY (PortletID) REFERENCES Portlet (PortletID),
                    CONSTRAINT PortletInstanceUserID FOREIGN KEY (UserID) REFERENCES User (UserID),
                    CONSTRAINT PortletInstancePK PRIMARY KEY (PortletInstance)
                );");
                createPortletInstance.ExecuteNonQuery();

                conn.Close();
            }
        }
    }
}