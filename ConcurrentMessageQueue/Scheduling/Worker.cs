using System;
using System.Threading;
using ConcurrentMessageQueue.Logging;

namespace ConcurrentMessageQueue.Scheduling
{
    /// <summary>
    /// 开启一个重复执行的后台任务
    /// </summary>
    internal class Worker
    {
        private readonly object _lockObject = new object();
        private readonly ILogger _logger;
        private readonly string _name;
        private readonly Action _action;
        private Status _status;

        /// <summary>
        /// 初始化一个重复执行的后台任务
        /// </summary>
        /// <param name="logger">日志</param>
        /// <param name="name">任务名称</param>
        /// <param name="action">将要执行的任务</param>
        public Worker(ILogger logger, string name, Action action)
        {
            _logger = logger;
            this._name = name;
            this._action = action;
            this._status = Status.Initial;
        }

        /// <summary>
        /// 开始运行该任务
        /// </summary>
        public Worker Start()
        {
            lock (_lockObject)
            {
                if (_status == Status.Running) return this;

                _status = Status.Running;
                new Thread(Loop)
                {
                    Name = string.Format("{0}.Worker", this._name),
                    IsBackground = true
                }.Start(this);

                return this;
            }
        }
        /// <summary>
        /// 停止运行服务
        /// </summary>
        public Worker Stop()
        {
            lock (_lockObject)
            {
                if (_status == Status.StopRequested) return this;

                _status = Status.StopRequested;

                return this;
            }
        }

        private void Loop(object data)
        {
            var worker = (Worker)data;

            while (worker._status == Status.Running)
            {
                try
                {
                    _action();
                }
                catch (ThreadAbortException)
                {
                    this._logger.Info("Worker thread caught ThreadAbortException, try to resetting, actionName:" + this._name);
                    Thread.ResetAbort();
                    this._logger.Info("Worker thread ThreadAbortException resetted, actionName:{0}" + this._name);
                }
                catch (Exception ex)
                {
                    this._logger.Error(string.Format("Worker thread has exception, actionName:{0}", this._name), ex);
                }
            }
        }

        enum Status
        {
            Initial,
            Running,
            StopRequested
        }
    }
}
