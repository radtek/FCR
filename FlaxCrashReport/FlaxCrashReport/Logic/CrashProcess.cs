﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaxCrashReport.Logic
{
    public class CrashData
    {
        #region Private members
        private List<EventLogEntry> eventLogs;
        private DateTime lastApplicationCrashTime;
        private string machineName;
        private string userName;
        #endregion

        public CrashData()
        {
            machineName = Data.SGeneral.Instance.Settings.MachineName,;
            userName = Data.SGeneral.Instance.Settings.UserName;
            lastApplicationCrashTime = Data.SGeneral.Instance.Settings.LastAppCrash;
        }

        /// <summary>
        /// Get all logs from System EventViewer made by Flax or System itself
        /// </summary>
        public void GetEventLogs()
        {

            EventLog el2 = new EventLog("Application");
            eventLogs = (from EventLogEntry elog in el2.Entries
                          where elog.TimeWritten > lastApplicationCrashTime
                          && elog.Message.ToString().Contains("Application: Users.exe") // <---- I know it's ugly
                          && elog.EntryType == EventLogEntryType.Error
                          orderby elog.TimeWritten ascending
                          select elog).ToList();
        }

        public void SetLastApplicationCrashTime()
        {
            if (eventLogs == null || eventLogs.Count() < 1) return;
            lastApplicationCrashTime = eventLogs.Max(m => m.TimeWritten);
        }


        /// <summary>
        /// Collects data from EventViewer and makes JSON files in Reports folder to be send by email
        /// This functions 
        /// </summary>
        public void GenerateJSONFiles()
        {

            foreach (EventLogEntry e in eventLogs)
            {
                Data.CrashData JSONDataObject = GenerateJSONLogData(e);

                var serializerSettings = new JsonSerializerSettings{ ContractResolver = new CamelCasePropertyNamesContractResolver() };
                var json = JsonConvert.SerializeObject(JSONDataObject, serializerSettings);
                string newreport = Data.SGeneral.Instance.Settings.ReportsPath + @"\Report_" + e.TimeWritten.ToString("yyyyMMddHHmmss") + ".json";
                File.WriteAllText(newreport, json);
            }
        }

        public Data.CrashData GenerateJSONLogData(EventLogEntry ele)
        {
            return new Data.CrashData
            {
                Category = ele.Category.ToString(),
                EntityType = ele.EntryType.ToString(),
                MachineName = ele.MachineName ?? Data.SGeneral.Instance.Settings.MachineName,
                Message = ele.Message.ToString(),
                Source = ele.Source.ToString(),
                TimeGenerated = ele.TimeGenerated,
                TimeWritten = ele.TimeWritten,
                UserName = ele.UserName ?? Data.SGeneral.Instance.Settings.UserName
            };

        }
    }
}