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
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE NAME='Portlet')
                BEGIN
                    CREATE TABLE Portlet(
                        PortletID INTEGER NOT NULL IDENTITY (1,1),
                        PortletName NVARCHAR(3000) NOT NULL,
                        PortletPath NVARCHAR(3000) NOT NULL,
                        PortletDescription NVARCHAR(3000) NOT NULL,
                        PortletIcon NVARCHAR(3000) NOT NULL,
                        CONSTRAINT PortletPK PRIMARY KEY (PortletID)
                    );
                END", conn);
                createPortlet.ExecuteNonQuery();

                Debug.WriteLine("Creating table for User if it doesn't exist already.");
                SqlCommand createUser = new SqlCommand(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE NAME='OneRingUser')
                BEGIN
                    CREATE TABLE OneRingUser(
                        UserID INTEGER NOT NULL IDENTITY (1,1),
                        GraphID NVARCHAR(3000),
                        CONSTRAINT UserPK PRIMARY KEY (UserID)
                    );
                END", conn);
                createUser.ExecuteNonQuery();

                // ConfigField represents the name and description of a field that a portlet has. By
                // way of analogy, let's suppose that an instance of a portlet needs a dictionary of
                // keys to values as it's configuration. A ConfigField represents the keys that must
                // exist in that dictionary. The values would be ConfigFieldInstance, while
                // ConfigFieldOption would act as a way of specifying a certain subset of options
                // that a user may select.
                Debug.WriteLine("Creating table for ConfigField if it doesn't exist already.");
                SqlCommand createConfigField = new SqlCommand(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE NAME='ConfigField')
                BEGIN
                    CREATE TABLE ConfigField(
                        ConfigFieldID INTEGER NOT NULL IDENTITY(1,1),
                        ConfigFieldName NVARCHAR(3000),
                        ConfigFieldDescription NVARCHAR(3000),
                        PortletID INTEGER NOT NULL,
                        CONSTRAINT ConfigFieldPortletID FOREIGN KEY (PortletID) REFERENCES Portlet (PortletID),
                        CONSTRAINT ConfigFieldPK PRIMARY KEY (ConfigFieldID)
                    );
                END", conn);
                createConfigField.ExecuteNonQuery();

                // ConfigFieldOption represents one or more choices that a user may make for a given
                // configuration field. The use case for this is that a portlet requries a
                // configuration field such as "data source", but the administrator wants the user
                // to only be able to select a few available values. Each of those few values will
                // be represented by a ConfigFieldOption, and the users choice will be represented
                // by the ConfigFieldOptionID column of the ConfigFieldInstance row.
                Debug.WriteLine("Creating table for ConfigFieldOption if it doesn't exist already.");
                SqlCommand createConfigFieldOption = new SqlCommand(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE NAME='ConfigFieldOption')
                BEGIN
                    CREATE TABLE ConfigFieldOption(
                        ConfigFieldOptionID INTEGER NOT NULL IDENTITY(1,1),
                        ConfigFieldOptionValue NVARCHAR(3000),
                        ConfigFieldID INTEGER NOT NULL,
                        CONSTRAINT ConfigFieldOptionConfigFieldID FOREIGN KEY (ConfigFieldID) REFERENCES ConfigField (ConfigFieldID),
                        CONSTRAINT ConfigFieldOptionPK PRIMARY KEY (ConfigFieldOptionID)
                    );
                END", conn);
                createConfigFieldOption.ExecuteNonQuery();

                // Notes for ConfigFieldInstance:
                // ConfigFieldInstance contains the user-provided value for a configurable field of
                // a portlet. ConfigFieldInstance may reference either a ConfigField or a
                // ConfigFieldOption. If it references a ConfigField, then ConfigFieldId and
                // ConfigFieldInstanceValue will have non-null values. If it references a
                // ConfigFieldOption, then ConfigFieldOptionID and ConfigFieldOptionValue will have
                // non-null values.
                Debug.WriteLine("Creating table for ConfigFieldInstance if it doesn't exist already.");
                SqlCommand createConfigFieldInstance = new SqlCommand(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE NAME='ConfigFieldInstance')
                BEGIN
                    CREATE TABLE ConfigFieldInstance(
                        ConfigFieldInstanceID INTEGER NOT NULL IDENTITY(1,1),

                        ConfigFieldID INTEGER,
                        ConfigFieldInstanceValue NVARCHAR(3000),

                        ConfigFieldOptionValue NVARCHAR(3000),
                        ConfigFieldOptionID INTEGER,

                        CONSTRAINT ConfigFieldInstanceConfigFieldID FOREIGN KEY (ConfigFieldID) REFERENCES ConfigField (ConfigFieldID),
                        CONSTRAINT ConfigFieldInstanceConfigFieldOptionID FOREIGN KEY (ConfigFieldOptionID) REFERENCES ConfigFieldOption (ConfigFieldOptionID),
                        CONSTRAINT ConfigFieldInstancePK PRIMARY KEY (ConfigFieldInstanceID)
                    );
                END", conn);
                createConfigFieldInstance.ExecuteNonQuery();

                Debug.WriteLine("Creating table for PortletInstance if it doesn't exist already.");
                SqlCommand createPortletInstance = new SqlCommand(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE NAME='PortletInstance')
                BEGIN
                    CREATE TABLE PortletInstance(
                        PortletInstanceID INTEGER NOT NULL IDENTITY(1,1),
                        PortletID INTEGER NOT NULL,
                        UserID INTEGER NOT NULL,

                        Height INTEGER NOT NULL,
                        Width INTEGER NOT NULL,
                        XPos INTEGER NOT NULL,
                        YPos INTEGER NOT NULL,

                        CONSTRAINT PortletInstancePortletID FOREIGN KEY (PortletID) REFERENCES Portlet (PortletID),
                        CONSTRAINT PortletInstanceUserID FOREIGN KEY (UserID) REFERENCES OneRingUser (UserID),
                        CONSTRAINT PortletInstancePK PRIMARY KEY (PortletInstanceID)
                    );
                END", conn);
                createPortletInstance.ExecuteNonQuery();

                conn.Close();
            }
        }
    }
}