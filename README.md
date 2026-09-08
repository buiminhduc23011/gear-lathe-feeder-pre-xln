# Gear Lathe Feeder Pre-XLN System

Hệ thống quản lý và điều khiển máy cấp phôi tiện bánh răng trước xử lý nhiệt (Gear Lathe Feeder Pre-XLN System), bao gồm dịch vụ Backend API, giao diện Web quản trị, ứng dụng Desktop WPF và các kịch bản triển khai tự động.

---

## 📦 Lưu trữ (Archive Component)

> **LƯU Ý:**  
> Thư mục `src/Tablet/` (bao gồm `Tablet.App` và `Delta.Plc`) được giữ lại như **bản lưu trữ (Archive / Legacy Reference)**. Phần này đã dừng phát triển, không còn duy trì và không dùng trong luồng hệ thống hiện tại.

---

## 🏗️ Cấu trúc dự án

```text
gear-lathe-feeder-pre-xln/
├── deploy/                     # Script build, đóng gói và hướng dẫn triển khai
│   ├── build.ps1               # Script build React + Publish .NET Server
│   ├── setup.ps1               # Script cài đặt & cấu hình Windows Service
│   ├── deploy.ps1              # Wrapper tổng hợp triển khai
│   ├── ScriptSetupApp_UsingInno.iss # Inno Setup script cho Desktop App
│   └── GUIDE.md                # Hướng dẫn chi tiết triển khai Production
│
└── src/
    ├── Server.Api/             # [ACTIVE] Backend Web API (.NET 10)
    ├── Web.React/              # [ACTIVE] Giao diện Web Client (React SPA)
    ├── Desktop.App/            # [ACTIVE] Ứng dụng Desktop (WPF / .NET)
    ├── Shared.Models/          # [ACTIVE] Thư viện Model & DTO dùng chung
    └── Tablet/                 # 📦 [ARCHIVE] Ứng dụng Tablet & Delta PLC (Bản lưu trữ, không dùng)
        ├── Delta.Plc/
        └── Tablet.App/
```

---

## 🚀 Các thành phần chính (Active)

### 1. Server.Api (`src/Server.Api/`)
* **Công nghệ:** .NET 10 Web API, Entity Framework Core, SQL Server, JWT Auth, SignalR.
* **Chức năng:** Quản lý cơ sở dữ liệu, API điều khiển, giao tiếp với máy sản xuất và phục vụ giao diện Web SPA qua `wwwroot`.

### 2. Web.React (`src/Web.React/`)
* **Công nghệ:** React, Node.js.
* **Chức năng:** Giao diện điều khiển & theo dõi thời gian thực dành cho người vận hành/kỹ thuật viên trên trình duyệt web.

### 3. Desktop.App (`src/Desktop.App/`)
* **Công nghệ:** WPF (.NET 10).
* **Chức năng:** Ứng dụng Desktop quản lý/giám sát trực tiếp trên máy tính trạm.

### 4. Shared.Models (`src/Shared.Models/`)
* **Chức năng:** Chứa các data contract, DTOs, enum dùng chung giữa Server API và Desktop App.

---

## 🛠️ Yêu cầu môi trường

| Công cụ | Phiên bản khuyến nghị |
|---------|-----------------------|
| **.NET SDK** | 10.0 trở lên |
| **Node.js** | 18 trở lên (kèm `npm` / `pnpm`) |
| **SQL Server** | 2019+ hoặc SQL Server Express |
| **PowerShell** | 5.1+ |

---

## 📦 Build & Triển khai

Chi tiết hướng dẫn đóng gói và triển khai xem tại: **[deploy/GUIDE.md](file:///c:/Users/ducbu/Documents/GitHub/gear-lathe-feeder-pre-xln/deploy/GUIDE.md)**.

### Tóm tắt câu lệnh Build Server & Web:

```powershell
# Build đóng gói Framework-dependent (yêu cầu .NET 10 trên server đích)
.\deploy\build.ps1

# Hoặc Build Self-contained (không cần .NET trên server đích)
.\deploy\build.ps1 -SelfContained -Runtime win-x64
```

Sản phẩm đóng gói sẽ nằm ở thư mục `dist/`, bao gồm cả `Server.Api` và Web SPA đã được tích hợp sẵn vào `dist/server/wwwroot/`.

---

## 📄 Giấy phép & Thông tin
Dự án nội bộ điều khiển tự động hóa cấp phôi máy tiện bánh răng trước xử lý nhiệt (Gear Lathe Feeder Pre-XLN).
