# SalesTrack Academy (MVP)

A role-based training platform for sales organizations.

## Stack

- ASP.NET Core MVC (.NET 10)
- ASP.NET Core Identity (Admin and Agent roles)
- EF Core + SQLite
- Razor views with custom UI styling

## Implemented Features

### Admin Features

- Creator dashboard with:
  - total courses
  - total agents
  - average completion rate
  - average quiz score
  - total comments (engagement metric)
- Course builder:
  - course create/edit with thumbnail
  - lesson types: Video, Audio, PDF, Text
  - lesson ordering with drag-and-drop + save
  - quick up/down reordering
- Quiz engine:
  - MCQ creation per lesson (4 options, one correct)
  - per-lesson passing score support
- User management and assignment:
  - assign courses directly to selected agents
  - create groups (for example, New Hires)
  - assign courses to groups
- Individual tracking:
  - per-agent progress and best quiz score snapshot

### Sales Agent Features

- My Learning dashboard:
  - assigned courses
  - progress bar per course
  - completion status
- Learning portal:
  - lesson player for video/audio/pdf/text
  - in-app PDF rendering
- Engagement:
  - comments on lesson pages
- Assessment:
  - take quizzes
  - score feedback
  - completion logic based on passing score

## Default Seeded Accounts

- Admin
  - email: admin@salestrack.local
  - password: Temp123!
- Agent
  - email: agent.a@salestrack.local
  - password: Temp123!

## Run

1. Open terminal in project root.
2. Run:

```bash
dotnet run
```

3. Open the URL shown in terminal.
4. Log in with a seeded account.

## Notes

- Database file is `app.db`.
- On startup, migrations are applied and initial demo data is seeded automatically.
- This MVP is optimized for demo and assignment delivery, and can be extended with:
  - certificate generation
  - richer analytics charts
  - real-time comments via SignalR
  - cloud media storage integration
