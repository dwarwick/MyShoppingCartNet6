using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Models
{
    public class JobSchedule
    {
        public JobSchedule(Type jobType, string cronExpression, int numSeconds)
        {
            JobType = jobType;
            CronExpression = cronExpression;
            seconds = numSeconds;
        }

        public Type JobType { get; }
        public string CronExpression { get; }
        public int seconds;
    }
}

