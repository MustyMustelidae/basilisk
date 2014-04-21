using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RemoteLibrary.Properties;
using RemoteLibrary.Util;
using RemoteLibrary.Util.Cached;

namespace RemoteLibrary.Tests
{
    public abstract class GenericCachedTypeInfoResolverTests<T> where T : ICachedTypeInfoResolver
    {
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

        public abstract CachedTypeInfoResolver GetNewInfoResolver();

        [Test]
        public void CanResolveAllPropertyMethodsForType()
        {
            var expectedMethods = new List<MethodInfo>();
            //Getters
            expectedMethods.AddRange(TestType
                .GetProperties()
                .Select(propertyInfo => propertyInfo.GetGetMethod())
                .Where(getMethod => getMethod != null));
            //Setters
            expectedMethods.AddRange(TestType
                .GetProperties()
                .Select(propertyInfo => propertyInfo.GetSetMethod())
                .Where(setMethod => setMethod != null));

            var resolver = GetNewInfoResolver();
            var methods = resolver.GetTypePropertyMethods(TestType, true, true);
            CollectionAssert.AreEquivalent(expectedMethods, methods);
        }

        [Test]
        public void CanResolveMethodWithNoReturnValueAndNoParam()
        {
            var testMethodName = VoidMethodWithNoParam;
            var expectedMethod = TestType.GetMethod(testMethodName);
            var resolver = GetNewInfoResolver();
            var method = resolver.GetMethodByName(TestType, testMethodName);
            Assert.AreEqual(expectedMethod, method);
        }

        [Test]
        public void CanResolveMethodWithReturnValueAndNoParam()
        {
            var testMethodName = StringMethodWithNoParam;
            var expectedMethod = TestType.GetMethod(testMethodName);
            var resolver = GetNewInfoResolver();
            var method = resolver.GetMethodByName(TestType, testMethodName);
            Assert.AreEqual(expectedMethod, method);
        }

        [Test]
        public void CanResolvePropertyGetterMethodsForType()
        {
            var expectedMethods = TestType
                .GetProperties()
                .Select(propertyInfo => propertyInfo.GetGetMethod())
                .Where(getMethod => getMethod != null);

            var resolver = GetNewInfoResolver();
            var methods = resolver.GetTypePropertyMethods(TestType, true, false);
            CollectionAssert.AreEquivalent(expectedMethods, methods);
        }

        [Test]
        public void CanResolvePropertySetterMethodsForType()
        {
            var expectedMethods = TestType
                .GetProperties()
                .Select(propertyInfo => propertyInfo.GetSetMethod())
                .Where(setMethod => setMethod != null);

            var resolver = GetNewInfoResolver();
            var methods = resolver.GetTypePropertyMethods(TestType, false, true);
            CollectionAssert.AreEquivalent(expectedMethods, methods);
        }

        [Test]
        public void CanResolvePropertyWithGetter()
        {
            var testPropName = PropertyWithGetter;
            var expectedMethod = TestType.GetProperty(testPropName).GetGetMethod();
            var resolver = GetNewInfoResolver();
            var method = resolver.GetTypePropertyMethodByName(TestType, testPropName, true);
            Assert.AreEqual(expectedMethod, method);
        }

        [Test]
        public void CanResolvePropertyWithGetterAndSetter()
        {
            var testPropName = PropertyWithGetterSetter;

            var expectedGetMethod = TestType.GetProperty(testPropName)
                .GetGetMethod();
            var expectedSetMethod = TestType.GetProperty(testPropName)
                .GetSetMethod();

            var resolver = GetNewInfoResolver();

            var setMethod = resolver
                .GetTypePropertyMethodByName(TestType, testPropName, false);

            var getMethod = resolver
                .GetTypePropertyMethodByName(TestType, testPropName, true);
            Assert.AreEqual(expectedGetMethod, getMethod);
            Assert.AreEqual(expectedSetMethod, setMethod);
        }

        [Test]
        public void CanResolvePropertyWithSetter()
        {
            var testPropName = PropertyWithSetter;
            var expectedMethod = TestType.GetProperty(testPropName).GetSetMethod();
            var resolver = GetNewInfoResolver();
            var method = resolver.GetTypePropertyMethodByName(TestType, testPropName, false);
            Assert.AreEqual(expectedMethod, method);
        }

        [Test]
        public void CanResolveTypeMemberMethods()
        {
            var expectedMethods = TestType.GetMethods();
            var resolver = GetNewInfoResolver();
            var methods = resolver.GetTypeMethods(TestType);
            CollectionAssert.AreEquivalent(expectedMethods, methods);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GetMethodByNameThrowsExceptionOnNullMethodName()
        {
            var resolver = GetNewInfoResolver();
            resolver.GetMethodByName(TestType, null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GetMethodByNameThrowsExceptionOnNullType()
        {
            var resolver = GetNewInfoResolver();
            resolver.GetMethodByName(null, VoidMethodWithNoParam);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GetTypeMethodsThrowsExceptionOnNullType()
        {
            var resolver = GetNewInfoResolver();
            resolver.GetTypeMethods(null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GetTypePropertiesThrowsExceptionOnNullType()
        {
            var resolver = GetNewInfoResolver();
            resolver.GetTypeProperties(null, true, true);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GetTypePropertyInfoByNameThrowsExceptionOnNullMethodName()
        {
            var resolver = GetNewInfoResolver();
            resolver.GetTypePropertyInfoByName(TestType, null, true, true);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GetTypePropertyInfoByNameThrowsExceptionOnNullType()
        {
            var resolver = GetNewInfoResolver();
            resolver.GetTypePropertyInfoByName(null, PropertyWithGetterSetter, true, true);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GetTypePropertyMethodByNameThrowsExceptionOnNullPropertyName()
        {
            var resolver = GetNewInfoResolver();
            resolver.GetTypePropertyMethodByName(TestType, null, true);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GetTypePropertyMethodByNameThrowsExceptionOnNullType()
        {
            var resolver = GetNewInfoResolver();
            resolver.GetTypePropertyMethodByName(null, PropertyWithGetterSetter, true);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GetTypePropertyMethodsThrowsExceptionOnNullType()
        {
            var resolver = GetNewInfoResolver();
            resolver.GetTypePropertyMethods(null, true, true);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void RegisterIfUnregisteredThrowsExceptionOnNullType()
        {
            var resolver = GetNewInfoResolver();
            resolver.RegisterIfUnregistered(null);
        }

        private class TestClass
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
    }
}