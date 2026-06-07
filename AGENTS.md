# AGENTS.md

## Scope
- Single-project repo: Blazor WebAssembly app in `src/Dpz.Core.Web.Dashboard/` (`net10.0`, `Microsoft.NET.Sdk.BlazorWebAssembly`).
- Solution `src/Dpz.Core.Web.Dashboard.sln` contains only this one project. There are no test projects and no CI workflows; `dotnet build` plus manual UI checks is the verification path.

## Fast commands (run from repo root)
- Restore: `dotnet restore src/Dpz.Core.Web.Dashboard.sln`
- Build (primary verification): `dotnet build src/Dpz.Core.Web.Dashboard/Dpz.Core.Web.Dashboard.csproj`
- Run locally: `dotnet run --project src/Dpz.Core.Web.Dashboard/Dpz.Core.Web.Dashboard.csproj`
- Publish: `dotnet publish src/Dpz.Core.Web.Dashboard/Dpz.Core.Web.Dashboard.csproj -c Release`

## Easy-to-miss runtime/setup facts
- `src/NuGet.config` adds GitHub Packages source `github-pengqian089` (`https://nuget.pkg.github.com/pengqian089/index.json`); restoring `dpz.core.enumlibrary` may require feed credentials.
- `dotnet run` reads `Properties/launchSettings.json`: `ASPNETCORE_ENVIRONMENT=Development`, app URLs `https://localhost:5010;http://localhost:5011`, `launchBrowser=false`.
- `wwwroot/appsettings.Development.json` points at local backends: `BaseAddress=https://localhost:53381` (API), `OIDC.Authority=https://localhost:7183`, `SourceSite=https://localhost:37701`, `CDNBaseAddress=https://localhost:5505`. Login/API flows fail unless those services are running.
- `Program.cs` throws on startup if `BaseAddress`, `CDNBaseAddress`, or `SourceSite` are missing from configuration.

## Frontend build pipeline (Vite/npm)
- Frontend sources live in `ClientApp/` (TypeScript, CSS). Build output is `wwwroot/assets/`.
- `build.ps1` is the unified entry point (run from `src/Dpz.Core.Web.Dashboard/`):
  - `.\build.ps1 prod` — clean + npm install + Vite build + sync `index.html` hashes/version + `dotnet build`
  - `.\build.ps1 dev` — Vite watch mode (frontend-only)
  - `.\build.ps1 typecheck` / `lint` / `format` / `format-check` / `check` / `clean`
- Prerequisites: Node.js + npm (for Vite, TypeScript, ESLint, Prettier).
- The Blazor app loads Vite-built ES modules via hash-named entries in `wwwroot/index.html`; `build.ps1` rewrites those `<script>` and `<link>` tags from the Vite manifest and syncs the loading-splash version from `<Version>` in the `.csproj`.
- For release artifacts that include frontend changes, run `.\build.ps1 prod` before `dotnet publish`.
- Detailed CSS/TS rules live in `src/CSS_MANAGEMENT.md` and `src/EncodingConventions.md`.

## Architecture guardrails
- Entrypoints: `src/Dpz.Core.Web.Dashboard/Program.cs` and `src/Dpz.Core.Web.Dashboard/App.razor`.
- DI registration in `Program.cs` (`RegisterInject`) is reflection- and namespace-driven:
  - interfaces must live in namespace `Dpz.Core.Web.Dashboard.Service`
  - implementations must be non-abstract concrete classes in namespace `Dpz.Core.Web.Dashboard.Service.Impl`
  - one implementation per interface — it picks the first match via `FirstOrDefault`; multiple impls of the same interface will silently lose all but one
  - types outside those namespaces are not auto-registered. To add a service, place files in `Service/` and `Service/Impl/` accordingly.
- All API calls should go through `IHttpService` (`Service/Impl/HttpService.cs`); it centralizes auth headers, redirects 401 responses to `/session-expired?returnUrl=...`, and exposes `GetPageAsync<T>` for paged endpoints.
- In `App.razor`, only pages with `@attribute [Authorize]` are auth-gated and use `MainLayout`; unannotated pages render with `PublicLayout`.

## Conventions worth preserving
- Page modules typically follow `Pages/<Module>/List.razor`, `Publish.razor`, `Edit.razor` with `.razor.cs` code-behind.
- New modules must be linked from `Shared/NavMenu.razor` to be reachable from the sidebar.
- `.csproj` excludes `Pages/Logs/List.razor` via `<_ContentIncludedByDefault Remove=...>`; do not assume every `.razor` under `Pages/` is compiled.
- `.editorconfig` enforces 4-space indentation, max line length 100, file-scoped namespaces, and `csharp_prefer_braces` (braces required even on single-line control statements).
- Prefer constructor / primary-constructor DI over `[Inject]`; private fields use `_camelCase` (see `src/EncodingConventions.md`).
- When docs disagree with `build.ps1`, `Program.cs`, or `.editorconfig`, trust the executable source.
