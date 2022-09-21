using G9ConfigManagement.Abstract;
using G9ConfigManagement.DataType;

namespace G9ConfigManagementNUnitTest.Sample
{
    public class SampleConfigForBindableItem : G9AConfigStructure<SampleConfigForBindableItem>
    {
        public string A = "G9";
        public int B = 99;
        public G9DtBindableMember<string> BindableDataForTest = new G9DtBindableMember<string>("G9TM");


        public override G9DtConfigVersion ConfigVersion { set; get; } = new G9DtConfigVersion(1, 3, 6, 9);
    }
}