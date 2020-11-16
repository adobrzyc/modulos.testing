

// ReSharper disable All

using Modulos.Testing;

namespace SimpleDomain.Tests
{
    public partial class V1
    {
        public partial class Scripts
        {
            [ModelDefinition]
            public class AddALotOfUsersUsingSql
            {
                [InlineScript] public static string AddUser = @"
begin tran
declare @did uniqueidentifier 
select @did = DepartmentId
from Departments 

DECLARE @cnt INT = 0;
WHILE @cnt < 100
BEGIN
   INSERT INTO [dbo].[Users]
           ([UserId]
           ,[Name]
           ,[SName]
           ,[IsActive]
           ,DepartmentId)
	select newid(),
		'Name - ' + cast(newid() as nvarchar(50)),	
		'SName - ' + cast(newid() as nvarchar(50)),	
		1, @did
   SET @cnt = @cnt + 1;
END;
commit tran;";

            }
        }

    }
}