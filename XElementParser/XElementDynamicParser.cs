using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Case_of_t.net.DynamicParsers {
    public sealed class XElementDynamicParser {
        private byte tryByteOut;
        private int tryIntOut;
        private long tryLongOut;
        private float tryFloatOut;
        private double tryDoubleOut;
        private decimal tryDecimalOut;

        public string ElementValueSeparator { get; set; }
        public bool ElementValueAlwaysString { get; set; }

        private bool SuppressAttrElePrefix {
            get { return !AppendPrefixToAvoidNameConflictBetweenElementAndAttribute; }
        }

        /// <summary>
        /// default <c>false</c>
        /// </summary>
        public bool AppendPrefixToAvoidNameConflictBetweenElementAndAttribute { get; set; }

        [DefaultValue(false)] // for only indicate in xmlcomment document
        public bool TryByteParse { get; set; }
        [DefaultValue(true)] // for only indicate in xmlcomment document
        public bool TryIntParse { get; set; }
        [DefaultValue(false)] // for only indicate in xmlcomment document
        public bool TryLongParse { get; set; }
        [DefaultValue(false)] // for only indicate in xmlcomment document
        public bool TryFloatParse { get; set; }
        [DefaultValue(true)] // for only indicate in xmlcomment document
        public bool TryDoubleParse { get; set; }
        [DefaultValue(false)] // for only indicate in xmlcomment document
        public bool TryDecimalParse { get; set; }

        public void SetAllTryParseOptionOff() {
            TryByteParse = false;
            TryIntParse = false;
            TryLongParse = false;
            TryFloatParse = false;
            TryDoubleParse = false;
            TryDecimalParse = false;
            
        }
        public void SetAllTryParseOptionOn() {
            TryByteParse = true;
            TryIntParse = true;
            TryLongParse = true;
            TryFloatParse = true;
            TryDoubleParse = true;
            TryDecimalParse = true;
        }
        public void SetAllTryParseOptionDefault() {
            TryByteParse = false;
            TryIntParse = true;
            TryLongParse = false;
            TryFloatParse = false;
            TryDoubleParse = true;
            TryDecimalParse = false;
        }


        public XElementDynamicParser(bool appendPrefixToAvoidNameConflictBetweenElementAndAttribute = false) {
            SetPropertyInitialValues();
            // overwrite constructor specified
            AppendPrefixToAvoidNameConflictBetweenElementAndAttribute = appendPrefixToAvoidNameConflictBetweenElementAndAttribute;
        }

        private void SetPropertyInitialValues() {
            AppendPrefixToAvoidNameConflictBetweenElementAndAttribute = false;
            SetAllTryParseOptionDefault();
            ElementValueSeparator = "";
            ElementValueAlwaysString = false;
        }


        public dynamic Parse(XElement target) {
            var item = new ExpandoObject();
            if (target != null) {
                Parse(item, target);
                return item;
            }
            return null;
        }


        private void Parse(ExpandoObject item, XElement target) {
            AddElementValueAndValuesAsProperties(item, target.Nodes().OfType<XText>());
            if (target.HasElements) {
                var namesWithCount = target.Elements()
                    .GroupBy(x => x.Name)
                    .Select(x => new {XName = x.Key, Count = x.Count()});

                foreach (var element in namesWithCount) {
                    if (element.Count == 1) {
                        ParseSingleElement(item, target.Elements(element.XName).First());
                    }
                    else {
                        ParseMultipleElement(item, target.Elements(element.XName));
                    }
                }
            }
            AddAttributesAsProperties(item, target.Attributes());    
        }


        private void ParseSingleElement(ExpandoObject parent, XElement target) {

            var item = new ExpandoObject();
            AddProperty(parent, (SuppressAttrElePrefix ? "" : "e") + target.Name.LocalName, item); // Add Element as property named element name.

            AddElementValueAndValuesAsProperties(item, target.Nodes().OfType<XText>());
            AddAttributesAsProperties(item, target.Attributes());
            Parse(item, target);
        }

        private void ParseMultipleElement(ExpandoObject parent, IEnumerable<XElement> targets) {
            var list = new List<ExpandoObject>();
            AddProperty(parent, (SuppressAttrElePrefix ? "" : "e") + targets.First().Name.LocalName, list); // Add Element as property named element name.
            foreach (var target in targets) {
                var item = new ExpandoObject();
                AddElementValueAndValuesAsProperties(item, target.Nodes().OfType<XText>());
                AddAttributesAsProperties(item, target.Attributes());
                Parse(item, target);
                list.Add(item);
            }
        }

        private void AddElementValueAndValuesAsProperties(IDictionary<string, object> that, IEnumerable<XText> texts) {

            var values = texts.Select(x => x.Value).ToList();
            var value = string.Join(ElementValueSeparator, values);

            AddProperty(that, "Values", (object)values); // Add Element Texts as `Values` (List<string>) property
            if (ElementValueAlwaysString) {
                AddProperty(that, "Value", (object) value); // `Value` as string
            }
            else {
                AddProperty(that, "Value", value); 
            }
        }


        private void AddAttributesAsProperties(ExpandoObject item, IEnumerable<XAttribute> attrs) {
            foreach (XAttribute x in attrs) {
                var name = (SuppressAttrElePrefix ? "" : "p") + x.Name.LocalName;
                AddProperty(item, name, x.Value);
            }
        }

        private void AddProperty(IDictionary<string, object> that, string name, List<ExpandoObject> value) {
            if (that.ContainsKey(name)) return;
            that.Add(name, value);
        }

        private void AddProperty(IDictionary<string, object> that, string name, ExpandoObject value) {
            if (that.ContainsKey(name)) return;
            that.Add(name, value);
        }

        private void AddProperty(IDictionary<string, object> that, string name, string value) {
            if (that.ContainsKey(name)) return;

            if (TryByteParse && byte.TryParse(value, out tryByteOut)) {
                that.Add(name, tryByteOut);
                return;
            }
            if (TryIntParse && int.TryParse(value, out tryIntOut)) {
                that.Add(name, tryIntOut);
                return;
            }
            if (TryLongParse && long.TryParse(value, out tryLongOut)) {
                that.Add(name, tryLongOut);
                return;
            }
            if (TryFloatParse && float.TryParse(value, out tryFloatOut)) {
                that.Add(name, tryFloatOut);
                return;
            }
            if (TryDoubleParse && double.TryParse(value, out tryDoubleOut)) {
                that.Add(name, tryDoubleOut);
                return;
            }
            if (TryDecimalParse && decimal.TryParse(value, out tryDecimalOut)) {
                that.Add(name, tryDecimalOut);
                return;
            }
            that.Add(name, value);
        }

        private void AddProperty(IDictionary<string, object> that, string name, object value) {
            if (that.ContainsKey(name)) return;
            that.Add(name, value);
        }
    }

}
