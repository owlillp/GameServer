export const routes = {
  home: "/",
  login: "/login",
  register: "/register",
  profile: "/profile",
  dashboard: "/dashboard",
  players: "/players",
  admin: "/admin",
  adminUsers: "/admin/users",
} as const;

export type RouteKey = keyof typeof routes;
