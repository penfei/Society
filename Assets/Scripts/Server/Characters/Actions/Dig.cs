using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Society.Characters
{
    public class Dig : BaseAction
    {
        protected override double RandomizeDuration()
        {
            return _random.NextDouble() * 4 + 4;
        }
    }
}
