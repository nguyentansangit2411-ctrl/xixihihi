# CLAUDE.md — xixihihi Web API

> File này là bộ nhớ dài hạn của dự án. Claude Code và Antigravity sẽ tự đọc file này mỗi session.
> **Luôn update file này khi có quyết định kiến trúc mới, thêm entity, hoặc thay đổi convention.**

## ⚠️ Quy tắc làm việc — BẮT BUỘC TUÂN THỦ

- **Làm từng bước nhỏ một.** Mỗi lần chỉ tạo hoặc sửa 1 file / 1 class / 1 feature nhỏ.
- **Dừng lại sau mỗi bước** và chờ người dùng review + confirm trước khi tiếp tục.
- **Không được tự động làm nhiều bước liên tiếp** dù các bước đó có liên quan nhau.
- Sau mỗi đoạn code, kết thúc bằng: *"✅ Bạn review xong thì mình tiếp tục bước tiếp theo nhé."*

---

## 📌 Tổng quan dự án

| Mục | Giá trị |
|-----|---------|
| Tên dự án | xixihihi — Website thanh lý đồ thể thao |
| Loại | Web API (Backend only, frontend sẽ làm sau) |
| Mục tiêu | Marketplace trung gian kết nối người mua/bán đồ thể thao đã qua sử dụng |
| Phase hiện tại | Phase 1 — Backend API |

---

## 🛠️ Tech Stack

### Runtime & Framework
- **Platform**: .NET 10
- **Framework**: ASP.NET Core Web API (Controller-based, không dùng Minimal API)
- **Language**: C# 13

### Database
- **DBMS**: SQL Server (latest)
- **ORM**: Entity Framework Core 9
- **Migration tool**: EF Core Migrations

### Authentication & Authorization
- **Strategy**: OAuth2 với Google (chỉ Google — Facebook để Phase 2)
- **Token**: JWT (Access Token + Refresh Token)
- **Library**: `Microsoft.AspNetCore.Authentication.JwtBearer`
- **KHÔNG dùng**: ASP.NET Identity (tự quản lý User entity)
- **KHÔNG làm Phase 1**: Facebook OAuth, Logout endpoint

### Thư viện chính
| Package | Mục đích |
|---------|---------|
| `AutoMapper` | DTO ↔ Entity mapping |
| `FluentValidation` | Validate request |
| `Serilog` | Structured logging |
| `Swashbuckle (Swagger)` | API docs |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | Cache (optional phase 2) |

---

## 🏗️ Kiến trúc: Clean Architecture

```
D:\Desktop\xixihihi\
├── Xixihihi.Domain/            # Layer 1 — Entities, Enums, Interfaces, Exceptions
│   ├── Entities/
│   ├── Enums/
│   ├── Exceptions/
│   └── Interfaces/
│       ├── Repositories/
│       └── Services/
│
├── Xixihihi.Application/       # Layer 2 — Use cases, DTOs, Validators, Mappings
│   ├── DTOs/
│   │   ├── Requests/
│   │   └── Responses/
│   ├── Features/                  # Group theo feature (Products, Orders, Users...)
│   │   └── Products/
│   │       ├── Commands/
│   │       └── Queries/
│   ├── Mappings/                  # AutoMapper profiles
│   ├── Validators/                # FluentValidation validators
│   └── Interfaces/                # Application-level service interfaces
│
├── Xixihihi.Infrastructure/    # Layer 3 — EF Core, Repos, External services
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/        # IEntityTypeConfiguration<T> cho mỗi entity
│   │   └── Migrations/
│   ├── Repositories/
│   ├── Services/                  # Email, Storage, OAuth helpers...
│   └── DependencyInjection.cs     # Extension method đăng ký Infrastructure services
│
└── Xixihihi.API/               # Layer 4 — Controllers, Middleware, Program.cs
    ├── Controllers/
    ├── Middleware/
    │   ├── ExceptionHandlingMiddleware.cs
    │   └── RequestLoggingMiddleware.cs
    ├── Extensions/
    ├── appsettings.json
    ├── appsettings.Development.json
    └── Program.cs
```

### Dependency Rule
```
API → Application → Domain
Infrastructure → Application → Domain
(Infrastructure KHÔNG được reference API)
```

---

