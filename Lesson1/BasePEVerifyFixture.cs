using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace Lesson1
{
    public abstract class BasePEVerifyFixture
    {
        private static readonly List<string> _disposalList = new List<string>();

        [SetUp]
        public void Init()
        {
            OnInit();
        }

        [TearDown]
        public void Term()
        {
            OnTerm();

            lock (_disposalList)
            {
                // Delete the files tagged for removal
                foreach (string file in _disposalList)
                {
                    if (!File.Exists(file))
                        continue;

                    File.Delete(file);
                }
                _disposalList.Clear();
            }
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnTerm()
        {
        }

        protected static void AutoDelete(string filename)
        {
            if (_disposalList.Contains(filename) || !File.Exists(filename))
                return;

            _disposalList.Add(filename);
        }

        protected void PEVerify(string assemblyLocation)
        {
            var pathKeys = new[]
                               {
                                   "sdkDir",
                                   "x86SdkDir",
                                   "sdkDirUnderVista"
                               };

            var process = new Process();
            string peVerifyLocation = string.Empty;


            peVerifyLocation = GetPEVerifyLocation(pathKeys, peVerifyLocation);

            if (!File.Exists(peVerifyLocation))
            {
                Console.WriteLine("Warning: PEVerify.exe could not be found. Skipping test.");
                
                return;
            }

            process.StartInfo.FileName = peVerifyLocation;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            process.StartInfo.Arguments = "\"" + assemblyLocation + "\" /VERBOSE";
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string processOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            string result = string.Format("PEVerify Exit Code: {0}", process.ExitCode);

            Console.WriteLine(GetType().FullName + ": " + result);

            if (process.ExitCode == 0)
                return;

            Console.WriteLine(processOutput);
            Assert.Fail("PEVerify output: " + Environment.NewLine + processOutput, result);
        }

        private static string GetPEVerifyLocation(IEnumerable<string> pathKeys, string peVerifyLocation)
        {
            foreach (string key in pathKeys)
            {
                string directory = ConfigurationManager.AppSettings[key];

                if (string.IsNullOrEmpty(directory))
                    continue;

                peVerifyLocation = Path.Combine(directory, "peverify.exe");

                if (File.Exists(peVerifyLocation))
                    break;
            }
            return peVerifyLocation;
        }
    }
}
