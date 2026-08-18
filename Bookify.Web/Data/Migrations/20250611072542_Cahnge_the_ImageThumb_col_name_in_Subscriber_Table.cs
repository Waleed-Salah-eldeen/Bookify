using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify.Web.Data.Migrations
{
    public partial class Cahnge_the_ImageThumb_col_name_in_Subscriber_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThumbnailImageUrl",
                table: "Subscribers",
                newName: "ImageThumbnailUrl");
        }
    }
}
