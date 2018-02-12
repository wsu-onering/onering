using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.SqlClient;

using Microsoft.Extensions.Configuration;

using onering.Models;

namespace onering.Database{
    public class Database : IOneRingDB {

        /// <summary>
        /// _connectionString is the connection string to the database. This is used implicitly by
        /// all queries to the database, except for the EnsureTablesCreated method, which requires
        /// the connectionstring be passed in when calling it. The goal of this is two fold:
        ///     1. Prevent over use of the EnsureTableCreated method
        ///     2. Keep all database connection management internal to this class
        /// </summary>
        private string _connectionString;
        private SqlConnection _conn;
        private IConfiguration _configuration;
        private void Open() {
            if (_conn.State != System.Data.ConnectionState.Open) {
                _conn.Open();
            }
        }
        private void Close() {
            if (_conn.State != System.Data.ConnectionState.Closed) {
                _conn.Close();
            }
        }
        public Database(IConfiguration configuration){
            _configuration = configuration;
            _connectionString = _configuration.GetSection("Databases")["OneRing"];
            _conn = new SqlConnection(_connectionString);
        }

        /// <summary>
        /// CreatePortlet creates a new portlet in the database with the same data as the provided portlet.
        /// </summary>
        /// <param name="portlet">The portlet inserted into the database will have all the values of the input object, except for the ID field. The ID field on the input portlet is ignored.</param>
        public void CreatePortlet(Portlet portlet) {
            string query = @"INSERT INTO Portlet (
                PortletName,
                PortletDescription,
                PortletPath,
                PortletIcon
            ) OUTPUT INSERTED.PortletID VALUES (
                @name,
                @description,
                @path,
                @icon
            )";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@name", portlet.Name);
                    cmd.Parameters.AddWithValue("@description", portlet.Description);
                    cmd.Parameters.AddWithValue("@path", portlet.Path);
                    cmd.Parameters.AddWithValue("@icon", portlet.Icon);
                    conn.Open();
                    int portletId = (int)cmd.ExecuteScalar();
                    if (portlet.ConfigFields != null) {
                        foreach (ConfigField field in portlet.ConfigFields) {
                            CreateConfigField(field, portletId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ListPortlet returns a list of all portlets.
        /// </summary>
        /// <returns>A list of all portlets. This list is not garuanteed to be sorted in any way.</returns>
        public List<Portlet> ListPortlets() {
            List<Portlet> portlets = new List<Portlet>();

            string query = @"SELECT * FROM Portlet";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader()) {
                        while (r.Read()) {
                            Portlet p = new Portlet();
                            p.ID = r.GetInt32(0);
                            p.Name = r.GetString(1);
                            p.Description = r.GetString(2);
                            p.Path = r.GetString(3);
                            p.Icon = r.GetString(4);
                            p.ConfigFields = ListConfigFields(p.ID);
                            portlets.Add(p);
                        }
                    }
                }
            }
            return portlets;
        }

