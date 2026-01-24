# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FinchServer is a web-based music library management application built with ASP.NET Core 9.0 and Blazor Server. It provides a UI for browsing, curating, and importing music through integration with the [Beets music library manager](https://beets.io/).

## Common Commands

### Build and Run
```bash
# Build the project (includes npm install, Tailwind CSS, and esbuild)
dotnet build FinchServer/FinchServer.csproj

# Run in development mode
dotnet run --project FinchServer/FinchServer.csproj

# Build for release (includes minification)
dotnet build FinchServer/FinchServer.csproj -c Release

# Publish self-contained single-file binary for macOS ARM64
dotnet publish FinchServer/FinchServer.csproj -c Release -r osx-arm64

# Publish for Linux
dotnet publish FinchServer/FinchServer.csproj -c Release -r linux-x64
```

### Database Management
```bash
# Create a new migration
dotnet ef migrations add MigrationName --project FinchServer/FinchServer.csproj

# Apply migrations (happens automatically on startup)
dotnet ef database update --project FinchServer/FinchServer.csproj
```

### Frontend Build Tools
```bash
# The following commands are run automatically during build via MSBuild targets
# but can be run manually for development:

cd FinchServer

# Build Tailwind CSS
npx @tailwindcss/cli -i ./Components/app.css -o ./wwwroot/app.css

# Build JavaScript bundle
./node_modules/.bin/esbuild ./Components/app.js --bundle --outfile=./wwwroot/app.js
```

## Architecture

### Dual Database Design

The application uses **two separate SQLite databases**:

1. **Finch Database** (`data/Finch.db`) - Application-owned data:
   - Playlists and playlist entries
   - Artist metadata cache (images from FanartTV)
   - Import job history
   - Uses `snake_case` naming convention via `DataContext`

2. **Beets Database** - Read-only reference to external Beets library:
   - Albums and tracks (items) from Beets
   - Accessed via `BeetsContext`
   - Location discovered from Beets config file

**Critical**: The Beets database is the source of truth for music data. Finch only reads from it and maintains a separate cache layer for metadata and user content (playlists).

### Beets Integration

FinchServer wraps the Beets CLI rather than using it as a library:

- `BeetsConfiguration` discovers Beets executable, config, and database paths
- `InteractiveCommand` class handles spawning and communicating with Beets processes
- Import workflow runs `beet import` as an interactive subprocess with terminal emulation
- Character-by-character stdout/stderr monitoring for real-time terminal display

### Project Structure

```
FinchServer/
├── Controllers/          # REST API endpoints (/api/v1/*)
│   ├── AlbumsController.cs
│   ├── ArtistsController.cs
│   ├── ItemsController.cs
│   ├── PlaylistsController.cs
│   ├── StatsController.cs
│   └── DTO/             # API data transfer objects
├── Components/          # Blazor UI components
│   ├── Pages/           # Routable pages (Dashboard, Albums, Artists, Playlists, Import)
│   ├── Modals/          # Dialog components
│   ├── Elements/        # Reusable UI components
│   ├── Layout/          # Main layout wrapper
│   ├── App.razor        # Root component
│   ├── Routes.razor     # Route definitions
│   ├── app.css          # Tailwind CSS source
│   └── app.js           # JavaScript bundle source
├── Database/            # Entity Framework Core models (Finch database)
│   ├── DataContext.cs
│   ├── Playlist.cs
│   ├── PlaylistEntry.cs
│   ├── Artist.cs
│   ├── ArtistImage.cs
│   └── ImportJob.cs
├── Beets/               # Beets integration
│   ├── BeetsContext.cs  # EF Core context for Beets database
│   ├── BeetsConfiguration.cs
│   ├── InteractiveCommand.cs
│   ├── Album.cs         # Beets album model
│   └── Item.cs          # Beets track model
├── Metadata/            # Artist metadata fetching
│   ├── MetadataManager.cs
│   └── FanartTV/        # FanartTV API integration
├── MusicImport/         # Music import workflow
│   └── MusicImporter.cs
├── Logging/             # Custom logging (Serilog)
├── Utilities/           # Helper extensions
├── Resources/           # Static file storage
│   ├── Artists/         # Artist images (served at /files/artists)
│   ├── Playlists/       # Playlist artwork
│   └── Thumbnails/      # Album thumbnails (300px width)
├── Temp/                # Temporary upload staging (gitignored)
├── Migrations/          # EF Core migrations
└── wwwroot/             # Static web assets (app.css, app.js generated)
```

