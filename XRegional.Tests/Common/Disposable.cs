using System;

namespace XRegional.Tests.Common
{
    public class Disposable : IDisposable
    {
        private readonly Action _action;

        public Disposable(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            if (_action != null)
                _action.Invoke();
        }
    }
}
