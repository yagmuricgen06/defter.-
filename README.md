# 📖 Defter

**Defter** is a personal blog platform that brings together software, coding, technology, and everyday life.

It is a personal digital space where I share what I learn, my experiences, thoughts, and the connections between technology and everyday life.

## 🎯 Project Purpose

The goal of Defter is to explore software and technology not only from a technical perspective, but also through their connection to everyday life.

I aim to use Defter as a space to document my learning journey, share my experiences, and continuously improve my own blogging platform.

## ✍️ Content

The blog will cover topics such as:

* 💻 Software and programming
* 🤖 Artificial intelligence and technology
* 🌐 Web development
* 📚 Learning processes and experiences
* 💡 Technology in everyday life
* 📝 Personal notes and thoughts
* 🔍 New technologies and tools

## 🛠️ Technologies

* HTML
* CSS
* JavaScript
* Node.js
* REST API
* JSON
* Git & GitHub

## ⚙️ Project Structure

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

Defter uses an independent Node.js-based backend service.

Blog posts and subscription data are stored in the `data/store.json` file.

### API

| Operation       | Endpoint                      |
| --------------- | ----------------------------- |
| Login           | `POST /api/auth/login`        |
| Published posts | `GET /api/posts`              |
| Single post     | `GET /api/posts/:slug`        |
| Subscribe       | `POST /api/subscriptions`     |
| Admin post list | `GET /api/admin/posts`        |
| Create post     | `POST /api/admin/posts`       |
| Update post     | `PUT /api/admin/posts/:id`    |
| Delete post     | `DELETE /api/admin/posts/:id` |

Admin endpoints require authentication.

## ▶️ Running the Project

To run the backend locally:

```bash
node server.js
```

Then open the local application address in your browser.

## 🔐 Security

Administrator credentials used during development are intended only for local development.

Before deploying the project to a production environment, sensitive values such as administrator credentials and token secrets should be managed through environment variables.

Never commit real passwords, API keys, or other sensitive information to the GitHub repository.

## 📌 Project Status

🚧 **Currently in development**

Defter is an ongoing project. New features, blog posts, and improvements will be added over time.

## 🔮 Future Plans

* [ ] Improve the blog post creation and publishing system
* [ ] Add a category system
* [ ] Add a search feature
* [ ] Improve responsive design
* [ ] Add Dark / Light Mode
* [ ] Improve the user experience
* [ ] Publish new content regularly
* [ ] Deploy the blog to a production environment

---

**Defter** — A digital space where I learn, write, build, and share. ✨