### Key Architectural Patterns

**Hybrid Rendering Model**: Blazor Server components with REST API endpoints for data access

**Service Layer Architecture**:
- Controllers → Services → Database Contexts → EF Core
- Dependency injection for all services (metadata fetcher, music importer, logger)
- DbContext pooling (128 connections) for performance

**Metadata Fetching Strategy**:
- Lazy loading: Fetch artist artwork on first request
- Caching: Store in Finch database to avoid repeated API calls
- Two-step lookup: Discogs ID → MusicBrainz ID → FanartTV API
- Image processing: Download originals + generate 300px thumbnails using ImageSharp

**Music Import Flow**:
```
User Upload → MusicImporter.Upload() → Store in Temp/Import/{jobId}/ + DB record
            → User configures settings
            → MusicImporter.Import() → Spawn interactive Beets process
            → Terminal emulation in browser (XtermBlazor)
            → User provides metadata corrections through terminal
            → Beets updates its database
            → Import job marked complete
```

## Environment Configuration

Create a `.env` file in the root directory with:

```bash
# Required: Path to Beets executable
BEETS_EXECUTABLE=/usr/local/bin/beet

# Required: FanartTV API key for artist artwork
# Get one at https://fanart.tv/get-an-api-key/
FANARTTV_APIKEY=your_api_key_here
```

The application will:
1. Load `.env` via `DotEnv.Load()` in `Program.cs`
2. Discover Beets config path by running `beet config -p`
3. Parse Beets config to find database path
4. Connect to both databases on startup

## Important Conventions

### Code Style
- **Namespace organization**: One class per file, file name matches class name
- **EF Core naming**: Finch database uses `snake_case` (via `UseSnakeCaseNamingConvention()`), Beets uses its own schema
- **JSON serialization**: API responses use `snake_case_lower` (configured in Program.cs)

### Database Access
- **Always use DbContext pooling**: Both `DataContext` and `BeetsContext` are pooled
- **Include related entities**: Use `.Include()` for eager loading to avoid N+1 queries
- **Read-only queries**: Use `.AsNoTracking()` when not modifying entities
- **Migrations**: Apply automatically on startup in `Program.cs`

### File Management
- **Static file providers**: Multiple configured in `Program.cs`:
  - `/wwwroot` - Default static files
  - `/files/artists` - Artist images from `Resources/Artists/`
- **Upload handling**: Files go to `Temp/Import/{jobId}/` during import
- **Image naming**: Use GUIDs for uploaded playlist artwork to prevent collisions

### Blazor Components
- **Render mode**: Interactive Server for all components
- **Component lifecycle**: Use `OnInitializedAsync` for data loading
- **State management**: Use component state; no global state manager
- **Modal pattern**: Modals are separate components in `Components/Modals/`

## External Dependencies

### Required External Tools
- **Beets**: Must be installed and configured on the system
  - Tested with Beets 1.6.x
  - Path specified in `BEETS_EXECUTABLE` environment variable
  - Must have a valid Beets library configured

### APIs
- **FanartTV**: For artist images and artwork
  - Requires API key in `FANARTTV_APIKEY`
  - Rate limits apply (application caches all responses)
- **MusicBrainz**: For resolving Discogs→MusicBrainz IDs
  - No API key required
  - Must include User-Agent header ("Finch/1.0.0")

## Debugging Tips

### Database Inspection
```bash
# View Finch database
sqlite3 FinchServer/data/Finch.db

# View Beets database (path from config)
sqlite3 ~/.config/beets/library.db
```

### Common Issues

**"BEETS_EXECUTABLE environment variable not set"**
- Create `.env` file with `BEETS_EXECUTABLE=/path/to/beet`
- Ensure path is correct (can use `~` for home directory)

**"Could not find library path in configuration file"**
- Beets config must have `library: /path/to/library.db`
- Run `beet config -p` to see config location

**Import fails silently**
- Check Beets logs and configuration
- Ensure Beets can run `beet import` from command line
- Verify `Temp/Import/` directory exists and is writable

**Artist images not loading**
- Check `FANARTTV_APIKEY` is valid
- Verify artist has Discogs ID in Beets database
- Check MusicBrainz has mapping from Discogs→MusicBrainz

### Logging

Logs are written using Serilog to rolling files:
- Development: Detailed logs to console and file
- Production: File-based logging with rotation
- Custom logger implementation in `Logging/Logger.cs`
