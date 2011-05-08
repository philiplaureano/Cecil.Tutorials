using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

namespace Lesson1
{
    public static class FakeConsole
    {
        private static int _callCount = 0;

        public static void ResetCallCount()
        {
            _callCount = 0;
        }

        public static int CallCount
        {
            get { return _callCount; }
        }

        public static void WriteLine(string text)
        {
            // Keep track of the number of times the console was called
            _callCount++;

            Console.Write("FakeConsole: ");
            Console.WriteLine(text);
        }
    }

    [TestFixture]
    public class SampleTutorialFixtureForRedirectingConsoleCalls : BaseFizzBuzzModificationFixture
    {
        protected override void OnInit()
        {
            FakeConsole.ResetCallCount();
        }

        [Test]
        public override void RunTest()
        {
            base.RunTest();

            // Make sure that the fake console was called at least once
            Assert.IsTrue(FakeConsole.CallCount > 0, "The modified IL should call the FakeConsole.WriteLine() method");
        }

        protected override void ModifyTargetMethod(MethodDefinition targetMethod)
        {
            // In this example, we are going to redirect all Console.WriteLine calls
            // to the FakeConsole.WriteLine() method
            var module = targetMethod.Module;
            var body = targetMethod.Body;

            // In order to use FakeConsole.WriteLine(), we need to use Cecil to "import"
            // the method into the modified assembly's module
            var writeLine = module.Import(typeof (FakeConsole).GetMethod("WriteLine"));

            var callInstructions = (from instruction in body.Instructions
                                   where instruction.OpCode == OpCodes.Call
                                   select instruction).ToArray();

            foreach(var instruction in callInstructions)
            {
                // Notice how replacing the old method with the FakeConsole.WriteLine() method
                // redirects all console calls to the FakeConsole class
                instruction.Operand = writeLine;
            }
        }
    }
}