## 🗄️ Domain Model — Entities chính

### Province & Ward (Khu vực - Mô hình 2 cấp từ 01/7/2025)
```csharp
// Seed data tĩnh — không soft delete
public class Province : BaseEntity
{
    public string Name { get; set; }          // "TP. Hồ Chí Minh"
    public string Code { get; set; }          // "HCM" — dùng để filter
    public ICollection<Ward> Wards { get; set; }
    public ICollection<User> Users { get; set; }
    public ICollection<Product> Products { get; set; }
}

public class Ward : BaseEntity
{
    public string Name { get; set; }          // "Phường Bến Nghé" hoặc "Đặc khu X"
    public Guid ProvinceId { get; set; }
    public Province Province { get; set; }
    public ICollection<User> Users { get; set; }
    public ICollection<Product> Products { get; set; }
}
```

### User
```csharp
// Tự quản lý, KHÔNG dùng IdentityUser
public class User : BaseEntity
{
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }

    // Thông tin liên hệ — buyer dùng để liên hệ seller ngoài platform
    public string? PhoneNumber { get; set; }
    public string? ZaloLink { get; set; }       // VD: "https://zalo.me/0909..."
    public string? FacebookLink { get; set; }   // VD: "https://facebook.com/..."

    public UserRole Role { get; set; }        // Buyer, Seller, Admin
    public UserStatus Status { get; set; }    // Active, Banned, Pending

    // Khu vực người dùng (optional, hiển thị trên profile)
    public Guid? ProvinceId { get; set; }
    public Guid? WardId { get; set; }
    public Province? Province { get; set; }
    public Ward? Ward { get; set; }

    public ICollection<ExternalLogin> ExternalLogins { get; set; }
    public ICollection<Product> Products { get; set; }
}

public class ExternalLogin : BaseEntity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; }      // "Google" (Facebook — Phase 2)
    public string ProviderKey { get; set; }   // Subject ID từ provider
    public User User { get; set; }
}
```

### Product (Đồ thanh lý)
```csharp
public class Product : BaseEntity
{
    public Guid SellerId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public bool IsNegotiable { get; set; } = false;   // Có thể thương lượng không

    public ProductCondition Condition { get; set; }   // New, LikeNew, Good, Fair
    public ProductStatus Status { get; set; }          // Draft, Active, Sold, Removed

    // Phương thức giao dịch mà seller chấp nhận (dùng để filter)
    // Buyer xem xong → liên hệ seller ngoài platform để thoả thuận
    public TransactionType TransactionType { get; set; }
    // DirectMeeting: gặp trực tiếp trao đổi
    // ShipCOD: seller gửi hàng, buyer trả tiền khi nhận

    public Guid CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Size { get; set; }

    // Khu vực sản phẩm (bắt buộc — dùng để filter theo vùng)
    public Guid ProvinceId { get; set; }
    public Guid? WardId { get; set; }
    public Province Province { get; set; }
    public Ward? Ward { get; set; }

    public ICollection<ProductImage> Images { get; set; }
    public User Seller { get; set; }
    public Category Category { get; set; }
}

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Url { get; set; }
    public int SortOrder { get; set; } = 0;
    public Product Product { get; set; }
}
```

### Order — **KHÔNG làm ở Phase 1 & 2**
> Platform Phase 1 chỉ là nơi đăng tin. Buyer xem sản phẩm → liên hệ seller qua SĐT/Zalo/Facebook → tự thoả thuận và giao dịch bên ngoài. Seller tự đánh dấu `ProductStatus = Sold` khi bán xong.
> Order module và thanh toán sẽ được thiết kế ở **Phase 3**.

### BaseEntity (mọi entity kế thừa)
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete
}
```

### Enums
```csharp
public enum UserRole { Buyer, Seller, Admin }
public enum UserStatus { Active, Banned, Pending }
public enum ProductCondition { New, LikeNew, Good, Fair }
public enum ProductStatus { Draft, Active, Sold, Removed }
public enum TransactionType { DirectMeeting, ShipCOD }
// OrderStatus — để dành Phase 3
```

---

## 🔐 Authentication Flow

```
1. FE gửi Google id_token
   POST /api/auth/google
   Body: { "token": "..." }

2. API verify token với Google API

