using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using G9AssemblyManagement;
using G9AssemblyManagement.DataType;
using G9ConfigManagement.Attributes;
using G9ConfigManagement.DataType;
using G9ConfigManagementNUnitTest.Sample;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace G9ConfigManagementNUnitTest
{
    /// <summary>
    ///     Test Class
    /// </summary>
    public class G9ConfigManagementUnitTest
    {

        private readonly string configPath =
#if (NET35 || NET40 || NET45)
            AppDomain.CurrentDomain.BaseDirectory;
#else
                        AppContext.BaseDirectory;
#endif

        /// <summary>
        ///     Helper method
        /// </summary>
        private void TestEquality(SampleConfig sample, SampleConfig target)
        {
            // Check config information
            Assert.True(target.ConfigInformation.ConfigFileName == nameof(SampleConfig) &&
                        target.ConfigInformation.ConfigFileExtension == "json" &&
                        target.ConfigInformation.ConfigPath == configPath

                        && target.ConfigInformation.ConfigFullPath == 
                        Path.Combine(configPath,
                            $"{nameof(SampleConfig)}.{target.ConfigInformation.ConfigFileExtension}")
            );

            // Check version
            Assert.AreEqual(sample.ConfigVersion, target.ConfigVersion);

            // Check values
            Assert.True(sample.UserName == target.UserName && sample.TestTimeSpan == target.TestTimeSpan &&
                        sample.Password == target.Password && sample.TypeOfUser == target.TypeOfUser &&
                        sample.ConfigVersion == target.ConfigVersion);

            // Check child values
            Assert.True(sample.SubConfig.Active == target.SubConfig.Active &&
                        sample.SubConfig.SaveTime == target.SubConfig.SaveTime &&
                        sample.SubConfig.StartDateTime == target.SubConfig.StartDateTime &&
                        sample.SubConfig.SampleSubTwo.Active == target.SubConfig.SampleSubTwo.Active &&
                        sample.SubConfig.SampleSubTwo.SaveTime == target.SubConfig.SampleSubTwo.SaveTime);
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Order(1)]
        public void InitializeConfig()
        {
            // Delete if file created before
            var fullPath = Path.Combine(configPath, $"{nameof(SampleConfig)}.json");
             if (File.Exists(fullPath))
                File.Delete(fullPath);

            // Initialize config file
            var config = SampleConfig.GetConfig();

            // Initialize config
            var sampleConfig = new SampleConfig();

            // Test values
            TestEquality(sampleConfig, config);

            // #### Test data in config file ####
            var configFileText = File.ReadAllText(fullPath);
            // Tests comment for a encrypted member.
            Assert.True(configFileText.Contains("/* Specifies an encrypted password (Algorithm: PKCS7/CBC,128). */"));
            // Tests password as an encrypted member.
            Assert.True(configFileText.Contains("\"Password\": \"a1wghMPO+eLGdANrVWrxcQ==\""));
            // Tests multi-line comments.
            Assert.True(configFileText.Contains("/* Specifies the type of user (multi-line comments) */\n\t/* Values: (Admin-Editor) */\n\t/* Default value: Admin */"));
            // Tests the config value
            Assert.True(configFileText.Contains("\"ConfigVersion\": \"9.6.3.1\""));
            // Tests a nested value in config
            Assert.True(configFileText.Contains("\"SaveTime\": 30,"));
            // Tests a second nested value in config
            Assert.True(configFileText.Contains("\"SaveTime\": 10,"));


        }

        [Test]
        [Order(2)]
        public void RestoreConfig()
        {
            // Initialize config file
            var config = SampleConfig.GetConfig();
            config.Password = "AAAAAAA";
            config.TypeOfUser = UserType.Admin;
            config.UserName = "BBBBBBB";
            SampleConfig.RestoreByConfigFile();

            // Initialize config
            var sampleConfig = new SampleConfig();

            // Test values
            TestEquality(sampleConfig, config);
        }

        [Test]
        [Order(3)]
        public void RemakeConfigFileByDefaultValue()
        {
            // Initialize config file
            SampleConfig.RemakeAndRestoreByDefaultValue();
            var configuration = SampleConfig.GetConfig();

            // Initialize config
            var sampleConfig = new SampleConfig();

            // Check config file name
            Assert.True(configuration.ConfigInformation.ConfigFileName == nameof(SampleConfig));

            // Check version
            Assert.True(configuration.ConfigVersion == sampleConfig.ConfigVersion);
        }

        [Test]
        [Order(4)]
        public void RemakeConfigFileByCustomValue()
        {
            // Initialize config file
            SampleConfig.RemakeAndRestoreByCustomValue(new SampleConfig
            {
                TestTimeSpan = TimeSpan.MaxValue,
                TypeOfUser = UserType.Admin,
                UserName = "#Iman#|#Kari#"
            });
            var configuration = SampleConfig.GetConfig();

            // Initialize config

            var sampleConfig = new SampleConfig();

            // Check config file name
            Assert.True(configuration.ConfigInformation.ConfigFileName == nameof(SampleConfig));

            // Check version
            Assert.True(configuration.ConfigVersion == sampleConfig.ConfigVersion);
        }

        [Test]
        [Order(5)]
        public void RemakeConfigFileByCurrentValue()
        {
            // Initialize config file
            var configuration = SampleConfig.GetConfig();
            configuration.UserName = "Kari Iman";
            configuration.TypeOfUser = UserType.Admin;
            configuration.SubConfig.SaveTime = 999999;
            configuration.RemakeConfigFileByCurrentValue();

            // Initialize config
            var sampleConfig = new SampleConfig();

            // Check config file name
            Assert.True(configuration.ConfigInformation.ConfigFileName == nameof(SampleConfig));

            // Check version
            Assert.True(configuration.ConfigVersion == sampleConfig.ConfigVersion);
        }

        [Test]
        [Order(6)]
        public void MultiThreadShockTest()
        {
            // Delete if file created before
            var fullPath = Path.Combine(configPath, $"{nameof(SampleConfig)}.json");
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            G9Assembly.PerformanceTools.MultiThreadShockTest(i =>
            {
                // Initialize config file
                var config = SampleConfig.GetConfig();

                // Test config methods

                config.RemakeConfigFileByCurrentValue();

                SampleConfig.RemakeAndRestoreByDefaultValue();

                SampleConfig.RemakeAndRestoreByCustomValue(new SampleConfig()
                {
                    Password = "&%&*$&$^(*"
                });

                SampleConfig.RestoreByConfigFile();
            }, 369);
        }

        [Test]
        [Order(7)]
        public void BindableValueTest()
        {
            // Delete if file created before
            var fullPath = Path.Combine(configPath, $"{nameof(SampleConfigForBindableItem)}.json");
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            // Flag
            var callBindableMemberOnChange = false;
            var changeCollection = new List<G9DtTuple<string>>();

            var config = SampleConfigForBindableItem.GetConfig();
            Assert.True(config.BindableDataForTest.CurrentValue == "G9TM");
            var type = config.BindableDataForTest.GetType();
            Assert.True(type.GetGenericTypeDefinition() == typeof(G9DtBindableMember<>));
            Assert.True(File.ReadAllText(fullPath)
                .Contains($"\"{nameof(config.BindableDataForTest)}\": \"G9TM\""));

            config.BindableDataForTest.OnChangeValue +=
                (newValue, oldValue) =>
                {
                    if (!callBindableMemberOnChange)
                        Assert.True(oldValue == "G9TM" && newValue == "Okay");
                    callBindableMemberOnChange = true;
                    changeCollection.Add(new G9DtTuple<string>(oldValue, newValue));
                };

            // The first change in the bindable member value happens by manual method.
            config.BindableDataForTest.SetNewValue("Okay");
            Assert.True(config.BindableDataForTest.CurrentValue == "Okay" && callBindableMemberOnChange);

            // The second change in the bindable member value happens by manual method.
            config.BindableDataForTest.SetNewValue("G9TM");
            Assert.True(config.BindableDataForTest.CurrentValue == "G9TM");

            // The member value change event doesn't occur because the file data doesn't have any change in the bindable member part.
            File.WriteAllText(fullPath, File.ReadAllText(fullPath, Encoding.UTF8) + "   ");
            Thread.Sleep(99);
            Assert.True(config.BindableDataForTest.CurrentValue == "G9TM");

            // The third change in the bindable member value happens by changing the bindable member part in the config file.
            File.WriteAllText(fullPath, File.ReadAllText(fullPath, Encoding.UTF8).Replace("G9TM", "NewG9TM"));
            Thread.Sleep(99);
            Assert.True(config.BindableDataForTest.CurrentValue == "NewG9TM");

            // Result
            Assert.True(changeCollection.Count == 3 &&
                        changeCollection[0].Item1 == "G9TM" && changeCollection[0].Item2 == "Okay" &&
                        changeCollection[1].Item1 == "Okay" && changeCollection[1].Item2 == "G9TM" &&
                        changeCollection[2].Item1 == "G9TM" && changeCollection[2].Item2 == "NewG9TM"
            );
        }

        [Test]
        [Order(7)]
        public void RequiredAttrTest()
        {
            try
            {
                var config = SampleConfigWithRequiredMember.GetConfig();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.True(e.Message ==
                            $"The member '{nameof(SampleConfigWithRequiredMember.DateTime)}' in the structure '{typeof(SampleConfigWithRequiredMember).FullName}' has the attribute '{nameof(G9AttrRequiredAttribute)}.' So, it can't be null or default.");
                Assert.True(e.InnerException?.Message ==
                            $"The member '{nameof(SampleConfigWithRequiredMember.FullName)}' in the structure '{typeof(SampleConfigWithRequiredMember).FullName}' has the attribute '{nameof(G9AttrRequiredAttribute)}.' So, it can't be null or default.");
            }
        }
    }
}