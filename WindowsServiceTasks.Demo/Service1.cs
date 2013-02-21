using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using WindowsServiceTasks.Base;
using WindowsServiceTasks.Demo.Services;

namespace WindowsServiceTasks.Demo
{
    public class Service1 : WindowsServiceBase
    {
        private readonly ILogger _logger;

        public Service1(ILogger logger, params IWindowsServiceTask[] windowsServiceTasks)
            : base(windowsServiceTasks)
        {
            _logger = logger;
        }
        
        protected override void LogException(Exception exception)
        {
            _logger.Log(exception);
        }
    }
}
