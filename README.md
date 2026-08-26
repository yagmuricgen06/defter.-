# Blog yönetim arka ucu

Bu bağımsız Node.js servisi ek paket kurmadan çalışır. Yazılar `data/store.json` dosyasında tutulur.

## Çalıştırma

```powershell
node server.js
```

Varsayılan yönetici bilgileri yalnızca geliştirme içindir: `admin@site.local` / `admin123!`.
Canlıya almadan önce ortam değişkenlerini tanımlayın:

```powershell
$env:ADMIN_EMAIL = 'yonetici@ornek.com'
$env:ADMIN_PASSWORD = 'guclu-bir-parola'
$env:TOKEN_SECRET = 'rastgele-ve-uzun-bir-gizli-anahtar'
node server.js
```

## API özeti

| İşlem | Adres |
| --- | --- |
| Giriş | `POST /api/auth/login` |
| Yayındaki yazılar | `GET /api/posts` |
| Tek yazı | `GET /api/posts/:slug` |
| Abone ol | `POST /api/subscriptions` |
| Yönetici yazı listesi | `GET /api/admin/posts` |
| Yazı oluştur | `POST /api/admin/posts` |
| Yazı güncelle | `PUT /api/admin/posts/:id` |
| Yazı sil | `DELETE /api/admin/posts/:id` |

Yönetici endpoint'lerinde girişten dönen belirteci `Authorization: Bearer <token>` başlığıyla gönderin.