        /// <summary>
        /// Returns a list of all portlets with names matching the provided prefix.
        /// </summary>
        /// <param name="namePrefix">The prefix to be searched for in the names of portlets.</param>
        /// <returns></returns>
        public List<Portlet> ListPortlets(string namePrefix) {
            List<Portlet> portlets = new List<Portlet>();

            string query = @"SELECT * FROM Portlet WHERE PortletName LIKE '%' + @namePrefix + '%'";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@namePrefix", namePrefix);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader()) {
                        while (r.Read()) {
                            Portlet p = new Portlet();
                            p.ID = r.GetInt32(0);
                            p.Name = r.GetString(1);
                            p.Description = r.GetString(2);
                            p.Path = r.GetString(3);
                            p.Icon = r.GetString(4);
                            p.ConfigFields = ListConfigFields(p.ID);
                            portlets.Add(p);
                        }
                    }
                }
            }

            return portlets;
        }

        /// <summary>
        /// Creates a ConfigField associated with the given portlet.
        /// </summary>
        /// <param name="cf">The contents of the configfield. The ID field and the PortletID field of this object are ignored.</param>
        /// <param name="portletId">The ID of the portlet to associate this ConfigField with.</param>
        public void CreateConfigField(ConfigField cf, int portletId) {
            string query = @"INSERT INTO ConfigField (
                ConfigFieldName,
                ConfigFieldDescription,
                PortletID
            ) output INSERTED.ConfigFieldID VALUES (
                @name,
                @description,
                @portletid
            )";
            // Because this query may be run while another query is being run using the class-wide
            // _conn, we create out own connection here.
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@name", cf.Name);
                    cmd.Parameters.AddWithValue("@description", cf.Description);
                    cmd.Parameters.AddWithValue("@portletid", portletId);
                    conn.Open();
                    int configFieldId = (int) cmd.ExecuteScalar();
                    if (cf.ConfigFieldOptions != null) {
                        foreach (ConfigFieldOption option in cf.ConfigFieldOptions) {
                            CreateConfigFieldOption(option, configFieldId);
                        }
                    }
                }
            }
        }
        // Retrieve all the configfield(s) assocaited with a given portlet.
        public List<ConfigField> ListConfigFields(int portletId) {
            List<ConfigField> fields = new List<ConfigField>();
            string query = @"SELECT * FROM ConfigField WHERE PortletID=@portletid";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@portletid", portletId);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader()) {
                        while (r.Read()) {
                            ConfigField cf = new ConfigField();
                            cf.ID = r.GetInt32(0);
                            cf.Name = r.GetString(1);
                            cf.Description = r.GetString(2);
                            cf.ConfigFieldOptions = ListConfigFieldOptions(cf.ID);
                            fields.Add(cf);
                        }
                    }
                }
            }
            return fields;
        }

        /// <summary>
        /// Creates a ConfigFieldOption with the provided value.
        /// </summary>
        /// <param name="option">The ConfigFieldOption object to create in the database. The ConfigFieldID field is ignored. Pass that as the other argument to this method.</param>
        /// <param name="configFieldId">The ID of the ConfigField to associate this ConfigFieldOption with.</param>
        public void CreateConfigFieldOption(ConfigFieldOption option, int configFieldId) {
            string query = @"INSERT INTO ConfigFieldOption (
                ConfigFieldOptionValue,
                ConfigFieldID
            ) VALUES (
                @optionvalue,
                @configfieldid
            )";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("optionvalue", option.Value);
                    cmd.Parameters.AddWithValue("configfieldid", configFieldId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<ConfigFieldOption> ListConfigFieldOptions(int configFieldId) {
            List<ConfigFieldOption> options = new List<ConfigFieldOption>();
            string query = @"SELECT * FROM ConfigFieldOption WHERE ConfigFieldID=@configfieldid";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@configfieldid", configFieldId);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader()) {
                        while (r.Read()) {
                            ConfigFieldOption co = new ConfigFieldOption();
                            co.ID = r.GetInt32(0);
                            co.Value = r.GetString(1);
                            options.Add(co);
                        }
                    }
                }
            }
            return options;
        }

        /// <summary>
        /// EnsureTablesCreated statically attempts to ensure that all the necessary database tables exist within the Database. If the tables do not exist, then the necessary SQL commands are run to create those tables. If the tables do exist, then EnsureTablesCreated does nothing.
        /// </summary>
        /// <param name="connectionString">The full connection string used to connect to the database. No attempts are made to derive the connection string from implicit sources (e.g. environment variables), meaning this parameter is required. </param>
        public static void EnsureTablesCreated(string connectionString) {
            using (SqlConnection conn = new SqlConnection(connectionString)) {
                conn.Open();

                Debug.WriteLine("Creating table for Portlet if it doesn't exist already.");
                SqlCommand createPortlet = new SqlCommand(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE NAME='Portlet')
                BEGIN
                    CREATE TABLE Portlet(
                        PortletID INTEGER NOT NULL IDENTITY (1,1),
                        PortletName NVARCHAR(3000) NOT NULL,
                        PortletDescription NVARCHAR(3000) NOT NULL,
                        PortletPath NVARCHAR(3000) NOT NULL,
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
    public interface IOneRingDB {
        void CreatePortlet(Portlet portlet);
        List<Portlet> ListPortlets();
        List<Portlet> ListPortlets(string namePrefix);
        void CreateConfigField(ConfigField cf, int portletId);
        List<ConfigField> ListConfigFields(int portletId);
        void CreateConfigFieldOption(ConfigFieldOption option, int configFieldId);
        List<ConfigFieldOption> ListConfigFieldOptions(int configFieldId);
    }
}