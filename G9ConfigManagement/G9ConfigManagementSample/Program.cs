using System;

namespace G9ConfigManagementSample
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Hello World!");
            var config = SampleConfig.GetConfig();
            Console.WriteLine(config.ApplicationName); // My Custom Application
            Console.WriteLine(config.CustomBindableMember); // Bindable Member
            Console.WriteLine(config.Color); // DarkMagenta
            // ...

            config.CustomBindableMember.OnChangeValue += (newValue, oldValue) =>
            {
                Console.WriteLine($"Old Value: {oldValue} | New Value: {newValue}");
            };
            config.CustomBindableMember.SetNewValue();
            Console.ReadLine();
        }
    }
}