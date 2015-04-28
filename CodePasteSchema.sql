IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [CodePaste].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [CodePaste] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [CodePaste] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [CodePaste] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [CodePaste] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [CodePaste] SET ARITHABORT OFF 
GO
ALTER DATABASE [CodePaste] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [CodePaste] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [CodePaste] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [CodePaste] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [CodePaste] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [CodePaste] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [CodePaste] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [CodePaste] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [CodePaste] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [CodePaste] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [CodePaste] SET  DISABLE_BROKER 
GO
ALTER DATABASE [CodePaste] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [CodePaste] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [CodePaste] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [CodePaste] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [CodePaste] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [CodePaste] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [CodePaste] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [CodePaste] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [CodePaste] SET  MULTI_USER 
GO
ALTER DATABASE [CodePaste] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [CodePaste] SET DB_CHAINING OFF 
GO
USE [CodePaste]
GO
/****** Object:  User [Network Service]    Script Date: 4/27/2015 11:13:11 AM ******/
CREATE USER [Network Service] FOR LOGIN [NT AUTHORITY\NETWORK SERVICE] WITH DEFAULT_SCHEMA=[dbo]
GO
sys.sp_addrolemember @rolename = N'db_owner', @membername = N'Network Service'
GO
sys.sp_addrolemember @rolename = N'db_accessadmin', @membername = N'Network Service'
GO
sys.sp_addrolemember @rolename = N'db_ddladmin', @membername = N'Network Service'
GO
sys.sp_addrolemember @rolename = N'db_datareader', @membername = N'Network Service'
GO
sys.sp_addrolemember @rolename = N'db_datawriter', @membername = N'Network Service'
GO
/****** Object:  FullTextCatalog [CodePasteFullText]    Script Date: 4/27/2015 11:13:11 AM ******/
CREATE FULLTEXT CATALOG [CodePasteFullText]WITH ACCENT_SENSITIVITY = OFF

