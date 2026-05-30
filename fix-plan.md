# Xixihihi — Master Document (29/05/2026)

> Tổng hợp toàn bộ: đánh giá 3 lần + kế hoạch fix + hướng dẫn deploy free stack
> **Điểm hiện tại: 88/100** — Sẵn sàng deploy sau khi hoàn thành checklist

---

## Phần 1 — Tổng quan đánh giá qua 3 lần build

| Hạng mục | Lần 1 | Lần 2 | Lần 3 |
|---|---|---|---|
| Kiến trúc | 9/10 | 9/10 | 9/10 |
| Authentication & Security | 4/10 | 9/10 | 9/10 |
| Database & EF Core | 8/10 | 9/10 | 9/10 |
| Error Handling & Logging | 8/10 | 8/10 | 9/10 |
| Tốc độ xử lý / Performance | — | 6/10 | 8/10 |
| Chi phí / Resource | — | 7/10 | 9/10 |
| Tests | 5/10 | 5/10 | 9/10 |
| **Tổng** | **65** | **83** | **88** |

### Trạng thái 15 tasks bảo mật & correctness

| # | Task | Trạng thái |
|---|---|---|
| 1 | Loại bỏ fallback JWT secret key | ✅ Done |
| 2 | Loại bỏ fallback AllowAnyOrigin CORS | ✅ Done |
| 3 | Bắt buộc Google ClientId validation | ✅ Done |
| 4 | Secrets management — appsettings sạch | ✅ Done |
| 5 | Middleware pipeline đúng thứ tự | ✅ Done |
| 6 | Index RefreshToken + hash SHA-256 | ✅ Done |
| 7 | Validate FK (Category, Province, Ward) | ✅ Done |
| 8 | Cloudinary dùng PublicId từ DB | ✅ Done |
| 9 | Email queue thay fire-and-forget | ✅ Done |
| 10 | Unit of Work pattern | ✅ Done |
| 11 | SavedSearch filter tại DB | ✅ Done |
| 12 | Health check — không đăng ký trùng | ✅ Done |
| 13 | Swagger tắt ở production | ✅ Done |
| 14 | Integration tests Auth flow (4 cases) | ✅ Done |
| 15 | Dockerfile + docker-compose | ✅ Done |

### Trạng thái Performance & Chi phí

| Vấn đề | Lần 2 | Lần 3 |
|---|---|---|
| `GetNotificationsQueryHandler` load toàn bộ memory | ❌ | ✅ Fixed |
| Search dùng `LIKE` không index | ❌ | ⚠️ Dùng `EF.Functions.Contains` nhưng thiếu FT Index |
| Cache Categories/Provinces | ❌ | ✅ Fixed — nhưng thiếu cache invalidation |
| Double query khi tạo Product | ❌ | ✅ Bỏ query thứ 2 — nhưng Seller bị null |
| Notification index tại DB | ❌ | ✅ `IX_Notifications_User_CreatedAt` |
| Cloudinary transformation resize | ❌ | ✅ Width/Height 1200, auto quality, auto format |
| Giới hạn ảnh per product | ❌ | ✅ Max 8 ảnh |
| Serilog log retention | ❌ | ✅ `retainedFileCountLimit: 7` |

---

## Phần 2 — Những việc phải làm trước khi deploy

### 🔴 Blocker 1 — Sửa `EF.Functions.Contains` trong `ProductRepository.cs`

`EF.Functions.Contains` dịch sang SQL `CONTAINS()` của SQL Server Full-Text Search — **sẽ crash ở production nếu không có Full-Text Index**, và cũng không hoạt động khi chuyển sang PostgreSQL (xem Phần 4).

**Cách fix (rollback về LIKE — đủ dùng cho giai đoạn thử nghiệm):**

```csharp
// File: Xixihihi.Infrastructure/Repositories/ProductRepository.cs

// XÓA
query = query.Where(p => EF.Functions.Contains(p.Title, search)
                      || EF.Functions.Contains(p.Description, search));

// THÊM
var searchLower = search.ToLower();
query = query.Where(p => p.Title.ToLower().Contains(searchLower)
                      || p.Description.ToLower().Contains(searchLower));
```

### 🔴 Blocker 2 — Xóa folder `scratch/`

```bash
rm -rf scratch/
```

