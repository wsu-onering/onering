using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.SqlClient;

using Microsoft.Extensions.Configuration;

using onering.Models;

namespace onering.Database
{
    public interface IOneRingDB
    {
        void CreatePortlet(Portlet portlet);
        List<Portlet> ListPortlets();
        List<Portlet> ListPortlets(string namePrefix);
        void CreateConfigField(ConfigField cf, int portletId);
        List<ConfigField> ListConfigFields(int portletId);
        void CreateConfigFieldOption(ConfigFieldOption option, int configFieldId);
        List<ConfigFieldOption> ListConfigFieldOptions(int configFieldId);
    }


    public class Database : IOneRingDB
    {
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

        public Database(IConfiguration configuration){
            _configuration = configuration;
            _connectionString = _configuration.GetSection("Databases")["OneRing"];
            _conn = new SqlConnection(_connectionString);
        }

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
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            Portlet portal = Portlet.ReadFromDb(reader);
                            portal.ConfigFields = ListConfigFields(portal.ID);
                            portlets.Add(portal);
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
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            Portlet portal = Portlet.ReadFromDb(reader);
                            portal.ConfigFields = ListConfigFields(portal.ID);
                            portlets.Add(portal);
                        }
                    }
                }
            }

            return portlets;
        }

        /// <summary>
        /// Returns a list of all portlets with ID matching the provided ID.
        /// </summary>
        /// <param name="id">The id of the portlet to return.</param>
        /// <returns></returns>
        public List<Portlet> ListPortlets(int id) {
            List<Portlet> portlets = new List<Portlet>();

            string query = @"SELECT * FROM Portlet WHERE PortletID=@portletid";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@portletid", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            Portlet portal = Portlet.ReadFromDb(reader);
                            portal.ConfigFields = ListConfigFields(portal.ID);
                            portlets.Add(portal);
                        }
                    }
                }
            }

            return portlets;
        }

        public void CreateOneRingUser(OneRingUser user) {
            string query = @"INSERT INTO OneRingUser (
                GraphID
            ) output INSERTED.UserID VALUES (
                @graphid
            )";

            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@graphid", user.GraphID);
                    conn.Open();
                    int userID = (int)cmd.ExecuteScalar();
                    if (user.PortletInstances != null) {
                        foreach (PortletInstance pi in user.PortletInstances) {
                            pi.User = new OneRingUser {ID = userID};
                            pi.Portlet = ListPortlets(pi.Portlet.Name)[0];
                            CreatePortletInstance(pi);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of all OneRingUsers with the given GraphID. This *should* be a list of
        /// length either 0 or 1, but there are no garuantees; it is possible for this list to be
        /// longer than 1.
        /// </summary>
        /// <param name="graphid">The GraphID to search by.</param>
        /// <returns></returns>
        public List<OneRingUser> ListOneRingUsers(string graphid) {
            List<OneRingUser> users = new List<OneRingUser>();

            string query = @"SELECT * FROM OneRingUser WHERE GraphID=@graphid";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@graphid", graphid);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            OneRingUser u = new OneRingUser();
                            u.ID = reader.GetInt32(0);
                            u.GraphID = reader.GetString(1);
                            u.PortletInstances = ListPortletInstances(u);
                            users.Add(u);
                        }
                    }
                }
            }

            return users;
        }

        public List<OneRingUser> ListBareOneRingUsers(OneRingUser user) {
            List<OneRingUser> users = new List<OneRingUser>();

            string query = @"SELECT * FROM OneRingUser WHERE UserID=@userid";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@UserID", user.ID);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            OneRingUser u = new OneRingUser();
                            u.ID = reader.GetInt32(0);
                            u.GraphID = reader.GetString(1);
                            users.Add(u);
                        }
                    }
                }
            }

            return users;
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
        public  List<ConfigField> ListConfigFields(int portletId) {
            List<ConfigField> fields = new List<ConfigField>();
            string query = @"SELECT * FROM ConfigField WHERE PortletID=@portletid";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@portletid", portletId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            ConfigField cf = ConfigField.ReadFromDb(reader);
                            cf.ConfigFieldOptions = ListConfigFieldOptions(cf.ID);
                            fields.Add(cf);
                        }
                    }
                }
            }
            return fields;
        }

        /// <summary>
        /// Returns all ConfigFields with an ID matching the ID of the provided ConfigField
        /// parameter.
        /// </summary>
        /// <param name="cf">A ConfigField model with it's ID set to the ID of the ConfigField to
        /// search for. All other fields of cf may be unset.</param>
        /// <returns>Returns a list of all ConfigFields with ID matching the ID of input cf. The
        /// returned list SHOULD always be of length 1 or 0, but this method makes no
        /// guarantees.</returns>
        public List<ConfigField> ListConfigFields(ConfigField inptCF) {
            List<ConfigField> fields = new List<ConfigField>();
            string query = @"SELECT * FROM ConfigField WHERE ConfigFieldID=@configfieldid";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@configfieldid", inptCF.ID);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            ConfigField cf = ConfigField.ReadFromDb(reader);
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
                ConfigFieldID,
                ConfigFieldOptionName,
                ConfigFieldOptionValue
            ) VALUES (
                @configfieldid,
                @optionname,
                @optionvalue
            )";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("configfieldid", configFieldId);
                    cmd.Parameters.AddWithValue("optionname", option.Name);
                    cmd.Parameters.AddWithValue("optionvalue", option.Value);
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
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            options.Add(ConfigFieldOption.ReadFromDb(reader));
                        }
                    }
                }
            }
            return options;
        }

        /// <summary>
        /// Finds all ConfigFieldOptions with the same ID as the ID of the ConfigFieldOption provided
        /// as input. No other fields of the input ConfigFieldOption need to be set.
        /// </summary>
        /// <param name="cfo">The ID field of this ConfigFieldOption is used, but no other fields are
        /// checked, and they may be uninitialized.</param>
        /// <returns>A list of all ConfigFieldOptions in the Database with an ID matching the ID of
        /// the input ConfigFieldOption. While the length of the returned list should be either 0 or
        /// 1, this method provides no garuantees about the size of the returned list.</returns>
        public List<ConfigFieldOption> ListConfigFieldOptions(ConfigFieldOption cfo) {
            List<ConfigFieldOption> options = new List<ConfigFieldOption>();
            string query = @"SELECT * FROM ConfigFieldOption WHERE ConfigFieldOptionID=@configfieldoptionid";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@configfieldoptionid", cfo.ID);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            options.Add(ConfigFieldOption.ReadFromDb(reader));
                        }
                    }
                }
            }
            return options;
        }

        public void CreatePortletInstance(PortletInstance inst) {
            string query = @"INSERT INTO PortletInstance (
                PortletID,
                UserID,
                Height,
                Width,
                XPos,
                YPos
            ) output INSERTED.PortletInstanceID VALUES (
                @portletid,
                @userid,
                @height,
                @width,
                @xpos,
                @ypos
            )";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("portletid", inst.Portlet.ID);
                    cmd.Parameters.AddWithValue("userid", inst.User.ID);
                    cmd.Parameters.AddWithValue("height", inst.Height);
                    cmd.Parameters.AddWithValue("width", inst.Width);
                    cmd.Parameters.AddWithValue("xpos", inst.XPos);
                    cmd.Parameters.AddWithValue("ypos", inst.YPos);

                    conn.Open();
                    int portletInstId = (int)cmd.ExecuteScalar();
                    if (inst.ConfigFieldInstances != null) {
                        foreach (ConfigFieldInstance ci in inst.ConfigFieldInstances) {
                            // Ensure that the PortletInstance now references this valid portlet
                            // instance with a valid ID so that the DB can correctly insert this
                            // ConfigFieldInstance.
                            inst.ID = portletInstId;
                            ci.PortletInstance = inst;
                            CreateConfigFieldInstance(ci);
                        }
                    }
                }
            }
        }

        // Finds all the PortletInstances which point to the user with the ID matching the ID of the
        // user object provided to this function.
        public List<PortletInstance> ListPortletInstances(OneRingUser user) {
            List<PortletInstance> insts = new List<PortletInstance>();
            string query = @"SELECT * FROM PortletInstance WHERE UserID=@userid";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@userid", user.ID);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            PortletInstance pi = PortletInstance.ReadFromDb(reader);
                            pi.Portlet = ListPortlets(reader.GetInt32(1))[0];
                            pi.User = ListBareOneRingUsers(new OneRingUser{ ID = reader.GetInt32(2) })[0];
                            pi.ConfigFieldInstances = ListConfigFieldInstances(pi);
                            // Ensure that the PortletInstance field of each ConfigFieldInstance is
                            // set to point to the newly-created PortletInstance. This does lead to
                            // a circular data structure, which may or may not be a problem. : /
                            foreach (ConfigFieldInstance cfi in pi.ConfigFieldInstances) {
                                cfi.PortletInstance = pi;
                            }
                            insts.Add(pi);
                        }
                    }
                }
            }
            return insts;
        }

        /// <summary>
        /// Lists all PortletInstances with an ID matching the ID of the provided PortletInstance.
        /// </summary>
        /// <param name="pi">The ID field of this PortletInstance is used as the ID of the
        /// PortletInstance in the DB to query for. However, no other fields of this input
        /// PortletInstance are used. This pattern is used to allow for overloads of the
        /// ListPortletInstance method with different input types for different
        /// relationships.</param>
        /// <returns>Returns a list of all PortletInstances with the ID of the input parameter "pi".
        /// The returned list *should* be of either length 0 or 1, but this method makes no
        /// guarantees about the length of the returned list.</returns>
        public List<PortletInstance> ListPortletInstances(PortletInstance pi) {
            List<PortletInstance> insts = new List<PortletInstance>();
            string query = @"SELECT * FROM PortletInstance WHERE PortletInstanceID=@portletinstanceid";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@portletinstanceid", pi.ID);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {

                            PortletInstance out_pi = PortletInstance.ReadFromDb(reader);
                            out_pi.Portlet = ListPortlets(reader.GetInt32(1))[0];
                            out_pi.User = ListOneRingUsers(reader.GetString(2))[0];
                            out_pi.ConfigFieldInstances = ListConfigFieldInstances(out_pi);
                            // Ensure that the PortletInstance field of each ConfigFieldInstance is
                            // set to point to the newly-created PortletInstance. This does lead to
                            // a circular data structure, which may or may not be a problem. : /
                            foreach (ConfigFieldInstance cfi in out_pi.ConfigFieldInstances) {
                                cfi.PortletInstance = out_pi;
                            }
                            insts.Add(out_pi);
                        }
                    }
                }
            }
            return insts;
        }

        // Creates a new ConfigFieldInstance in the DB. The PortletInstance field of the provided
        // ConfigFieldInstance must be populated with a valid object. The fields ConfigField,
        // ConfigFieldInstanceValue, and ConfigFieldOptionID may be null based on the conditions
        // described in the ConfigFieldInstance model.
        public void CreateConfigFieldInstance(ConfigFieldInstance inst) {
            string query = @"INSERT INTO ConfigFieldInstance (
                ConfigFieldID,
                ConfigFieldInstanceValue,
                ConfigFieldOptionID,
                PortletInstanceID
            ) VALUES (
                @configfieldid,
                @configfieldinstancevalue,
                @configfieldoptionid,
                @portletinstanceid
            )";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {

                    object confFieldId;
                    object confFieldVal;
                    if (inst.ConfigField == null) {
                        confFieldId = DBNull.Value;
                    } else {
                        confFieldId = inst.ConfigField.ID;
                    }
                    if (inst.ConfigFieldInstanceValue == null) {
                        confFieldVal = DBNull.Value;
                    } else {
                        confFieldVal = inst.ConfigFieldInstanceValue;
                    }
                    cmd.Parameters.AddWithValue("configfieldid", confFieldId);
                    cmd.Parameters.AddWithValue("configfieldinstancevalue", confFieldVal);

                    object optID;
                    if (inst.ConfigFieldOption == null) {
                        optID = DBNull.Value;
                    } else {
                        optID = inst.ConfigFieldOption.ID;
                    }
                    cmd.Parameters.AddWithValue("configfieldoptionid", optID);
                    cmd.Parameters.AddWithValue("portletinstanceid", inst.PortletInstance.ID);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<ConfigFieldInstance> ListConfigFieldInstances(PortletInstance pi) {
            List<ConfigFieldInstance> insts = new List<ConfigFieldInstance>();
            string query = @"SELECT * FROM ConfigFieldInstance WHERE PortletInstanceID=@portletinstanceid";
            using (SqlConnection conn = new SqlConnection(this._connectionString)) {
                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@portletinstanceid", pi.ID);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            ConfigFieldInstance cfi = new ConfigFieldInstance();
                            cfi.ID = (int)reader["ConfigFieldInstanceID"];
                            if (reader["ConfigFieldID"] != DBNull.Value) {
                                cfi.ConfigField = ListConfigFields(new ConfigField{ ID = (int)reader["ConfigFieldID"]})[0];
                                cfi.ConfigFieldInstanceValue = (string)reader["ConfigFieldInstanceValue"];
                            }
                            if (reader["ConfigFieldOptionID"] != DBNull.Value) {
                                cfi.ConfigFieldOption = ListConfigFieldOptions(new ConfigFieldOption{ ID = (int) reader["configFieldOptionID"]})[0];
                            }
                            // Because PortletInstances have ConfigFieldInstances, a naive finding
                            // of these would lead to an infinite loop. For now, we'll just set the
                            // ID values and move on with our lives.
                            cfi.PortletInstance = new PortletInstance{ ID = (int)reader["PortletInstanceID"]};
                            insts.Add(cfi);
                        }
                    }
                }
            }
            return insts;
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
                        ConfigFieldOptionName NVARCHAR(3000),
                        ConfigFieldOptionValue NVARCHAR(3000),
                        ConfigFieldID INTEGER NOT NULL,
                        CONSTRAINT ConfigFieldOptionConfigFieldID FOREIGN KEY (ConfigFieldID) REFERENCES ConfigField (ConfigFieldID),
                        CONSTRAINT ConfigFieldOptionPK PRIMARY KEY (ConfigFieldOptionID)
                    );
                END", conn);
                createConfigFieldOption.ExecuteNonQuery();

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

                // Notes for ConfigFieldInstance:
                // ConfigFieldInstance contains the user-provided value for a configurable field of
                // a portlet. ConfigFieldInstance may reference either a ConfigField or a
                // ConfigFieldOption. If it references a ConfigField, then ConfigFieldId and
                // ConfigFieldInstanceValue will have non-null values. If it references a
                // ConfigFieldOption, then ConfigFieldOptionID will have a non-null value.
                Debug.WriteLine("Creating table for ConfigFieldInstance if it doesn't exist already.");
                SqlCommand createConfigFieldInstance = new SqlCommand(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE NAME='ConfigFieldInstance')
                BEGIN
                    CREATE TABLE ConfigFieldInstance(
                        ConfigFieldInstanceID INTEGER NOT NULL IDENTITY(1,1),

                        ConfigFieldID INTEGER,
                        ConfigFieldInstanceValue NVARCHAR(3000),

                        ConfigFieldOptionID INTEGER,

                        PortletInstanceID INTEGER NOT NULL,

                        CONSTRAINT ConfigFieldInstanceConfigFieldID FOREIGN KEY (ConfigFieldID) REFERENCES ConfigField (ConfigFieldID),
                        CONSTRAINT ConfigFieldInstanceConfigFieldOptionID FOREIGN KEY (ConfigFieldOptionID) REFERENCES ConfigFieldOption (ConfigFieldOptionID),
                        CONSTRAINT ConfigFieldInstancePortletInstanceID FOREIGN KEY (PortletInstanceID) REFERENCES PortletInstance (PortletInstanceID),
                        CONSTRAINT ConfigFieldInstancePK PRIMARY KEY (ConfigFieldInstanceID)
                    );
                END", conn);
                createConfigFieldInstance.ExecuteNonQuery();

                conn.Close();
            }
        }
    }
}