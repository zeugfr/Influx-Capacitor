using System;
using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Event;

namespace Tharga.InfluxCapacitor.Collector.Interface
{
    public interface IConfigBusiness
    {
        IConfig LoadFiles();
        IConfig LoadFiles(string[] configurationFilenames);
        IDatabaseConfig OpenDatabaseConfig();
        void SaveDatabaseUrl(string url);
        void SaveDatabaseConfig(string databaseName, string username, string password);
        void SaveApplicationConfig(int flushSecondsInterval, bool debugMode);
        void InitiateApplicationConfig();
        IEnumerable<string> GetConfigFiles();
        bool CreateConfig(string fileName, List<ICounterGroup> counterGroups);
        event EventHandler<InvalidConfigEventArgs> InvalidConfigEvent;
    }
}