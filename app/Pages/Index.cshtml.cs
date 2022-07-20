using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Configuration;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VersionInfo.Pages
{
    public class IndexModel : PageModel
    {
        public class AppList
        {
            public string? appName { get; set; }
            public string? appVersion { get; set; }
            public string? appColor { get; set; }
        }
        public class DbList
        {
            public string? dbName { get; set; }
            public string? dbVersion { get; set; }
            public string? dbColor { get; set; }
        }
        public class appVersionJson
        {
            public string? AssemblyVersion { get; set; } = default!;
            public string? ProductVersion { get; set; } = default!;
        }
        public class dbVersionJson
        {
            public string? RepositoryURL { get; set; } = default!;
            public string? Branch { get; set; } = default!;
            public string? RevisionNumber { get; set; } = default!;
            public string? BuildNumber { get; set; } = default!;
            public string? BuildAgent { get; set; } = default!;
            public string? Version { get; set; } = default!;
        }
        public class buildJson
        {
            public appVersionJson? Application { get; set; }
            public dbVersionJson? Database { get; set; }
        }
        public IList<AppList> _apps = new List<AppList>();
        public IList<DbList> _dbs = new List<DbList>();

        private readonly IConfiguration _config;
        private readonly ILogger<IndexModel> _logger;
        public IndexModel(IConfiguration config, ILogger<IndexModel> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void OnGet()
        {
            // Central Apps endpoints
            var endpointsArray = _config.GetSection("VersionInfoEndpoints");
            if (endpointsArray != null)
            {
                foreach (IConfigurationSection section in endpointsArray.GetChildren())
                {
                    string _endpointName = section.GetValue<string>("Name");
                    string _endpointUri = section.GetValue<string>("Endpoint");
                    string _endpointDBVersion = "ERROR";
                    string _endpointAppVersion = "ERROR";
                    string _endpointColor = "green";

                    try
                    {
                        var restcall = new WebClient();
                        string responseString = restcall.DownloadString(_endpointUri);
                        Debug.Print("Endpoint Json response: {0}", responseString);
                        buildJson? _buildJson = JsonConvert.DeserializeObject<buildJson>(responseString);
                        if (_buildJson.Application.ProductVersion != null) { _endpointAppVersion = _buildJson.Application.ProductVersion; }
                        if (_buildJson.Database.Version != null) { _endpointDBVersion = _buildJson.Database.Version; }
                    }
                    catch
                    {
                        _endpointColor = "red";
                    }
                    Debug.Print("========================\nendpointName: {0}", _endpointName);
                    Debug.Print("endpointUri: {0}", _endpointUri);
                    Debug.Print("endpointAppVersion: {0}", _endpointAppVersion);
                    Debug.Print("endpointDBVersion: {0}", _endpointDBVersion);
                    Debug.Print("endpointColor: {0}\n========================", _endpointColor);
                    _apps.Add(new AppList { appName = _endpointName, appVersion = _endpointAppVersion, appColor = _endpointColor });
                    _dbs.Add(new DbList { dbName = _endpointName, dbVersion = _endpointDBVersion, dbColor = _endpointColor });
                }
            }
            // Local App files
            var appsArray = _config.GetSection("VersionInfoFiles");
            if (appsArray != null)
            {
                foreach (IConfigurationSection section in appsArray.GetChildren())
                {
                    string _appName = section.GetValue<string>("Name");
                    string _appFile = section.GetValue<string>("File");
                    string _appVersion = "NOT INSTALLED";
                    string _appColor = "green";
                    try
                    {
                        FileVersionInfo? myFileVersionInfo = FileVersionInfo.GetVersionInfo(_appFile);
                        if (myFileVersionInfo.FileVersion != null) { _appVersion = myFileVersionInfo.FileVersion; }
                        //_logger.LogInformation($"Version: {_appVersion}");
                    }
                    catch
                    {
                        _appColor = "red";
                    }
                    Debug.Print("========================\nappName: {0}", _appName);
                    Debug.Print("appFile: {0}", _appFile);
                    Debug.Print("appVersion: {0}", _appVersion);
                    Debug.Print("appColor: {0}\n========================", _appColor);
                    _apps.Add(new AppList { appName = _appName, appVersion = _appVersion, appColor = _appColor });
                }
            }
            // Central DBs
            var dbArray = _config.GetSection("VersionInfoDatabases");
            if (dbArray != null)
            {
                foreach (IConfigurationSection section in dbArray.GetChildren())
                {
                    string _dbName = section.GetValue<string>("DBName");
                    string _dbExtendedProprty = section.GetValue<string>("ExtendedPropertyName");
                    string _dbConnString = section.GetValue<string>("ConnectionString");
                    string _dbVersion = "NOT INSTALLED";
                    string _dbColor = "green";
                    string _sql = "SELECT value from sys.extended_properties where name = '" + _dbExtendedProprty + "'";
                    SqlConnectionStringBuilder connstr = new SqlConnectionStringBuilder();
                    connstr.ConnectionString = _dbConnString;
                    connstr.ConnectTimeout = 1;
                    connstr.PersistSecurityInfo = false;
                    connstr.Encrypt = false;
                    Debug.Print("SQL ConnectionString: {0}", connstr.ToString());
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(connstr.ToString()))
                        {
                            if (connection.State == 0)
                            {
                                connection.Open();
                            }
                            Debug.Print("SQL ServerVersion: {0}", connection.ServerVersion);
                            Debug.Print("SQL State: {0}", connection.State);
                            using (SqlCommand command = new SqlCommand(_sql, connection))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    Debug.Print("Rows: {0}", reader.HasRows);
                                    reader.Read();
                                    if (reader.HasRows)
                                    {
                                        string readerJson = reader.GetString(0);
                                        Debug.Print("Database JSON response: {0}", readerJson);
                                        dbVersionJson? _dbVersionJson = JsonConvert.DeserializeObject<dbVersionJson>(readerJson);
                                        if (_dbVersionJson.Version != null) { _dbVersion = _dbVersionJson.Version; }
                                    }
                                    else
                                    {
                                        _dbVersion = "NO VALUE";
                                        _dbColor = "orange";
                                    }
                                }
                                connection.Close();
                            }
                        }
                    }
                    catch
                    {
                        _dbColor = "red";
                    }
                    Debug.Print("========================\ndbName: {0}", _dbName);
                    Debug.Print("dbExtendedProprty: {0}", _dbExtendedProprty);
                    Debug.Print("dbVersion: {0}", _dbVersion);
                    Debug.Print("dbColor: {0}\n========================", _dbColor);
                    _dbs.Add(new DbList { dbName = _dbName, dbVersion = _dbVersion, dbColor = _dbColor });
                }
            }
        }
    }
}