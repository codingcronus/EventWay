using System;

namespace EventWayCore.Core
{
    public static class CombGuid
    {
        public static readonly Guid Empty = Guid.Parse("00000000-0000-0000-0000-000000000000");

        private static readonly Func<Guid> GeneratorCore = () =>
        {
            var destinationArray = Guid.NewGuid().ToByteArray();
            var time = new DateTime(1900, 1, 1);
            var now = DateTime.UtcNow;
            var span = new TimeSpan(now.Ticks - time.Ticks);
            var timeOfDay = now.TimeOfDay;
            var bytes = BitConverter.GetBytes(span.Days);
            var array = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333));
            Array.Reverse(bytes);
            Array.Reverse(array);
            Array.Copy(bytes, bytes.Length - 2, destinationArray, destinationArray.Length - 6, 2);
            Array.Copy(array, array.Length - 4, destinationArray, destinationArray.Length - 4, 4);
            return new Guid(destinationArray);
        };

        private static Func<Guid> _generator = GeneratorCore;

        public static Guid Generate()
        {
            return _generator();
        }

        public static void Reset()
        {
            _generator = GeneratorCore;
        }

        public static IDisposable Stub(Guid value)
        {
            _generator = () => value;
            return new DisposableAction(Reset);
        }
    }
}