Folder này chứa script nội bộ, không cần trong production codebase.

### 🟡 Nên làm — Cache invalidation cho Category handlers

**File:** `CreateCategoryCommandHandler.cs`, `UpdateCategoryCommandHandler.cs`, `DeleteCategoryCommandHandler.cs`

Hiện tại cache Categories được set khi read nhưng không bị xóa khi mutate — admin tạo/sửa/xóa category xong, client vẫn thấy danh sách cũ trong 1 giờ.

Inject `IMemoryCache` vào constructor của 3 handler trên (tương tự `GetCategoriesQueryHandler`), sau đó thêm 1 dòng sau `SaveChangesAsync`:

```csharp
await _unitOfWork.SaveChangesAsync(cancellationToken);
_cache.Remove("categories_all"); // thêm dòng này
```

### 🟡 Nên làm — `POST /products` trả về Seller null

**File:** `CreateProductCommandHandler.cs`

```csharp
var dto = _mapper.Map<ProductDto>(product); // product.Seller == null
```

Thay bằng manual DTO không có Seller — client gọi `GET /products/{id}` nếu cần full details:

```csharp
var dto = new ProductDto
{
    Id = product.Id,
    Title = product.Title,
    Description = product.Description,
    Price = product.Price,
    IsNegotiable = product.IsNegotiable,
    Condition = product.Condition,
    Status = product.Status,
    TransactionType = product.TransactionType,
    CategoryId = product.CategoryId,
    Brand = product.Brand,
    Size = product.Size,
    ProvinceId = product.ProvinceId,
    WardId = product.WardId,
    CreatedAt = product.CreatedAt,
    Images = Enumerable.Empty<ProductImageDto>()
};
```

### 🟡 Nên làm — Phân quyền Admin cho Category endpoints

**File:** `CategoryController.cs`

`UserRole.Admin` đã có trong enum nhưng chưa được áp dụng — hiện tại bất kỳ user nào có JWT hợp lệ đều có thể tạo/sửa/xóa category.

```csharp
[Authorize(Roles = "Admin")]
[HttpPost]
public async Task<IActionResult> CreateCategory(...) { ... }

[Authorize(Roles = "Admin")]
[HttpPut("{id}")]
public async Task<IActionResult> UpdateCategory(...) { ... }

[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteCategory(...) { ... }
```

---

## Phần 3 — Stack deploy miễn phí $0/tháng

> SQL Server (có phí) → **PostgreSQL trên Supabase (miễn phí)**
> Backend host → **Render.com free tier**

```
[Client / Vercel]  →  [Render.com - ASP.NET Core]  →  [Supabase - PostgreSQL]
                                   ↓
                           [Cloudinary - Images]
                           [Gmail SMTP - Email]
```

| Dịch vụ | Tier | Chi phí/tháng |
|---|---|---|
| Frontend | Vercel Free | $0 |
| Backend .NET | Render Free | $0 |
| Database | Supabase Free (500MB) | $0 |
| File upload | Cloudinary Free (25 credits) | $0 |
| Email | Gmail SMTP (500/ngày) | $0 |
| **Tổng** | | **$0** |

> ⚠️ **Vercel không chạy được ASP.NET Core backend** — Vercel chỉ support Node.js/Python/Go serverless. Backend .NET phải host trên Render hoặc tương đương.

> ⚠️ **Render free tier spin down sau 15 phút không có request** — request đầu tiên sau khi sleep mất ~30 giây wake up. Chấp nhận được khi thử nghiệm với bạn bè.

---

## Phần 4 — Thay đổi code để chuyển sang PostgreSQL

### 4.1 Đổi package trong `Xixihihi.Infrastructure/Xixihihi.Infrastructure.csproj`

```xml
<!-- XÓA -->
<PackageReference Include="AspNetCore.HealthChecks.SqlServer" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.8" />

<!-- THÊM -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
<PackageReference Include="AspNetCore.HealthChecks.Npgsql" Version="9.0.0" />
```

```bash
dotnet restore
```

### 4.2 Đổi provider trong `Xixihihi.Infrastructure/DependencyInjection.cs`

```csharp
// XÓA
options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))

// THÊM
options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
```

```csharp
// XÓA health check
.AddSqlServer(configuration.GetConnectionString("DefaultConnection")!, name: "database")

// THÊM
.AddNpgsql(configuration.GetConnectionString("DefaultConnection")!, name: "database")
```

