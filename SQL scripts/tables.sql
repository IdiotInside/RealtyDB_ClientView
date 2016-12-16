use RealtyAgency
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

create table Спрос
(
	ID int IDENTITY(1,1),
	[ID недвижимости] int NOT NULL,
	[ID клиента] int NOT NULL,
	[Начальная цена] decimal (19,2) not null,
	[Конечная цена] decimal (19,2) not null,
	[Дата добавления] datetime not null,
	CONSTRAINT [PK_Спрос] PRIMARY KEY CLUSTERED (ID ASC),
	CONSTRAINT [FK_Спрос_Недвижимость] FOREIGN KEY ([ID недвижимости]) REFERENCES [Недвижимость] (ID),
	CONSTRAINT [FK_Спрос_Клиент] FOREIGN KEY ([ID клиента]) REFERENCES [Клиент] (ID)
)
GO

create table Предложение
(
	ID int IDENTITY(1,1),
	[ID недвижимости] int NOT NULL,
	[ID клиента] int not null,
	[ID сотрудника] int not null,
	[Желаемая сумма] decimal (19,2) NOT NULL,
	[Дата добавления] datetime not null,
	CONSTRAINT [PK_Предложение] PRIMARY KEY CLUSTERED (ID ASC),
	CONSTRAINT [FK_Предложение_Недвижимость] FOREIGN KEY ([ID недвижимости]) REFERENCES [Недвижимость] (ID),
	CONSTRAINT [FK_Предложение_Клиент] FOREIGN KEY ([ID клиента]) REFERENCES [Клиент] (ID),
	CONSTRAINT [FK_Предложение_Клиент] FOREIGN KEY ([ID сотрудника]) REFERENCES [Сотрудник] (ID)
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



--[Удалённость от города в КМ] int, --це для земельных участков
--[Строение на участке] bit, --Це для земельных участков
--[Дачное общество] varchar(150), Це для земельных участков
--Назначение varchar(50),
--[Тип жилого помещения] varchar(150),
--[Тип загородного дома] varchar(150),
--[Тип коммерческой недвижимости] varchar(150),
--[Тип офисного помещения] varchar(150),
--[Тип продаваемого жилья] varchar(150),
--[Тип земельного участка] varchar(150),
--[Статус жилого помещения] varchar(150),
--[Атрибуты ЖН] varchar(150),
--[Атрибуты КН] varchar(150),
--[Атрибуты ЗУ] varchar(150),
--[Возраст здания] varchar(40),
--[Тип здания] varchar(60), --ТРЦ, Бизнес центр, офисное здание, жилой дом
--[Тип дома] varchar(60),--панельный/мк/к
--[Серия квартиры] varchar(200),
--[Тип жилья] varchar(150),
--[Условия АЖН] varchar(150),
--[Условия АНН] varchar(150),
--[Кол-во комнат] smallint, 
--[Кол-во этажей в доме] varchar(20),
--[Этаж помещения] tinyint,
--Телефон varchar(150),
	