using System;
using System.Collections.Generic;

namespace XRegional.Tests.Common
{
    public class DisposableList : List<IDisposable>, IDisposable
    {
        public void Dispose()
        {
            // dispose from the end of the list to preserve the order!!
            for (int i = Count - 1; i >= 0; --i)
                this[i].Dispose();
            Clear();
        }

        public void Add(Action dispose)
        {
            Add(new Disposable(dispose));
        }
    }
}
