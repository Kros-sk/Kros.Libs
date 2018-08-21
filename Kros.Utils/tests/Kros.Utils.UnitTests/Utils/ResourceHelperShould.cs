using FluentAssertions;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace Kros.Utils.UnitTests.Utils
{
    public class ResourceHelperShould
    {
        [Fact]
        public void InitializeGracefully()
        {
            string test = Kros.Utils.UnitTests.Properties.Resources.Test;
            string txt = Kros.Utils.UnitTests.Properties.Resources.data_txt;
            byte[] bin = Kros.Utils.UnitTests.Properties.Resources.data_bin;

            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
            string test2 = Kros.Utils.UnitTests.Properties.Resources.Test;
            string txt2 = Kros.Utils.UnitTests.Properties.Resources.data_txt;
            byte[] bin2 = Kros.Utils.UnitTests.Properties.Resources.data_bin;

            Action createWithoutAssembly = () => new ResourceHelper(null);
            createWithoutAssembly.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("resourcesAssembly");

            var helper = new ResourceHelper(Assembly.GetExecutingAssembly(), string.Empty);
            helper.BaseNamespace.Should().BeNull("BaseNamespace initialized to empty string.");

            helper = new ResourceHelper(Assembly.GetExecutingAssembly(), "   ");
            helper.BaseNamespace.Should().BeNull("BaseNamespace initialized to whitespace string.");
        }

        [Fact]
        public void ReturnCorrectResourceName()
        {
            var helper = new ResourceHelper(Assembly.GetExecutingAssembly());
            helper.GetResourceName("data.txt").Should().Be("data.txt", "BaseNamespace is not set.");

            helper = new ResourceHelper(Assembly.GetExecutingAssembly(), "Kros.Utils.UnitTests.Resources");
            helper.GetResourceName("data.txt").Should().Be("Kros.Utils.UnitTests.Resources.data.txt", "BaseNamespace is set.");
        }

        [Fact]
        public void ReturnStringResourceContent()
        {
            var helper = new ResourceHelper(Assembly.GetExecutingAssembly());
            string content = helper.GetString("Kros.Utils.UnitTests.Resources.data.txt");
            content.Should().Be("EN Lorem ipsum.", "BaseNamespace is not set.");

            helper = new ResourceHelper(Assembly.GetExecutingAssembly(), "Kros.Utils.UnitTests.Resources");
            content = helper.GetString("data.txt");
            content.Should().Be("EN Lorem ipsum.", "BaseNamespace is set.");
        }

        [Fact]
        public void ReturnBinaryResourceContent()
        {
            byte[] expectedData = new byte[] { 1, (int)'e', (int)'n', 0xFF };

            byte[] content;
            var helper = new ResourceHelper(Assembly.GetExecutingAssembly());
            using (Stream stream = helper.GetStream("Kros.Utils.UnitTests.Resources.data.bin"))
            {
                content = ReadStreamContent(stream);
                content.Should().Equal(expectedData, "BaseNamespace is not set.");
            }

            helper = new ResourceHelper(Assembly.GetExecutingAssembly(), "Kros.Utils.UnitTests.Resources");
            using (Stream stream = helper.GetStream("data.bin"))
            {
                content = ReadStreamContent(stream);
                content.Should().Equal(expectedData, "BaseNamespace is set.");
            }
        }

        private static byte[] ReadStreamContent(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            return buffer;
        }
    }
}
