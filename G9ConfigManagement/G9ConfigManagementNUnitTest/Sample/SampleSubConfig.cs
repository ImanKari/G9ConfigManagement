using System;
using System.Collections.Generic;
using System.Text;
using G9ConfigManagement.Attributes;

namespace G9ConfigManagementNUnitTest.Sample
{
    public class SampleSubConfig
    {

        public SampleSubConfig()
        {
            Active = true;
            SaveTime = 30;
            SampleSubTwo = new SampleSubSubConfig();
        }

        public bool Active { set; get; }

        [Hint("Set save time in second")]
        public int SaveTime { set; get; }

        public SampleSubSubConfig SampleSubTwo { set; get; }


        [Hint("Set start date time")]
        public DateTime StartDateTime { set; get; }

    }
}