3. Nếu ExternalLogin chưa tồn tại → tạo User mới + ExternalLogin
   Nếu đã tồn tại → load User hiện có

4. Trả về:
   {
     "accessToken": "...",   // JWT, expires 15 phút
     "refreshToken": "...",  // Opaque token, expires 7 ngày, lưu DB
     "user": { ... }
   }

5. Refresh: POST /api/auth/refresh  { "refreshToken": "..." }
```

### JWT Claims
```
sub: userId (Guid)
email: user email
role: UserRole
name: DisplayName
```

---

## 📐 Conventions & Coding Standards

### Naming
| Loại | Convention | Ví dụ |
|------|-----------|-------|
| Class / Interface | PascalCase | `ProductService`, `IProductRepository` |
| Method | PascalCase | `GetByIdAsync` |
| Private field | _camelCase | `_productRepository` |
| Variable | camelCase | `productList` |
| Constant | UPPER_SNAKE | `MAX_IMAGE_COUNT` |
| DTO suffix | Request/Response | `CreateProductRequest`, `ProductResponse` |

### API Response Format (thống nhất toàn dự án)
```json
// Success
{
  "success": true,
  "data": { ... },
  "message": null
}

// Error
{
  "success": false,
  "data": null,
  "message": "Mô tả lỗi",
  "errors": [ "chi tiết lỗi 1", "chi tiết lỗi 2" ]
}

// Paginated
{
  "success": true,
  "data": {
    "items": [ ... ],
    "totalCount": 100,
    "page": 1,
    "pageSize": 20,
    "totalPages": 5
  }
}
```

### Repository Pattern
- Interface định nghĩa trong `Domain/Interfaces/Repositories/`
- Implementation trong `Infrastructure/Repositories/`
- Generic base: `IBaseRepository<T>` với CRUD cơ bản
- Specific repo extend thêm method riêng

### Async/Await
- **Tất cả** DB call phải dùng `async/await`
- Method tên phải có suffix `Async`: `GetByIdAsync`, `CreateAsync`

### Soft Delete
- Không bao giờ hard delete entity chính (User, Product)
- Set `IsDeleted = true`, mọi query phải filter `WHERE IsDeleted = false`
- Dùng Global Query Filter trong EF Core

### Error Handling
- Dùng custom Exceptions: `NotFoundException`, `UnauthorizedException`, `BusinessException`
- `ExceptionHandlingMiddleware` bắt tất cả exception → trả về response format chuẩn
- Không để exception raw leak ra controller

---

## 🌐 API Endpoints (thiết kế)

```
Auth
  POST   /api/auth/google                   Đăng nhập bằng Google
  POST   /api/auth/refresh                  Refresh access token
  (Facebook & Logout — Phase 2)

Users
  GET    /api/users/me                      Thông tin user hiện tại (full, kể cả contact)
  PUT    /api/users/me                      Cập nhật profile (tên, avatar, SĐT, Zalo, Facebook, khu vực)
  GET    /api/users/{id}                    Xem profile seller (public — có contact info để buyer liên hệ)

Locations (seed data tĩnh, không thay đổi runtime)
  GET    /api/provinces                     Danh sách tỉnh/thành phố
  GET    /api/provinces/{id}/wards          Danh sách phường/xã/đặc khu theo tỉnh

Categories
  GET    /api/categories                    Danh sách danh mục
  POST   /api/categories                    Tạo danh mục [Admin]
  PUT    /api/categories/{id}               Cập nhật [Admin]
  DELETE /api/categories/{id}               Xoá [Admin]

Products
  GET    /api/products                      Danh sách + filter + search + paginate
    ?search=          Tìm theo title
    ?categoryId=      Lọc theo danh mục
    ?provinceId=      Lọc theo tỉnh/thành
    ?wardId=          Lọc theo phường/xã
    ?condition=       Lọc theo tình trạng (New/LikeNew/Good/Fair)
    ?transactionType= Lọc theo hình thức (DirectMeeting/ShipCOD)
    ?minPrice=        Giá từ
    ?maxPrice=        Giá đến
    ?isNegotiable=    Có thương lượng không
    ?page=&pageSize=  Phân trang (pageSize tối đa 50)
    ?sortBy=          price_asc | price_desc | newest (default: newest)

  GET    /api/products/{id}                 Chi tiết sản phẩm (kèm contact info của seller)
  POST   /api/products                      Tạo sản phẩm mới [Seller] → trả 201 Created
  PUT    /api/products/{id}                 Cập nhật [Seller, chủ sở hữu]
  PATCH  /api/products/{id}/status          Đổi trạng thái: Active/Sold/Draft [Seller, chủ sở hữu]
  DELETE /api/products/{id}                 Soft delete [Seller, chủ sở hữu]
  POST   /api/products/{id}/images          Upload ảnh [Seller] (tối đa 10 ảnh, 5MB/ảnh)
  DELETE /api/products/{id}/images/{imgId}  Xoá ảnh [Seller]

