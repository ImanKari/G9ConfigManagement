using System;
using System.Collections.Generic;
using System.Text;
using G9ConfigManagement.Attributes;

namespace G9ConfigManagementNUnitTest.Sample
{
    public class SampleSubSubConfig
    {

        public SampleSubSubConfig()
        {
            Active = true;
            SaveTime = 10;
        }

        public bool Active { set; get; }

        [G9ConfigHint("Set save time in second")]
        public int SaveTime { set; get; }


        [G9ConfigHint("Set start date time")]
        public DateTime StartDateTime { set; get; }

    }
}
