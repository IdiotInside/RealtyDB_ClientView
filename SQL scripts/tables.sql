use RealtyAgency
GO

create table ������������
(
	ID int IDENTITY(1,1),
	������ varchar(500),
	[��� �������] int not null default(0),--enum ����� ��� �����������
	������� float,
	�������� varchar(50),
	CONSTRAINT [PK_������������] PRIMARY KEY CLUSTERED (ID ASC),
)
GO

create table [����� ������������]
(
	ID int IDENTITY(1,1) primary key,
	[ID �������] int not null constraint object_FK foreign key references ������������(ID) on update cascade on delete cascade , 
	������ int not null default(0), --enum
	��������� int not null default(0), --enum
	����� int not null default(0), --enum
	���� int not null default(0),--enum
	����� varchar(100),
	[����� ����] varchar(5),
	������� int not null default(0), --enum
	[������� ������] int not null default(0), --enum
	[��� ����] int not null default(0),--enum
	[����� ��������] int not null default(0),--enum
	[���-�� ������] int not null default(0), --enum
	[���������] int not null --constraint checkFloors check ([����]<=[���������]),--enum
)
ALTER TABLE [����� ������������] ADD CONSTRAINT floor_check CHECK([����]<=[���������]);

GO
create table [����� ������������ ������]
(
	ID int identity(1,1) primary key,
	[ID ��] int not null constraint rp_rent_FK foreign key references [����� ������������](ID) on update cascade on delete cascade, 
	[������� ������] int not null default(0),--enum
)

GO
create table [����� ������������ �������]
(
	ID int identity(1,1) primary key,
	[ID ��] int not null constraint rp_sale_FK foreign key references [����� ������������](ID) on update cascade on delete cascade , 
	[������� �������] int not null default(0)--enum
)
GO

create table ������
(
	ID int IDENTITY(1,1) primary key,
	�������� varchar (50)
)
GO

create table �����
(
	ID int IDENTITY(1,1),
	[ID ������������] int NOT NULL,
	[ID �������] int NOT NULL,
	[��������� ����] decimal (19,2) not null,
	[�������� ����] decimal (19,2) not null,
	[���� ����������] datetime not null,
	CONSTRAINT [PK_�����] PRIMARY KEY CLUSTERED (ID ASC),
	CONSTRAINT [FK_�����_������������] FOREIGN KEY ([ID ������������]) REFERENCES [������������] (ID),
	CONSTRAINT [FK_�����_������] FOREIGN KEY ([ID �������]) REFERENCES [������] (ID)
)
GO

create table �����������
(
	ID int IDENTITY(1,1),
	[ID ������������] int NOT NULL,
	[ID �������] int not null,
	[ID ����������] int not null,
	[�������� �����] decimal (19,2) NOT NULL,
	[���� ����������] datetime not null,
	CONSTRAINT [PK_�����������] PRIMARY KEY CLUSTERED (ID ASC),
	CONSTRAINT [FK_�����������_������������] FOREIGN KEY ([ID ������������]) REFERENCES [������������] (ID),
	CONSTRAINT [FK_�����������_������] FOREIGN KEY ([ID �������]) REFERENCES [������] (ID),
	CONSTRAINT [FK_�����������_������] FOREIGN KEY ([ID ����������]) REFERENCES [���������] (ID)
)
GO

create table [������: ����������� ����]
(
	ID int IDENTITY(1,1) primary key,
	[ID �������] int constraint Client_J_FK foreign key references ������(ID) on update no action on delete no action,
	������������ varchar(50) not null unique
)
GO

create table [������: ���������� ����]
(
	ID int IDENTITY(1,1) primary key,
	[ID �������] int constraint Client_P_FK foreign key references ������(ID) on update no action on delete no action,
	��� varchar(60) not null, 
	������� char(10) check (������� like '%[0-9]%' or ������� = null)
)	
GO
CREATE UNIQUE INDEX uniqueClientPassport ON [������: ���������� ����](�������) WHERE ������� IS NOT NULL;



--[���������� �� ������ � ��] int, --�� ��� ��������� ��������
--[�������� �� �������] bit, --�� ��� ��������� ��������
--[������ ��������] varchar(150), �� ��� ��������� ��������
--���������� varchar(50),
--[��� ������ ���������] varchar(150),
--[��� ����������� ����] varchar(150),
--[��� ������������ ������������] varchar(150),
--[��� �������� ���������] varchar(150),
--[��� ������������ �����] varchar(150),
--[��� ���������� �������] varchar(150),
--[������ ������ ���������] varchar(150),
--[�������� ��] varchar(150),
--[�������� ��] varchar(150),
--[�������� ��] varchar(150),
--[������� ������] varchar(40),
--[��� ������] varchar(60), --���, ������ �����, ������� ������, ����� ���
--[��� ����] varchar(60),--���������/��/�
--[����� ��������] varchar(200),
--[��� �����] varchar(150),
--[������� ���] varchar(150),
--[������� ���] varchar(150),
--[���-�� ������] smallint, 
--[���-�� ������ � ����] varchar(20),
--[���� ���������] tinyint,
--������� varchar(150),
	