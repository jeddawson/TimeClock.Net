﻿CREATE TABLE [Employee] (
    [EmployeeID] [nvarchar](128) NOT NULL,
    [FirstName] [nvarchar](max),
    [LastName] [nvarchar](max),
    [Terminated] [bit] NOT NULL,
    [Pin] [nvarchar](max),
    [DepartmentID] [int] NOT NULL,
    [ManagerID] [nvarchar](128),
    CONSTRAINT [PK_Employee] PRIMARY KEY ([EmployeeID])
)
CREATE INDEX [IX_ManagerID] ON [Employee]([ManagerID])
CREATE INDEX [IX_DepartmentID] ON [Employee]([DepartmentID])
CREATE TABLE [Punch] (
    [PunchID] [int] NOT NULL IDENTITY,
    [InTime] [datetime] NOT NULL,
    [OutTime] [datetime],
    [DepartmentID] [int] NOT NULL,
    [EmployeeID] [nvarchar](128),
    [PunchTypeID] [int] NOT NULL,
    CONSTRAINT [PK_Punch] PRIMARY KEY ([PunchID])
)
CREATE INDEX [IX_EmployeeID] ON [Punch]([EmployeeID])
CREATE INDEX [IX_DepartmentID] ON [Punch]([DepartmentID])
CREATE INDEX [IX_PunchTypeID] ON [Punch]([PunchTypeID])
CREATE TABLE [Line] (
    [LineID] [int] NOT NULL IDENTITY,
    [PunchID] [int] NOT NULL,
    [TimecardID] [int] NOT NULL,
    [SplitStart] [datetime] NOT NULL,
    [SplitEnd] [datetime] NOT NULL,
    [PayTypeID] [int] NOT NULL,
    CONSTRAINT [PK_Line] PRIMARY KEY ([LineID])
)
CREATE INDEX [IX_PayTypeID] ON [Line]([PayTypeID])
CREATE INDEX [IX_PunchID] ON [Line]([PunchID])
CREATE INDEX [IX_TimecardID] ON [Line]([TimecardID])
CREATE TABLE [PayType] (
    [PayTypeID] [int] NOT NULL IDENTITY,
    [DailyMax] [int] NOT NULL,
    [WeeklyMax] [int] NOT NULL,
    [Description] [nvarchar](max),
    [NextPayType_PayTypeID] [int],
    [Department_DepartmentID] [int],
    CONSTRAINT [PK_PayType] PRIMARY KEY ([PayTypeID])
)
CREATE INDEX [IX_NextPayType_PayTypeID] ON [PayType]([NextPayType_PayTypeID])
CREATE INDEX [IX_Department_DepartmentID] ON [PayType]([Department_DepartmentID])
CREATE TABLE [MessageViewed] (
    [EmployeeID] [int] NOT NULL,
    [MessageID] [int] NOT NULL,
    [DateViewed] [datetime],
    [Employee_EmployeeID] [nvarchar](128),
    CONSTRAINT [PK_MessageViewed] PRIMARY KEY ([EmployeeID], [MessageID])
)
CREATE INDEX [IX_Employee_EmployeeID] ON [MessageViewed]([Employee_EmployeeID])
CREATE TABLE [Timecard] (
    [TimecardID] [int] NOT NULL IDENTITY,
    [EmployeeID] [nvarchar](128),
    [PayPeriod] [datetime] NOT NULL,
    CONSTRAINT [PK_Timecard] PRIMARY KEY ([TimecardID])
)
CREATE INDEX [IX_EmployeeID] ON [Timecard]([EmployeeID])
CREATE TABLE [Message] (
    [MessageID] [int] NOT NULL IDENTITY,
    [Body] [nvarchar](max),
    [ManagerID] [nvarchar](max),
    [Manager_EmployeeID] [nvarchar](128),
    CONSTRAINT [PK_Message] PRIMARY KEY ([MessageID])
)
CREATE INDEX [IX_Manager_EmployeeID] ON [Message]([Manager_EmployeeID])
CREATE TABLE [Company] (
    [CompanyID] [int] NOT NULL IDENTITY,
    [Name] [nvarchar](max),
    CONSTRAINT [PK_Company] PRIMARY KEY ([CompanyID])
)
CREATE TABLE [Department] (
    [DepartmentID] [int] NOT NULL IDENTITY,
    [Name] [nvarchar](max),
    [Location] [nvarchar](max),
    [PayPeriodSeed] [datetime] NOT NULL,
    [PayPeriodInterval] [int] NOT NULL,
    [Company_CompanyID] [int],
    CONSTRAINT [PK_Department] PRIMARY KEY ([DepartmentID])
)
CREATE INDEX [IX_Company_CompanyID] ON [Department]([Company_CompanyID])
CREATE TABLE [Holiday] (
    [HolidayID] [int] NOT NULL IDENTITY,
    [Description] [nvarchar](max),
    [Date] [datetime] NOT NULL,
    [Repeats] [int] NOT NULL,
    CONSTRAINT [PK_Holiday] PRIMARY KEY ([HolidayID])
)
CREATE TABLE [PunchType] (
    [PunchTypeID] [int] NOT NULL IDENTITY,
    [Description] [nvarchar](max),
    [PunchInOption] [nvarchar](max),
    CONSTRAINT [PK_PunchType] PRIMARY KEY ([PunchTypeID])
)
CREATE TABLE [EmployeeMessages] (
    [EmployeeID] [nvarchar](128) NOT NULL,
    [MessageID] [int] NOT NULL,
    CONSTRAINT [PK_EmployeeMessages] PRIMARY KEY ([EmployeeID], [MessageID])
)
CREATE INDEX [IX_EmployeeID] ON [EmployeeMessages]([EmployeeID])
CREATE INDEX [IX_MessageID] ON [EmployeeMessages]([MessageID])
CREATE TABLE [HolidayDepartment] (
    [Holiday_HolidayID] [int] NOT NULL,
    [Department_DepartmentID] [int] NOT NULL,
    CONSTRAINT [PK_HolidayDepartment] PRIMARY KEY ([Holiday_HolidayID], [Department_DepartmentID])
)
CREATE INDEX [IX_Holiday_HolidayID] ON [HolidayDepartment]([Holiday_HolidayID])
CREATE INDEX [IX_Department_DepartmentID] ON [HolidayDepartment]([Department_DepartmentID])
ALTER TABLE [Employee] ADD CONSTRAINT [FK_Employee_Employee_ManagerID] FOREIGN KEY ([ManagerID]) REFERENCES [Employee] ([EmployeeID])
ALTER TABLE [Employee] ADD CONSTRAINT [FK_Employee_Department_DepartmentID] FOREIGN KEY ([DepartmentID]) REFERENCES [Department] ([DepartmentID]) ON DELETE CASCADE
ALTER TABLE [Punch] ADD CONSTRAINT [FK_Punch_Employee_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [Employee] ([EmployeeID])
ALTER TABLE [Punch] ADD CONSTRAINT [FK_Punch_Department_DepartmentID] FOREIGN KEY ([DepartmentID]) REFERENCES [Department] ([DepartmentID]) ON DELETE CASCADE
ALTER TABLE [Punch] ADD CONSTRAINT [FK_Punch_PunchType_PunchTypeID] FOREIGN KEY ([PunchTypeID]) REFERENCES [PunchType] ([PunchTypeID]) ON DELETE CASCADE
ALTER TABLE [Line] ADD CONSTRAINT [FK_Line_PayType_PayTypeID] FOREIGN KEY ([PayTypeID]) REFERENCES [PayType] ([PayTypeID]) ON DELETE CASCADE
ALTER TABLE [Line] ADD CONSTRAINT [FK_Line_Punch_PunchID] FOREIGN KEY ([PunchID]) REFERENCES [Punch] ([PunchID]) ON DELETE CASCADE
ALTER TABLE [Line] ADD CONSTRAINT [FK_Line_Timecard_TimecardID] FOREIGN KEY ([TimecardID]) REFERENCES [Timecard] ([TimecardID]) ON DELETE CASCADE
ALTER TABLE [PayType] ADD CONSTRAINT [FK_PayType_PayType_NextPayType_PayTypeID] FOREIGN KEY ([NextPayType_PayTypeID]) REFERENCES [PayType] ([PayTypeID])
ALTER TABLE [PayType] ADD CONSTRAINT [FK_PayType_Department_Department_DepartmentID] FOREIGN KEY ([Department_DepartmentID]) REFERENCES [Department] ([DepartmentID])
ALTER TABLE [MessageViewed] ADD CONSTRAINT [FK_MessageViewed_Employee_Employee_EmployeeID] FOREIGN KEY ([Employee_EmployeeID]) REFERENCES [Employee] ([EmployeeID])
ALTER TABLE [Timecard] ADD CONSTRAINT [FK_Timecard_Employee_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [Employee] ([EmployeeID])
ALTER TABLE [Message] ADD CONSTRAINT [FK_Message_Employee_Manager_EmployeeID] FOREIGN KEY ([Manager_EmployeeID]) REFERENCES [Employee] ([EmployeeID])
ALTER TABLE [Department] ADD CONSTRAINT [FK_Department_Company_Company_CompanyID] FOREIGN KEY ([Company_CompanyID]) REFERENCES [Company] ([CompanyID])
ALTER TABLE [EmployeeMessages] ADD CONSTRAINT [FK_EmployeeMessages_Employee_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES [Employee] ([EmployeeID]) ON DELETE CASCADE
ALTER TABLE [EmployeeMessages] ADD CONSTRAINT [FK_EmployeeMessages_Message_MessageID] FOREIGN KEY ([MessageID]) REFERENCES [Message] ([MessageID]) ON DELETE CASCADE
ALTER TABLE [HolidayDepartment] ADD CONSTRAINT [FK_HolidayDepartment_Holiday_Holiday_HolidayID] FOREIGN KEY ([Holiday_HolidayID]) REFERENCES [Holiday] ([HolidayID]) ON DELETE CASCADE
ALTER TABLE [HolidayDepartment] ADD CONSTRAINT [FK_HolidayDepartment_Department_Department_DepartmentID] FOREIGN KEY ([Department_DepartmentID]) REFERENCES [Department] ([DepartmentID]) ON DELETE CASCADE
CREATE TABLE [__MigrationHistory] (
    [MigrationId] [nvarchar](255) NOT NULL,
    [CreatedOn] [datetime] NOT NULL,
    [Model] [varbinary](max) NOT NULL,
    [ProductVersion] [nvarchar](32) NOT NULL,
    CONSTRAINT [PK___MigrationHistory] PRIMARY KEY ([MigrationId])
)
BEGIN TRY
    EXEC sp_MS_marksystemobject '__MigrationHistory'
END TRY
BEGIN CATCH
END CATCH
INSERT INTO [__MigrationHistory] ([MigrationId], [CreatedOn], [Model], [ProductVersion]) VALUES ('201205280032303_InitialCreate', '2012-05-28T00:35:18.924Z', 0x1F8B0800000000000400E51DCB8EDCB8F11E20FFD0E8531260A7C70EB0D8357A76E19DB19341D60FB81DE738A0BB396361D55247523B33DF96433E29BF103D293EAAF81225753BB719912C168B5545B2AABAEABFFFFECFFAE7C77DBCF84AB33C4A93ABE5B38BCBE58226DB7417250F57CB6371FFDD0FCB9F7FFAFDEFD6AF76FBC7C5A7AEDFF3AA5F3932C9AF965F8AE2F062B5CAB75FE89EE417FB689BA5797A5F5C6CD3FD8AECD2D5F3CBCB1F56CF2E57B404B12C612D16EB0FC7A488F6B4FEA7FCF73A4DB6F4501C49FC26DDD1386FBF972D9B1AEAE22DD9D3FC40B6F46AF9B11C771DA7DBDF2E9ABECBC5CB3822251E1B1ADF3B2275F96385D4924D574EF8AA44AC78FAF874A0F5A457CB57FB439C3E51CAF72AFBFD8D3E091FCA4FEFB3F440B3E2E903BD97C6DEDE2C172B71FC4A06C08603632B74CA051659B92BCBC5EBE891EE7EA5C943F1E56A794FE29C2E176FC863F7E5D9F31F968BBF2751B989E5A0223B96CD6F8F714C3EC794F55F69E77E1D657951FDE93875F9A766EAE67FFDCCBF929926FE48B37D949082EEBAA97F49D39892C49978EFA36472EC6FE88164C59E2645CF2EB749F1E7E7CED8BF210979A05960AE03D7F0967C8D1E48512A141887E5E2038DEBF6FC4B746804FCA2138B3BD6E97596EE3FA43127325DDBDD263D66DB8A9752A4C347923DD0C21EB3F7C7A4D428B91E33D609C0AC6DC331EB3AB862F629A2FFA2BB3734CFCB85191094FB02788A5D7074A57EAE5857CA7C4BB29D0161AE1B802B6BC5D1ECBBB8626847512D2DCD54D4D26FBDEACF24ED4955B38ECF31550FF439A3D840BDC6397CFF6253A419FD0B4D6856A9D8F7A4286856DE236E77B45E457B6EBF387C6F7774FFB8BA7C5E1DDD2B922469516F8B49B1DD26151374A8DE946834FF3BEAC777C7C204675A853DD23DC16611F5F657733AAF0115B75FA30491B57AB2BBB6BD9732EEB3225F7CDB20C9AA40F8085635CE47AEBA71672056962AC070F96AD5F350389B431C159BA294ACC1725E837A95EC06037A4F9EC28A480B1014928A71EE58875E4AF8EF8A98088DC34EA076669F33A82793F329644BE19310981B12C54FF58D7F08A7FF83D2DF0280B9A1F9368B0E0DDE63BF59508E7E4B1F0B1D5777BC29F4E38E00B5593D0A803E8358BDBDB235F7DFB0B601B07B3B5F184B82E7D3B0476110CF9592D792CDE91265BD35DD69E2B32BFC49E44A67FB53EC2414D19C1747F2F49E6651EA71BA7ADC1CBB7D512F8F628BA234A4E610FAC287270788BEB5C89E0447FE92EE9E26379DF91ABC421E803A8357BB8590BD4B6A52F8576E77B57D741A2288F1A39B1C377E74E87BC9D775BA3F90E4C947BEDAA13EF2C50D3D03F99AC0B06EBD5FBD01C467CB44F389EBAEB9185FFE5F360E3042A45B32CDC300BB1D6CA8FE8E68FBFE6EA0953B4CB3AF240EF50EEF340EA41B7B0EBB63DD7AE5A8B62AAA1BE812567B7313701D411C59BB0ECBBE53489F0A3701E055515B7518FA7A56FE9AC6D18E3CC108B68D77FD3C028640B37210427DA0A3D0C226642622EB0753B16DD692B1EB33E83ADC2EDA47F7B7437D143F37F40CB4FEA4C619E0913E58F57EA0074A2A8108A37005190B238B329FEBE4D5DF27E76D13E5FD2B5EBEB973B28BCEC8ED8D13237937B721547714B2FD844E42A511F685093D5CF8FA659EA7DBA84649BA5DB077A9B8C457C96E610ACA68A8CC3F254BC21EE3223AC4D1B644E26AF92785741AB8EC5C35C3BDBCB878A6802EE58E6615BB93F8BAA47C9191282954218D926D7420B1010B699CBB0578C566925B4ADD44934A2E0D04B64181337EA818B08924CD6322D47AC5B18B9E8B20CB3EB6E15A337FBFE7CC4160CF4A3AE78019B2CA4C0E04101C76187EB0F7AE47AC764F3BAC1774F89917FA4C66A9F5BBE486C6B4A08B97DBA28E0BBD26F996EC548D57AA955D00698310B76173D4B9E82468D0360C9E7D0A21E32227501180C228388EA8438BC6E40718158039ED98DD8BBD002258ED2F1C3EE5C45BC0A2074E3D016329118FC6B35AB99D789ED51AD0903E83B83710CF604B9BE302209160380A53F29014946ADC6F2C42351C4721B1ADDC0C6210809EC11C6822392C314431EF658F1FF3844FA3B9117FE934CA1BA6868D0CE03E7F273184573F1C812985B00FB4364A0710751D4EF4D4786D0BAE0EADD19515CEA1D3154A9C8556979DD6D886A31E6C45C7BABC6F30BFB71783FA8891FD29E6737ED95945CCC756B8030B70776108EA7C5F3D869C77D861F11AA719079B39E402EE3CE84DB3401370ADD91061A4F35BEBCEF3624C2F05ACA38F8DF6D305073829611D3D4220328122061CA816BBAF79A6CDC994333DF170C2CCC58D1E0FBD13E045C83F87EDBAD659D76F7BE746763824B42E798F13C853183B47BC0DDB2B5E791B340D576D9D47DFD99AEE6A8C14DC585A3B20ECD392E47E7463B50EA98995114A156B7361004B384A8500480457448D47B31C53942368C6BDB9EB341255037D94A3109B411B5AA8C154BD8754B90E29D22602618E5B0544CB3186F16DFCBA32BAB1AA982667A13FEAEC9D901B40746F8ACEDEA50092EC610670DCAFBA1548FD1BDF12271C1B2388E641108130D863C10043883951A0F05ADA00A88F3653A0B0C3CE86CDD0BDEE15A60486131F95EFFBF073AE1B96924196698B6000B60641D414ED60E1FEB78124E88AB2AB0521C0DFA0A9B4303AB4C5D344E3D2E6D6D18BAE86203A27B6052C0F92883F36556981FBB605C441EF368771ABF5344B07FDD9E3AC59F8153AB0FD98AB55BD4448567B1EDDEE98D0ED36E02435D1CC63B96A4E138DF883173A5868E51B9DBFF8CBD730331D879041CE9CA2A186CE9FE5E4D1F2A70DE2C0E200CA27FA604AC9BFB55309A4736ED9B8B738ECB90B84861C88576A4C69E110D37008E26CB175B7F8F385EA5DB121AA074194DFAEA9D4D07A0AAC7C05003B6BC980790746BA38003F93D35DA11CF445404D61A32302D002FA598C4A0C9337C1D69FC0AD41B89E6B08A2F12070D0B807434892707BA6250AE261B0F7310C238CEA1A184972A01FFF680963BE80686CDCC38832C93504FC81834A11A39DD5DAD2CAADA27F976A08A2B3ADDAD177209BB08BBE9E4F40E3ABB5F97520A7C8D6D6111F2AE2CF0590E70A6E8CB534C7CABC6EF15645ADA883E4A6FBF502B3F5B1B6F5AA4924DB7E58AF908CB3EB37E4708892072E036DFB65B169D2CF5E7FB7714F32BB6F60ACB6028165CB249BA948B3F2C0955AAB636747EBCCAC37A4209F49F53B94EBDD5EE9666BD9ECA6930D9CEAB67526A46E44F5B764456D93F1F6B650D512DC8E7E5DAEAD12847A9914383AD4A18B2A0F308949A6C960729DC6C77D620E7FC1A171496F7960DC677B587D1A5B1E54FFD51E129F979687C57FB78756E7A6E5C1D41FECC78B5E441E90DEBF8843E47E81C283D3FC30A5125F8997142782C2B38A874714022B11C175A4838000493A6B2026E940C6A11BDD05860B9B8D458BE370BAAC993C98EE9B3D1496349307C33ECEC97E615588E0DA52288FF9BC666368CC02E3C0CE6A66CC1A86899BE161A8366DD3610ABA144CAD69DC9C0022C147EB0A1A5913C58B43E35356F2D0F8EF8ED0EAAC950AACFAAB03B5FA1F2D09F4C27FCB349F5EC6AFF32E8A197654D8A8666C6448DAA22A91A59314D421FB6A0F894B29C983E23EBB286AEEC7DCA29EE61A4E86832433F8203ED22486B4E226C3F8690E322E6F9C702BC332D1697981CBF728F26881440ACCC8093A9BBC0313C02928ADF61F1F3ACD8114F846D4677A94545DF7F964B65E637A77177F7FC1B7DFF79042DAA45EE4C1345FBEC9875C6FBF1FB4D58873C262ABD1911871B93C833C71D1CC853A58AA5D02B349CCB43D7ABBB5C306613906ADF6483778BAF7A9FD66A18F2796C64F783EB1AF1EBABCC9CB07EAF3A6C903669F9D0F84DB379F0C9B328FC8201E0513A1D5604C0C8A8EC468CD653FE3698CE653D3C1F2BDE0EB2E89EAF5D005024B36C603611F4F866B380FC9703BA2EF83151F1BDAB23515FF4819BC54130F6B9A9E13447F1568BB10C34ACD560AB1BFA541A272C0C1960828685425938B2D8307A8CD7FC3E1E4872EFAF38740A6172D8AD769B28BEA5FB7DCE655D2379620CE9916B25BD39993D018450B6F9D32C6C928016C9421A6D172B34C4FD13BDD9BD491B50C41938130469E41819E77C359D582B2831955090A34BFA6FBBE968F66608391B0BF811BDB410BC889489CE309738D9908E1B49B9B5EC3355AD7C59E81D0B8CB117583AF12F3545F219967F09643C1A2568FFDBEBBFD9B1E20261E0B6A494DBDD1E64E63BC71DC763C06D612535FBBC570D56124C66036028326CD0F72B13FF2F676E2274DA0A5E536E91FF0779A87BC234369E2380372D49D2F7385542D7DC8A59D6EE9FBFBBFBA34C1A5A7405E6B9C83BC14477D7A5952C3CC504A68A9DC851911DA2FEC7F165ADA86750AF1A635ADAAE8D19A46791B622AC779365D968B92145FA35D15E3B979CA0BBABFA83A5C6CFE195FC751A5835887F24216DDD3BCF898FE4693AB651586BA5CBC8C23923781BF4E11AC2C977B9EEF62207EB5DA3DE9FE11B61EA24F85C3E42BC9B65F48A616AE73AC61C0857F8A90FFB0278F7F74AEA9430202E343401B709FA3C2BD3E4E9404C1062AAA1425EEF828F5D86CB6724021AAD9ABA6D74432D65F70A4A258ED7C57022D02543B87E04CC91B7E72EE59CADC8CE099D40F1F85C120261E5CF5DB078A5AF3DB9BDBE59ADFDE80948AD40179E9446A6C8FC254726D6C1F7E502A63FB00018AD10C3A1C11170088A09B42BDB3D0AD034EC7B32E73ED75FD902BE67AB18F52E2DAF7E4040DF293DC8B4EA78AF628AA66C4AB845CDCDAF21839B752D2A36C0B5F027A90C6459F1143A04D2F852752D67894BDF67C089F6D89E1532222F06690CAFD0E020696F01D729D96A300FDCF66C0E310FAD27422C54DC7B9A207BE19F38546BD19442A341AF2B5754AF53BCF6243C15A9A63A877AD07FFF41F27C12CD41E2F16574D36EC24B5F14FEACE5F83EBC6522FDEC1FAD1595A5C1EDE0EA40F50613564059EC64B64012F70D19D396AECD46BB59977EE6AA99E5546E6ADDFE0C49DC12B364C5BA2C1898B4FA022837301C7102AA6CD75CE019AA23EE3948A05CBF1E23AE1A9D6879957A14CCB3F73A9127B1E3A013DE251D463FE2A1ED397EC98BE46C74916E4F0AC822EFC3C6AC24AE5629EF9C96A51A3E9681CFCBA4E0C83A472729C6E2A6D6353CF7CB612E693328A9B9C4FC624B3562677ACA03C67D1E42979459396C6C5893B0EC7CC5E05D958BBC5EF30323D879A518AC1C5AEF2DAF91C2FFC3A6D66B5FE05F1C4AF24FBC27D610AF53970DAB93F791C78D2E1670E53DA5174455F4632A74835D7388052CB37635E3165D8C32736FC4C774A4E4112CD8DC4244E179D33660DA70BCEECCCA0AD7D33AEBAF0A8077F4E7C8067E143BD3AF3EB0653DD1B6E17851F44BBD6B51F765D09C41578F23C874836279E70BCD658FD4C7BCA13439B76C0575B8CECF483B48DD2F6CD681D434E89D33C84EC7246F89D20DF2C77399D2FBAEC19A3F29676E60958CB36E3815A414AD852F66D12E652439E005C463F1EF1B4900E31A34E0C664C69A19B5B9F7B62265633E8B153F1679F06C3CD65F4F163BB39CD3FAFC44410ECBA2817E792F7BB4D14221DBB4089F6BEA949FC70B5DC7D4E4BA668620AFB56E54E2FCED03ABC14F0ED770876DB6400DC382814B8CD67086CD36242B7B369AA08B33C9710CA5DA301BC640C532691DAA1A9A42E86097BEB8A3257DF044DD3B7DA2D095F8C761946F0EC3DA980EFF36B01E059A3013CAFD19419F8466812BEDD300FBB3D2893B0166806D668236518E3F66DA8B45931AF72A945758676E7A54E6A69458476DAAD02FA68E8A9D93843F564829690D6074A236F63B6545CA382231B3F9DC5E853AC058CD42EB55AF8588B566250753B6C2E53EABFC3C279C9D5DC0BBAAFE10B19FBECE9784B1DAFEC2C5C6776A6650A016BC00AF180361141F1CE23A491D32D8CBF81F5F5F402ED5E1B3383ED1B1452E343F5491624C501016BD2450A0928CAF739B158D32CBB05E5820776CD14D6128425853016F3D8D3ABF3EDA547ED8935E46494B3B36B0E485D0042A073127C490159E20312A07796EBD6AEABDD367CD94EF2EFB15825D5B9BA52ADBF38ECEE42FB1A5686D93B512BC2889B1072857208236FD061923F848165DF9DF615A34F2F1EF43503EDB6D23602195AD3AC1519E02CB37E2C3B231134CE10800CF6C9A221E709B710C48E61B240AB10820A86C65C6F490C23570C3BD9C72389924998B5AD578DE1A4FD50FEAB640C5EAF3E1C932A4546F3DF0DCDA3871E44951339A15BC160CDFADC26F76967489730EABA2889050AB22305799915D13DD91665F3B614872879582E3E91F8588BD067BABB4DDE1D8BC3B128974CF79F6381D72AFBBB6EFEF54AC179DDA48EC8432CA14433AAB28ABC4B7E3946F18EE1FD1AF8613E02A232ECB7E936AABD2CAAB41B0F4F0CD2DB54BECB63805AF2317FC4475AEA981258FE2ED990AF14C7CD4C439162EB9B883C6464CF53B0F9D262B221E5CCDC14E504FC887EBEF2DF925D77FBC79FFE0758C5780D2BDC0000, '4.3.1')
