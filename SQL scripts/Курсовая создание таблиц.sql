USE RealtyAgency
GO

IF OBJECT_ID('Заявка') IS NOT NULL
DROP TABLE [Заявка];
GO

IF OBJECT_ID('Клиент: физическое лицо') IS NOT NULL
DROP TABLE [Клиент: физическое лицо];
GO

IF OBJECT_ID('Клиент: юридическое лицо') IS NOT NULL
DROP TABLE [Клиент: юридическое лицо];
GO

IF OBJECT_ID('Реализованная недвижимость') IS NOT NULL
DROP TABLE [Реализованная недвижимость];
GO

IF OBJECT_ID('Услуга') IS NOT NULL
DROP TABLE [Услуга];
GO

IF OBJECT_ID('Сотрудник') IS NOT NULL
DROP TABLE [Сотрудник];
GO

IF OBJECT_ID('Клиент') IS NOT NULL
DROP TABLE [Клиент];
GO

IF OBJECT_ID('Отдел') IS NOT NULL
DROP TABLE [Отдел];
GO

IF OBJECT_ID('Должность') IS NOT NULL
DROP TABLE [Должность];
GO


IF OBJECT_ID('Жилая недвижимость аренда') IS NOT NULL
DROP TABLE [Жилая недвижимость аренда];
GO

IF OBJECT_ID('Жилая недвижимость продажа') IS NOT NULL
DROP TABLE [Жилая недвижимость продажа];
GO

IF OBJECT_ID('Жилая недвижимость') IS NOT NULL
DROP TABLE [Жилая недвижимость];
GO

IF OBJECT_ID('Недвижимость') IS NOT NULL
DROP TABLE [Недвижимость];
GO


IF OBJECT_ID('Тип услуги') IS NOT NULL
DROP TABLE [Тип услуги];
GO

IF OBJECT_ID('Цель реализации') IS NOT NULL
DROP TABLE [Цель реализации];
GO

CREATE TABLE [Должность] 
(
	ID int IDENTITY(1,1),
	Название varchar(30) NOT NULL,
  CONSTRAINT [PK_Должность] PRIMARY KEY CLUSTERED (ID ASC),
  CONSTRAINT [Должность_Название_Уникальность] UNIQUE (Название)
)
GO
CREATE TABLE [Отдел] 
(
	ID int IDENTITY(1,1),
	Название varchar(30) NOT NULL,
  CONSTRAINT [PK_Отдел] PRIMARY KEY CLUSTERED (ID ASC),
  CONSTRAINT [Отдел_Название_Уникальность] UNIQUE (Название)
)
GO
CREATE TABLE [Сотрудник] 
(
	ID int IDENTITY(1,1),
	[ID должности] int NOT NULL,
	[ID отдела] int NOT NULL,
	ФИО varchar(100) NOT NULL,
	Паспорт char(10) check (Паспорт like '%[0-9]%') NOT NULL,--
	ИНН char(12) check (ИНН  like '%[0-9]%') NOT NULL,--
	СНИЛС char(11) check (СНИЛС  like '%[0-9]%') NOT NULL,--
	Контакты varchar(70),
	[Дата трудоустройства] date NOT NULL,
	[Дата окончания работы] date,--
  CONSTRAINT [PK_Сотрудник] PRIMARY KEY CLUSTERED (ID ASC),
  CONSTRAINT [FK_Сотрудник_Должность] FOREIGN KEY ([ID должности]) REFERENCES [Должность] (ID),
  CONSTRAINT [FK_Сотрудник_Отдел] FOREIGN KEY ([ID отдела]) REFERENCES [Отдел] (ID),
  CONSTRAINT [Сотрудник_ИНН_Уникальность] UNIQUE (ИНН),
  CONSTRAINT [Сотрудник_Паспорт_Уникальность] UNIQUE (Паспорт),
  CONSTRAINT [Сотрудник_СНИЛС_Уникальность] UNIQUE (СНИЛС)
)
GO

create table Недвижимость
(
	ID int IDENTITY(1,1),
	Детали varchar(500),
	[Тип запроса] int not null default(0),--enum спрос или предложение
	Площадь float,
	Ориентир varchar(50),
	CONSTRAINT [PK_Недвижимость] PRIMARY KEY CLUSTERED (ID ASC),
)
GO

create table [Жилая недвижимость]
(
	ID int IDENTITY(1,1) primary key,
	[ID объекта] int not null constraint object_FK foreign key references Недвижимость(ID) on update cascade on delete cascade , 
	Статус int not null default(0), --enum
	Состояние int not null default(0), --enum
	Район int not null default(0), --enum
	Этаж int not null default(0),--enum
	Улица varchar(100),
	[Номер дома] varchar(5),
	Комфорт int not null default(0), --enum
	[Возраст здания] int not null default(0), --enum
	[Тип дома] int not null default(0),--enum
	[Серия квартиры] int not null default(0),--enum
	[Кол-во комнат] int not null default(0), --enum
	[Этажность] int not null --constraint checkFloors check ([Этаж]<=[Этажность]),--enum
)
ALTER TABLE [Жилая недвижимость] ADD CONSTRAINT floor_check CHECK([Этаж]<=[Этажность]);

GO
create table [Жилая недвижимость аренда]
(
	ID int identity(1,1) primary key,
	[ID ЖН] int not null constraint rp_rent_FK foreign key references [Жилая недвижимость](ID) on update cascade on delete cascade, 
	[Условия аренды] int not null default(0),--enum
)

GO
create table [Жилая недвижимость продажа]
(
	ID int identity(1,1) primary key,
	[ID ЖН] int not null constraint rp_sale_FK foreign key references [Жилая недвижимость](ID) on update cascade on delete cascade , 
	[Условия продажи] int not null default(0)--enum
)
GO

