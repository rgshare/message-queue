using System;
using System.Collections.Generic;
using JobsQueue.Scheduling;

namespace JobsQueue
{
    internal sealed class WorkerManager : List<Worker>
    {
        private readonly ILogger _logger;
        private readonly int _size;
        private bool _running;

        public WorkerManager(ILogger logger, int size)
        {
            _logger = logger;
            this._size = size;
        }

        public void Start(string name, Action hanndle)
        {
            if (this._running)
            {
                return;
            }

            for (int i = 0; i < this._size; i++)
            {
                var worker = new Worker(this._logger, string.Format("{0}[{1}]", name, i), hanndle);
                worker.Start();

                this.Add(worker);
            }

            _running = true;
        }

        public void Stop()
        {
            for (int i = 0; i < this._size; i++)
            {
                this[i].Stop();
            }

            _running = false;
        }

    }
}
