﻿using QuestViva.Engine;

namespace QuestViva.EngineTests
{
    [TestClass]
    public class FilesTests
    {
        [TestMethod]
        public void TestConvertDottedProperties()
        {
            Assert.AreEqual("obj___DOT___prop", Engine.Utility.ConvertVariablesToFleeFormat("obj.prop"));
            Assert.AreEqual("obj1___DOT___prop, obj2___DOT___prop", Engine.Utility.ConvertVariablesToFleeFormat("obj1.prop, obj2.prop"));
            Assert.AreEqual("(\"myfile.html\")", Engine.Utility.ConvertVariablesToFleeFormat("(\"myfile.html\")"));
            Assert.AreEqual("\"myfile.html\"", Engine.Utility.ConvertVariablesToFleeFormat("\"myfile.html\""));
            Assert.AreEqual("obj1___DOT___prop \"test.html\" obj2___DOT___prop", Engine.Utility.ConvertVariablesToFleeFormat("obj1.prop \"test.html\" obj2.prop"));
        }

        [TestMethod]
        public void TestDecimalPointsNotConvertedToDottedProperties()
        {
            Assert.AreEqual("3.141", Engine.Utility.ConvertVariablesToFleeFormat("3.141"));
        }

        [TestMethod]
        public void TestRemoveComments()
        {
            Assert.AreEqual("msg (\"Something\")", Engine.Utility.RemoveComments("msg (\"Something\")"));
            Assert.AreEqual("msg (\"Something\")", Engine.Utility.RemoveComments("msg (\"Something\")//comment"));
            Assert.AreEqual("", Engine.Utility.RemoveComments("//comment"));
            Assert.AreEqual("msg (\"Something with // two slashes\")", Engine.Utility.RemoveComments("msg (\"Something with // two slashes\")"));
            Assert.AreEqual("msg (\"Something with // two slashes\")", Engine.Utility.RemoveComments("msg (\"Something with // two slashes\")//comment"));
            Assert.AreEqual("msg (\"Something with // two slashes\")", Engine.Utility.RemoveComments("msg (\"Something with // two slashes\")//comment \"with a string\""));
            Assert.AreEqual("msg (\"A quote \\\"with // two slashes\\\"\")", Engine.Utility.RemoveComments("msg (\"A quote \\\"with // two slashes\\\"\")"));
        }

        [TestMethod]
        public void TestObscureStrings()
        {
            string input = "This is \"a test\" of obscuring strings";
            string result = Engine.Utility.ObscureStrings(input);
            Assert.AreEqual(input.Length, result.Length);
            Assert.IsTrue(result.StartsWith("This is \""));
            Assert.IsTrue(result.EndsWith("\" of obscuring strings"));
            Assert.IsFalse(result.Contains("a test"));

            //missing end quote
            try
            {
                Engine.Utility.ObscureStrings("\"missing end quote");
                Assert.Fail();
            }
            catch (MismatchingQuotesException)
            {
            }
        }

        [TestMethod]
        public void TestObscureStringsWithNestedQuotes()
        {
            string input = "This is \"a test \\\"with a nested quote\\\"\" of obscuring strings";
            string result = Engine.Utility.ObscureStrings(input);
            Assert.AreEqual(input.Length, result.Length);
            Assert.IsTrue(result.StartsWith("This is \""));
            Assert.IsTrue(result.EndsWith("\" of obscuring strings"));
            Assert.IsFalse(result.Contains("a test"));
            Assert.IsFalse(result.Contains("a nested quote"));
        }

        [TestMethod]
        public void TestSplitParameter()
        {
            // ("This contains two parameters, even though the first has a comma", "this is the second parameter")

            string param1 = "\"This contains two parameters, even though the first has a comma\"";
            string param2 = "\"this is the second parameter\"";
            string input = param1 + ", " + param2;
            var result = Engine.Utility.SplitParameter(input);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(param1, result[0]);
            Assert.AreEqual(param2, result[1]);
        }

        [TestMethod]
        public void TestSplitParameterWithNestedQuotes()
        {
            // ("This contains \"a nested quote, with a comma\"", "this is the second parameter")

            string param1 = "\"This contains \\\"a nested quote, with a comma\\\"\"";
            string param2 = "\"this is the second parameter\"";
            string input = param1 + ", " + param2;
            var result = Engine.Utility.SplitParameter(input);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(param1, result[0]);
            Assert.AreEqual(param2, result[1]);
        }

