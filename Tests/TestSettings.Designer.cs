﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tests {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.2.0.0")]
    internal sealed partial class TestSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static TestSettings defaultInstance = ((TestSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new TestSettings())));
        
        public static TestSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=tomato; Initial Catalog={0}; Integrated Security=true; Connection Tim" +
            "eout=1000; Max Pool Size=50;")]
        public string ConnectionString {
            get {
                return ((string)(this["ConnectionString"]));
            }
            set {
                this["ConnectionString"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("__resequel_TestScopeDatabase_FeelFreeToDeleteIt")]
        public string DatabaseName {
            get {
                return ((string)(this["DatabaseName"]));
            }
            set {
                this["DatabaseName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("USE [master]\r\nGO\r\n\r\nif exists( select * from sys.databases where name = \'{0}\' )\r\n" +
            "begin\r\n\tDROP DATABASE {0}\r\nend\r\nGO\r\n\r\n/****** Object:  Database [{0}]    ******/" +
            "\r\nCREATE DATABASE [{0}]\r\n CONTAINMENT = NONE\r\n\r\nALTER DATABASE [{0}] SET COMPATI" +
            "BILITY_LEVEL = 120\r\nGO\r\n\r\n--IF (1 = FULLTEXTSERVICEPROPERTY(\'IsFullTextInstalled" +
            "\'))\r\n--begin\r\n--EXEC [{0}].[dbo].[sp_fulltext_database] @action = \'enable\'\r\n--en" +
            "d\r\n--GO\r\n\r\nALTER DATABASE [{0}] SET ANSI_NULL_DEFAULT OFF \r\nGO\r\n\r\nALTER DATABASE" +
            " [{0}] SET ANSI_NULLS OFF \r\nGO\r\n\r\nALTER DATABASE [{0}] SET ANSI_PADDING OFF \r\nGO" +
            "\r\n\r\nALTER DATABASE [{0}] SET ANSI_WARNINGS OFF \r\nGO\r\n\r\nALTER DATABASE [{0}] SET " +
            "ARITHABORT OFF \r\nGO\r\n\r\nALTER DATABASE [{0}] SET AUTO_CLOSE OFF \r\nGO\r\n\r\nALTER DAT" +
            "ABASE [{0}] SET AUTO_SHRINK OFF \r\nGO\r\n\r\nALTER DATABASE [{0}] SET AUTO_UPDATE_STA" +
            "TISTICS ON \r\nGO\r\n\r\nALTER DATABASE [{0}] SET CURSOR_CLOSE_ON_COMMIT OFF \r\nGO\r\n\r\nA" +
            "LTER DATABASE [{0}] SET CURSOR_DEFAULT  GLOBAL \r\nGO\r\n\r\nALTER DATABASE [{0}] SET " +
            "CONCAT_NULL_YIELDS_NULL OFF \r\nGO\r\n\r\nALTER DATABASE [{0}] SET NUMERIC_ROUNDABORT " +
            "OFF \r\nGO\r\n\r\nALTER DATABASE [{0}] SET QUOTED_IDENTIFIER OFF \r\nGO\r\n\r\nALTER DATABAS" +
            "E [{0}] SET RECURSIVE_TRIGGERS OFF \r\nGO\r\n\r\nALTER DATABASE [{0}] SET  DISABLE_BRO" +
            "KER \r\nGO\r\n\r\nALTER DATABASE [{0}] SET AUTO_UPDATE_STATISTICS_ASYNC OFF \r\nGO\r\n\r\nAL" +
            "TER DATABASE [{0}] SET DATE_CORRELATION_OPTIMIZATION OFF \r\nGO\r\n\r\nALTER DATABASE " +
            "[{0}] SET TRUSTWORTHY OFF \r\nGO\r\n\r\nALTER DATABASE [{0}] SET ALLOW_SNAPSHOT_ISOLAT" +
            "ION OFF \r\nGO\r\n\r\nALTER DATABASE [{0}] SET PARAMETERIZATION SIMPLE \r\nGO\r\n\r\nALTER D" +
            "ATABASE [{0}] SET READ_COMMITTED_SNAPSHOT OFF \r\nGO\r\n\r\nALTER DATABASE [{0}] SET H" +
            "ONOR_BROKER_PRIORITY OFF \r\nGO\r\n\r\nALTER DATABASE [{0}] SET RECOVERY FULL \r\nGO\r\n\r\n" +
            "ALTER DATABASE [{0}] SET  MULTI_USER \r\nGO\r\n\r\nALTER DATABASE [{0}] SET PAGE_VERIF" +
            "Y CHECKSUM  \r\nGO\r\n\r\nALTER DATABASE [{0}] SET DB_CHAINING OFF \r\nGO\r\n\r\nALTER DATAB" +
            "ASE [{0}] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) \r\nGO\r\n\r\nALTER DATABASE [" +
            "{0}] SET TARGET_RECOVERY_TIME = 60 SECONDS \r\nGO\r\n\r\nALTER DATABASE [{0}] SET DELA" +
            "YED_DURABILITY = DISABLED \r\nGO\r\n\r\n--ALTER DATABASE [{0}] SET QUERY_STORE = OFF\r\n" +
            "--GO\r\n\r\nALTER DATABASE [{0}] SET  READ_WRITE \r\nGO\r\n\r\n\r\nUSE [{0}]\r\nGO\r\n/****** Ob" +
            "ject:  Table [dbo].[TestTable0]   ******/\r\nSET ANSI_NULLS ON\r\nGO\r\nSET QUOTED_IDE" +
            "NTIFIER ON\r\nGO\r\nCREATE TABLE [dbo].[TestTable0](\r\n\t[id] [int] NOT NULL,\r\n\t[name]" +
            " [varchar](100) NOT NULL,\r\n\t[additional] [varchar](100) NULL,\r\n CONSTRAINT [PK_T" +
            "estTable0] PRIMARY KEY CLUSTERED \r\n(\r\n\t[id] ASC\r\n)WITH (PAD_INDEX = OFF, STATIST" +
            "ICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LO" +
            "CKS = ON) ON [PRIMARY]\r\n) ON [PRIMARY]\r\nGO\r\n\r\nINSERT [dbo].[TestTable0] ([id], [" +
            "name]) VALUES (1, N\'Record 1\')\r\nGO\r\nINSERT [dbo].[TestTable0] ([id], [name]) VAL" +
            "UES (2, N\'Record 2\')\r\nGO\r\nINSERT [dbo].[TestTable0] ([id], [name]) VALUES (3, N\'" +
            "Record 3\')\r\nGO\r\n\r\n\r\nCREATE NONCLUSTERED INDEX [TestTable0_Index0] ON [dbo].[Test" +
            "Table0]\r\n(\r\n\t[name] ASC\r\n)\r\nINCLUDE ( [additional]) WITH (PAD_INDEX = OFF, STATI" +
            "STICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF" +
            ", ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]\r\nGO\r\n\r\n\r\n\r\n\r\nCREATE " +
            "TABLE [dbo].[TestTable1](\r\n\t[id] [int] NOT NULL,\r\n\t[name] [varchar](100) NOT NUL" +
            "L,\r\n\t[additional] [varchar](100) NULL,\r\n CONSTRAINT [PK_TestTable1] PRIMARY KEY " +
            "CLUSTERED \r\n(\r\n\t[id] ASC\r\n)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, " +
            "IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]\r" +
            "\n) ON [PRIMARY]\r\nGO\r\n\r\nCREATE TABLE [dbo].[TestTable2](\r\n\t[id] [int] NOT NULL,\r\n" +
            "\t[name] [varchar](100) NOT NULL,\r\n\t[additional] [varchar](100) NULL,\r\n CONSTRAIN" +
            "T [PK_TestTable2] PRIMARY KEY CLUSTERED \r\n(\r\n\t[id] ASC\r\n)WITH (PAD_INDEX = OFF, " +
            "STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_" +
            "PAGE_LOCKS = ON) ON [PRIMARY]\r\n) ON [PRIMARY]\r\nGO\r\n\r\n\r\nCREATE TABLE [dbo].[TestT" +
            "able3](\r\n\t[id] [int] NOT NULL,\r\n\t[name] [varbinary](max) NOT NULL,\r\n\t[additional" +
            "] [varbinary](max) NULL,\r\n\t[custom_column] [int] NOT NULL,\r\n\t[database_version] " +
            "[int] NOT NULL,\r\n CONSTRAINT [PK_TestTable3] PRIMARY KEY CLUSTERED \r\n(\r\n\t[id] AS" +
            "C\r\n)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, A" +
            "LLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]\r\n) ON [PRIMARY]\r\nGO\r\n\r\n" +
            "\r\nuse [master]\r\nGO")]
        public string CreateDatabaseScript {
            get {
                return ((string)(this["CreateDatabaseScript"]));
            }
            set {
                this["CreateDatabaseScript"] = value;
            }
        }
    }
}
