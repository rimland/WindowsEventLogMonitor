using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsEventLogMonitor
{
    class Program
    {
        /// <summary>
        ///  This member is used to wait for events.
        /// </summary>
        static AutoResetEvent signal;
        /// <summary>
        /// The friendly name of the process.
        /// </summary>
        private static string processName = ConfigurationManager.AppSettings["ProcessName"];
        /// <summary>
        /// exe 文件名
        /// </summary>
        private static string moduleName = ConfigurationManager.AppSettings["MainModuleName"];

        static void Main(string[] args)
        {
            signal = new AutoResetEvent(false);
            EventLog myNewLog = new EventLog("Application", ".", "Application Error");

            myNewLog.EntryWritten += new EntryWrittenEventHandler(MyOnEntryWritten);
            myNewLog.EnableRaisingEvents = true;

            Console.CancelKeyPress += (sender, e) =>
            {
                if (e.SpecialKey == ConsoleSpecialKey.ControlC)
                {
                    e.Cancel = true;
                    signal.Set();
                }
            };

            Output($"监控 Windows Event log，如果 {moduleName} 异常退出，则自动重启。");
            signal.WaitOne();
        }

        public static void MyOnEntryWritten(object source, EntryWrittenEventArgs e)
        {
            Output($"In EntryWrittenEventHandler");
            EventLogEntry entry = e.Entry;

            if (entry.EntryType == EventLogEntryType.Error)
            {
                var strArr = entry.ReplacementStrings;
                if (strArr.Contains(moduleName))
                {
                    Thread.Sleep(500);

                    var filePath = strArr.FirstOrDefault(s => s.EndsWith($@"\{moduleName}") && s.Contains(":"));

                    //先强制结束原来的程序
                    var processes = Process.GetProcessesByName(processName);
                    if (processes.Length > 0)
                    {
                        var process = processes.FirstOrDefault(p => p.MainModule.FileName == filePath);
                        if (process != null)
                        {
                            Output($"强制结束原来的程序：{process.MainModule.FileName}");
                            process.Kill();
                            Thread.Sleep(500);
                        }
                    }
                    Output($"启动 {filePath}");
                    //启动进程
                    Process.Start(filePath);
                    Output("启动成功");
                }
            }
        }

        private static void Output(string msg)
        {
            Console.WriteLine($"{DateTime.Now}  {msg}");
        }

        //private static void Test()
        //{
        //    //int i = 0;
        //    //EventLog eventlog = new EventLog();
        //    //eventlog.Log = "Application"; //"Application"应用程序, "Security"安全, "System"系统
        //    //EventLogEntryCollection eventLogEntryCollection = eventlog.Entries;
        //    //foreach (EventLogEntry entry in eventLogEntryCollection)
        //    //{
        //    //  string info = string.Empty;
        //    //  if (entry.EntryType == EventLogEntryType.Error)
        //    //  {
        //    //    var strArr = entry.ReplacementStrings;
        //    //    if (strArr.Contains(moduleName))
        //    //    {
        //    //      Thread.Sleep(500);

        //    //      var filePath = strArr.FirstOrDefault(s => s.EndsWith($@"\{moduleName}") && s.Contains(":"));

        //    //      Output($"检测到 {filePath}");

        //    //      foreach (var item in entry.ReplacementStrings)
        //    //      {
        //    //        Output(item);
        //    //      }

        //    //      //info += "类型：" + entry.EntryType.ToString() + ";";
        //    //      //info += "日期" + entry.TimeGenerated.ToLongDateString() + ";";
        //    //      //info += "时间" + entry.TimeGenerated.ToLongTimeString() + ";";
        //    //      //info += "来源" + entry.Source.ToString() + ";";
        //    //      //Console.WriteLine(info);

        //    //      i++;
        //    //      if (i >= 3)
        //    //        break;
        //    //    }
        //    //  }
        //    //}
        //    //Console.ReadLine();
        //    //return;

        //    //string sourceName = "SampleApplicationSource";

        //    //long InformationMsgId = 10;
        //    //long WarningMsgId = 11;

        //    //EventInstance myInfoEvent = new EventInstance(InformationMsgId, 0, EventLogEntryType.Information);
        //    //EventInstance myWarningEvent = new EventInstance(WarningMsgId, 0, EventLogEntryType.Warning);

        //    //// Insert the method name into the event log message.
        //    //string[] insertStrings = { "EventLogSamples.WriteEventSample2", "EventLogSamples.WriteEventSample3", "EventLogSamples.WriteEventSample4", "EventLogSamples.WriteEventSample5", "EventLogSamples.WriteEventSample6" };
        //    //// Write the events to the event log.

        //    //EventLog.WriteEvent(sourceName, myInfoEvent);

        //    ////Append binary data to the warning event entry.
        //    //byte[] binaryData = { 7, 8, 9, 10 };
        //    //EventLog.WriteEvent(sourceName, myWarningEvent, null, insertStrings);
        //}
/*
process[0].MainModule.ModuleName
"demo.exe"
process[0].MainModule.FileName
"D:\Services\SmsReport\TaoBaoONSPushService(from_demo)-2\demo.exe"
*/


    }
}


