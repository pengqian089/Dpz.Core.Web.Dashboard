# AGENTS.md

## Scope
- Single-project repo: Blazor WebAssembly app in `src/Dpz.Core.Web.Dashboard/` (`net10.0`, `Microsoft.NET.Sdk.BlazorWebAssembly`).
- Solution file at `src/Dpz.Core.Web.Dashboard.slnx` (new format) contains only this one project. The legacy `.sln` does not exist. There are no test projects and no CI workflows; `dotnet build` plus manual UI checks is the verification path.
- `.editorconfig` is at the repo root. C# conventions within: 4-space indent, max line 100, file-scoped namespaces, braces required on all control statements, strict nullable.

## Fast commands (run from repo root unless noted)
- Restore: `dotnet restore src/Dpz.Core.Web.Dashboard.slnx`
- Build (primary verification): `dotnet build src/Dpz.Core.Web.Dashboard/Dpz.Core.Web.Dashboard.csproj`
- C# formatting: `dotnet csharpier src/` (csharpier v1.3.0 is in `dotnet-tools.json`; not covered by npm scripts)
- Frontend check (run from `src/Dpz.Core.Web.Dashboard/`): `.\build.ps1 check` (typecheck + lint + format-check in one pass)
- Run locally: `dotnet run --project src/Dpz.Core.Web.Dashboard/Dpz.Core.Web.Dashboard.csproj`
- Publish: `dotnet publish src/Dpz.Core.Web.Dashboard/Dpz.Core.Web.Dashboard.csproj -c Release`

## Easy-to-miss runtime/setup facts
- `src/NuGet.config` adds GitHub Packages source `github-pengqian089` (`https://nuget.pkg.github.com/pengqian089/index.json`). **This file overrides default sources** — `nuget.org` is NOT listed. All package resolution goes through this single GitHub Packages feed; restoring `dpz.core.enumlibrary` may require feed credentials.
- `dotnet run` reads `Properties/launchSettings.json`: `ASPNETCORE_ENVIRONMENT=Development`, app URLs `https://localhost:5010;http://localhost:5011`, `launchBrowser=false`. There is only one profile (`"Dpz.Core.Web.Dashboard"`).
- `wwwroot/appsettings.json` (production) and `wwwroot/appsettings.Development.json` (development) configure endpoints. Development points at local backends: `BaseAddress=https://localhost:53381` (API), `OIDC.Authority=https://localhost:7183`, `SourceSite=https://localhost:37701`, `CDNBaseAddress=https://localhost:5505`. Login/API flows fail unless those services are running.
- `Program.cs` throws on startup if `BaseAddress`, `CDNBaseAddress`, or `SourceSite` are missing from configuration. Note: `OIDC.Authority` and `OIDC.ClientId` are NOT validated on startup.

## Frontend build pipeline (Vite/npm)
- Frontend sources live in `ClientApp/` (TypeScript, CSS). Build output is `wwwroot/assets/`.
- `build.ps1` is the unified entry point (run from `src/Dpz.Core.Web.Dashboard/`):
  - `.\build.ps1 prod` or `.\build.ps1 build` — clean + npm install + Vite build + sync `index.html` hashes/version + `dotnet build`
  - `.\build.ps1 dev` — Vite watch mode (frontend-only)
  - `.\build.ps1 typecheck` / `lint` / `format` / `format-check` / `check` / `clean`
- Prerequisites: Node.js + npm (for Vite, TypeScript, ESLint, Prettier).
- The Blazor app loads Vite-built ES modules via hash-named entries in `wwwroot/index.html`; `build.ps1` rewrites those `<script>` and `<link>` tags from the Vite manifest and syncs the loading-splash version from `<Version>` in the `.csproj`.
- Vite output uses `[name].[hash].js` (hash in filename, not query string) because some CDNs ignore query strings during caching. `cssCodeSplit: true` and `base: "./"` are set.
- For release artifacts that include frontend changes, run `.\build.ps1 prod` before `dotnet publish`.
- JS interop modules must be registered as Vite entry points in `vite.config.ts` and loaded at runtime via `IAssetManifestService.GetAssetPathAsync("src/interop/xxx.ts")`.
- Frontend code conventions are enforced by ESLint (`ClientApp/eslint.config.js` — 100 char max, curly braces required, no trailing comments) and Prettier (`ClientApp/.prettierrc.json` — 4-space tab width, 100 char print width, no trailing commas). TypeScript config is strict with `noUnusedLocals`/`noUnusedParameters`.