Sellers
  GET    /api/sellers/{sellerId}/ratings         Danh sách rating của seller
  GET    /api/sellers/{sellerId}/ratings/summary Thống kê rating
  POST   /api/sellers/{sellerId}/ratings         Đánh giá seller [Buyer, không tự rate]
  PUT    /api/ratings/{ratingId}                 Cập nhật đánh giá [chủ rating]

Notifications
  GET    /api/notifications                 Danh sách thông báo [Auth]
  PATCH  /api/notifications/{id}/read       Đánh dấu đã đọc [Auth]

SavedSearches
  GET    /api/saved-searches                Danh sách tìm kiếm đã lưu [Auth]
  POST   /api/saved-searches                Lưu tìm kiếm [Auth]
  DELETE /api/saved-searches/{id}           Xoá tìm kiếm đã lưu [Auth]
```

> ℹ️ **Không có Order/Payment API ở Phase 1 & 2.** Buyer liên hệ seller trực tiếp qua SĐT/Zalo/Facebook.

---

## ⚙️ Configuration (appsettings.json structure)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=XixihihiDb;..."
  },
  "Jwt": {
    "SecretKey": "",
    "Issuer": "Xixihihi",
    "Audience": "XixihihiClient",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "OAuth": {
    "Google": {
      "ClientId": "",
      "ClientSecret": ""
    }
  },
  "Cloudinary": {
    "CloudName": "",
    "ApiKey": "",
    "ApiSecret": ""
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "",
    "Password": "",
    "SenderName": "",
    "SenderEmail": ""
  },
  "AllowedOrigins": ""
}
```

> ⚠️ **KHÔNG commit secret thật vào appsettings.json.** Dùng `dotnet user-secrets` cho dev, environment variable cho production.

---

## 📋 Development Checklist (Phase 1)

### Setup
- [x] Tạo solution với 4 projects (Domain, Application, Infrastructure, API)
- [x] Cấu hình project references đúng dependency rule
- [x] Cài đặt NuGet packages
- [x] Cấu hình EF Core + AppDbContext
- [x] Cấu hình Swagger + JWT bearer trong Swagger UI

### Core
- [x] BaseEntity + soft delete global filter
- [x] ExceptionHandlingMiddleware
- [x] ApiResponse<T> wrapper
- [x] Generic Repository base
- [x] JWT service (generate + validate)
- [x] OAuth verification service (Google)

### Features (theo thứ tự)
- [x] Auth module (Google login, refresh token)
- [x] Location module (Province + Ward — seed data)
- [x] User module (profile CRUD + contact info: SĐT, Zalo, Facebook)
- [x] Category module
- [x] Product module (CRUD + image upload + filter theo khu vực + TransactionType)
- [x] Seller Rating module
- [x] Notification module
- [x] SavedSearch module

### Quality
- [x] FluentValidation cho tất cả Request DTOs
- [x] Unit tests cho Application layer
- [x] Integration tests cho API endpoints

---

## 🚨 Known Issues — Cần Fix (theo thứ tự ưu tiên)

