using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Test.Properties;
using Versioning;

using Xunit;

namespace Test
{
    public class FileTest : IDisposable
    {
        private const string FileName = "version.txt";

        public FileTest()
        {
            File.WriteAllBytes(FileName, Resources.version);
        }


        [Fact]
        public void TestMajor()
        {
            var task = CreateUpdateVersionTask(VersionPart.Major);
            
            Assert.True(task.Execute());
            var vers = GrabVersion();
            Assert.Equal(2, vers.Major);
            Assert.Equal(0, vers.Minor);
            Assert.Equal(0, vers.Build);
            Assert.Equal(0, vers.Revision);
        }

        [Fact]
        public void TestMinor()
        {
            var task = CreateUpdateVersionTask(VersionPart.Minor);

            Assert.True(task.Execute());
            var vers = GrabVersion();
            Assert.Equal(1, vers.Major);
            Assert.Equal(1, vers.Minor);
            Assert.Equal(0, vers.Build);
            Assert.Equal(0, vers.Revision);
        }

        [Fact]
        public void TestBuild()
        {
            var task = CreateUpdateVersionTask(VersionPart.Build);

            Assert.True(task.Execute());
            var vers = GrabVersion();
            Assert.Equal(1, vers.Major);
            Assert.Equal(0, vers.Minor);
            Assert.Equal(1, vers.Build);
            Assert.Equal(0, vers.Revision);
        }

        [Fact]
        public void TestRevision()
        {
            var task = CreateUpdateVersionTask(VersionPart.Revision);

            Assert.True(task.Execute());
            var vers = GrabVersion();
            Assert.Equal(1, vers.Major);
            Assert.Equal(0, vers.Minor);
            Assert.Equal(0, vers.Build);
            Assert.Equal(1, vers.Revision);
        }

        [Fact]
        public void TestGetVersion()
        {
            var task = CreateGetVersionTask();
            
            Assert.True(task.Execute());
            
            Assert.Equal(1, task.Major);
            Assert.Equal(0, task.Minor);
            Assert.Equal(0, task.Build);
            Assert.Equal(0, task.Revision);

            Assert.Equal("1.0.0.0", task.Version);
        }
        /*
        [Fact]
        public void TestGetMinor()
        {
            var task = CreateUpdateVersionTask(VersionPart.Minor);

            Assert.True(task.Execute());
            var vers = GrabVersion();
            Assert.Equal(1, vers.Major);
            Assert.Equal(1, vers.Minor);
            Assert.Equal(0, vers.Build);
            Assert.Equal(0, vers.Revision);
        }

        [Fact]
        public void TestGetBuild()
        {
            var task = CreateUpdateVersionTask(VersionPart.Build);

            Assert.True(task.Execute());
            var vers = GrabVersion();
            Assert.Equal(1, vers.Major);
            Assert.Equal(0, vers.Minor);
            Assert.Equal(1, vers.Build);
            Assert.Equal(0, vers.Revision);
        }

        [Fact]
        public void TestGetRevision()
        {
            var task = CreateUpdateVersionTask(VersionPart.Revision);

            Assert.True(task.Execute());
            var vers = GrabVersion();
            Assert.Equal(1, vers.Major);
            Assert.Equal(0, vers.Minor);
            Assert.Equal(0, vers.Build);
            Assert.Equal(1, vers.Revision);
        }*/


        private FileVersion CreateUpdateVersionTask(VersionPart part)
        {
            return new FileVersion()
                {
                    PartToIncrement = part.ToString(),
                    TaskAction = TaskAction.Increment.ToString(),
                    File = new TaskItem(FileName)
                };
        }

        public GetVersion CreateGetVersionTask()
        {
            return new GetVersion
                {
                    File = new TaskItem(FileName)
                };
        }

        

        public Version GrabVersion()
        {
            using (var sr = new StreamReader(FileName, Encoding.ASCII))
            {
                return Version.Parse(sr.ReadLine());
            }
        }


        public void Dispose()
        {
           File.Delete("version.txt");
        }
    }
}
