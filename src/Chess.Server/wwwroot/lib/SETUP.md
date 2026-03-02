# SignalR Client Setup

The SignalR JavaScript client library needs to be placed in this directory.

## Option 1: Download from GitHub (Recommended)
1. Go to: https://github.com/dotnet/aspnetcore/releases
2. Find the latest release and download the `aspnetcore-runtime-X.X.X-win-x64.zip`
3. Extract and find: `shared\Microsoft.AspNetCore.App\X.X.X\wwwroot\lib\signalr\`
4. Copy `signalr.min.js` and `signalr.js` to this directory

## Option 2: Use LibMan (Library Manager)
Run from the Chess.Server project directory:
```bash
libman install @microsoft/signalr@latest -d wwwroot/lib/signalr
```

## Option 3: Use npm
```bash
npm install @microsoft/signalr
cp node_modules/@microsoft/signalr/dist/browser/signalr.min.js wwwroot/lib/
```

## Option 4: Direct Download
Download directly from jsDelivr:
```
https://cdn.jsdelivr.net/npm/@microsoft/signalr@8.0.5/dist/browser/signalr.min.js
```

After placing the file here, the application will serve it locally and won't require CDN access.
