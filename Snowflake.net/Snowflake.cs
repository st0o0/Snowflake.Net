using System;

namespace Snowflake.net
{
    public class Snowflake
    {
        private const long _epoch = 1288834974657L;

        private const int _workerIdBits = 5;
        private const int _dataCenterIdBits = 5;
        private const int _sequenceIdBits = 12;
        private const long _maxWorkerId = -1L ^ (-1L << _workerIdBits);
        private const long _maxDataCenterId = -1L ^ (-1L << _dataCenterIdBits);

        private const int _workerIdShift = _sequenceIdBits;
        private const int _dataCenterIdShift = _sequenceIdBits + _workerIdBits;
        private const int _timestampLeftShift = _sequenceIdBits + _workerIdBits + _dataCenterIdBits;
        private const long _squenceMask = -1L ^ (-1L << _sequenceIdBits);

        private long _sequence = 0L;
        private long _lastTimestamp = -1L;

        readonly object _lock = new Object();

        public Snowflake(long workerId, long dataCenterId, long sequence = 0L)
        {
            this.WorkerId = workerId;
            this.DataCenterId = dataCenterId;
            this.Sequence = sequence;
        }

        public long WorkerId { get; protected set; }

        public long DataCenterId { get; protected set; }
        
        public long Sequence { get; internal set; }

        public virtual long NextId()
        {
            lock (_lock)
            {
                var timestamp = TimeGen();



            }
        }


        protected virtual long TimeGen()
        {
            return System.CurrentTimeMillis();
        }
    }
}
