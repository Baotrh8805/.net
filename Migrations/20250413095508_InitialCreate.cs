using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyQuanNet.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MayTinh",
                columns: table => new
                {
                    MaMay = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenMay = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrangThai = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DonGia = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MoTa = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ThoiGianTao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ThoiGianXoa = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MayTinh", x => x.MaMay);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NguoiDungs",
                columns: table => new
                {
                    MaNguoiDung = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HoTen = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenDangNhap = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatKhau = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoDienThoai = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoDu = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    NgayTaoTaiKhoan = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NgayXoaTaiKhoan = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TrangThai = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungs", x => x.MaNguoiDung);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NapTiens",
                columns: table => new
                {
                    MaNapTien = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaNguoiDung = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoTien = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ThoiGianNap = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PhuongThuc = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaNguoiDungNavigationMaNguoiDung = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NapTiens", x => x.MaNapTien);
                    table.ForeignKey(
                        name: "FK_NapTiens_NguoiDungs_MaNguoiDungNavigationMaNguoiDung",
                        column: x => x.MaNguoiDungNavigationMaNguoiDung,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SuDungMays",
                columns: table => new
                {
                    MaSuDung = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaMay = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaNguoiDung = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TongThoiGian = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    TongTien = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MaMayNavigationMaMay = table.Column<string>(type: "varchar(10)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaNguoiDungNavigationMaNguoiDung = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuDungMays", x => x.MaSuDung);
                    table.ForeignKey(
                        name: "FK_SuDungMays_MayTinh_MaMayNavigationMaMay",
                        column: x => x.MaMayNavigationMaMay,
                        principalTable: "MayTinh",
                        principalColumn: "MaMay");
                    table.ForeignKey(
                        name: "FK_SuDungMays_NguoiDungs_MaNguoiDungNavigationMaNguoiDung",
                        column: x => x.MaNguoiDungNavigationMaNguoiDung,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_NapTiens_MaNguoiDungNavigationMaNguoiDung",
                table: "NapTiens",
                column: "MaNguoiDungNavigationMaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_SuDungMays_MaMayNavigationMaMay",
                table: "SuDungMays",
                column: "MaMayNavigationMaMay");

            migrationBuilder.CreateIndex(
                name: "IX_SuDungMays_MaNguoiDungNavigationMaNguoiDung",
                table: "SuDungMays",
                column: "MaNguoiDungNavigationMaNguoiDung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NapTiens");

            migrationBuilder.DropTable(
                name: "SuDungMays");

            migrationBuilder.DropTable(
                name: "MayTinh");

            migrationBuilder.DropTable(
                name: "NguoiDungs");
        }
    }
}