create table Клиент
(
	ID int IDENTITY(1,1) primary key,
	Контакты varchar (50)
)
GO


create table Заявка
(
	ID int IDENTITY(1,1),
	[ID недвижимости] int NOT NULL,
	[ID клиента] int NOT NULL,
	[ID сотрудника] int not null,
	[Дата добавления] datetime not null,
	[Цена вопроса] decimal (19,2) not null,
	[Тип заявки] bit not null default(0),--0 - предложение, 1 - спрос
	[Отметка о выполнении] bit not null default(0),
	CONSTRAINT [PK_Заявка] PRIMARY KEY CLUSTERED (ID ASC),
	CONSTRAINT [FK_Заявка_Недвижимость] FOREIGN KEY ([ID недвижимости]) REFERENCES [Недвижимость] (ID),
	CONSTRAINT [FK_Заявка_Клиент] FOREIGN KEY ([ID клиента]) REFERENCES [Клиент] (ID),
	CONSTRAINT [FK_Заявка_Сотрудник] FOREIGN KEY ([ID сотрудника]) REFERENCES [Сотрудник] (ID)
)
GO

create table [Клиент: юридическое лицо]
(
	ID int IDENTITY(1,1) primary key,
	[ID клиента] int constraint Client_J_FK foreign key references Клиент(ID) on update no action on delete no action,
	Наименование varchar(50) not null unique
)
GO

create table [Клиент: физическое лицо]
(
	ID int IDENTITY(1,1) primary key,
	[ID клиента] int constraint Client_P_FK foreign key references Клиент(ID) on update no action on delete no action,
	ФИО varchar(60) not null, 
	Паспорт char(10) check (Паспорт like '%[0-9]%' or Паспорт = null)
)	
GO
CREATE UNIQUE INDEX uniqueClientPassport ON [Клиент: физическое лицо](Паспорт) WHERE Паспорт IS NOT NULL;




CREATE TABLE [Тип услуги]
(
	ID int IDENTITY (1,1) constraint PK_ServiceType primary key,
	[Тип услуги] varchar(30) not null constraint UQ_ServiceType unique
)
GO
CREATE TABLE [Услуга] 
(
	ID int IDENTITY(1,1),
	[ID клиента] int NOT NULL,
	[ID сотрудника] int NOT NULL,
	[Сумма комиссии] decimal (19,2) NOT NULL,
	[Дата выполнения] date NOT NULL,
	[ID типа услуги] int NOT NULL  constraint FK_ServiceType foreign key references [Тип услуги](ID) on delete no action on update cascade,
	[Дата получения комиссии] date NOT NULL,
  CONSTRAINT [PK_Услуга] PRIMARY KEY CLUSTERED (ID ASC),
  CONSTRAINT [FK_Услуга_Клиент] FOREIGN KEY ([ID клиента]) REFERENCES [Клиент] (ID),
  CONSTRAINT [FK_Услуга_Сотрудник] FOREIGN KEY ([ID сотрудника]) REFERENCES [Сотрудник] (ID)
)
GO

CREATE TABLE [Реализованная недвижимость]
 (
	[ID услуги] int,
	[ID недвижимости] int,
	[Сумма реализации] decimal (19,2) NOT NULL,
	CONSTRAINT [FK_Реализованная_недвижимость_Услуга] FOREIGN KEY ([ID услуги]) REFERENCES [Услуга] (ID), 
	CONSTRAINT [FK_Реализованная_недвижимость_Недвижимость] FOREIGN KEY ([ID недвижимости]) REFERENCES [Недвижимость] (ID),
	CONSTRAINT [PK_Реализованная_недвижимость] PRIMARY KEY CLUSTERED ([ID услуги],[ID недвижимости] ASC)
)
GO
create table [Цель реализации]
(
	ID int IDENTITY (1,1) constraint PK_DRT primary key, 
	[Тип цели] varchar(30) not null constraint UQ_DRT unique
)

--create table Спрос
--(
--	ID int IDENTITY(1,1),
--	[ID недвижимости] int NOT NULL,
--	[ID клиента] int NOT NULL,
--	[ID сотрудника] int not null,
--	[Дата добавления] datetime not null,
--	[Начальная цена] decimal (19,2) not null,
--	[Конечная цена] decimal (19,2) not null,
--	CONSTRAINT [PK_Спрос] PRIMARY KEY CLUSTERED (ID ASC),
--	CONSTRAINT [FK_Спрос_Недвижимость] FOREIGN KEY ([ID недвижимости]) REFERENCES [Недвижимость] (ID),
--	CONSTRAINT [FK_Спрос_Клиент] FOREIGN KEY ([ID клиента]) REFERENCES [Клиент] (ID)
--	CONSTRAINT [FK_Спрос_Сотрудник] FOREIGN KEY ([ID сотрудника]) REFERENCES [Сотрудник] (ID)
--)
--GO

--create table Предложение
--(
--	ID int IDENTITY(1,1),
--	[ID недвижимости] int NOT NULL,
--	[ID клиента] int not null,
--	[ID сотрудника] int not null,
--	[Желаемая сумма] decimal (19,2) NOT NULL,
--	[Дата добавления] datetime not null,
--	CONSTRAINT [PK_Предложение] PRIMARY KEY CLUSTERED (ID ASC),
--	CONSTRAINT [FK_Предложение_Недвижимость] FOREIGN KEY ([ID недвижимости]) REFERENCES [Недвижимость] (ID),
--	CONSTRAINT [FK_Предложение_Клиент] FOREIGN KEY ([ID клиента]) REFERENCES [Клиент] (ID),
--	CONSTRAINT [FK_Предложение_Сотрудник] FOREIGN KEY ([ID сотрудника]) REFERENCES [Сотрудник] (ID)
--)