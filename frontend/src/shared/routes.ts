export const routes = {
  home: "/",
  login: "/login",
  register: "/register",
  dashboard: "/dashboard",
  players: "/players",
} as const;

export type RouteKey = keyof typeof routes;
