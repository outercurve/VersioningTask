using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;

namespace Versioning
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Microsoft.Build.Framework;

    /// <summary>
    /// <b>Valid TaskActions are:</b>
    /// <para><i>Increment</i> (<b>Required: </b>File <b>Optional: </b>Increment <b>Output: </b>Value)</para>
    /// <para><i>Reset</i> (<b>Required: </b>File <b>Optional: </b>Value <b>Output: </b>Value)</para>
    /// <para><b>Remote Execution Support:</b> No</para>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Project ToolsVersion="4.0" DefaultTargets="Default" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    ///     <PropertyGroup>
    ///         <TPath>$(MSBuildProjectDirectory)\..\MSBuild.ExtensionPack.tasks</TPath>
    ///         <TPath Condition="Exists('$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks')">$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks</TPath>
    ///     </PropertyGroup>
    ///     <Import Project="$(TPath)"/>
    ///     <Target Name="Default">
    ///         <!-- Perform a default increment of 1 -->
    ///         <MSBuild.ExtensionPack.FileSystem.FileVersion TaskAction="Increment" File="C:\a\MyVersionfile.txt">
    ///             <Output TaskParameter="Value" PropertyName="NewValue"/>
    ///         </MSBuild.ExtensionPack.FileSystem.FileVersion>
    ///         <Message Text="$(NewValue)"/>
    ///         <!-- Perform an increment of 5 -->
    ///         <MSBuild.ExtensionPack.FileSystem.FileVersion TaskAction="Increment" File="C:\a\MyVersionfile2.txt" Increment="5">
    ///             <Output TaskParameter="Value" PropertyName="NewValue"/>
    ///         </MSBuild.ExtensionPack.FileSystem.FileVersion>
    ///         <Message Text="$(NewValue)"/>
    ///         <!-- Reset a file value -->
    ///         <MSBuild.ExtensionPack.FileSystem.FileVersion TaskAction="Reset" File="C:\a\MyVersionfile3.txt" Value="10">
    ///             <Output TaskParameter="Value" PropertyName="NewValue"/>
    ///         </MSBuild.ExtensionPack.FileSystem.FileVersion>
    ///         <Message Text="$(NewValue)"/>
    ///     </Target>
    /// </Project>
    /// ]]></code>
    /// </example>
    public class FileVersion : Task
    {
    
        private FileInfo versionFile;
        private int increment = 1;
        private bool changedAttribute;
        private Encoding fileEncoding = Encoding.ASCII;
        private Version _value = new Version(0,0,1,0);
        private TaskAction _action = Versioning.TaskAction.Increment;
        private VersionPart _part = VersionPart.Build;


        /// <summary>
        /// The file to store the incrementing version in.
        /// </summary>
        public ITaskItem File { get; set; }

        /// <summary>
        /// Value to increment by. Default is 1.
        /// </summary>
        public int Increment
        {
            get { return this.increment; }
            set { this.increment = value; }
        }

        public string TaskAction { 
            get
            {
                return _action.ToString();
            }
            set
            {
                _action = (TaskAction)Enum.Parse(typeof (TaskAction), value, true);
            } 
        }

        public string PartToIncrement
        {
            get { return _part.ToString(); }
            set { _part = (VersionPart) Enum.Parse(typeof (VersionPart), value, true); }
        }

        /// <summary>
        /// Gets value returned from the file, or used to reset the value in the file. Default is 0.
        /// </summary>
        [Output]
        public string Value
        {
            get { return _value.ToString(4); }
        }

        /// <summary>
        /// Performs the action of this task.
        /// </summary>
        public override bool Execute()
        {
          

            this.versionFile = new FileInfo(this.File.ItemSpec);

            // Create the file if it doesn't exist
            if (!this.versionFile.Exists)
            {
                using (FileStream fs = this.versionFile.Create())
                {
                }
            }

            // First make sure the file is writable.
            FileAttributes fileAttributes = System.IO.File.GetAttributes(this.versionFile.FullName);

            // If readonly attribute is set, reset it.
            if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Making File Writeable: {0}", this.versionFile.FullName));
                System.IO.File.SetAttributes(this.versionFile.FullName, fileAttributes ^ FileAttributes.ReadOnly);
                this.changedAttribute = true;
            }
            
            switch (_action)
            {
                case Versioning.TaskAction.Increment:
                    this.IncrementValue();
                    break;
                case Versioning.TaskAction.Reset:
                    this.ResetValue();
                    break;
                default:
                    this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "Invalid TaskAction passed: {0}", this.TaskAction));
                    return false;
            }

            if (this.changedAttribute)
            {
                this.LogTaskMessage(MessageImportance.Low, "Making file readonly");
                System.IO.File.SetAttributes(this.versionFile.FullName, FileAttributes.ReadOnly);
            }

            return !Log.HasLoggedErrors;
        }

        private void ResetValue()
        {
            this.WriteFile();
        }

        private void IncrementValue()
        {
            Version currentValue;

            using (StreamReader streamReader = new StreamReader(this.versionFile.FullName, this.fileEncoding, true))
            {
                currentValue = Version.Parse(streamReader.ReadLine());
                if (this.fileEncoding == null)
                {
                    this.fileEncoding = streamReader.CurrentEncoding;
                }
            }

            this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Read: {0} from: {1}", currentValue, this.versionFile.FullName));
            _value = IncrementVersionPart(currentValue, this.Increment, _part);
            if (currentValue != this._value)
            {
                this.WriteFile();
            }
        }

        public static Version IncrementVersionPart(Version currentVersion, int amountToIncrement, VersionPart partToIncrement)
        {
            switch (partToIncrement)
            {
                case VersionPart.Major:
                    return new Version(currentVersion.Major + amountToIncrement, currentVersion.Minor, currentVersion.Build, currentVersion.Revision);
                case VersionPart.Minor:
                    return new Version(currentVersion.Major, currentVersion.Minor + amountToIncrement, currentVersion.Build, currentVersion.Revision);
                case VersionPart.Build:
                    return new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build + amountToIncrement, currentVersion.Revision);
                case VersionPart.Revision:
                    return new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build, currentVersion.Revision + amountToIncrement);
            }

            throw new Exception("You can't get here");
        }

        private void WriteFile()
        {
            // Write out the new file.
            using (StreamWriter streamWriter = new StreamWriter(this.versionFile.FullName, false, this.fileEncoding))
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Writing: {0} to: {1}", this.Value, this.versionFile.FullName));
                streamWriter.Write(this.Value);
            }    
        }

        private void LogTaskMessage(MessageImportance importance, string msg)
        {
            try
            {
                Log.LogMessage(importance, msg);
            }
            catch (Exception)
            {
                
                
            }
            
        }

        
    }

    

    public enum VersionPart
    {
        Major,
        Minor,
        Build,
        Revision
    }

    public enum TaskAction
    {
        Increment,
        Reset
    }


}
