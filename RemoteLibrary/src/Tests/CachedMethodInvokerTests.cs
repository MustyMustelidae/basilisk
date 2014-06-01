using System;
using System.Reflection;
using Moq;
using NUnit.Framework;
using RemoteLibrary.Properties;
using RemoteLibrary.Util.Cached;

namespace RemoteLibrary.Tests
{
    [TestFixture]
    public class CachedMethodInvokerTests
    {
        public interface ITestClass
        {
            [UsedImplicitly]
            string PropertyWithGetter { get; }

            [UsedImplicitly]
            string PropertyWithSetter { set; }

            [UsedImplicitly]
            string PropertyWithGetterSetter { get; set; }

            [UsedImplicitly]
            void VoidMethodWithNoParam();

            [UsedImplicitly]
            void VoidMethodWithOneParam(string param);

            [UsedImplicitly]
            string StringMethodWithOneParam(string param);

            [UsedImplicitly]
            string StringMethodWithNoParam();
        }

        private class TestClass : ITestClass
        {
            [UsedImplicitly] private string _someSettableProp;

            [UsedImplicitly]
            public string PropertyWithGetter
            {
                get { return TestReturnValue; }
            }

            [UsedImplicitly]
            public string PropertyWithSetter
            {
                set { _someSettableProp = value; }
            }

            [UsedImplicitly]
            public string PropertyWithGetterSetter { get; set; }

            [UsedImplicitly]
            public void VoidMethodWithNoParam()
            {
            }

            [UsedImplicitly]
            public void VoidMethodWithOneParam(string param)
            {
            }

            [UsedImplicitly]
            public string StringMethodWithOneParam(string param)
            {
                return param;
            }

            [UsedImplicitly]
            public string StringMethodWithNoParam()
            {
                return TestReturnValue;
            }
        }

        private const string TestReturnValue = "TestReturnVal";


        private static string VoidMethodWithNoParam
        {
            get { return "VoidMethodWithNoParam"; }
        }

        private static string VoidMethodWithOneParam
        {
            get { return "VoidMethodWithOneParam"; }
        }

        private static string StringMethodWithNoParam
        {
            get { return "StringMethodWithNoParam"; }
        }

        private static string StringMethodWithOneParam
        {
            get { return "StringMethodWithOneParam"; }
        }

        private static string PropertyWithGetter
        {
            get { return "PropertyWithGetter"; }
        }

        private static string PropertyWithSetter
        {
            get { return "PropertyWithSetter"; }
        }

        private static string PropertyWithGetterSetter
        {
            get { return "PropertyWithGetterSetter"; }
        }

        private static Type TestType
        {
            get { return typeof (TestClass); }
        }

        private MethodInfo DefaultTestMethodInfo
        {
            get { return TestType.GetMethod(VoidMethodWithNoParam); }
        }

        private static CachedMethodInvoker GetInvoker()
        {
            return new CachedMethodInvoker();
        }

        private static TestClass GetTestObject()
        {
            return new TestClass();
        }

        [Test]
        public void CanInvokePropertyWithGetter()
        {
            var testMock = new Mock<ITestClass>();

            testMock.SetupGet(testObject => testObject.PropertyWithGetter)
                .Returns(TestReturnValue)
                .Verifiable();

            var invoker = GetInvoker();

            var testType = typeof (ITestClass);

            var testInfo = testType.GetProperty(PropertyWithGetter);
            var returnVal = invoker.Invoke(TestType, testMock.Object, testInfo.GetGetMethod(), null) as string;
            testMock.VerifyGet(testObject => testObject.PropertyWithGetter, Times.Once);
            StringAssert.AreEqualIgnoringCase(returnVal, TestReturnValue);
        }

        [Test]
        public void CanInvokePropertyWithGetterAndSetter()
        {
            var testMock = new Mock<ITestClass>();


            testMock.SetupProperty(testObject => testObject.PropertyWithGetterSetter)
                .SetupSet(testObject => testObject.PropertyWithGetterSetter = TestReturnValue)
                .Verifiable();

            testMock.SetupProperty(testObject => testObject.PropertyWithGetterSetter)
                .SetupGet(testObject => testObject.PropertyWithGetterSetter)
                .Returns(TestReturnValue)
                .Verifiable();

            var invoker = GetInvoker();

            var testType = typeof (ITestClass);

            var testInfo = testType.GetProperty(PropertyWithGetterSetter);
            invoker.Invoke(TestType, testMock.Object, testInfo.GetSetMethod(), new object[] {TestReturnValue});

            testMock.VerifySet(testObject => testObject.PropertyWithGetterSetter = TestReturnValue, Times.Once);


            var returnVal = invoker.Invoke(TestType, testMock.Object, testInfo.GetGetMethod(), null) as string;

            testMock.VerifyGet(testObject => testObject.PropertyWithGetterSetter, Times.Once);

            StringAssert.AreEqualIgnoringCase(TestReturnValue, returnVal);
        }

