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
        // (Toàn bộ phần cấu hình bảng được giữ nguyên như bạn gửi)
        // Có thể copy lại nguyên phần cũ bạn đã viết
        // ...
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
