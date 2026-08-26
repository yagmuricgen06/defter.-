# 📖 Defter

**Defter**, yazılım, kodlama ve teknoloji dünyasını günlük yaşamla buluşturan kişisel bir blog platformudur.

Teknik bilgilerin yanında öğrenme süreçlerimi, deneyimlerimi, düşüncelerimi ve teknolojiyle günlük hayat arasındaki bağlantıları paylaşabileceğim kişisel bir dijital alan olarak geliştiriyorum.

## 🎯 Projenin Amacı

Defter'in amacı, yazılım ve teknoloji konularını yalnızca teknik açıdan değil, günlük hayatla ilişkilendirerek ele almak.

Öğrendiklerimi ve deneyimlerimi düzenli olarak paylaşırken aynı zamanda zaman içerisinde geliştirebileceğim kişisel bir blog platformu oluşturmayı hedefliyorum.

## ✍️ İçerik

Blogda zaman içerisinde şu konulara yer vermeyi planlıyorum:

* 💻 Yazılım ve programlama
* 🤖 Yapay zeka ve teknoloji
* 🌐 Web geliştirme
* 📚 Öğrenme süreçleri ve deneyimler
* 💡 Teknolojinin günlük hayattaki yeri
* 📝 Kişisel notlar ve düşünceler
* 🔍 Yeni teknolojiler ve araçlar

## 🛠️ Kullanılan Teknolojiler

* HTML
* CSS
* JavaScript
* Node.js
* REST API
* JSON
* Git & GitHub

## ⚙️ Proje Yapısı

```text
Defter/
├── index.html
├── post.html
├── admin.html
├── admin-login.html
├── server.js
├── Program.cs
├── Defter.Api.csproj
├── package.json
├── appsettings.json
├── README.md
└── data/
    └── store.json
```

## 🚀 Backend

Defter'in backend tarafında Node.js tabanlı bağımsız bir servis kullanılmaktadır.

Yazılar ve abonelik bilgileri `data/store.json` dosyasında tutulmaktadır.

### API

| İşlem                 | Endpoint                      |
| --------------------- | ----------------------------- |
| Giriş                 | `POST /api/auth/login`        |
| Yayındaki yazılar     | `GET /api/posts`              |
| Tek yazı              | `GET /api/posts/:slug`        |
| Abone ol              | `POST /api/subscriptions`     |
| Yönetici yazı listesi | `GET /api/admin/posts`        |
| Yazı oluştur          | `POST /api/admin/posts`       |
| Yazı güncelle         | `PUT /api/admin/posts/:id`    |
| Yazı sil              | `DELETE /api/admin/posts/:id` |

Yönetici endpoint'leri kimlik doğrulama gerektirir.

## ▶️ Çalıştırma

Projeyi yerel ortamda çalıştırmak için:

```bash
node server.js
```

Ardından tarayıcı üzerinden uygulamanın yerel adresine erişilebilir.

## 🔐 Güvenlik

Geliştirme ortamında kullanılan yönetici bilgileri yalnızca yerel kullanım içindir.

Proje canlı ortama taşınmadan önce yönetici bilgileri ve token secret gibi hassas değerlerin **ortam değişkenleri** üzerinden yönetilmesi gerekir.

Gerçek şifre, API anahtarı veya gizli bilgilerin GitHub repository'sine eklenmemesine dikkat edilmelidir.

## 📌 Proje Durumu

🚧 **Geliştirme aşamasında**

Defter aktif olarak geliştirilmektedir. Yeni özellikler, yazılar ve iyileştirmeler zaman içerisinde eklenecektir.

## 🔮 Gelecek Planları

* [ ] Blog yazısı oluşturma ve yayınlama sistemini geliştirmek
* [ ] Kategori sistemi eklemek
* [ ] Arama özelliği eklemek
* [ ] Responsive tasarımı geliştirmek
* [ ] Dark / Light Mode eklemek
* [ ] Kullanıcı deneyimini geliştirmek
* [ ] Yeni içerikler eklemek
* [ ] Blogu canlı ortama taşımak

---

**Defter** — Öğrendiklerimi yazdığım, geliştirdiğim ve paylaştığım dijital alan. ✨