GO
/****** Object:  StoredProcedure [dbo].[LogSnippetClick]    Script Date: 4/27/2015 11:13:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	Logs a Snippet Click into the SnippetClick table
-- =============================================
CREATE PROCEDURE [dbo].[LogSnippetClick]
	-- Add the parameters for the stored procedure here	
	@SnippetId AS VARCHAR(MAX),
	@IpAddress AS VARCHAR(MAX)			
AS
BEGIN

	SET NOCOUNT ON;
	
	-- check if don't allow updating if this ip address has already 
	-- clicked on this snippet in the last 10 minutes
    select Id from SnippetClicks 
		WHERE snippetId = @SnippetId AND ipaddress = @IpAddress AND 
			  DATEDIFF(minute,  Entered, GETDATE() ) <= 10	  

	 IF @@ROWCOUNT = 0	
	 BEGIN	 			
		INSERT INTO SnippetClicks 
			(SnippetId,IpAddress,Entered) VALUES 
			(@SnippetId,@IpAddress,GETDATE())		  
		UPDATE CodeSnippets SET VIEWS = VIEWS + 1 
		    WHERE id = @SnippetId
	 END
	 ELSE
	 BEGIN
	    -- clean up
		DELETE FROM SnippetClicks WHERE DATEDIFF(minute,Entered,GETDATE()) > 10
	 END
END	 

GO
/****** Object:  Table [dbo].[AdditionalSnippets]    Script Date: 4/27/2015 11:13:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdditionalSnippets](
	[Id] [nvarchar](40) NOT NULL,
	[SnippetId] [nvarchar](40) NULL,
	[Title] [nvarchar](40) NULL,
	[Code] [ntext] NULL,
	[Language] [nvarchar](40) NULL,
	[Version] [decimal](12, 4) NULL,
	[tstamp] [timestamp] NULL,
 CONSTRAINT [PK_AdditionalSnippets] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ApplicationLog]    Script Date: 4/27/2015 11:13:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApplicationLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Entered] [datetime] NOT NULL CONSTRAINT [DF_ApplicationLog_Entered]  DEFAULT (getdate()),
	[Message] [nvarchar](255) NULL,
	[ErrorLevel] [int] NOT NULL CONSTRAINT [DF_ApplicationLog_ErrorLevel]  DEFAULT ((0)),
	[Details] [nvarchar](4000) NULL,
	[ErrorType] [nvarchar](50) NULL,
	[StackTrace] [nvarchar](1500) NULL,
	[Url] [nvarchar](255) NULL,
	[QueryString] [nvarchar](255) NULL,
	[IpAddress] [nvarchar](20) NULL,
	[Referrer] [nvarchar](255) NULL,
	[UserAgent] [nvarchar](255) NULL,
	[PostData] [nvarchar](2048) NULL,
	[RequestDuration] [decimal](9, 3) NOT NULL CONSTRAINT [DF_ApplicationLog_RequestDuration]  DEFAULT ((-1))
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CodeSnippets]    Script Date: 4/27/2015 11:13:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CodeSnippets](
	[Id] [nvarchar](40) NOT NULL CONSTRAINT [DF_CodeSnippets_Id]  DEFAULT (''),
	[UserId] [nvarchar](40) NULL CONSTRAINT [DF_CodeSnippets_UserId]  DEFAULT (''),
	[Code] [nvarchar](max) NULL CONSTRAINT [DF_CodeSnippets_Code]  DEFAULT (''),
	[FormattedCode] [nvarchar](max) NULL,
	[Title] [nvarchar](255) NULL CONSTRAINT [DF_CodeSnippets_Title]  DEFAULT (''),
	[Comment] [nvarchar](2000) NULL CONSTRAINT [DF_CodeSnippets_Comment]  DEFAULT (''),
	[Tags] [nvarchar](255) NULL CONSTRAINT [DF_CodeSnippets_Tags]  DEFAULT (''),
	[IsPrivate] [bit] NOT NULL CONSTRAINT [DF_CodeSnippets_IsPrivate]  DEFAULT ((0)),
	[ShowLineNumbers] [bit] NOT NULL CONSTRAINT [DF_CodeSnippets_ShowLineNumbers]  DEFAULT ((0)),
	[Author] [nvarchar](200) NULL CONSTRAINT [DF_CodeSnippets_Name]  DEFAULT (''),
	[Theme] [nvarchar](50) NULL CONSTRAINT [DF_CodeSnippets_Theme]  DEFAULT (''),
	[Language] [nvarchar](40) NULL CONSTRAINT [DF_CodeSnippets_Language]  DEFAULT (''),
	[IsAbuse] [bit] NOT NULL CONSTRAINT [DF_CodeSnippets_IsAbuse]  DEFAULT ((0)),
	[Entered] [datetime] NOT NULL CONSTRAINT [DF_CodeSnippets_Entered]  DEFAULT (getdate()),
	[Views] [int] NOT NULL CONSTRAINT [DF_CodeSnippets_Views]  DEFAULT ((0)),
	[LinkedUrl] [nvarchar](512) NULL,
	[LinkedUrlUpdated] [datetime] NOT NULL CONSTRAINT [DF_CodeSnippets_LiinkedUrlUpdated]  DEFAULT (getdate()),
	[IsTemporary] [bit] NOT NULL CONSTRAINT [DF_CodeSnippets_IsTemporary]  DEFAULT ((0)),
	[Version] [decimal](12, 4) NOT NULL CONSTRAINT [DF_CodeSnippets_Version]  DEFAULT ((1.0)),
	[tstamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_CodeSnippets] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Comments]    Script Date: 4/27/2015 11:13:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Comments](
	[Id] [nvarchar](40) NOT NULL,
	[SnippetId] [nvarchar](40) NOT NULL CONSTRAINT [DF_Comments_SnippetId]  DEFAULT (''),
	[CommentText] [nvarchar](4000) NULL CONSTRAINT [DF_Comments_CommentText]  DEFAULT (''),
	[Author] [nvarchar](50) NOT NULL CONSTRAINT [DF_Comments_UserId]  DEFAULT (''),
	[Entered] [datetime] NOT NULL CONSTRAINT [DF_Comments_Entered]  DEFAULT (getdate()),
	[UserId] [nvarchar](50) NOT NULL CONSTRAINT [DF_Comments_UserId_1]  DEFAULT (''),
	[tstamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_Comments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Favorites]    Script Date: 4/27/2015 11:13:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Favorites](
	[Id] [nvarchar](40) NOT NULL CONSTRAINT [DF_Favorites_Id]  DEFAULT (''),
	[UserId] [nvarchar](40) NOT NULL CONSTRAINT [DF_Favorites_RelatedId]  DEFAULT (''),
	[SnippetId] [nvarchar](40) NOT NULL,
	[Title] [nvarchar](128) NULL CONSTRAINT [DF_Favorites_Title]  DEFAULT (''),
	[Type] [int] NOT NULL CONSTRAINT [DF_Favorites_Type]  DEFAULT ((1)),
	[Entered] [datetime] NOT NULL CONSTRAINT [DF_Favorites_Entered]  DEFAULT (getdate()),
	[tstamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_Favorites] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Lookups]    Script Date: 4/27/2015 11:13:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Lookups](
	[Pk] [int] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Value] [nvarchar](255) NOT NULL,
	[LongValue] [ntext] NULL,
	[IntValue] [int] NOT NULL,
	[DecimalValue] [decimal](12, 2) NOT NULL,
	[tstamp] [timestamp] NOT NULL,
	[Value2] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_Lookups] PRIMARY KEY CLUSTERED 
(
	[Pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SnippetClicks]    Script Date: 4/27/2015 11:13:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SnippetClicks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SnippetId] [nvarchar](20) NOT NULL CONSTRAINT [DF_ViewClicks_SnippetId]  DEFAULT (''),
	[IpAddress] [nvarchar](40) NOT NULL CONSTRAINT [DF_ViewClicks_IpAddress]  DEFAULT (''),
	[Entered] [datetime] NOT NULL CONSTRAINT [DF_ViewClicks_Entered]  DEFAULT (getdate()),
 CONSTRAINT [PK_SnippetClicks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SpamKeywords]    Script Date: 4/27/2015 11:13:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SpamKeywords](
	[Pk] [int] IDENTITY(1,1) NOT NULL,
	[Keyword] [nvarchar](200) NULL,
 CONSTRAINT [PK_SpamKeywords] PRIMARY KEY CLUSTERED 
(
	[Pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserGroups]    Script Date: 4/27/2015 11:13:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserGroups](
	[Id] [nvarchar](40) NOT NULL,
	[UserId] [nvarchar](40) NOT NULL,
	[GroupUserId] [nvarchar](40) NOT NULL,
	[GroupName] [nvarchar](128) NOT NULL,
	[Access] [int] NOT NULL,
	[tstamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_UserGroups] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Users]    Script Date: 4/27/2015 11:13:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [nvarchar](40) NOT NULL,
	[Name] [nvarchar](128) NOT NULL CONSTRAINT [DF_Users_Name]  DEFAULT (''),
	[Email] [nvarchar](255) NOT NULL CONSTRAINT [DF_Users_Email]  DEFAULT (''),
	[Password] [nvarchar](50) NOT NULL CONSTRAINT [DF_Users_Password]  DEFAULT (''),
	[LastLanguage] [nvarchar](50) NOT NULL CONSTRAINT [DF_Users_LastLanguage]  DEFAULT (''),
	[Theme] [nvarchar](50) NULL,
	[InActive] [bit] NOT NULL CONSTRAINT [DF_Users_Active]  DEFAULT ((0)),
	[IsAdmin] [bit] NOT NULL CONSTRAINT [DF_Users_IsAdmin]  DEFAULT ((0)),
	[Visits] [int] NOT NULL CONSTRAINT [DF_Users_Visits]  DEFAULT ((0)),
	[Snippets] [int] NOT NULL CONSTRAINT [DF_Users_Snippets]  DEFAULT ((0)),
	[Updated] [datetime] NOT NULL CONSTRAINT [DF_Users_Updated]  DEFAULT (getdate()),
	[Entered] [datetime] NOT NULL CONSTRAINT [DF_Users_Entered]  DEFAULT (getdate()),
	[OpenId] [nvarchar](128) NOT NULL CONSTRAINT [DF_Users_openid]  DEFAULT (''),
	[OpenIdClaim] [nvarchar](255) NOT NULL CONSTRAINT [DF_Users_OpenIdClaim]  DEFAULT (''),
	[Validator] [nvarchar](20) NULL,
	[tstamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserTokens]    Script Date: 4/27/2015 11:13:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserTokens](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Token] [nvarchar](50) NOT NULL,
	[UserId] [nvarchar](50) NOT NULL,
	[Entered] [datetime] NOT NULL CONSTRAINT [DF_UserTokens_Entered]  DEFAULT (getdate()),
	[Expires] [datetime] NOT NULL CONSTRAINT [DF_UserTokens_Expires]  DEFAULT (getdate()+(1200)),
	[tstamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_UserTokens] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_Users_Email]    Script Date: 4/27/2015 11:13:12 AM ******/
