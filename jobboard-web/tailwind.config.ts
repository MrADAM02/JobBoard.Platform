import type { Config } from "tailwindcss";
import colors from "tailwindcss/colors";

// darkMode: 'class' is required to pair with @nuxtjs/color-mode's class-based
// toggling (classSuffix: '' in nuxt.config.ts makes it emit a bare "dark"/"light"
// class on <html>). Tailwind's default "media" strategy would ignore the
// cookie-persisted user choice and follow only OS prefers-color-scheme.
export default {
  darkMode: "class",
  // Introducing this config file turns off @nuxtjs/tailwindcss's zero-config
  // content auto-detection, so every directory containing class strings must
  // be listed explicitly here.
  content: [
    "./app/components/**/*.{vue,js,ts}",
    "./app/layouts/**/*.vue",
    "./app/pages/**/*.vue",
    "./app/composables/**/*.{js,ts}",
    "./app/app.vue",
    "./app/error.vue",
  ],
  theme: {
    extend: {
      // Aliased (not hardcoded emerald-*/amber-* everywhere) so a future
      // rebrand is a one-line change here instead of a find-replace across
      // every page. primary/accent reuse Tailwind's stock emerald/amber
      // scales (deep green + gold), matching the reference design's palette.
      colors: {
        primary: colors.emerald,
        accent: colors.amber,
        cream: {
          50: "#FAF9F2",
          100: "#F5F3E7",
          200: "#EBE8D6",
        },
      },
      // @nuxt/fonts auto-detects both and self-hosts them at build time -
      // falls back to the standard stacks if that ever fails. "display" is
      // the heavy grotesk used for headings; "sans" stays the body font.
      fontFamily: {
        sans: ["Plus Jakarta Sans", "ui-sans-serif", "system-ui", "sans-serif"],
        display: ["Space Grotesk", "ui-sans-serif", "system-ui", "sans-serif"],
      },
    },
  },
  plugins: [],
} satisfies Config;
