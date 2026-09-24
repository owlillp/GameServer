# AGENTS.md

## Project layout

- `frontend/` — Next.js (App Router) + React 19 + TypeScript + Tailwind CSS v4 UI.
- `backend/` — game server backend.

## Frontend: Modern Web Guidance (required)

This project installs the Chrome "Modern Web Guidance" skills (`.opencode/skills/`).

For **any** frontend task — HTML, CSS, client-side JavaScript/TypeScript, React components, layout, animation, forms, accessibility, or web performance — you MUST use the `modern-web-guidance` skill **before** writing code:

1. Search: `npx -y modern-web-guidance@latest search "<action-oriented query>" --skill-version 2026_09_04-7de96777`
2. Retrieve: `npx -y modern-web-guidance@latest retrieve "<id>"`
3. Apply the retrieved guide, including its documented fallbacks.

Do **not** default to legacy patterns (jQuery-style DOM code, JS-driven layout hacks, heavyweight libs) when a native modern web platform feature fits. Prefer platform APIs and progressive enhancement.

For Chrome Extension work (`manifest.json`, service worker, content scripts, popups, side panel, `chrome.*` APIs, Chrome Web Store publishing), also use the `chrome-extensions` skill.

## Browser Baseline target

**Browser Support:** Baseline 2024 (evergreen Chrome, Edge, Firefox, Safari).

Features at Baseline 2024 or earlier are safe to use without fallbacks. For features newer than this target, follow the fallback guidance in the retrieved guide. If the project's support requirements change, update this line.

## Frontend commands (run from `frontend/`)

- Lint: `npm run lint`
- Format: `npm run format`
- Build/typecheck: `npm run build`
