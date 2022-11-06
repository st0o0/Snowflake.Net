using System;

namespace Snowflake
{
    public class Snowflake
    {
        private const long _epoch = 1288834974657L;

        private const int _workerIdBits = 5;
        private const int _datacenterIdBits = 5;
        private const int _sequenceIdBits = 12;
        private const long _maxWorkerId = -1L ^ -1L << _workerIdBits;
        private const long _maxDataCenterId = -1L ^ -1L << _datacenterIdBits;

        private const int _workerIdShift = _sequenceIdBits;
        private const int _datacenterIdShift = _sequenceIdBits + _workerIdBits;
        private const int _timestampLeftShift = _sequenceIdBits + _workerIdBits + _datacenterIdBits;
        private const long _squenceMask = -1L ^ -1L << _sequenceIdBits;

        private long _sequence = 0L;
        private long _lastTimestamp = -1L;

        readonly object _lock = new object();

        public Snowflake(long workerId, long dataCenterId, long sequence = 0L)
        {
            WorkerId = workerId;
            DataCenterId = dataCenterId;
            Sequence = sequence;
        }

        public long WorkerId { get; protected set; }

        public long DataCenterId { get; protected set; }

        public long Sequence { get; internal set; }

        public virtual long NextId()
        {
            lock (_lock)
            {
                long timestamp = TimeGen();

                if (timestamp < _lastTimestamp)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (_lastTimestamp == timestamp)
                {
                    _sequence = _sequence + 1 & _squenceMask;
                    if (_sequence == 0)
                    {
                        timestamp = TilNextMillis(_lastTimestamp);
                    
                    }

                }
                else
                {
                    _sequence = 0;
                }
                _lastTimestamp = timestamp;
                return ((timestamp - _epoch) << _timestampLeftShift) | (_datacenterIdBits << _datacenterIdShift) | (_workerIdBits << _workerIdShift) | _sequence;
            }
        }

        protected virtual long TilNextMillis(long lastTimestamp)
        {
            long timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }
            return timestamp;

        }

        protected virtual long TimeGen()
        {
            return Internals.System.CurrentTimeMillis();
        }
    }
}
