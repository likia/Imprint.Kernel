using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Imprint.Core
{
    public enum RedialStatus
    {
        Connected,
        Connecting,
        Disconnected,
    }

    public interface IRedial
    {
        void Online();

        void Offline();

        RedialStatus GetStatus();
    }
}
