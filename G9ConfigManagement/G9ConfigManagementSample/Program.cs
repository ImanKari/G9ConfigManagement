using System;

namespace G9ConfigManagementSample
{
    internal class Program
    {
        private static void Main()
        {
            var config = SampleConfig.GetConfig();
            config.CustomBindableMember.OnChangeValue += (newValue, oldValue) =>
            {
                Console.WriteLine($"Old Value: {oldValue} | New Value: {newValue}");
            };
            Console.WriteLine(config.CustomBindableMember.CurrentValue);
            config.CustomBindableMember.SetNewValue("New Value");
            Console.ReadLine();
        }
    }
}