using System;

namespace FileStorage
{
    class TimeSerivice : ITimeSerivice
    {
        public DateTime UTCNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}