### 4.3 Xóa migrations cũ và generate lại

Migrations cũ có SQL Server syntax — phải tạo lại hoàn toàn.

```bash
# Xóa tất cả file .cs trong Migrations (Windows)
del Xixihihi.Infrastructure\Migrations\*.cs

# Mac/Linux
rm Xixihihi.Infrastructure/Migrations/*.cs

# Generate lại
dotnet ef migrations add InitialCreate \
  --project Xixihihi.Infrastructure \
  --startup-project Xixihihi.API
```

### 4.4 Đổi `docker-compose.yml` sang PostgreSQL (để dev local)

```yaml
services:
  api:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=db;Database=xixihihi;Username=postgres;Password=postgres
      - Jwt__SecretKey=local_dev_secret_key_min_32_chars_long
      - AllowedOrigins=http://localhost:3000
      - OAuth__Google__ClientId=your-client-id
    depends_on:
      - db

  db:
    image: postgres:16-alpine
    environment:
      - POSTGRES_DB=xixihihi
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

volumes:
  pgdata:
```

---

## Phần 5 — Hướng dẫn deploy từng bước

### Bước 1 — Tạo database trên Supabase

1. Vào [supabase.com](https://supabase.com) → Sign up → **New project**
2. Điền:
   - **Name**: `xixihihi`
   - **Database Password**: mật khẩu mạnh, lưu lại
   - **Region**: Southeast Asia (Singapore)
   - **Plan**: Free
3. Chờ ~2 phút khởi tạo
4. Vào **Project Settings** → **Database** → **Connection string** → tab **URI**
5. Copy chuỗi dạng:
   ```
   postgresql://postgres:[YOUR-PASSWORD]@db.xxxxxxxxxxxx.supabase.co:5432/postgres
   ```
6. Chạy migration lên Supabase:
   ```bash
   # Set ConnectionStrings__DefaultConnection = chuỗi Supabase vừa copy (trong User Secrets)
   dotnet ef database update \
     --project Xixihihi.Infrastructure \
     --startup-project Xixihihi.API
   ```

### Bước 2 — Tạo Gmail App Password

1. Vào [myaccount.google.com](https://myaccount.google.com) → **Security**
2. Bật **2-Step Verification** nếu chưa có
3. Vào [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)
4. Chọn **Mail** → **Other** → đặt tên `Xixihihi` → **Generate**
5. Copy mật khẩu 16 ký tự — dùng cho `Smtp__Password`

### Bước 3 — Cấu hình Google OAuth

1. Vào [console.cloud.google.com](https://console.cloud.google.com) → **APIs & Services** → **Credentials** → **Create OAuth 2.0 Client ID**
2. Application type: **Web application**
3. Thêm **Authorized JavaScript origins**:
   ```
   https://your-frontend.vercel.app
   http://localhost:3000
   ```
4. Copy **Client ID** và **Client Secret**

### Bước 4 — Push code lên GitHub

Đảm bảo trước khi push:
- Folder `scratch/` đã xóa
- `appsettings.json` không có credentials
- `.gitignore` bao gồm `appsettings.Development.json`

### Bước 5 — Deploy backend lên Render.com

1. Vào [render.com](https://render.com) → Sign up bằng GitHub
2. **New** → **Web Service** → Connect GitHub repo
3. Cấu hình:
   - **Runtime**: Docker (tự detect Dockerfile)
   - **Region**: Singapore
   - **Plan**: Free
4. Thêm **Environment Variables**:

| Key | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Connection string Supabase |
| `Jwt__SecretKey` | Chuỗi random ≥32 ký tự (tạo tại [randomkeygen.com](https://randomkeygen.com)) |
| `Jwt__Issuer` | `Xixihihi` |
| `Jwt__Audience` | `XixihihiClient` |
| `Jwt__AccessTokenExpiryMinutes` | `15` |
| `Jwt__RefreshTokenExpiryDays` | `7` |
| `OAuth__Google__ClientId` | Google Client ID |
| `OAuth__Google__ClientSecret` | Google Client Secret |
| `Cloudinary__CloudName` | Cloudinary cloud name |
| `Cloudinary__ApiKey` | Cloudinary API key |
| `Cloudinary__ApiSecret` | Cloudinary API secret |
| `Smtp__Host` | `smtp.gmail.com` |
| `Smtp__Port` | `587` |
| `Smtp__Username` | Gmail address |
| `Smtp__Password` | Gmail App Password (16 ký tự) |
| `Smtp__SenderName` | `Xixihihi Marketplace` |
| `Smtp__SenderEmail` | Gmail address |
| `AllowedOrigins` | URL Vercel frontend (sau khi deploy xong) |

5. Click **Create Web Service** → build ~3-5 phút
6. Copy URL backend: `https://xixihihi-api.onrender.com`

### Bước 6 — Deploy frontend lên Vercel

1. Vào [vercel.com](https://vercel.com) → Import GitHub repo frontend
2. Thêm env var:
   ```
   NEXT_PUBLIC_API_URL=https://xixihihi-api.onrender.com
   ```
3. Deploy → copy URL Vercel: `https://xixihihi.vercel.app`
4. Quay lại **Render** → cập nhật `AllowedOrigins` = URL Vercel thật
5. Quay lại **Google Console** → thêm URL Vercel vào Authorized origins

### Bước 7 — Kiểm tra sau deploy

```bash
BASE=https://xixihihi-api.onrender.com

# Health check
curl $BASE/health
# Mong đợi: {"status":"Healthy",...}

# Categories (không cần auth)
curl $BASE/api/categories

# Provinces (không cần auth)
curl $BASE/api/locations/provinces

# Test search products
curl "$BASE/api/products?search=yonex"
```

---

## Phần 6 — Checklist tổng hợp

### Code changes (làm trên local trước khi push)

- [ ] **BLOCKER**: Sửa search trong `ProductRepository.cs` — xóa `EF.Functions.Contains`, dùng `.ToLower().Contains()`
- [ ] **BLOCKER**: Xóa folder `scratch/`
- [ ] Đổi package trong `.csproj`: `SqlServer` → `Npgsql`
- [ ] Đổi `UseSqlServer` → `UseNpgsql` và health check trong `DependencyInjection.cs`
- [ ] Xóa migrations cũ và generate lại với PostgreSQL
- [ ] Đổi `docker-compose.yml` sang PostgreSQL image
- [ ] Thêm `_cache.Remove("categories_all")` vào 3 category mutation handlers
- [ ] Sửa `CreateProductCommandHandler` — map ProductDto manual, không để Seller null
- [ ] Thêm `[Authorize(Roles = "Admin")]` cho Create/Update/Delete Category endpoints
- [ ] Push code lên GitHub

### Infrastructure

- [ ] Tạo Supabase project (Singapore), lấy connection string
- [ ] Chạy `dotnet ef database update` lên Supabase
- [ ] Tạo Gmail App Password
- [ ] Cấu hình Google OAuth Console, lấy Client ID/Secret
- [ ] Deploy backend lên Render, set đủ 17 env vars
- [ ] Test `GET /health` trả 200
- [ ] Deploy frontend lên Vercel, set `NEXT_PUBLIC_API_URL`
- [ ] Cập nhật `AllowedOrigins` trên Render với URL Vercel thật
- [ ] Cập nhật Google OAuth Console với URL Vercel thật
- [ ] Test end-to-end: login Google → tạo sản phẩm → upload ảnh → search

---

## Phần 7 — Roadmap sau khi go-live

Không cần làm ngay, nhưng cần làm khi có user thật:

**Ưu tiên cao:**
- **Logout endpoint** — hiện không có, refresh token không bị invalidate khi user logout từ client
- **Phân quyền Admin** — đã note ở Phần 2, nên làm trước go-live thật sự

**Ưu tiên trung bình:**
- **Full-Text Search PostgreSQL** — khi sản phẩm nhiều lên, dùng `EF.Functions.ToTsVector` thay cho `LIKE`
- **Soft delete cleanup job** — notifications `IsDeleted = true` tích lũy theo thời gian, cần Hangfire job dọn định kỳ

**Ưu tiên thấp (khi scale):**
- **Redis** thay `IMemoryCache` — khi deploy multi-instance, cache không share giữa các pod
- **Resend/Brevo** thay Gmail SMTP — Gmail giới hạn 500 email/ngày, Resend free 3.000/tháng