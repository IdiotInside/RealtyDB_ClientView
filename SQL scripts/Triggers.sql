use RealtyAgency
GO
drop trigger removeOrder
GO
create trigger removeOrder on RealtyAgency.dbo.Услуга after insert, update, delete
as
begin

	update RealtyAgency.dbo.Заявка set [Отметка о выполнении]=1 
		from inserted inner join RealtyAgency.dbo.[Реализованная недвижимость] rr on inserted.ID=rr.[ID Услуги]
					  inner join Недвижимость r on r.ID=rr.[ID недвижимости] 
					  inner join 
end;