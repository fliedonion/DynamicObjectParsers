using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;
using Case_of_t.net.DynamicParsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Case_of_t.net.DynamicParsers {

    [TestClass]
    public class TestXElementDynamicParser {

        [TestClass]
        public class CanParse {
            public TestContext TestContext { get; set; }
            private XElementDynamicParser parser;

            [TestInitialize]
            public void BeforeEach() {
                parser = new XElementDynamicParser();
            }

            [TestMethod]
            [TestCase(@"<a>12</a>", 12)]
            public void TopElementValue() {
                TestContext.Run((string a, object b) => {
                    dynamic x = parser.Parse(XElement.Parse(a));
                    (x.Value as object).Is(b);
                });
            }

            [TestMethod]
            [TestCase(@"<a><![CDATA[abc]]></a>", "abc")]
            [TestCase(@"<a><![CDATA[ab]]><![CDATA[c]]></a>", "abc")]
            public void TopElementCdata() {
                TestContext.Run((string a, object b) => {
                    dynamic x = parser.Parse(XElement.Parse(a));
                    (x.Value as object).Is(b);
                });
            }

            [TestMethod]
            [TestCase(@"<a test=""testattr""></a>", "testattr")]
            public void TopElementAttribute() {
                TestContext.Run((string src, object exp) => {
                    dynamic x = parser.Parse(XElement.Parse(src));
                    (x.test as object).Is(exp);
                });
            }

            [TestMethod]
            [TestCase(@"<a test=""testattr"">12<b><c> <cd><e><f attrF=""attrF"">F</f><f attrF=""attrF"">F</f></e></cd> <cA><B><C attrC=""attrC"">C</C></B></cA> </c></b></a>", "testattr", 12, "attrF", "F", "attrC", "C")]
            public void DeepElement() {
                TestContext.Run((string src, object av, object ev, object afv, object efv, object aCv, object eCv) => {
                    dynamic x = parser.Parse(XElement.Parse(src));
                    (x.test as object).Is(av);
                    (x.Value as object).Is(ev);
                    foreach (var f in x.b.c.cd.e.f) {
                        (f.attrF as object).Is(afv);
                        (f.Value as object).Is(efv);
                    }
                    (x.b.c.cA.B.C.attrC as object).Is(aCv);
                    (x.b.c.cA.B.C.Value as object).Is(eCv);
                });
            }


            [TestMethod]
            [TestCase(@"<a><b>1</b><b>2</b><b>3</b><b>4</b></a>", new object[]{1,2,3,4})]
            public void MultiElementsValue() {
                TestContext.Run((string src, object[] expects) => {
                    dynamic x = parser.Parse(XElement.Parse(src));
                    var expectIndex = 0;

                    foreach (var ele in x.b) {
                        (ele.Value as object).Is(expects[expectIndex]);
                        expectIndex++;
                    }
                });
            }

            [TestMethod]
            [TestCase(@"<a><b attr=""1""></b><b attr=""2""></b><b attr=""3""></b><b attr=""4""></b></a>", new object[] { 1, 2, 3, 4 })]
            public void MultiElementAttributes() {
                TestContext.Run((string src, object[] expects) => {
                    dynamic x = parser.Parse(XElement.Parse(src));
                    var expectIndex = 0;

                    foreach (var ele in x.b) {
                        (ele.attr as object).Is(expects[expectIndex]);
                        expectIndex++;
                    }
                });
            }
            

            [TestMethod]
            [TestCase(@"<a><b>12</b></a>", 12)]
            [TestCase(@"<a><b>1.2</b></a>", 1.2)]
            [TestCase(@"<a><b>1.213131</b></a>", 1.213131)]
            [TestCase(@"<a><b>1.2.</b></a>", "1.2.")]
            public void ValueWithDefault() {
                TestContext.Run((string a, object b) => {
                    var x = parser.Parse(XElement.Parse(a));
                    (x.b.Value as object).Is(b);
                });
            }




            [TestMethod]
            [TestCaseSource("DecimalIfCouldSource")]
            public void DecimalIfCould() {
                parser.SetAllTryParseOptionOff();
                parser.TryDecimalParse = true;

                TestContext.Run((string src, object exp) => {
                    dynamic x = parser.Parse(XElement.Parse(src));
                    (x.b.Value as object).Is(exp);
                    (x.b.at as object).Is(exp);
                });
            }

            public static object[] DecimalIfCouldSource = {
                new object[] {@"<a><b at=""12"">12</b></a>", 12m},
                new object[] {@"<a><b at=""1.2"">1.2</b></a>", 1.2m},
                new object[] {@"<a><b at=""1.2."">1.2.</b></a>", "1.2."},
            };



            [TestMethod]
            public void MultiAttributes() {
                var xml = XElement.Parse("<a><b attra=\"1\" attrb=\"abc\" attrc=\"1.5\"></b></a>");
                dynamic x = parser.Parse(xml);

                (x.b.attra as object).Is(1);
                (x.b.attrb as object).Is("abc");
                (x.b.attrc as object).Is(1.5);
            }



            [TestMethod]
            [TestCase("<a><b>1<a>12</a>2</b></a>", new object[] { 12, "1", "2"})]
            [TestCase("<a><b>1<a>12</a>\n2</b></a>", new object[] { "1\n2", "1", "\n2" })]
            [TestCase("<a><b>1\n<a>12</a>2</b></a>", new object[] { "1\n2", "1\n", "2" })]
            [TestCase("<a><b>1<a>12</a>\r\n2</b></a>", new object[] { "1\n2", "1", "\n2" })]
            [TestCase("<a><b>1\r\n<a>12</a>2</b></a>", new object[] { "1\n2", "1\n", "2" })]
            [TestCase("<a><b><![CDATA[1]]><a>12</a><![CDATA[2]]></b></a>", new object[] { 12, "1", "2" })]
            public void ElementValues() {

                TestContext.Run((string src, object[] exp)=>{
                    dynamic x = parser.Parse(XElement.Parse(src));
                    (x.b.Value as object).Is(exp[0]);
                    (x.b.Values[0] as object).Is(exp[1]);
                    (x.b.Values[1] as object).Is(exp[2]);
                });
            }

            [TestMethod]
            [TestCase("<a><b>1<a>12</a>2</b></a>", new object[] { "1 2", "1", "2" })]
            [TestCase("<a><b>1<a>12</a>\n2</b></a>", new object[] { "1 \n2", "1", "\n2" })]
            [TestCase("<a><b>1\n<a>12</a>2</b></a>", new object[] { "1\n 2", "1\n", "2" })]
            public void ElementValueSeparatorSeparatesValue() {

                parser.ElementValueSeparator = " ";

                TestContext.Run((string src, object[] exp) => {
                    dynamic x = parser.Parse(XElement.Parse(src));
                    (x.b.Value as object).Is(exp[0]);
                    (x.b.Values[0] as object).Is(exp[1]);
                    (x.b.Values[1] as object).Is(exp[2]);
                });

            }

            [TestMethod]
            [TestCase(@"<a-b><b>12</b></a-b>", 12)]
            public void NameWithUnusableCharForCSharpVariable() {
                TestContext.Run((string a, object b) => {
                    var x = parser.Parse(XElement.Parse(a));
                    (x.b.Value as object).Is(b);
                });
            }

            [TestMethod]
            [TestCase(@"<a-b><b.x data.attr=""x"">12</b.x></a-b>", "b.x", "data.attr", 12, "x")]
            [TestCase(@"<a-b><b-x data-attr=""x"">12</b-x></a-b>", "b-x", "data-attr", 12, "x")]
            public void NameWithUnusableCharForCSharpVariableCanAccessThroughIDictionary() {
                TestContext.Run((string xml, string ename, string aname, object expE, object expA) => {
                    var x = parser.Parse(XElement.Parse(xml)) as IDictionary<string, object>;
                    (((dynamic)(x[ename])).Value as object).Is(expE);
                    ((IDictionary<string, object>)x[ename])[aname].Is(expA);

                });
            }

        }

        [TestClass]
        public class TryParsePriority {
            public TestContext TestContext { get; set; }
            private XElementDynamicParser parser;
            [TestInitialize]
            public void BeforeEach() {
                parser = new XElementDynamicParser();
                parser.SetAllTryParseOptionOn();
            }

            [TestMethod]
            [TestCase(@"<a><b at=""1.2"">1.2</b></a>", 1.2f)]
            [TestCase(@"<a><b at=""1.213131"">1.213131</b></a>", 1.213131f)]
            [TestCase(@"<a><b at=""0.666666666666666"">0.666666666666666</b></a>", 0.666666666666666f)]
            [TestCase(@"<a><b at=""1.2."">1.2.</b></a>", "1.2.")]
            public void FloatHaveHigherPriorityThanDouble() {
                
                TestContext.Run((string src, object exp) => {
                    dynamic x = parser.Parse(XElement.Parse(src));
                    (x.b.Value as object).Is(exp);
                    (x.b.at as object).Is(exp);
                });
            }

            [TestMethod]
            [TestCase(@"<a><b at=""12"">12</b></a>", (byte)12)]
            [TestCase(@"<a><b at=""255"">255</b></a>", (byte)255)]
            [TestCase(@"<a><b at=""256"">256</b></a>", 256)]
            public void ByteHaveHigherPriorityThanInt() {
                TestContext.Run((string src, object exp) => {
                    dynamic x = parser.Parse(XElement.Parse(src));
                    (x.b.Value as object).Is(exp);
                    (x.b.at as object).Is(exp);
                });
            }
        }

        [TestClass]
        public class ResolveNameConflictsWhenSuppressPrefix {
            public TestContext TestContext { get; set; }
            private XElementDynamicParser parser;

            [TestInitialize]
            public void BeforeEach() {
                parser = new XElementDynamicParser();
            }

            [TestMethod]
            public void AttributePriorThanSameNameChildElement() {
                var xml = XElement.Parse("<a><b a=\"1\"><a>12</a></b></a>");
                dynamic x = parser.Parse(xml);
                (x.b.a as object).Is(1);
            }


            [TestMethod]
            public void ElementValuePriorThanAttributeAndChildThatHaveSameName() {
                var xml = XElement.Parse("<a><b Value=\"abdefg\">PROIR<Value>xyz</Value></b></a>");
                dynamic x = parser.Parse(xml);
                (x.b.Value as object).Is("PROIR");
            }
        }

        [TestClass]
        public class ResolveNameConflictsWithPrefix {
            public TestContext TestContext { get; set; }
            private XElementDynamicParser parser;

            [TestInitialize]
            public void BeforeEach() {
                parser = new XElementDynamicParser();
                parser.AppendPrefixToAvoidNameConflictBetweenElementAndAttribute = true;
            }

            [TestMethod]
            public void EachItemsHavePrefix() {
                var xml = XElement.Parse("<a><b Value=\"abdefg\">opq<Value>xyz</Value>RST</b></a>");
                dynamic x = parser.Parse(xml);
                (x.eb.Value as object).Is("opqRST", "Element Value");
                (x.eb.pValue as object).Is("abdefg", "Attribute(prop's p)");
                (x.eb.eValue.Value as object).Is("xyz", "ChildElement Value");
            }
        }

    }

}
