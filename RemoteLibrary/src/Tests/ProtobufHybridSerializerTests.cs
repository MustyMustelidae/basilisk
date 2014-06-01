using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using ProtoBuf;
using ProtoBuf.Meta;
using RemoteLibrary.Messages;
using RemoteLibrary.Messages.Values;
using RemoteLibrary.Serialization;

namespace RemoteLibrary.Tests
{
    public class ProtobufHybridSerializerTests : GenericRemoteInterfaceSerializerTests<ProtobufHybridSerializer>
    {
        [ProtoContract]
        public class ContractTestClass
        {
            public ContractTestClass()
            {
            }

            public ContractTestClass(string testField)
            {
                TestField = testField;
            }

            [ProtoMember(1)]
            public string TestField { get; set; }
        }

        static ProtobufHybridSerializerTests()
        {
            RuntimeTypeModel.Default.Add(typeof (ContractTestClass), true);
            var messageProtoModel = RuntimeTypeModel.Default[typeof (BaseRpcMessage)];
            var maxFieldNumber = messageProtoModel.GetSubtypes()
                .Select(type => type.FieldNumber)
                .Max() + 100;
            messageProtoModel.AddSubType(maxFieldNumber,
                typeof (TestBaseRemoteProxyInvocationMessage));
        }

        protected override ProtobufHybridSerializer GetNewInterfaceSerializer()
        {
            return new ProtobufHybridSerializer();
        }

        [Test]
        public void CanSerializeThenDeserializeContractArgument()
        {
            var serializer = GetNewInterfaceSerializer();
            Debug.Assert(!serializer.IsBinaryType(typeof (ContractTestClass)));
            const string testString = "HelloWorld";
            var testObject = new ContractTestClass(testString);
            var serializedTestObject = serializer.SerializeArgumentObject(testObject);
            var argument = new SerializedRpcValue
            {
                ArgumentBytes = serializedTestObject,
                ArgumentType = typeof (ContractTestClass)
            };


            var deserializedArgument = serializer.DeserializeArgumentToObject(argument) as ContractTestClass;
            Assert.NotNull(deserializedArgument);
            Assert.AreEqual(testObject.TestField, deserializedArgument.TestField);
        }

        [Test]
        public void RecognizesContractType()
        {
            var serializer = GetNewInterfaceSerializer();
            var binaryType = serializer.IsBinaryType(typeof (ContractTestClass));
            Assert.IsFalse(binaryType);
        }
    }
}