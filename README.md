# StreamingAPI

A lightweight, RESTful backend API for a streaming platform (inspired by TODD / BeIN Connect), developed during my Software Engineering Internship at Digiturk. Built with C# .NET 10 and a local JSON database.

## 🚀 Tech Stack

**C# .NET 10** (Web API) | **JWT** (Authentication) | **System.Text.Json** | **Swagger** | **Python** (TMDB Data Enrichment)

## ✨ Key Features

* **Full CRUD & Discovery:** Manage movies and series with advanced search, filtering (type/genre), sorting, and pagination.
* **Secure Admin Panel:** JWT-based authentication.
* **Rich Endpoints:** Dedicated routes for stats, genres, recently added, most viewed, and most liked content.
* **Automated Data Enrichment:** Python script (`update_db.py`) automatically fetches TMDB posters and YouTube trailers.

## 🛠️ Quick Start

```bash
git clone https://github.com/your-username/StreamingAPI.git
cd StreamingAPI
dotnet run

```

* **API:** `http://localhost:5162`
* **Swagger UI:** `http://localhost:5162/swagger`
* *(Optional)* Run `python3 update_db.py` to fetch external poster and trailer data.

## 📡 API Overview

* **Auth:** `POST /api/auth/login` *(Default credentials: admin / admin123)*
* **Content Operations:** `GET`, `POST`, `PUT`, `DELETE /api/content`
* **Queries for `GET /api/content`:** `search`, `type`, `genre`, `sortBy`, `desc`, `page`, `pageSize`
* **Analytics & Lists:** `/stats`, `/genres`, `/recent`, `/most-viewed`, `/most-liked`

## 📦 Content Model (JSON Snippet)

```json
{
  "id": "guid", "type": "movie|series", "title": "string", "genre": "string",
  "year": 2024, "videoUrl": "url", "thumbnailUrl": "url", "viewCount": 0
}

```

