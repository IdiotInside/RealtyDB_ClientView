use RealtyAgency
GO
drop trigger removeOrder
GO
create trigger removeOrder on RealtyAgency.dbo.������ after insert, update, delete
as
begin

	update RealtyAgency.dbo.������ set [������� � ����������]=1 
		from inserted inner join RealtyAgency.dbo.[������������� ������������] rr on inserted.ID=rr.[ID ������]
					  inner join ������������ r on r.ID=rr.[ID ������������] 
					  inner join 
end;