﻿using System.ServiceProcess;
using System.Timers;

namespace FlaxCrashReport
{
    public partial class Main : ServiceBase
    {

        // TODO:
        // NEED TO IMPLEMENT SETTINGS BETTER
        // ADD MORE ENUMERATIONS?
        // READ MORE OF "CLEAN CODE" BOOK AND IMPLEMENT IT HERE


        Timer timer = new Timer();

        /// <summary>
        /// Main function, initialize components
        /// Code below //Debug used for VS debugging the service
        /// </summary>
        public Main()
        {
            InitializeComponent();
            //DEBUG => uncomment code below!
            OnElapsedTime(null, null);
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Called od service starting
        /// Sends email that service has started
        /// Calls function OnElapsedTime each minute
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            Logic.MainLogic.CreateAndSendFCREmail("Service started.", Enumerations.EStatus.EmailSubjectStatus.FCR_STARTED);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = System.TimeSpan.FromMinutes(Data.Settings.Instance.Interval).TotalMilliseconds;
            timer.Enabled = true;
        }

        /// <summary>
        /// Called on service stopping
        /// Sends email that service has stopped
        /// </summary>
        protected override void OnStop()
        {
            Logic.MainLogic.CreateAndSendFCREmail("Service stopped.", Enumerations.EStatus.EmailSubjectStatus.FCR_STOPPED);
        }

        /// <summary>
        /// This will fire every minute
        /// Calls function to check error in EventViewer and send emails
        /// Also sends OK Status email every 24h
        /// Global try-catch here to create FCR_CRASH.json if Service fails somewhere
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            Logic.MainLogic.CheckAndProcessLogs();
        }

    }
}
