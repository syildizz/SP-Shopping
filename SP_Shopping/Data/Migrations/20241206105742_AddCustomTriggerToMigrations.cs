using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SP_Shopping.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomTriggerToMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.IsSqlServer())
            {
                var sql =
                """
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
                """;
                migrationBuilder.Sql(sql);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.IsSqlServer())
            {
                var sql =
                """
                    DROP TRIGGER trg_DeleteUsers;
                """;
                migrationBuilder.Sql(sql);
            }
        }
    }
}
