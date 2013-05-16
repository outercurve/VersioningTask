
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Versioning;
using Xunit;

namespace Test
{
    public class TestClass
    {
        public readonly Version DefaultVersion = new Version(1,0,0,0);

        [Fact]
        public void MajorTest()
        {
            var ret = FileVersion.IncrementVersionPart(DefaultVersion, 1, VersionPart.Major);
            Assert.Equal(2, ret.Major);
            Assert.Equal(0, ret.Minor);
            Assert.Equal(0, ret.Build);
            Assert.Equal(0, ret.Revision);

        }

        [Fact]
        public void MinorTest()
        {
            var ret = FileVersion.IncrementVersionPart(DefaultVersion, 1, VersionPart.Minor);
            Assert.Equal(1, ret.Major);
            Assert.Equal(1, ret.Minor);
            Assert.Equal(0, ret.Build);
            Assert.Equal(0, ret.Revision);

        }

        [Fact]
        public void BuildTest()
        {
            var ret = FileVersion.IncrementVersionPart(DefaultVersion, 1, VersionPart.Build);
            Assert.Equal(1, ret.Major);
            Assert.Equal(0, ret.Minor);
            Assert.Equal(1, ret.Build);
            Assert.Equal(0, ret.Revision);

        }

        [Fact]
        public void RevisionTest()
        {
            var ret = FileVersion.IncrementVersionPart(DefaultVersion, 1, VersionPart.Revision);
            Assert.Equal(1, ret.Major);
            Assert.Equal(0, ret.Minor);
            Assert.Equal(0, ret.Build);
            Assert.Equal(1, ret.Revision);

        }






    }
}
