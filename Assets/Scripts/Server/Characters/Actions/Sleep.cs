using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Society.Characters
{
    public class Sleep : BaseAction
    {
        protected override double RandomizeDuration()
        {
            return _random.NextDouble() * 3 + 7;
        }
    }
}
