using System;

namespace MvcMusicStore.Infrastructure
{
    public interface IAzureCache
    {
        bool Exists<T>(string key);
        T Get<T>(string key);
        void Put<T>(string key, object value);
        void Put<T>(string key, object value, TimeSpan timeOut);
        T MakeCached<T>(string key, Func<string, T> resultFunc);
        T MakeCached<T>(string key, Func<string, T> resultFunc, TimeSpan absoluteExpiry);
    }
}