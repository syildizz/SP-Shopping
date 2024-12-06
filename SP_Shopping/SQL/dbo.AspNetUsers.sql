CREATE TRIGGER trg_DeleteUsers
ON dbo.[AspNetUsers]
INSTEAD OF DELETE
AS
BEGIN
	-- Delete uncascading foreign key in Products
    DELETE FROM dbo.[Products]
    WHERE SubmitterId IN (SELECT Id FROM deleted);

	-- Delete users
    DELETE FROM dbo.[AspNetUsers]
    WHERE Id IN (SELECT Id FROM deleted);
END;
