using System;
using System.Collections.Generic;
using System.Threading;

namespace ConcurrentMessageQueue.Scheduling
{
    internal sealed class Interval
    {
        private readonly ILogger _logger;
        private readonly object _lockObject;
        private readonly Dictionary<string, TimerBasedTask> _taskDict;

        //Interval理应是单例模式，但为了方便unittest，改成实例模式
        internal Interval(ILogger logger)
        {
            this._logger = logger;
            this._lockObject = new object();
            this._taskDict = new Dictionary<string, TimerBasedTask>();
        }

        /// <summary>
        /// 开启一个定时执行的任务
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <param name="action">将要执行的动作</param>
        /// <param name="interval">回调方法执行的时间间隔</param>
        public void StartTask(string name, Action action, int interval)
        {
            StartTask(name, action, interval, interval);
        }

        /// <summary>
        /// 开启一个定时执行的任务
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <param name="action">将要执行的动作</param>
        /// <param name="dueTime">首次调用前的延迟时间量</param>
        /// <param name="interval">回调方法执行的时间间隔</param>
        public void StartTask(string name, Action action, int dueTime, int interval)
        {
            lock (_lockObject)
            {
                if (_taskDict.ContainsKey(name)) return;
                var timer = new Timer(TaskCallback, name, Timeout.Infinite, Timeout.Infinite);
                _taskDict.Add(name, new TimerBasedTask { Name = name, Action = action, Timer = timer, DueTime = dueTime, Period = interval, Stopped = false });
                timer.Change(dueTime, Timeout.Infinite);
            }
        }
        public void StopTask(string name)
        {
            lock (_lockObject)
            {
                if (_taskDict.ContainsKey(name))
                {
                    var task = _taskDict[name];
                    task.Stopped = true;
                    task.Timer.Dispose();
                    _taskDict.Remove(name);
                }
            }
        }

        private void TaskCallback(object obj)
        {
            var taskName = (string)obj;
            TimerBasedTask task;

            if (_taskDict.TryGetValue(taskName, out task))
            {
                try
                {
                    if (!task.Stopped)
                    {
                        task.Action();
                    }
                }
                catch (ObjectDisposedException) { }
                catch (Exception ex)
                {
                    this._logger.Log(LogLevel.Error, string.Format(
                        "Task has exception, name: {0}, due: {1}, period: {2}\r\n{3}", 
                        task.Name, 
                        task.DueTime, 
                        task.Period,
                        ex));
                }
                finally
                {
                    try
                    {
                        if (!task.Stopped)
                        {
                            task.Timer.Change(task.Period, Timeout.Infinite);
                        }
                    }
                    catch (ObjectDisposedException) { }
                    catch (Exception ex)
                    {
                        this._logger.Log(LogLevel.Error, string.Format(
                            "Timer change has exception, name: {0}, due: {1}, period: {2}、\r\n{3}", 
                            task.Name, task.DueTime, task.Period, ex));
                    }
                }
            }
        }

        class TimerBasedTask
        {
            public string Name;
            public Action Action;
            public Timer Timer;
            public int DueTime;
            public int Period;
            public bool Stopped;
        }
    }
}
