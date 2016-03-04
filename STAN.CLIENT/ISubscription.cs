using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STAN.Client
{
    public interface ISubscription
    {
        void Unsubscribe();
    }
}
