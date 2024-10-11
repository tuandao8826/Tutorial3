using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.Domain.Model.Options
{
    public class PollyOptions
    {
        public int RetryCount {  get; set; }
        public int TimeWaitAfterFailed {  get; set; }
    }
}
