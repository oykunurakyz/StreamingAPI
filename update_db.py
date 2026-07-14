import json
import os
import time
import urllib.request
import urllib.parse

API_KEY = "308f79d27124cbfd796645b2c991d8a0"
BASE_URL = "https://api.themoviedb.org/3"
IMAGE_BASE = "https://image.tmdb.org/t/p/w500"

def tmdb_get(url):
    req = urllib.request.Request(url)
    with urllib.request.urlopen(req) as r:
        return json.loads(r.read().decode())

def get_movie_data(title, year):
    url = BASE_URL + "/search/movie?api_key=" + API_KEY + "&query=" + urllib.parse.quote(title) + "&year=" + str(year)
    data = tmdb_get(url)
    if not data["results"]:
        return "", ""
    movie = data["results"][0]
    movie_id = movie["id"]
    poster = IMAGE_BASE + movie["poster_path"] if movie.get("poster_path") else ""
    videos = tmdb_get(BASE_URL + "/movie/" + str(movie_id) + "/videos?api_key=" + API_KEY)
    trailer = ""
    for v in videos["results"]:
        if v["type"] == "Trailer" and v["site"] == "YouTube":
            trailer = "https://www.youtube.com/embed/" + v["key"]
            break
    return poster, trailer

def get_series_data(title, year):
    url = BASE_URL + "/search/tv?api_key=" + API_KEY + "&query=" + urllib.parse.quote(title) + "&first_air_date_year=" + str(year)
    data = tmdb_get(url)
    if not data["results"]:
        return "", ""
    show = data["results"][0]
    show_id = show["id"]
    poster = IMAGE_BASE + show["poster_path"] if show.get("poster_path") else ""
    videos = tmdb_get(BASE_URL + "/tv/" + str(show_id) + "/videos?api_key=" + API_KEY)
    trailer = ""
    for v in videos["results"]:
        if v["type"] == "Trailer" and v["site"] == "YouTube":
            trailer = "https://www.youtube.com/embed/" + v["key"]
            break
    return poster, trailer

db_path = os.path.join(os.path.dirname(__file__), "Data", "db.json")

with open(db_path, "r") as f:
    contents = json.load(f)

for item in contents:
    title = item.get("Title") or item.get("title", "")
    year = item.get("Year") or item.get("year", 0)
    content_type = item.get("Type") or item.get("type", "")

    print("Processing: " + title)

    if content_type == "movie":
        poster, trailer = get_movie_data(title, year)
    else:
        poster, trailer = get_series_data(title, year)

    if poster:
        if "ThumbnailUrl" in item:
            item["ThumbnailUrl"] = poster
        else:
            item["thumbnailUrl"] = poster
    if trailer:
        if "VideoUrl" in item:
            item["VideoUrl"] = trailer
        else:
            item["videoUrl"] = trailer

    time.sleep(0.3)

with open(db_path, "w") as f:
    json.dump(contents, f, indent=2, ensure_ascii=False)

print("Done! db.json updated.")
