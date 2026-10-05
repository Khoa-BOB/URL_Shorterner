# Design URL shortener

## UI
![Web UI](docs/web-ui.png)

## Run the Vue UI

Requires Node.js 22.12+ and the .NET 10 SDK.

Start the backend from the repository root:

```sh
dotnet run --project UrlShortener --launch-profile http
```

In development, the backend creates the SQLite tables on first startup.

In a second terminal, start the frontend:

```sh
cd Frontend
npm install
npm run dev
```

Open the address printed by Vite (normally http://localhost:5173), paste an
HTTP or HTTPS URL, and click **Shorten URL**. The resulting link points to
the backend at http://localhost:5001; use **Copy link** to share it.

The development proxy forwards `/url/shorten` to the backend. If its port
changes, update `Frontend/vite.config.js`. For deployment, serve the frontend
and API on the same origin or configure a reverse proxy for this endpoint.

Run `npm test` for the API check and `npm run build` to generate `Frontend/dist`.

[ByteByteGo_Guidance](https://bytebytego.com/courses/system-design-interview/design-a-url-shortener)

## Problem:
- Write operation: 100 million URLs are generated per day
- Write operation per second: 100 milion / 24 / 3600 = 1160
- Read operation: Assuming ratio of read operation to write operation is 10:1, read operation per second: 1160 * 10 = 11600
- Assuming the URL shortener service will run for 10 years, this means we must support 100 million * 365 * 10 = 365 bilion records
- Assume average URL length is 100
- Storage requirement over 10 years: 365 bilion * 100 bytes - 36.5 TB

## API endpoints:

1. Url shortening: To create a new short URL, a client sends a POST request, which contains one parameter
2. Url redirecting: To redirect a short URL to the corresponding long URL, a lient send a GET request.
