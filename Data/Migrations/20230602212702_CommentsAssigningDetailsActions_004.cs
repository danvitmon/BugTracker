using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class CommentsAssigningDetailsActions_004 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketAttachments_AspNetUsers_UserId",
                table: "TicketAttachments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "TicketAttachments",
                newName: "BTUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketAttachments_UserId",
                table: "TicketAttachments",
                newName: "IX_TicketAttachments_BTUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TicketAttachments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(800)",
                oldMaxLength: 800,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "TicketAttachments",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAttachments_AspNetUsers_BTUserId",
                table: "TicketAttachments",
                column: "BTUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketAttachments_AspNetUsers_BTUserId",
                table: "TicketAttachments");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "TicketAttachments");

            migrationBuilder.RenameColumn(
                name: "BTUserId",
                table: "TicketAttachments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketAttachments_BTUserId",
                table: "TicketAttachments",
                newName: "IX_TicketAttachments_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TicketAttachments",
                type: "character varying(800)",
                maxLength: 800,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAttachments_AspNetUsers_UserId",
                table: "TicketAttachments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
