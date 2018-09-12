

using HBM.WT.API.WTX;
using HBM.WT.API.WTX.Jet;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetbusTest
{
    [TestFixture]
    public class CommentMethodsTests
    {
        private TestJetbusConnection _jetTestConnection;
        private WtxJet _wtxObj;

        private int value;

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable T_UnitValueTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.t_UnitValue_Fail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.t_UnitValue_Success).ExpectedResult = 1;

            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable KG_UnitValueTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.kg_UnitValue_Fail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.kg_UnitValue_Success).ExpectedResult = 1;

            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable G_UnitValueTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.g_UnitValue_Fail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.g_UnitValue_Success).ExpectedResult = 1;

            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable LB_UnitValueTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.lb_UnitValue_Fail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.lb_UnitValue_Success).ExpectedResult = 1;

            }
        }

        [SetUp]
        public void Setup()
        {
            value = 0;
        }
     

        [Test, TestCaseSource(typeof(CommentMethodsTests), "T_UnitValueTestCases")]
        public void testUnit_t(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            value = _wtxObj.Unit;

            Assert.AreEqual("t", _wtxObj.UnitStringComment());
        }     

        /*
        [Test, TestCaseSource(typeof(CommentMethodsTests), "KG_UnitValueTestCases")]
        public void testUnit_kg(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            value = _wtxObj.Unit;

            Assert.AreEqual("kg", _wtxObj.UnitStringComment());
        }

        [Test, TestCaseSource(typeof(CommentMethodsTests), "G_UnitValueTestCases")]
        public void testUnit_g(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            value = _wtxObj.Unit;

            Assert.AreEqual("g", _wtxObj.UnitStringComment());
        }

        [Test, TestCaseSource(typeof(CommentMethodsTests), "LB_UnitValueTestCases")]
        public void testUnit_lb(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            value = _wtxObj.Unit;

            Assert.AreEqual("lb", _wtxObj.UnitStringComment());
        }
        */

        // A6 = lb = 0x530000 = 10100110000000000000000
        // 02 = kg = 0x20000  = 100000000000000000
        // 4B = g  = 0x4B0000 = 10010110000000000000000
        // 4C = t  = 0x4C0000 = 10011000000000000000000

        private void OnConnect(bool obj)
        {
            throw new NotImplementedException();
        }
    }
}
