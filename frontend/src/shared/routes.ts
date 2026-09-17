export const routes = {
  home: "/",
  players: "/players",
} as const;

export type RouteKey = keyof typeof routes;
