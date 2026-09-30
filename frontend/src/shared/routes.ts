export const routes = {
  home: "/",
  login: "/login",
  register: "/register",
  profile: "/profile",
  dashboard: "/dashboard",
  players: "/players",
  clans: "/clans",
  admin: "/admin",
  adminUsers: "/admin/users",
  adminClans: "/admin/clans",
} as const;

export type RouteKey = keyof typeof routes;