CREATE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  FullTextIndex     Script Date: 4/27/2015 11:13:12 AM ******/
CREATE FULLTEXT INDEX ON [dbo].[CodeSnippets](
[Code] LANGUAGE 'English', 
[Title] LANGUAGE 'English')
KEY INDEX [PK_CodeSnippets]ON ([CodePasteFullText], FILEGROUP [PRIMARY])
WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM)


GO
ALTER TABLE [dbo].[Lookups] ADD  CONSTRAINT [DF_Lookups_Value]  DEFAULT ('') FOR [Value]
GO
ALTER TABLE [dbo].[Lookups] ADD  CONSTRAINT [DF_Lookups_IntValue]  DEFAULT ((0)) FOR [IntValue]
GO
ALTER TABLE [dbo].[Lookups] ADD  CONSTRAINT [DF_Lookups_DecimalValue]  DEFAULT ((0.00)) FOR [DecimalValue]
GO
ALTER TABLE [dbo].[Lookups] ADD  CONSTRAINT [DF_Lookups_Value2]  DEFAULT ('') FOR [Value2]
GO
ALTER TABLE [dbo].[UserGroups] ADD  CONSTRAINT [DF_UserGroups_Id]  DEFAULT ('') FOR [Id]
GO
ALTER TABLE [dbo].[UserGroups] ADD  CONSTRAINT [DF_UserGroups_UserId]  DEFAULT ('') FOR [UserId]
GO
ALTER TABLE [dbo].[UserGroups] ADD  CONSTRAINT [DF_UserGroups_GroupUserId]  DEFAULT ('') FOR [GroupUserId]
GO
ALTER TABLE [dbo].[UserGroups] ADD  CONSTRAINT [DF_UserGroups_GroupName]  DEFAULT ('') FOR [GroupName]
GO
ALTER TABLE [dbo].[UserGroups] ADD  CONSTRAINT [DF_Table_1_GroupAccess]  DEFAULT ((0)) FOR [Access]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Cookie assigned to user. If same user accesses snippet he can edit.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CodeSnippets', @level2type=N'COLUMN',@level2name=N'UserId'
GO
USE [master]
GO
ALTER DATABASE [CodePaste] SET  READ_WRITE 
GO
