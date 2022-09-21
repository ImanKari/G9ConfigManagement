using System;
using G9JSONHandler.Attributes;

namespace G9ConfigManagementNUnitTest.Sample
{
    public class SampleSubConfig
    {
        public SampleSubConfig()
        {
            Active = true;
            SaveTime = 30;
            StartDateTime = DateTime.Parse("1990-09-01");
            SampleSubTwo = new SampleSubSubConfig();
        }

        public bool Active { set; get; }

        public int SaveTime { set; get; }

        [G9AttrComment("Specifies a second nested config in the main config structure.")]
        public SampleSubSubConfig SampleSubTwo { set; get; }

        public DateTime StartDateTime { set; get; }
    }
}