- [ ] **#1** Secret key hardcode trong `appsettings.json` → dùng `dotnet user-secrets`
- [ ] **#2** `AutoMapper` đã cài nhưng chưa dùng → tạo `MappingProfile`, bỏ manual mapping trong handlers
- [ ] **#3** N+1 query trong `ProductCreatedEventHandler` → include User khi query SavedSearch, dùng `AddRangeAsync`, tách email ra background job
- [ ] **#4** Thiếu CORS configuration trong `Program.cs` → thêm `AddCors` + `UseCors`
- [ ] **#5** `FluentValidation.ValidationException` không được map trong Middleware → đang trả 500 thay vì 400
- [ ] **#6** `RateSellerCommandValidator` chưa check `ReviewerId != SellerId` → tự đánh giá chính mình
- [ ] **#7** Rate limiting chưa có → dùng `Microsoft.AspNetCore.RateLimiting` built-in
- [ ] **#8** `PageSize` không có giới hạn tối đa → ai cũng có thể gọi `?pageSize=999999`
- [ ] **#9** `ProductDto` expose `Email` của Seller ở list view → tạo `SellerPublicDto` không có Email
- [ ] **#10** `POST /api/products` trả `200 OK` thay vì `201 Created`
- [ ] **#11** `Size` field trong Product entity nhưng bị bỏ sót khỏi Command/DTO/Request
- [ ] **#12** Serilog chưa được cấu hình trong `Program.cs`

---

## 🧪 Testing Strategy

### 1. Unit Tests (Application & Domain Layer)
- **Framework**: `xUnit` + `Moq` + `FluentAssertions`
- **Mục tiêu**: Test handlers, validators, mapping profiles — mock toàn bộ repository & external services
- **Tiêu chuẩn**: Coverage tối thiểu 80% cho Application layer

### 2. Integration Tests (API & Infrastructure Layer)
- **Framework**: `xUnit` + `Microsoft.AspNetCore.Mvc.Testing`
- **Database**: In-Memory Database hoặc `Testcontainers`
- **Mục tiêu**: Test luồng API thực tế, HTTP status codes, response format, auth

### 3. Nguyên tắc viết Test
- **Naming**: `[MethodName]_[StateUnderTest]_[ExpectedBehavior]`
- **Pattern**: AAA (Arrange - Act - Assert)
- **Independence**: Mỗi test case độc lập, không phụ thuộc state nhau

---

## 🗺️ Phase 2 — User Interaction & Advanced Features

- [x] Seller Rating module
- [x] Search nâng cao (filter nhiều tiêu chí)
- [x] Notification (email + in-app khi có sản phẩm mới match SavedSearch)
- [ ] Facebook OAuth login
- [ ] Logout / Revoke refresh token
- [ ] Chat/Messaging trong platform
- [ ] Wishlist / Bookmark sản phẩm
- [ ] Report sản phẩm / seller lừa đảo
- [ ] Rate limiting
- [ ] CORS origins cụ thể khi có frontend
- [ ] SignalR real-time notification

## 🗺️ Phase 3 — E-Commerce (Tương lai xa)

- [ ] Order module
- [ ] Thanh toán: VNPay / MoMo / chuyển khoản xác nhận
- [ ] Concurrency handling: RowVersion token chống double-selling

---

## 📝 Log quyết định kiến trúc

| Ngày | Quyết định | Lý do |
|------|-----------|-------|
| Init | Không dùng ASP.NET Identity | Tự quản lý User đơn giản hơn khi chỉ dùng OAuth2 |
| Init | JWT 15 phút + Refresh 7 ngày | Bảo mật tốt, UX không bị logout liên tục |
| Init | Soft delete toàn bộ | Audit trail, không mất data |
| Init | Guid làm PK | Distributed-friendly, không expose auto-increment |
| Init | Không có Order ở Phase 1 & 2 | Platform chỉ đăng tin — buyer/seller tự liên hệ và giao dịch ngoài |
| Init | Thêm ContactInfo vào User | Buyer cần SĐT/Zalo/Facebook để liên hệ seller |
| Init | TransactionType trên Product | Chỉ là thông tin mô tả hình thức seller muốn — không tạo flow trong hệ thống |
| Init | Province/Ward là seed data tĩnh | Dữ liệu hành chính VN (mô hình 2 cấp từ 01/7/2025) không đổi thường xuyên |
| Init | ProvinceId bắt buộc trên Product | Bộ lọc khu vực là tính năng cốt lõi của marketplace |
| Phase 1 | Chỉ Google OAuth, bỏ Facebook | Đơn giản hoá scope Phase 1, Facebook thêm ở Phase 2 |
| Phase 1 | Chưa làm thanh toán | Platform trung gian — giao dịch xảy ra ngoài platform |