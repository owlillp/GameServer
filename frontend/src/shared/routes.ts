export const routes = {
  home: "/",
  login: "/login",
  register: "/register",
  profile: "/profile",
  dashboard: "/dashboard",
  players: "/players",
} as const;

export type RouteKey = keyof typeof routes;