        [Test]
        public void CanInvokePropertyWithSetter()
        {
            var testMock = new Mock<ITestClass>();

            testMock.SetupSet(testObject => testObject.PropertyWithSetter = TestReturnValue)
                .Verifiable();

            var invoker = GetInvoker();

            var testType = typeof (ITestClass);

            var testInfo = testType.GetProperty(PropertyWithSetter);
            invoker.Invoke(TestType, testMock.Object, testInfo.GetSetMethod(), new object[] {TestReturnValue});
            testMock.VerifySet(testObject => testObject.PropertyWithSetter = TestReturnValue, Times.Once);
        }

        [Test]
        public void CanInvokeWithReturnValue()
        {
            var testMock = new Mock<ITestClass>();

            testMock.Setup(testObject => testObject.StringMethodWithNoParam()).Returns(TestReturnValue)
                .Verifiable();

            var invoker = GetInvoker();

            var testType = typeof (ITestClass);

            var testInfo = testType.GetMethod(StringMethodWithNoParam);
            var returnVal = invoker.Invoke(TestType, testMock.Object, testInfo, null) as string;
            testMock.Verify(testObject => testObject.StringMethodWithNoParam(), Times.Once);

            StringAssert.AreEqualIgnoringCase(returnVal, TestReturnValue);
        }

        [Test]
        public void CanInvokeWithReturnValueAndParam()
        {
            var testMock = new Mock<ITestClass>();

            testMock.Setup(testObject => testObject.StringMethodWithOneParam(TestReturnValue)).Returns(TestReturnValue)
                .Verifiable();

            var invoker = GetInvoker();

            var testType = typeof (ITestClass);

            var testInfo = testType.GetMethod(StringMethodWithOneParam);
            var returnVal =
                invoker.Invoke(TestType, testMock.Object, testInfo, new object[] {TestReturnValue}) as string;
            testMock.Verify(testObject => testObject.StringMethodWithOneParam(TestReturnValue), Times.Once);

            StringAssert.AreEqualIgnoringCase(returnVal, TestReturnValue);
        }

        [Test]
        public void CanInvokeWithoutReturnValue()
        {
            var testMock = new Mock<ITestClass>();

            testMock.Setup(testObject => testObject.VoidMethodWithNoParam())
                .Verifiable();

            var invoker = GetInvoker();

            var testType = typeof (ITestClass);

            var testInfo = testType.GetMethod(VoidMethodWithNoParam);
            invoker.Invoke(TestType, testMock.Object, testInfo, null);
            testMock.Verify(testObject => testObject.VoidMethodWithNoParam(), Times.Once);
        }

        [Test]
        public void CanInvokeWithoutReturnValueWithParam()
        {
            var testMock = new Mock<ITestClass>();

            testMock.Setup(testObject => testObject.VoidMethodWithOneParam(TestReturnValue))
                .Verifiable();

            var invoker = GetInvoker();

            var testType = typeof (ITestClass);

            var testInfo = testType.GetMethod(VoidMethodWithOneParam);
            invoker.Invoke(TestType, testMock.Object, testInfo, new object[] {TestReturnValue});
            testMock.Verify(testObject => testObject.VoidMethodWithOneParam(TestReturnValue), Times.Once);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ThrowsExceptionOnNullMethodInfo()
        {
            var testObject = GetTestObject();
            var invoker = GetInvoker();
            invoker.Invoke(TestType, testObject, null, null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ThrowsExceptionOnNullTargetForNonStaticMethod()
        {
            var testObject = GetTestObject();
            var invoker = GetInvoker();
            var testType = testObject.GetType();
            var testInfo = testType.GetMethod(VoidMethodWithOneParam);
            invoker.Invoke(TestType, null, testInfo, null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ThrowsExceptionOnNullType()
        {
            var testObject = GetTestObject();
            var invoker = GetInvoker();
            invoker.Invoke(null, testObject, DefaultTestMethodInfo, null);
        }
    }
}