## Architecture guardrails
- Entrypoints: `src/Dpz.Core.Web.Dashboard/Program.cs` and `src/Dpz.Core.Web.Dashboard/App.razor`.
- DI registration in `Program.cs` — `RegisterInject` is a static local function inside the top-level statements block. It scans assemblies with reflection:
  - interfaces must live in namespace `Dpz.Core.Web.Dashboard.Service`
  - implementations must be non-abstract concrete classes in namespace `Dpz.Core.Web.Dashboard.Service.Impl`
  - one implementation per interface — it picks the first match via `FirstOrDefault`; multiple impls of the same interface will silently lose all but one
  - types outside those namespaces are not auto-registered. To add a service, place files in `Service/` and `Service/Impl/` accordingly.
  - All auto-registered services are scoped.
- All API calls should go through `IHttpService` (`Service/IHttpService.cs`, impl `Service/Impl/HttpService.cs`); it centralizes auth headers, redirects 401 responses to `/session-expired?returnUrl=...`, and exposes `GetPageAsync<T>` for paged endpoints (default page size 10).
- **`IHttpService` serializes all HTTP requests** via a static `SemaphoreSlim(1)` — only one request is in-flight at a time. Do not call `IHttpService` methods concurrently expecting parallel execution.
- UI dialogs, toasts, and notifications go through `IAppDialogService` (`Service/IAppDialogService.cs`), not standard Blazor patterns. Prefer the newer `AppDialogOptions`-based API; legacy compat methods (`AlertAsync`, `ConfirmAsync`, etc.) exist but are deprecated.
- `Program.BaseAddress`, `Program.CdnBaseAddress`, and `Program.WebHost` are static globals used throughout the app to access configuration.
- `Program.cs` registers **two** `HttpClient` instances in DI: (1) the auth-wrapped `"ServerAPI"` named client via `IHttpClientFactory`, and (2) a bare `HttpClient` with the same `BaseAddress`. Both are scoped. Most code consumes the factory-created client.
- In `App.razor`, only pages with `@attribute [Authorize]` are auth-gated and use `MainLayout`; unannotated pages render with `PublicLayout`. `NotFound` pages render with the `NotFound` layout.
- `MainLayout` performs an additional permission check on init: it reads the `Permissions` claim from auth state and redirects to `/no-permission` if the `Permissions.System` flag is not set. This means all `[Authorize]`-gated pages require the `System` permission — not just any authenticated user.
- `_Imports.razor` provides global usings for all Razor files: `System.Net.Http`, `Microsoft.AspNetCore.*`, `Dpz.Core.Web.Dashboard`, `.Shared`, `.Models`, `.Helper`, `.Service`, `.Shared.Components`. Do not add redundant `@using` directives in `.razor` files.
- `EnumConverter<T>` and `EnumNullableConverter<T>` (`EnumConverter.cs`) provide JSON converters for string-based enum serialization. The `Permissions` enum itself comes from the external `dpz.core.enumlibrary` NuGet package.

## Conventions worth preserving
- Page modules typically follow `Pages/<Module>/List.razor`, `Publish.razor`, `Edit.razor` with `.razor.cs` code-behind. Some modules (e.g., `Picture`, `Mumble`) use `Post.razor` instead of `Publish.razor`.
- New modules must be linked from `Shared/NavMenu.razor` to be reachable from the sidebar.
- `.csproj` excludes `Pages/Logs/List.razor` via `<_ContentIncludedByDefault Remove=...>`. The file does not currently exist on disk — this is a pre-emptive exclusion. Do not assume every `.razor` under `Pages/` is compiled.
- `.csproj` also excludes a single specific file: `wwwroot/js/modules/upload-interop.js` (not a wildcard). Vite interop output actually goes to `wwwroot/assets/`, so this exclusion is likely a legacy remnant.
- Prefer constructor / primary-constructor DI over `[Inject]`; private fields use `_camelCase`.
- When docs disagree with `build.ps1`, `Program.cs`, or `.editorconfig`, trust the executable source.
- C# specifics: empty collections over null, no `IEnumerable<T>` double enumeration, structured logging only (no string concatenation in log messages), no public fields, one type per `.cs` file, no trailing comments.
- `opencode.json` is gitignored and contains provider API keys — **never commit or expose these keys.**
- `.github/copilot-instructions.md` exists but some claims are stale (e.g., "cleancss" dependency, `.js` filenames); prefer this `AGENTS.md` + the actual source files.
