using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;
using SampleLibrary;
using SampleLibrary.Interfaces;

namespace Lesson1
{
    [TestFixture]
    public abstract class BaseFizzBuzzModificationFixture : BasePEVerifyFixture
    {   
        [Test]
        public virtual void RunTest()
        {
            Run(ModifyTargetMethod);
        }

        protected void Run(Action<MethodDefinition> modifyTargetMethod)
        {
            var targetMethodName = "Print";
            var typeName = "FizzBuzz";
            var assemblyLocation = typeof(FizzBuzz).Assembly.Location;

            var assembly = AssemblyDefinition.ReadAssembly(assemblyLocation);
            var module = assembly.MainModule;
            var greeterType = (from t in module.Types
                               where t.Name == typeName
                               select t).First();

            // Modify the print method             
            var greeterMethod = (from m in greeterType.Methods
                                 where m.Name == targetMethodName
                                 select m).First();

            modifyTargetMethod(greeterMethod);

            // Save the assembly for diagnostic purposes
            assembly.Write("output.dll");

            // Run PEVerify and make sure that the assembly is valid
            PEVerify("output.dll");

            // Load the modified assembly into memory            
            ExecuteModifiedAssembly(assembly);
        }

        private void ExecuteModifiedAssembly(AssemblyDefinition targetAssembly)
        {
            var memoryStream = new MemoryStream();
            targetAssembly.Write(memoryStream);

            // Convert the modified assembly into
            // an assembly that will be loaded by System.Reflection
            var bytes = memoryStream.GetBuffer();
            var assembly = Assembly.Load(bytes);
            var modifiedGreeterType = assembly.GetTypes()[0];
            var printer = (IPrinter)Activator.CreateInstance(modifiedGreeterType);
            
            for(var i = 0; i < 100; i++)
            {
                printer.Print(i);
            }
        }

        // Implement this method to modify the FizzBuzz.Print() method and
        // see how it affects the original assembly
        protected abstract void ModifyTargetMethod(MethodDefinition targetMethod);
    }
}
