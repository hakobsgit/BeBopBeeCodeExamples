using System;
using Configs;
using Services.Sockets;
using UniRx;
using Zenject;

namespace Services {
    public class TimeService : IInitializable {
        [Inject] private SocketResponseService _socketResponseService;
        [Inject] private TimeServiceConfig _config;

        private long _deltaTime;
        
        public void Initialize() {
            _socketResponseService.DateTimeUpdate.Subscribe(dateTimeData => {
                if (dateTimeData == null) {
                    return;
                }
                if (Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - dateTimeData.Time) > _config.AcceptableMillisDifferenceWithBackend) {
                    _deltaTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - dateTimeData.Time;
                } 
            });
        }

        public long GetDelta(long time) {
            return Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - CorrectTimeByDelta(time));
        }

        public int GetDeltaSeconds(long time) {
            return (int)(Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - CorrectTimeByDelta(time)) / 1000f);
        }

        public bool IsPast(long time) {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - CorrectTimeByDelta(time) > 100;
        }

        public long CorrectTimeByDelta(long time) {
            return time - _deltaTime;
        }
    }
}