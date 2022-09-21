using System;
using G9ConfigManagement.Abstract;
using G9ConfigManagement.Attributes;
using G9ConfigManagement.DataType;

namespace G9ConfigManagementNUnitTest.Sample
{
    public class SampleConfigWithRequiredMember : G9AConfigStructure<SampleConfigWithRequiredMember>
    {

        [G9AttrRequired]
        public string FullNameA = "G9TM";

        [G9AttrRequired]
        public DateTime DateTimeB = DateTime.Today;


        [G9AttrRequired]
        public string FullName;

        [G9AttrRequired]
        public DateTime DateTime;

        public override G9DtConfigVersion ConfigVersion { set; get; } = new G9DtConfigVersion(9, 6, 3, 1);
    }
}