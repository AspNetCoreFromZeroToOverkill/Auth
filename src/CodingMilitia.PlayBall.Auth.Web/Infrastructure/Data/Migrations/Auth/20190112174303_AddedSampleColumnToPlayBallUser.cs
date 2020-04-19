using Microsoft.EntityFrameworkCore.Migrations;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Migrations.Auth
{
    public partial class AddedSampleColumnToPlayBallUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sample",
                schema: "public",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sample",
                schema: "public",
                table: "AspNetUsers");
        }
    }
}
