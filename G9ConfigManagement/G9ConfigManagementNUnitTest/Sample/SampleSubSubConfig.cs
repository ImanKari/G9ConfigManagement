using System;

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

        public int SaveTime { set; get; }


        public DateTime StartDateTime { set; get; } = DateTime.MaxValue;
    }
}