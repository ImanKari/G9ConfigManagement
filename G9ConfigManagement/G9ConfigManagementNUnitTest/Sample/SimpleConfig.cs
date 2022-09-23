using G9ConfigManagement.Abstract;
using G9ConfigManagement.DataType;

namespace G9ConfigManagementNUnitTest.Sample
{
    public class SimpleConfig:G9AConfigStructure<SimpleConfig>
    {
        public string A { set; get; } = "A";

        public int B { set; get; } = 0;

        /// <inheritdoc />
        public override G9DtConfigVersion ConfigVersion { get; set; } 
            = new G9DtConfigVersion(9, 6, 3, 1);
    }
}