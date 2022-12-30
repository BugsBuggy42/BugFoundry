namespace Projects.Buggary.Source.Tests
{
    using BuggaryEditor.TextEditors.OpenEditor;
    using FluentAssertions;
    using NUnit.Framework;

    public class TextHelperTests
    {
        private EditorTextHelper sut = new ();

        [Test]
        [TestCase("<some>C<one>", 6, 0)]
        [TestCase("<some>C<one>", 7, -1)]
        [TestCase("<some>C<one>", 8, -1)]
        [TestCase("<some>C<one>", 11, -1)]
        public void MarkedToCleanIndex(string text, int index, int expected)
        {
            int? actual = this.sut.MarkedToCleanIndex(text, index);
            actual.Should().Be(expected);
        }

        [Test]
        public void MarkedToCleanIndexInsideTag()
        {
            int actual = this.sut.MarkedToCleanIndex("<some>C<one>", 7);
            actual.Should().Be(-1);
        }

        [Test]
        [TestCase("<some>C<one>", 6, false)]
        [TestCase("<some>C<one>", 7, true)]
        [TestCase("<some>C<one>", 8, true)]
        [TestCase("<some>C<one>", 11, true)]
        public void InsideATag(string text, int index, bool expected)
        {
            bool actual = this.sut.InsideATag(text, index);
            actual.Should().Be(expected);
        }

        [Test]
        [TestCase("<some>C<one> ", 12, 6)]
        public void TastyIcicle(string text, int index, int expected)
        {
            int? actual = this.sut.MarkedToCleanIndex(text, index);
            actual.Should().Be(expected);
        }
    }
}