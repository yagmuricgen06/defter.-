using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:3000");
var sessions = new ConcurrentDictionary<string, byte>();
var adminEmail = builder.Configuration["Admin:Email"] ?? throw new InvalidOperationException("Admin:Email yapılandırması eksik.");
var adminPassword = builder.Configuration["Admin:Password"] ?? throw new InvalidOperationException("Admin:Password yapılandırması eksik.");
var store = new DefterStore(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "defter.json"), adminEmail, adminPassword);
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
bool IsAdmin(HttpRequest request) { var value = request.Headers.Authorization.ToString(); return value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) && sessions.ContainsKey(value[7..].Trim()); }
app.MapPost("/api/auth/login", (LoginRequest login) => { if (!store.VerifyLogin(login.Email, login.Password)) return Results.Json(new { error = "E-posta veya şifre hatalı." }, statusCode: StatusCodes.Status401Unauthorized); var token = Convert.ToHexString(Guid.NewGuid().ToByteArray()); sessions.TryAdd(token, 0); return Results.Ok(new { token }); });
app.MapGet("/api/posts", () => Results.Ok(new { posts = store.GetPublishedPosts() }));
app.MapGet("/api/posts/{slug}", (string slug) => { var post = store.GetPublishedPost(slug); return post is null ? Results.NotFound(new { error = "Yazı bulunamadı." }) : Results.Ok(new { post }); });
app.MapPost("/api/subscriptions", (SubscriptionRequest request) => { if (string.IsNullOrWhiteSpace(request.Email) || !Regex.IsMatch(request.Email.Trim(), @"^[^\s@]+@[^\s@]+\.[^\s@]+$")) return Results.BadRequest(new { error = "Geçerli bir e-posta adresi girin." }); var added = store.AddSubscriber(request.Email.Trim()); return Results.Ok(new { message = added ? "Aboneliğiniz kaydedildi." : "Bu e-posta zaten kayıtlı." }); });
app.MapGet("/api/admin/posts", (HttpRequest request) => !IsAdmin(request) ? Results.Unauthorized() : Results.Ok(new { posts = store.GetAllPosts(), subscribers = store.SubscriberCount }));
app.MapGet("/api/admin/settings", (HttpRequest request) => !IsAdmin(request) ? Results.Unauthorized() : Results.Ok(new { email = store.AdminEmail }));
app.MapPut("/api/admin/settings/password", (HttpRequest request, PasswordChangeRequest change) => { if (!IsAdmin(request)) return Results.Unauthorized(); if (string.IsNullOrWhiteSpace(change.NewPassword) || change.NewPassword.Length < 8) return Results.BadRequest(new { error = "Yeni şifre en az 8 karakter olmalıdır." }); return store.ChangePassword(change.CurrentPassword, change.NewPassword) ? Results.Ok(new { message = "Şifre güncellendi." }) : Results.BadRequest(new { error = "Mevcut şifre doğru değil." }); });
app.MapPost("/api/admin/posts", (HttpRequest request, PostRequest post) => { if (!IsAdmin(request)) return Results.Unauthorized(); var error = Validate(post); if (error is not null) return Results.BadRequest(new { error }); return Results.Created("/api/admin/posts", new { post = store.CreatePost(post) }); });
app.MapPut("/api/admin/posts/{id:guid}", (HttpRequest request, Guid id, PostRequest post) => { if (!IsAdmin(request)) return Results.Unauthorized(); var error = Validate(post); if (error is not null) return Results.BadRequest(new { error }); var updated = store.UpdatePost(id, post); return updated is null ? Results.NotFound(new { error = "Yazı bulunamadı." }) : Results.Ok(new { post = updated }); });
app.MapDelete("/api/admin/posts/{id:guid}", (HttpRequest request, Guid id) => { if (!IsAdmin(request)) return Results.Unauthorized(); return store.DeletePost(id) ? Results.NoContent() : Results.NotFound(new { error = "Yazı bulunamadı." }); });
app.Run();
static string? Validate(PostRequest post) => string.IsNullOrWhiteSpace(post.Title) ? "Başlık zorunludur." : string.IsNullOrWhiteSpace(post.Content) ? "İçerik zorunludur." : post.Status is not ("published" or "draft") ? "Durum published veya draft olmalıdır." : null;
record LoginRequest(string? Email, string? Password); record SubscriptionRequest(string? Email); record PostRequest(string? Title, string? Content, string? Status); record PasswordChangeRequest(string? CurrentPassword, string? NewPassword);
sealed class BlogPost { public Guid Id { get; init; } = Guid.NewGuid(); public string Title { get; set; } = ""; public string Content { get; set; } = ""; public string? Excerpt { get; set; } public string Status { get; set; } = "draft"; public string Slug { get; set; } = ""; public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow; public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow; public DateTimeOffset? PublishedAt { get; set; } }
sealed class AdminSettings { public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
sealed class DefterData { public List<BlogPost> Posts { get; init; } = []; public List<string> Subscribers { get; init; } = []; public AdminSettings Settings { get; set; } = new(); }
sealed class DefterStore
{
    private readonly object gate = new(); private readonly string path; private readonly JsonSerializerOptions json = new() { WriteIndented = true }; private DefterData data;
    public DefterStore(string path, string defaultEmail, string defaultPassword) { this.path = path; Directory.CreateDirectory(Path.GetDirectoryName(path)!); data = File.Exists(path) ? JsonSerializer.Deserialize<DefterData>(File.ReadAllText(path), json) ?? new() : Seed(); if (string.IsNullOrWhiteSpace(data.Settings.Email)) data.Settings.Email = defaultEmail; if (string.IsNullOrWhiteSpace(data.Settings.Password)) data.Settings.Password = defaultPassword; Save(); }
    public int SubscriberCount { get { lock (gate) return data.Subscribers.Count; } }
    public string AdminEmail { get { lock (gate) return data.Settings.Email; } }
    public bool VerifyLogin(string? email, string? password) { lock (gate) return string.Equals(email?.Trim(), data.Settings.Email, StringComparison.OrdinalIgnoreCase) && password == data.Settings.Password; }
    public bool ChangePassword(string? current, string? next) { lock (gate) { if (current != data.Settings.Password || string.IsNullOrWhiteSpace(next)) return false; data.Settings.Password = next; Save(); return true; } }
    public IReadOnlyList<BlogPost> GetPublishedPosts() { lock (gate) return data.Posts.Where(p => p.Status == "published").OrderByDescending(p => p.PublishedAt).ToList(); }
    public IReadOnlyList<BlogPost> GetAllPosts() { lock (gate) return data.Posts.OrderByDescending(p => p.UpdatedAt).ToList(); }
    public BlogPost? GetPublishedPost(string slug) { lock (gate) return data.Posts.FirstOrDefault(p => p.Status == "published" && p.Slug == slug); }
    public bool AddSubscriber(string email) { lock (gate) { if (data.Subscribers.Any(x => string.Equals(x, email, StringComparison.OrdinalIgnoreCase))) return false; data.Subscribers.Add(email); Save(); return true; } }
    public BlogPost CreatePost(PostRequest request) { lock (gate) { var now = DateTimeOffset.UtcNow; var post = new BlogPost { Title = request.Title!.Trim(), Content = request.Content!.Trim(), Status = request.Status!, Slug = UniqueSlug(request.Title!), UpdatedAt = now, PublishedAt = request.Status == "published" ? now : null }; data.Posts.Add(post); Save(); return post; } }
    public BlogPost? UpdatePost(Guid id, PostRequest request) { lock (gate) { var post = data.Posts.FirstOrDefault(p => p.Id == id); if (post is null) return null; post.Title = request.Title!.Trim(); post.Content = request.Content!.Trim(); post.Status = request.Status!; post.UpdatedAt = DateTimeOffset.UtcNow; if (post.Status == "published" && post.PublishedAt is null) post.PublishedAt = post.UpdatedAt; if (post.Status == "draft") post.PublishedAt = null; Save(); return post; } }
    public bool DeletePost(Guid id) { lock (gate) { var post = data.Posts.FirstOrDefault(p => p.Id == id); if (post is null) return false; data.Posts.Remove(post); Save(); return true; } }
    private string UniqueSlug(string title) { var slug = Slugify(title); var candidate = slug; var suffix = 2; while (data.Posts.Any(p => p.Slug == candidate)) candidate = $"{slug}-{suffix++}"; return candidate; }
    private void Save() => File.WriteAllText(path, JsonSerializer.Serialize(data, json));
    private static string Slugify(string text) { var normalized = text.ToLowerInvariant().Replace('ı', 'i').Replace('ş', 's').Replace('ğ', 'g').Replace('ü', 'u').Replace('ö', 'o').Replace('ç', 'c'); var slug = Regex.Replace(normalized, "[^a-z0-9]+", "-").Trim('-'); return string.IsNullOrEmpty(slug) ? "yazi" : slug; }
    private static DefterData Seed() { var now = DateTimeOffset.UtcNow; return new() { Posts = [new BlogPost { Title = "Bir fonksiyonu üç kez yeniden yazmanın hikayesi", Content = "İlk sürüm çalıştı. Sonra isimler daha iyi oldu. Sonunda kodun sade hâli ortaya çıktı.", Status = "published", Slug = "bir-fonksiyonu-uc-kez-yeniden-yazmanin-hikayesi", PublishedAt = now, UpdatedAt = now }] }; }
}
