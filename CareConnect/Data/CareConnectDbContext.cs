using CareConnect.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace CareConnect.Data
{
    public class CareConnectDbContext : DbContext
    {
        public CareConnectDbContext(
            DbContextOptions<CareConnectDbContext> options)
            : base(options)
        {
        }
        public DbSet<NhatKyChamSoc> NhatKyChamSocs
        {
            get;
            set;
        }
        public DbSet<ThanhToan> ThanhToans
    => Set<ThanhToan>();
        public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
        public DbSet<VaiTro> VaiTros => Set<VaiTro>();
        public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
        public DbSet<DanhGia> DanhGias
    => Set<DanhGia>();
        public DbSet<HuyLich> HuyLichs
    => Set<HuyLich>();
        public DbSet<TuChoiLich> TuChoiLichs { get; set; }
        public DbSet<DichVu> DichVus { get; set; }
        public DbSet<CauHinhGoiY> CauHinhGoiYs { get; set; }
        public DbSet<BenhNhan> BenhNhans => Set<BenhNhan>();
        public DbSet<ChungChi> ChungChis
    => Set<ChungChi>();
        public DbSet<NguoiChamSoc> NguoiChamSocs => Set<NguoiChamSoc>();

        public DbSet<DatLich> DatLichs => Set<DatLich>();

        public DbSet<ThongBao> ThongBaos => Set<ThongBao>();
        public DbSet<LichNhacThuoc> LichNhacThuocs
    => Set<LichNhacThuoc>();
        public DbSet<LichNhacTaiKham> LichNhacTaiKhams
    => Set<LichNhacTaiKham>();
        public DbSet<LichLamViec> LichLamViecs
    => Set<LichLamViec>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<HuyLich>()
                .HasOne(x => x.DatLich)
                .WithMany()
                .HasForeignKey(x => x.MaDatLich)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<HuyLich>()
                .HasOne(x => x.KhachHang)
                .WithMany()
                .HasForeignKey(x => x.MaKhachHang)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<HuyLich>()
                .HasIndex(x => x.MaDatLich)
                .IsUnique();
            modelBuilder.Entity<TaiKhoan>()
                .HasOne(t => t.VaiTro)
                .WithMany(v => v.TaiKhoans)
                .HasForeignKey(t => t.MaVaiTro)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.Email)
                .IsUnique();

            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();
            modelBuilder.Entity<KhachHang>()
               .HasOne(k => k.TaiKhoan)
               .WithOne()
               .HasForeignKey<KhachHang>(k => k.MaTaiKhoan)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<NhatKyChamSoc>()
                .HasOne(x => x.DatLich)
                .WithOne(x => x.NhatKyChamSoc)
                .HasForeignKey<NhatKyChamSoc>(
                    x => x.MaDatLich)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ThanhToan>()
                .HasOne(x => x.DatLich)
                .WithOne(x => x.ThanhToan)
                .HasForeignKey<ThanhToan>(
                    x => x.MaDatLich)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DanhGia>()
                .HasOne(x => x.DatLich)
                .WithOne(x => x.DanhGia)
                .HasForeignKey<DanhGia>(
                    x => x.MaDatLich)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LichNhacTaiKham>()
                .HasOne(x => x.BenhNhan)
                .WithMany()
                .HasForeignKey(x => x.MaBenhNhan)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LichLamViec>()
                .HasOne(x => x.NguoiChamSoc)
                .WithMany()
                .HasForeignKey(x => x.MaNguoiChamSoc)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}