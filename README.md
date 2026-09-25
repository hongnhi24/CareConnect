# CARECONNECT - HỆ THỐNG CHĂM SÓC NGƯỜI CAO TUỔI TẠI NHÀ

## 1. Giới thiệu

CareConnect là hệ thống hỗ trợ quản lý dịch vụ chăm sóc người cao tuổi tại nhà.

Hệ thống hỗ trợ các chức năng chính như:

- Quản lý khách hàng
- Quản lý bệnh nhân
- Quản lý người chăm sóc
- Quản lý dịch vụ
- Đặt lịch chăm sóc
- Quản lý lịch làm việc
- Quản lý nhật ký chăm sóc
- Quản lý lịch nhắc thuốc
- Quản lý lịch nhắc tái khám
- Quản lý thanh toán
- Quản lý đánh giá
- Quản lý thông báo

---

## 2. Công nghệ sử dụng

- ASP.NET Core MVC
- C#
- Razor View (.cshtml)
- SQL Server
- SQL Server Management Studio (SSMS)
- Visual Studio

---

## 3. Cấu trúc thư mục

```text
CareConnect/
├── CareConnect/              # Source code chính của hệ thống
├── CareConnect.slnx          # File Solution để mở project bằng Visual Studio
├── CareConnect_Final.bak     # File backup cơ sở dữ liệu
└── README.md                 # Hướng dẫn cài đặt và chạy hệ thống
```

> Tên file `.bak` có thể khác tùy theo tên file được đóng gói cùng đồ án.

---

## 4. Yêu cầu môi trường

Để chạy hệ thống, máy cần cài đặt:

- Windows 10 hoặc Windows 11
- Visual Studio
- .NET SDK phù hợp với project
- SQL Server 2022
- SQL Server Management Studio (SSMS)

Trong Visual Studio nên cài workload:

- ASP.NET and web development

---

## 5. Khôi phục cơ sở dữ liệu

Cơ sở dữ liệu được cung cấp dưới dạng file backup `.bak`.

### Các bước Restore Database

1. Mở **SQL Server Management Studio (SSMS)**.
2. Kết nối với SQL Server trên máy.
3. Trong **Object Explorer**, chuột phải vào **Databases**.
4. Chọn **Restore Database...**.
5. Tại mục **Source**, chọn **Device**.
6. Nhấn nút `...`.
7. Chọn **Add**.
8. Chọn file backup:

```text
CareConnect_Final.bak
```

9. Nhấn **OK** để quay lại cửa sổ Restore Database.
10. Kiểm tra tên database là:

```text
CareConnect
```

11. Nhấn **OK** để bắt đầu Restore.
12. Sau khi Restore thành công, nhấn **Refresh** tại mục Databases.

Database `CareConnect` sẽ xuất hiện trong SQL Server.

> File backup được tạo từ SQL Server 2022. Nên sử dụng SQL Server 2022 hoặc phiên bản mới hơn để Restore.

---

## 6. Kiểm tra kết nối cơ sở dữ liệu

Sau khi Restore database, cần kiểm tra chuỗi kết nối trong source code.

Tên database sử dụng:

```text
CareConnect
```

Tên SQL Server trên mỗi máy có thể khác nhau, vì vậy nếu chương trình không kết nối được database thì cần chỉnh lại `Server` trong connection string cho phù hợp với máy đang chạy.

Ví dụ:

```text
Server=TEN_SERVER;Database=CareConnect;Trusted_Connection=True;TrustServerCertificate=True;
```

Trong đó `TEN_SERVER` phải được thay bằng tên SQL Server trên máy đang chạy project.

---

## 7. Chạy chương trình

1. Restore database `CareConnect` theo hướng dẫn ở trên.
2. Mở Visual Studio.
3. Mở file:

```text
CareConnect.slnx
```

4. Chờ Visual Studio load project và restore các package cần thiết.
5. Kiểm tra lại connection string.
6. Chọn project CareConnect làm project khởi động nếu cần.
7. Nhấn **Run** hoặc **F5** để chạy hệ thống.

---

## 8. Dữ liệu mẫu trong database

Database đã có sẵn dữ liệu phục vụ cho việc chạy thử hệ thống, bao gồm:

- Khách hàng: 60
- Bệnh nhân: 84
- Người chăm sóc: 51
- Đặt lịch: 204
- Lịch nhắc thuốc: 113
- Lịch nhắc tái khám: 60
- Thanh toán: 202
- Đánh giá: 75

Các dữ liệu được liên kết với nhau thông qua khóa chính, khóa ngoại và các ràng buộc dữ liệu trong SQL Server.

---

## 9. Lưu ý

- Không mở trực tiếp file `.bak` bằng Notepad, Visual Studio Code hoặc các ứng dụng thông thường.
- File `.bak` phải được Restore thông qua SQL Server Management Studio.
- Nếu Restore thành công nhưng chương trình không chạy được, hãy kiểm tra lại connection string.
- Không cần chạy lại các file seed dữ liệu nếu database đã được Restore từ file `.bak`.
- Không nên tạo lại database cùng tên nếu database `CareConnect` đã tồn tại và đang hoạt động bình thường.