        [TestMethod]
        public void TestConvertVariableNamesWithSpaces()
        {
            Assert.AreEqual("my___SPACE___variable", Engine.Utility.ConvertVariablesToFleeFormat("my variable"));
            Assert.AreEqual("my___SPACE___variable, other___SPACE___variable", Engine.Utility.ConvertVariablesToFleeFormat("my variable, other variable"));
            Assert.AreEqual("my___SPACE___variable, \"some text\", other___SPACE___variable", Engine.Utility.ConvertVariablesToFleeFormat("my variable, \"some text\", other variable"));
            Assert.AreEqual("my___SPACE___long___SPACE___variable___SPACE___name", Engine.Utility.ConvertVariablesToFleeFormat("my long variable name"));
        }

        [TestMethod]
        public void TestNamesNearKeywordsNotConverted()
        {
            Assert.AreEqual("not my___SPACE___variable", Engine.Utility.ConvertVariablesToFleeFormat("not my variable"));
            Assert.AreEqual("my___SPACE___variable or other___SPACE___variable", Engine.Utility.ConvertVariablesToFleeFormat("my variable or other variable"));
            Assert.AreEqual("(not SomeFunction(\"hello there\"))", Engine.Utility.ConvertVariablesToFleeFormat("(not SomeFunction(\"hello there\"))"));
        }

        [TestMethod]
        public void TestIsValidAttributeName_ValidAttributes()
        {
            Assert.IsTrue(Engine.Utility.IsValidAttributeName("attribute"));
            Assert.IsTrue(Engine.Utility.IsValidAttributeName("attribute name"));
            Assert.IsTrue(Engine.Utility.IsValidAttributeName("attribute name2"));
            Assert.IsTrue(Engine.Utility.IsValidAttributeName("a"));
            Assert.IsTrue(Engine.Utility.IsValidFieldName("object"));
        }

        [TestMethod]
        public void TestIsValidAttributeName_InvalidAttributes()
        {
            Assert.IsFalse(Engine.Utility.IsValidAttributeName("attribute "));
            Assert.IsFalse(Engine.Utility.IsValidAttributeName("2attribute"));
            Assert.IsFalse(Engine.Utility.IsValidAttributeName("attri.bute"));
            Assert.IsFalse(Engine.Utility.IsValidAttributeName("this and that"));
            Assert.IsFalse(Engine.Utility.IsValidAttributeName("object"));
        }

        [TestMethod]
        public void TestGetParameter()
        {
            Assert.AreEqual("\"parameter\"", Engine.Utility.GetParameter("msg (\"parameter\")"));
            Assert.AreEqual("\"parameter 1\", \"parameter 2\"", Engine.Utility.GetParameter("msg (\"parameter 1\", \"parameter 2\")"));
            Assert.AreEqual("\"parameter with a bracket ) in a string\"", Engine.Utility.GetParameter("msg (\"parameter with a bracket ) in a string\")"));
        }

        [TestMethod]
        public void TestResolveObjectDotAttribute_Variable()
        {
            string name = "somevar";
            string obj;
            string variable;
            Engine.Utility.ResolveObjectDotAttribute(name, out obj, out variable);
            Assert.AreEqual(null, obj);
            Assert.AreEqual("somevar", variable);
        }

        [TestMethod]
        public void TestResolveObjectDotAttribute_OneObject()
        {
            string name = "someobject.somevar";
            string obj;
            string variable;
            Engine.Utility.ResolveObjectDotAttribute(name, out obj, out variable);
            Assert.AreEqual("someobject", obj);
            Assert.AreEqual("somevar", variable);
        }

        [TestMethod]
        public void TestResolveObjectDotAttribute_TwoObjects()
        {
            string name = "someobject.anotherobject.somevar";
            string obj;
            string variable;
            Engine.Utility.ResolveObjectDotAttribute(name, out obj, out variable);
            Assert.AreEqual("someobject.anotherobject", obj);
            Assert.AreEqual("somevar", variable);
        }

        [TestMethod]
        public void TestResolveObjectDotAttribute_ThreeObjects()
        {
            string name = "someobject.anotherobject.thirdobject.somevar";
            string obj;
            string variable;
            Engine.Utility.ResolveObjectDotAttribute(name, out obj, out variable);
            Assert.AreEqual("someobject.anotherobject.thirdobject", obj);
            Assert.AreEqual("somevar", variable);
        }

        [TestMethod]
        public void TestResolveObjectDotAttribute_ThreeObjectsAndSpaces()
        {
            string name = "some object.another object.thirdobject.some var";
            string obj;
            string variable;
            Engine.Utility.ResolveObjectDotAttribute(name, out obj, out variable);
            Assert.AreEqual("some object.another object.thirdobject", obj);
            Assert.AreEqual("some var", variable);
        }
    }
}
