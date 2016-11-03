using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Case_of_t.net.DynamicParsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Case_of_t.net.DynamicParsers {
    [TestClass]
    public class TestXml1Has {
        public TestContext TestContext { get; set; }
        private XElementDynamicParser parser;

        [TestInitialize]
        public void BeforeEach() {
            parser = new XElementDynamicParser();
        }

        [TestMethod]
        public void ItemListHasAtLeastOneItem() {
            var xml = parser.Parse(XElement.Load("TestXml1.xml"));

            var o = xml.itemList.item as List<ExpandoObject>;
            o.Count.Is(x => x > 0);

        }

        [TestMethod]
        public void ItemListHasExpectCarolineData() {
            var xml = parser.Parse(XElement.Load("TestXml1.xml"));

            var o = xml.itemList.item as List<ExpandoObject>;
            o.Cast<dynamic>().Any(x => x.Name.Value == "Caroline").IsTrue();
        }

        [TestMethod]
        public void NestedListHasSameItemList() {
            var xml = parser.Parse(XElement.Load("TestXml1.xml"));

            var o = xml.itemList.item as List<ExpandoObject>;
            var n = xml.NestedList.itemList.item as List<ExpandoObject>;
            o.IsStructuralEqual(n);

            var itemList1 = parser.Parse(XElement.Load("TestXml1.xml").Element("itemList")).item as List<ExpandoObject>;
            var itemList2 = parser.Parse(XElement.Load("TestXml1.xml").Element("NestedList").Element("itemList")).item as List<ExpandoObject>;
            itemList1.IsStructuralEqual(itemList2);
            itemList1.IsNotNull();

        }

        [TestMethod]
        public void NestedListHasDifferenctPlaceList() {
            var xml = parser.Parse(XElement.Load("TestXml1.xml"));

            var o = xml.PlaceList.Place as List<ExpandoObject>;
            var n = xml.NestedList.PlaceList.Place as List<ExpandoObject>;
            o.IsNotStructuralEqual(n);

            var itemList1 = parser.Parse(XElement.Load("TestXml1.xml").Element("PlaceList")).Place as List<ExpandoObject>;
            var itemList2 = parser.Parse(XElement.Load("TestXml1.xml").Element("NestedList").Element("PlaceList")).Place as List<ExpandoObject>;
            itemList1.IsNotStructuralEqual(itemList2);
            itemList1.IsNotNull();
        }


    }
}
