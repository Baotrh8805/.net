using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Project.Models; // Đảm bảo namespace chứa các lớp MayTinh, NguoiDung, v.v.

namespace Project.Data;

public partial class ProjetcNetQuanLyMayTinhContext : DbContext
{
    public ProjetcNetQuanLyMayTinhContext()
    {
    }

    public ProjetcNetQuanLyMayTinhContext(DbContextOptions<ProjetcNetQuanLyMayTinhContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MayTinh> MayTinhs { get; set; } = null!;
    public virtual DbSet<NapTien> NapTiens { get; set; } = null!;
    public virtual DbSet<NguoiDung> NguoiDungs { get; set; } = null!;
    public virtual DbSet<SuDungMay> SuDungMays { get; set; } = null!;

    // 🔥 Đã xoá OnConfiguring để tránh lỗi UseSqlServer
    // Nếu bạn đã cấu hình chuỗi kết nối ở Program.cs → KHÔNG cần OnConfiguring ở đây

   protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Cấu hình bảng MayTinh
    modelBuilder.Entity<MayTinh>(entity =>
    {
        entity.HasKey(e => e.MaMay); // Khóa chính
        entity.Property(e => e.TenMay).HasMaxLength(50);
        entity.Property(e => e.TrangThai).HasMaxLength(30);
        entity.Property(e => e.DonGia).HasColumnType("decimal(10, 2)");
    });

    // Cấu hình bảng NguoiDung
    modelBuilder.Entity<NguoiDung>(entity =>
    {
        entity.HasKey(e => e.MaNguoiDung); // Khóa chính
        entity.Property(e => e.HoTen).HasMaxLength(100);
        entity.Property(e => e.SoDienThoai).HasMaxLength(15);
        entity.Property(e => e.SoDu).HasColumnType("decimal(18, 2)");
    });

    // Cấu hình bảng SuDungMay
    modelBuilder.Entity<SuDungMay>(entity =>
    {
        entity.HasKey(e => e.MaSuDung); // Khóa chính

        // Quan hệ với MayTinh
        entity.HasOne(d => d.MaMayNavigation)
            .WithMany(p => p.SuDungMays)
            .HasForeignKey(d => d.MaMay)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // Quan hệ với NguoiDung
        entity.HasOne(d => d.MaNguoiDungNavigation)
            .WithMany(p => p.SuDungMays)
            .HasForeignKey(d => d.MaNguoiDung)
            .OnDelete(DeleteBehavior.ClientSetNull);
    });

    OnModelCreatingPartial(modelBuilder);
}

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
